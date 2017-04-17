using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class ConfigMappingItem
    {
        public ConfigMappingItem(string Item)
        {
            string[] ItemArray = Item.Split(' ');
            FrontendName = ItemArray[0];
            FrontendDataType = ItemArray[1];
            FrontendStart = int.Parse(ItemArray[2]);
            FrontendCount = int.Parse(ItemArray[3]);
            BackendName = ItemArray[4];
            BackendDataType = ItemArray[5];
            BackendStart = int.Parse(ItemArray[6]);
            BackendCount = int.Parse(ItemArray[7]);
            Port = int.Parse(ItemArray[8]);
        }

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

        public int FrontendIndex { get; set; }
        public int BackendIndex { get; set; }
        public string FrontendDataType { get; set; }
        public int FrontendStart { get; set; }
        public int FrontendCount { get; set; }
        public string BackendDataType { get; set; }
        public int BackendStart { get; set; }
        public int BackendCount { get; set; }
        public int Port { get; set; }
    }
}
