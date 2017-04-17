using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessController
    {
        public List<HarnessInfo> HarnessInfoList = new List<HarnessInfo>();
        public List<HarnessSession> SessionList = new List<HarnessSession>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();

        public string ConfigPath { get; set; }

        public void ParsingConfigFormFile()
        {
            string ConfigData;
            using (StreamReader sr = new StreamReader(ConfigPath))
            {
                ConfigData = sr.ReadToEnd();
            }
            ParsingConfig(ConfigData);
        }
        public void ParsingConfig(string Config)
        {
            int DataType = 0;
            string[] ConfigArray = Config.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
                            SessionList.Add(new HarnessSession(Line));
                        }
                        break;
                    case 2:
                        MappingItemList.Add(new ConfigMappingItem(Line));
                        break;
                }
            }
            foreach (var Session in SessionList)
            {
                Session.HarnessSocketInfo = HarnessInfoList.Where(a => a.Type == Session.END && a.Index == Session.Index).First();
                Session.MappingItemList = (from a in MappingItemList
                                           where (Session.END == EnumEnd.Frontend && a.FrontendIndex == Session.Index) ||
                                                 (Session.END == EnumEnd.Backend && a.BackendIndex == Session.Index)
                                           select a).ToList();
                ConfigMappingItem Item = Session.MappingItemList.FirstOrDefault();
                if (Item != null)
                {
                    string Name = "";
                    if (Session.END == EnumEnd.Backend)
                    {
                        Name = Item.BackendName;
                    }
                    else
                    {
                        Name = Item.FrontendName;
                    }

                    if (Name.StartsWith("101"))
                    {
                        Session.Protocol = EnumProtocolType.IEC101;
                    }
                    else if (Name.StartsWith("104"))
                    {
                        Session.Protocol = EnumProtocolType.IEC104;
                    }
                    else if (Name.StartsWith("Mo"))
                    {
                        Session.Protocol = EnumProtocolType.Modbus;
                    }
                    else if (Name.StartsWith("DN"))
                    {
                        Session.Protocol = EnumProtocolType.DNP3;
                    }
                    else
                    {
                        throw new Exception("No support :" + Name);
                    }
                }
            }
        }


        public void Setup()
        {
            foreach (var Session in SessionList)
            {
                Session.SocketClientConnect();
                Session.Setup();
                if (Session.END == EnumEnd.Backend)
                {
                    Session.AddPoint();
                }
            }
        }
    }
    public class HarnessInfo
    {
        public EnumEnd Type { get; set; }
        public int Index { get; set; }
        public int Port { get; set; }
        public string IPAddress { get; set; }
    }
}
