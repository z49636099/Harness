using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace HarnessControl
{
    public class Communication_Harness : CommunicationBase
    {
        private ProcessController P = new ProcessController();
        /// <summary>Command Client to Harness</summary>
        public SocketClient SocketClient { get; set; }
        public string SlaveName { get; set; }

        public string Connection_Type { get; set; }

        public List<int> SlaveNames = new List<int>();

        private string CommandHead
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

        Dictionary<string, string> DicParameter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Communication_Harness()
        {

        }

        //public Communication_Harness(string[] _SettingInfo)
        //{
        //    SettingInfo = _SettingInfo;
        //    Name = SettingInfo[0];
        //    #region Get Connection Type
        //    if (SettingInfo[1].Contains("ETH"))
        //    {
        //        COMM_Type = EnumCOMM_Type.ETH;
        //    }
        //    else if (SettingInfo[1].Contains("VCOM"))
        //    {
        //        COMM_Type = EnumCOMM_Type.VCOM;
        //    }
        //    else
        //    {
        //        COMM_Type = EnumCOMM_Type.COM;
        //    }
        //    #endregion
        //}

        public override void Setup()
        {
            P.HarnessProcessStart(@"C:\Program Files (x86)\Triangle MicroWorks\Protocol Test Harness\bin\tmwtest.exe",
                          $"-tcl \"source {Application.StartupPath.Replace("\\", "/") + "/"}SocketServer.tcl\";\"CreateSocketServer {PCInfo.Port}\"");
            Thread.Sleep(3000);
            SocketClientConnect();
            HarnessSetup();
            if (END == EnumEnd.Backend)
            {
                AddPoint();
            }
        }

        /// <summary>Connect Harness with Socket</summary>
        public void SocketClientConnect()
        {
            SocketClient = new SocketClient();
            SocketClient.Connect(PCInfo.IPAddress, PCInfo.Port);
            if (END == EnumEnd.Backend)
            {
                BackendServer.HarnessSocket = SocketClient;
                Task.Factory.StartNew(new Action(() =>
                {
                    BackendServer.Start<BackendSocketAccept_Harness>(BackendSocketPort);
                }));
            }
        }

        public void HarnessSetup()
        {
            List<string> Cmd = new List<string>();
            string CName = END.ToString() + Index;
            switch (Protocol)
            {
                case EnumProtocolType.Modbus:
                    Cmd = ModbusSetup(CName);
                    break;
                case EnumProtocolType.IEC104:
                    Cmd = IEC104Setup(CName);
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

        public override void ParsingXML(XmlNode node)
        {
            foreach (XmlNode Par in node.ChildNodes)
            {
                string ParName = Par.Name;
                switch (ParName)
                {
                    case "SlaveID":
                        SlaveNames.AddRange(Par.InnerText.Split(',').Select(a => int.Parse(a)).ToArray());
                        break;
                    case "Connection_Type":
                        Connection_Type = Par.InnerText;
                        break;
                    case "Parameter":
                        foreach (XmlAttribute ParNode in Par.Attributes)
                        {
                            DicParameter.Add(ParNode.Name, ParNode.Value);
                        }
                        break;
                    case "SocketPort":
                        BackendSocketPort = int.Parse(Par.InnerText);
                        break;
                }
            }
        }


        public override void Clear()
        {
            SocketClient.TelnetClinet.Close();
        }

        public void AddPoint()
        {
            try
            {
                string AddCmd = "";
                string ClearCmd = "";
                switch (Protocol)
                {
                    case EnumProtocolType.Modbus:
                        ClearCmd = "smbdb session {0} action clear";
                        AddCmd = "{0} add point {1} value 0 session {2}";
                        break;
                    case EnumProtocolType.IEC104:
                        ClearCmd = "s104db sector {0} action clear";
                        AddCmd = "{0} add ioa {1} value 0 sector {2}";
                        break;
                    case EnumProtocolType.IEC101:
                        ClearCmd = "s101db sector {0} action clear";
                        AddCmd = "{0} add ioa {1} value 0 sector {2}";
                        break;
                    case EnumProtocolType.DNP3:
                        ClearCmd = "sdnpdb session {0} action clear";
                        AddCmd = "{0} add value 0 session {2}";
                        break;
                }
                foreach (int SlaveID in SlaveNames)
                {
                    SocketClient.Send(string.Format(ClearCmd, SlaveID), -1);
                }
                Thread.Sleep(1000);
                foreach (var MappingItem in MappingItemList)
                {
                    string Cmd = Command_Harness.GetSlaveCommand(MappingItem.BackendDataType, Protocol);
                    int EndPoint = MappingItem.BackendStart + MappingItem.BackendCount - 1;
                    for (int pointIndex = MappingItem.BackendStart; pointIndex <= EndPoint; pointIndex++)
                    {
                        SocketClient.Send(string.Format(AddCmd, Cmd, pointIndex, MappingItem.BackendSlaveID));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Add point fail : " + ex.Message);
            }
        }

        private List<string> ModbusSetup(string Name)
        {
            List<string> Cmd = new List<string>();
            string ChannelCmd = CommandHead + "mbopenchannel";
            string SessionCmd = CommandHead + "mbopensession";
            string ChannelName = CommandHead + "MB_" + Name;
            if (COMM_Type == EnumCOMM_Type.ETH)
            {
                Cmd.Add(string.Format("{0} name {1} host {2} portNum {3} localIpAddress {4}", ChannelCmd, ChannelName, DicParameter["IPAddress"], DicParameter["PortNum"], DicParameter["LocalIpAddress"]));
            }
            else
            {
                Cmd.Add(string.Format("{0} name {1} port {2} baud {3} dataBits {4} parity {5} stopBits {6} type {7}",
                        ChannelCmd, ChannelName, DicParameter["COMPort"], DicParameter["BaudRate"], DicParameter["DataBits"],
                        DicParameter["Parity"], DicParameter["StopBits"], Connection_Type));
            }
            foreach (int SlaveID in SlaveNames)
            {
                string SessionName = CommandHead + "Session" + SlaveID;
                Cmd.Add(string.Format("{0} name {1} address {2}", SessionCmd, SessionName, SlaveID));
            }

            return Cmd;
        }

        private List<string> IEC104Setup(string Name)
        {
            List<string> Cmd = new List<string>();
            string ChannelCmd = CommandHead + "104openchannel";
            string SessionCmd = CommandHead + "104opensession";
            string SectorCmd = CommandHead + "104opensector";
            string ChannelName = CommandHead + "104_" + Name;
            string SessionName = CommandHead + "104Session_" + Name;

            if (COMM_Type == EnumCOMM_Type.ETH)
            {
                Cmd.Add(string.Format("{0} name {1} host {2} portNum {3} localIpAddress {4}", ChannelCmd, ChannelName, DicParameter["IPAddress"], DicParameter["PortNum"], DicParameter["LocalIpAddress"]));
                Cmd.Add(string.Format("{0} name {1}", SessionCmd, SessionName));
            }
            else
            {
                Cmd.Add(string.Format("{0} name {1} port {2} baud {3} dataBits {4} parity {5} stopBits {6} type {7}",
                       ChannelCmd, ChannelName, DicParameter["COMPort"], DicParameter["BaudRate"], DicParameter["DataBits"],
                       DicParameter["Parity"], DicParameter["StopBits"], Connection_Type));
                Cmd.Add(string.Format("{0} name {1} ", SessionCmd, SessionName));
            }

            foreach (int SlaveID in SlaveNames)
            {
                string SectorName = CommandHead + "104Sector_" + SlaveID;
                Cmd.Add(string.Format("{0} name {1} address {2} session {3}", SectorCmd, SectorName, SlaveID));
            }
            return Cmd;
        }

    }
}
