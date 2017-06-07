using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace HarnessControl
{
    public abstract class CommunicationBase
    {
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();

        public BackendSocketServer BackendServer = new BackendSocketServer();
        //public string[] SettingInfo { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public PCNetworkInfo PCInfo { get; set; }
        public EnumEnd END { get; set; }
        public EnumProtocolType Protocol { get; set; }
        public EnumCOMM_Type COMM_Type { get; set; }

        public int BackendSocketPort { get; set; }

        public abstract void Setup();

        public abstract void Clear();

        public abstract void ParsingXML(XmlNode node);

        public static CommunicationBase CreateCommunication(EnumProtocolType protocol)
        {
            switch (protocol)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    return new Communication_Harness();
                case EnumProtocolType.IEC61850:
                    return new Communication61850();
                default:
                    throw new Exception("No Create " + new StackFrame().GetMethod().DeclaringType);
            }
        }
    }
}
