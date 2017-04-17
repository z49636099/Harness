using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessSession
    {
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();
        public HarnessInfo HarnessSocketInfo { get; set; }
        public int Index { get; set; }
        public EnumEnd END { get; set; }
        public int BackendSocketPort { get; set; }

        /// <summary>Command Client to Harness</summary>
        public HarnessTCPClient SocketClient { get; set; }
        public EnumProtocolType Protocol { get; set; }

        public EnumConnectionType ConnectionType { get; set; }

        public HarnessBackendSocketServer BackendServer = new HarnessBackendSocketServer();
        public string CommandHead
        {
            get
            {
                if (END == EnumEnd.Frontend)
                {
                    return "m";
                }
                else
                {
                    return "s";
                }
            }
        }

        public string[] SettingInfo { get; set; }
        public HarnessSession(string SettingData)
        {
            SettingInfo = SettingData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            #region Get Connection Type
            if (SettingInfo[1].Contains("ETH"))
            {
                ConnectionType = EnumConnectionType.ETH;
            }
            else
            {
                ConnectionType = EnumConnectionType.COM;
            }
            #endregion

            if (SettingInfo[0].Contains("Frontend"))
            {
                END = EnumEnd.Frontend;
            }
            else
            {
                END = EnumEnd.Backend;

                if (ConnectionType == EnumConnectionType.ETH)
                {
                    BackendSocketPort = int.Parse(SettingInfo[6]);
                }
                else
                {
                    BackendSocketPort = int.Parse(SettingInfo[10]);
                }
            }

            //Get Index ex:Backend01 & Frontend01 ,Index is 1
            Match M = Regex.Match(SettingInfo[0], @"^[a-zA-Z]*(\d*)$");
            Index = int.Parse(M.Groups[1].Value);
        }

        public void SocketClientConnect()
        {
            SocketClient = new HarnessTCPClient();
            SocketClient.Connect(HarnessSocketInfo.IPAddress, HarnessSocketInfo.Port);
            if(END == EnumEnd.Backend)
            {
                BackendServer.HarnessSocket = SocketClient;
                Task.Factory.StartNew(new Action(() => { BackendServer.Start(BackendSocketPort); }));
            }
        }

        public void Setup()
        {
            List<string> Cmd = new List<string>();
            switch (Protocol)
            {
                case EnumProtocolType.Modbus:
                    Cmd = ModbusSetup();
                    break;
                case EnumProtocolType.IEC104:
                    Cmd = IEC104Setup();
                    break;
                case EnumProtocolType.IEC101:
                    break;
                case EnumProtocolType.DNP3:
                    break;
            }
            foreach (var cmd in Cmd)
            {
                SocketClient.Send(cmd);
            }
        }

        public void AddPoint()
        {
            try
            {
                string AddCmd = "";
                switch (Protocol)
                {
                    case EnumProtocolType.Modbus:
                        SocketClient.Send("smbdb action clear");
                        AddCmd = "{0} add point {1} value 0";
                        break;
                    case EnumProtocolType.IEC104:
                        SocketClient.Send("s104db action clear");
                        AddCmd = "{0} add ioa {1} value 0";
                        break;
                    case EnumProtocolType.IEC101:
                        SocketClient.Send("s101db action clear");
                        AddCmd = "{0} add ioa {1} value 0";
                        break;
                    case EnumProtocolType.DNP3:
                        SocketClient.Send("sdnpdb action clear");
                        AddCmd = "{0} add value 0";
                        break;
                }
                foreach (var MappingItem in MappingItemList)
                {
                    string Cmd = HarnessCommand.GetServerCommand(MappingItem.BackendDataType, Protocol);
                    int EndPoint = MappingItem.BackendStart + MappingItem.BackendCount - 1;
                    for (int pointIndex = MappingItem.BackendStart; pointIndex <= EndPoint; pointIndex++)
                    {
                        SocketClient.Send(string.Format(AddCmd, Cmd, pointIndex));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Add point fail : " + ex.Message);
            }
        }


        private List<string> ModbusSetup()
        {
            List<string> Cmd = new List<string>();
            string ChannelCmd = CommandHead + "mbopenchannel";
            string SessionCmd = CommandHead + "mbopensession";
            string ChannelName = CommandHead + "MB_" + SettingInfo[0];
            string SessionName = CommandHead + "Session" + SettingInfo[0];
            if (ConnectionType == EnumConnectionType.ETH)
            {
                Cmd.Add(string.Format("{0} name {1} host {2} portNum {3} localIpAddress {4}", ChannelCmd, ChannelName, SettingInfo[2], SettingInfo[3], SettingInfo[4]));
                Cmd.Add(string.Format("{0} name {1} address {2}", SessionCmd, SessionName, SettingInfo[5]));
            }
            else
            {
                Cmd.Add(string.Format("{0} name {1} port {2} baud {3} dataBits {4} parity {5} stopBits {6} type {7}",
                        ChannelCmd, ChannelName, SettingInfo[2], SettingInfo[4], SettingInfo[5],
                        SettingInfo[6], SettingInfo[7], SettingInfo[3]));
                Cmd.Add(string.Format("{0} name {1} address {2}", SessionCmd, SessionName, SettingInfo[7]));
            }
            return Cmd;
        }

        private List<string> IEC104Setup()
        {
            List<string> Cmd = new List<string>();
            string ChannelCmd = CommandHead + "104openchannel";
            string SessionCmd = CommandHead + "104opensession";
            string SectorCmd = CommandHead + "104opensector";
            string ChannelName = CommandHead + "104_" + SettingInfo[0];
            string SessionName = CommandHead + "104Session_" + SettingInfo[0];
            string SectorName = CommandHead + "104Sector_" + SettingInfo[0];

            if (ConnectionType == EnumConnectionType.ETH)
            {
                Cmd.Add(string.Format("{0} name {1} host {2} portNum {3} localIpAddress {4}", ChannelCmd, ChannelName, SettingInfo[2], SettingInfo[3], SettingInfo[4]));
                Cmd.Add(string.Format("{0} name {1}", SessionCmd, SessionName));
                Cmd.Add(string.Format("{0} name {1} address {2} session 0", SectorCmd, SectorName, SettingInfo[5]));
            }
            else
            {
                Cmd.Add(string.Format("{0} name {1} localIpAddress {2} portNum {3} host {4}", ChannelCmd, ChannelName, SettingInfo[4], SettingInfo[3], SettingInfo[2]));
                Cmd.Add(string.Format("{0} name {1} address {2}", SessionCmd, SessionName, SettingInfo[5]));
                Cmd.Add(string.Format("{0} name {1} address {2}", SectorCmd, SectorName, SettingInfo[7]));


            }
            return Cmd;
        }

    }
}
