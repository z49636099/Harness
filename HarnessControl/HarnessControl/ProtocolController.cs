using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace HarnessControl
{
    public class ProtocolController
    {
        public List<PCNetworkInfo> HarnessInfoList = new List<PCNetworkInfo>();
        public List<CommunicationBase> CommunicationList = new List<CommunicationBase>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();

        public atopPortocolBase Frontend { get; set; }
        private List<ProcessController> ProcessList = new List<ProcessController>();

        public void ParsingConfigXml(string xml)
        {
            try
            {
                XmlDocument XmlDoc = new XmlDocument();
                XmlDoc.LoadXml(xml);
                XmlNodeList NodeLists = XmlDoc.SelectNodes("root");

                foreach (XmlNode node in NodeLists[0]["MappingData"])
                {
                    ConfigMappingItem Item = new ConfigMappingItem(node);
                    MappingItemList.Add(Item);
                }

                foreach (XmlNode node in NodeLists[0]["Setting"])
                {
                    string EndString = node.Name;
                    EnumProtocolType Protocol = GetProtocolType(node.Attributes["Protocol"].Value);
                    CommunicationBase Communication = CommunicationBase.CreateCommunication(Protocol);
                    Communication.Protocol = Protocol;
                    Communication.END = EndString.Contains("Backend") ? EnumEnd.Backend : EnumEnd.Frontend;
                    Communication.Index = int.Parse(node.Attributes["Number"].Value);
                    Communication.COMM_Type = GetConnectionType(node.Attributes["COMM_Type"].Value);

                    Communication.ParsingXML(node);

                    var NetworkInfo = HarnessInfoList.Where(a => a.Type == Communication.END && a.Index == Communication.Index).First();
                    Communication.PCInfo = NetworkInfo;
                    var ItemList = (from a in MappingItemList
                                    where (Communication.END == EnumEnd.Frontend && a.FrontendIndex == Communication.Index) ||
                                          (Communication.END == EnumEnd.Backend && a.BackendIndex == Communication.Index)
                                    select a).ToList();
                    Communication.MappingItemList = ItemList;
                    CommunicationList.Add(Communication);
                }

            }
            catch (Exception ex)
            {

            }
        }

        /*
        public void ParsingConfig(string Config)
        {
            int DataType = 0;
            string[] ConfigArray = Config.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> SettingList = new List<string[]>();
            foreach (var Line in ConfigArray)
            {
                if (string.IsNullOrWhiteSpace(Line))
                {
                    continue;
                }
                if (Line.Contains("Setting"))
                {
                    DataType = 1;
                    continue;
                }
                if (Line.Contains("Mapping"))
                {
                    DataType = 2;
                    continue;
                }
                switch (DataType)
                {
                    case 1:
                        if (Line.StartsWith("Frontend") || Line.StartsWith("Backend"))
                        {
                            SettingList.Add(Line.Split(' '));
                            //SessionList.Add(new HarnessSession(Line));
                        }
                        break;
                    case 2:
                        MappingItemList.Add(new ConfigMappingItem(Line));
                        break;
                }
            }
            foreach (var SettingLine in SettingList)
            {
                var SettingInfo = GetSettingInfo(SettingLine[0]);
                var EndType = SettingInfo.Item1;
                var Index = SettingInfo.Item2;
                EnumProtocolType ProtocolType;
                //PC Network Info
                var NetworkInfo = HarnessInfoList.Where(a => a.Type == SettingInfo.Item1 && a.Index == SettingInfo.Item2).First();
                var ItemList = (from a in MappingItemList
                                where (EndType == EnumEnd.Frontend && a.FrontendIndex == Index) ||
                                      (EndType == EnumEnd.Backend && a.BackendIndex == Index)
                                select a).ToList();

                ConfigMappingItem Item = ItemList.FirstOrDefault();
                if (Item != null)
                {
                    continue;
                }
                if (EndType == EnumEnd.Backend)
                {
                    ProtocolType = Item.BackendProtocolType;
                }
                else
                {
                    ProtocolType = Item.FrontendProtocolType;
                }
                CommunicationBase Communication;
                switch (ProtocolType)
                {
                    case EnumProtocolType.DNP3:
                    case EnumProtocolType.IEC101:
                    case EnumProtocolType.IEC104:
                    case EnumProtocolType.Modbus:
                        Communication = new Communication_Harness(SettingLine);
                        break;
                    case EnumProtocolType.IEC61850:
                        Communication = new Communication61850(SettingLine);
                        break;
                    default:
                        continue;
                }
                Communication.END = EndType;
                Communication.Index = Index;
                Communication.PCInfo = NetworkInfo;
                Communication.Name = SettingLine[0];
                Communication.Protocol = ProtocolType;
                Communication.MappingItemList = ItemList;
                CommunicationList.Add(Communication);
            }
        }
        */

        public void Setup(string SessionName)
        {
            //var Communication = CommunicationList.Where(a => a.Name == SessionName).FirstOrDefault();
            //if (Communication == null)
            //{
            //    throw new Exception("No find " + SessionName);
            //}

            Match M = Regex.Match(SessionName, @"^[a-zA-Z]*(\d*)$");
            int Index = int.Parse(M.Groups[1].Value);
            var Communication = CommunicationList.Where(a => SessionName.Contains(a.END.ToString()))
                                                 .Where(a => Index == a.Index)
                                                 .FirstOrDefault();
            if (Communication == null)
            {
                throw new Exception("No find " + SessionName);
            }

            Communication.Setup();
            Thread.Sleep(3000);
            if (Communication.END == EnumEnd.Frontend)
            {
                switch (Communication.Protocol)
                {
                    case EnumProtocolType.Modbus:
                        Frontend = new Modbus(Communication);
                        break;
                    case EnumProtocolType.IEC101:
                    case EnumProtocolType.IEC104:
                        Frontend = new IEC_10X(Communication);
                        break;
                    case EnumProtocolType.DNP3:
                        break;
                    case EnumProtocolType.IEC61850:
                        break;
                }
                //Frontend.FrontendCommunication = Communication;
                Frontend.BackendCommunication = CommunicationList.Where(a => a.END == EnumEnd.Backend).ToList();
                foreach (var ClientSession in CommunicationList.Where(a => a.END == EnumEnd.Backend).OrderBy(a => a.Index))
                {
                    PCNetworkInfo Info = HarnessInfoList.Where(a => a.Type == EnumEnd.Backend)
                                                       .Where(a => a.Index == ClientSession.Index)
                                                       .Select(a => a).First();
                    SocketClient SocketClient = new SocketClient();
                    SocketClient.Connect(Info.IPAddress, ClientSession.BackendSocketPort);
                    //SocketClient.Connect("10.0.176.200", ClientSession.BackendSocketPort);
                    Frontend.SocketClientList.Add(SocketClient);
                }
            }
        }

        public void Close()
        {
            Frontend.SocketClientList.ForEach(a => a.TelnetClinet.Close());
            //CommunicationList.ForEach(a => a.SocketClient.TelnetClinet.Close());
            ProcessList.ForEach(a => a.HarnessProcess.Close());
            CommunicationList.Clear();
            MappingItemList.Clear();
        }

        private Tuple<EnumEnd, int> GetSettingInfo(string Setting)
        {
            Match M = Regex.Match(Setting, @"^[a-zA-Z]*(\d*)$");
            int Index = int.Parse(M.Groups[1].Value);
            if (Setting.Contains("Frontend"))
            {
                return new Tuple<EnumEnd, int>(EnumEnd.Frontend, Index);
            }
            else
            {
                return new Tuple<EnumEnd, int>(EnumEnd.Backend, Index);
            }
        }

        private EnumProtocolType GetProtocolType(string Protocol)
        {
            switch (Protocol.ToUpper())
            {
                case "MODBUS":
                    return EnumProtocolType.Modbus;
                case "IEC104":
                    return EnumProtocolType.IEC104;
                case "IEC101":
                    return EnumProtocolType.IEC101;
                case "DNP3":
                    return EnumProtocolType.DNP3;
                case "61850":
                    return EnumProtocolType.IEC61850;
                default:
                    throw new Exception("Get protocol fail : " + Protocol);
            }
        }

        private EnumCOMM_Type GetConnectionType(string Connect)
        {
            switch (Connect.ToUpper())
            {
                case "COM":
                    return EnumCOMM_Type.COM;
                case "VCOM":
                    return EnumCOMM_Type.VCOM;
                case "ETH":
                    return EnumCOMM_Type.ETH;
                default:
                    throw new Exception("Get Connect fail : " + Connect);
            }
        }
    }
    public class PCNetworkInfo
    {
        public EnumEnd Type { get; set; }
        public int Index { get; set; }
        public int Port { get; set; }
        public string IPAddress { get; set; }
    }
}
