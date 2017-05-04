using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessFrontendSocketServer
    {
        public TcpListener Listener;
        public List<HarnessFrontendSocketAccept> ClientList = new List<HarnessFrontendSocketAccept>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();
        public HarnessTCPClient HarnessSocket { get; set; }


        public void Start(int Port)
        {
            Listener = new TcpListener(IPAddress.Parse(GetLocalIP()), Port);
            atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is ready : " +Listener.LocalEndpoint);
            TcpClient clientSocket = new TcpClient();
            try
            {
                Listener.Start();
                int counter = 0;
                while (true)
                {
                    counter += 1;
                    clientSocket = Listener.AcceptTcpClient();
                    atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is connect :" + clientSocket.Client.LocalEndPoint);
                    HarnessFrontendSocketAccept client = new HarnessFrontendSocketAccept();
                    ClientList.Add(client);
                    client.startClient(clientSocket, counter.ToString());
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server Exception :" + Listener.LocalEndpoint + "==>" + ex.Message);
            }
            foreach (var client in ClientList)
            {
                client.Close();
            }
            Listener.Stop();
            atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is stop :" + Listener.LocalEndpoint);
        }

        private string GetLocalIP()
        {
            // 取得本機名稱
            String strHostName = Dns.GetHostName();

            // 取得本機的 IpHostEntry 類別實體
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (var IP in iphostentry.AddressList)
            {
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                {
                    return IP.ToString();
                }
            }
            throw new Exception("Get local ip fail.");
        }
    }

}
