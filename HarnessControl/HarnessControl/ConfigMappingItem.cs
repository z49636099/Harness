using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace HarnessControl
{
    public class ConfigMappingItem
    {
        public ConfigMappingItem(string Item)
        {
            MappingString = Item;
            string[] ItemArray = Item.Split(' ');
            FrontendName = ItemArray[0];
            BackendName = ItemArray[4];
            BackendProtocolType = GetProtocolType(BackendName);
            FrontendProtocolType = GetProtocolType(FrontendName);
            FrontendDataType = ItemArray[1];
            BackendDataType = ItemArray[5];

            switch (FrontendProtocolType)
            {
                case EnumProtocolType.IEC61850:
                    FrontendServer = ItemArray[2];
                    FrontendTag = ItemArray[3];
                    break;
                default:
                    FrontendStart = int.Parse(ItemArray[2]);
                    FrontendCount = int.Parse(ItemArray[3]);
                    break;
            }

            switch (BackendProtocolType)
            {
                case EnumProtocolType.IEC61850:
                    BackendServer = ItemArray[2];
                    BackendTag = ItemArray[3];
                    break;
                default:
                    BackendStart = int.Parse(ItemArray[6]);
                    BackendCount = int.Parse(ItemArray[7]);
                    break;
            }
        }

        public ConfigMappingItem(XmlNode node)
        {
            foreach (XmlNode EndNode in node.ChildNodes)
            {
                string End = EndNode.Name;
                switch (End)
                {
                    case "Frontend":
                        foreach(XmlAttribute Att in EndNode.Attributes)
                        {
                            string AttName = Att.Name;
                            switch (AttName)
                            {
                                case "Number":
                                    FrontendIndex = int.Parse(Att.Value);
                                    break;
                                case "DataType":
                                    FrontendDataType = Att.Value;
                                    break;
                                case "StartAddress":
                                    FrontendStart = int.Parse(Att.Value);
                                    break;
                                case "Quantity":
                                    FrontendCount = int.Parse(Att.Value);
                                    break;
                                case "SLAVEID":
                                    FrontendSlaveID = int.Parse(Att.Value);
                                    break;
                            }
                        }
                        break;
                    case "Backend":
                        foreach (XmlAttribute Att in EndNode.Attributes)
                        {
                            string AttName = Att.Name;
                            switch (AttName)
                            {
                                case "Number":
                                    BackendIndex = int.Parse(Att.Value);
                                    break;
                                case "DataType":
                                    BackendDataType = Att.Value;
                                    break;
                                case "StartAddress":
                                    BackendStart = int.Parse(Att.Value);
                                    break;
                                case "Quantity":
                                    BackendCount = int.Parse(Att.Value);
                                    break;
                                case "SLAVEID":
                                    BackendSlaveID = int.Parse(Att.Value);
                                    break;
                            }
                        }
                        break;
                }
            }
        }


        public string MappingString { get; private set; }

        private string _FrontendName { get; set; }
        public string FrontendName
        {
            get { return _FrontendName; }
            set
            {
                _FrontendName = value;
                Match M = Regex.Match(value, @"^.*-(\d*)$");
                FrontendIndex = int.Parse(M.Groups[1].Value);
            }
        }

        public int FrontendSlaveID { get; set; }
        public int BackendSlaveID { get; set; }

        private string _BackendName { get; set; }
        public string BackendName
        {
            get { return _BackendName; }
            set
            {
                _BackendName = value;
                Match M = Regex.Match(value, @"^.*-(\d*)$");
                BackendIndex = int.Parse(M.Groups[1].Value);
            }
        }

        private EnumProtocolType GetProtocolType(string Name)
        {
            if (Name.StartsWith("101"))
            {
                return EnumProtocolType.IEC101;
            }
            else if (Name.StartsWith("104"))
            {
                return EnumProtocolType.IEC104;
            }
            else if (Name.StartsWith("Mo"))
            {
                return EnumProtocolType.Modbus;
            }
            else if (Name.StartsWith("DN"))
            {
                return EnumProtocolType.DNP3;
            }
            else
            {
                throw new Exception("No support :" + Name);
            }
        }

        public EnumProtocolType FrontendProtocolType { get; set; }
        public int FrontendIndex { get; set; }
        public int BackendIndex { get; set; }
        public string FrontendDataType { get; set; }
        public int FrontendStart { get; set; }
        public int FrontendCount { get; set; }
        public string FrontendServer { get; set; }
        public string FrontendTag { get; set; }

        public EnumProtocolType BackendProtocolType { get; set; }
        public string BackendDataType { get; set; }
        public int BackendStart { get; set; }
        public int BackendCount { get; set; }
        public string BackendServer { get; set; }
        public string BackendTag { get; set; }
        public int Port { get; set; }
    }
}
