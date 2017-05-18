using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarnessControl
{
    public class HarnessController
    {
        public List<HarnessInfo> HarnessInfoList = new List<HarnessInfo>();
        public List<HarnessSession> SessionList = new List<HarnessSession>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();

        public atopPortocolBase Frontend { get; set; }
        private List<HarnessProcess> ProcessList = new List<HarnessProcess>();

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
                    if (Session.END == EnumEnd.Backend)
                    {
                        Session.Protocol = Item.BackendProtocolType;
                    }
                    else
                    {
                        Session.Protocol = Item.FrontendProtocolType;
                    }
                }
            }
        }


        public void Setup(string SessionName)
        {
            var Session = SessionList.Where(a => a.Name == SessionName).FirstOrDefault();
            if (Session == null)
            {
                throw new Exception("No find " + SessionName);
            }
            HarnessProcess P = new HarnessProcess();
            P.ProcessStart(@"C:\Program Files (x86)\Triangle MicroWorks\Protocol Test Harness\bin\tmwtest.exe",
                          $"-tcl \"source {Application.StartupPath.Replace("\\", "/") + "/"}SocketServer.tcl\";\"CreateSocketServer {Session.HarnessSocketInfo.Port}\"");
            ProcessList.Add(P);
            Thread.Sleep(3000);
            Session.SocketClientConnect();
            Session.Setup();
            Thread.Sleep(3000);
            if (Session.END == EnumEnd.Backend)
            {
                Session.AddPoint();
            }
            else
            {
                switch (Session.Protocol)
                {
                    case EnumProtocolType.Modbus:
                        Frontend = new Modbus();
                        break;
                    case EnumProtocolType.IEC104:
                        Frontend = new IEC_10X();
                        break;
                }
                Frontend.Session = Session;
                foreach (var ClientSession in SessionList.Where(a => a.END == EnumEnd.Backend).OrderBy(a => a.Index))
                {
                    HarnessInfo Info = HarnessInfoList.Where(a => a.Type == EnumEnd.Backend)
                                                       .Where(a => a.Index == ClientSession.Index)
                                                       .Select(a => a).First();
                    HarnessTCPClient SocketClient = new HarnessTCPClient();
                    SocketClient.Connect(Info.IPAddress, ClientSession.BackendSocketPort);
                    //SocketClient.Connect("10.0.176.200", ClientSession.BackendSocketPort);
                    Frontend.SocketClientList.Add(SocketClient);
                }
            }
        }

        public void Close()
        {
            Frontend.SocketClientList.ForEach(a => a.TelnetClinet.Close());
            SessionList.ForEach(a => a.SocketClient.TelnetClinet.Close());
            ProcessList.ForEach(a => a.Process.Close());
            SessionList.Clear();
            MappingItemList.Clear();
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
