using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessBackendSocketServer
    {
        public TcpListener Listener;
        public List<HarnessBackendSocketAccept> ClientList = new List<HarnessBackendSocketAccept>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();
        public HarnessTCPClient HarnessSocket { get; set; }


        public void Start(int Port)
        {
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
            atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is ready :127.0.0.1:" + Port);
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
                    HarnessBackendSocketAccept client = new HarnessBackendSocketAccept();
                    client.HarnessSocket = HarnessSocket;
                    ClientList.Add(client);
                    client.startClient(clientSocket, counter.ToString());
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server Exception :127.0.0.1:" + Port + "==>" + ex.Message);
            }
            foreach (var client in ClientList)
            {
                client.Close();
            }
            Listener.Stop();
            atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is stop :127.0.0.1:" + Port);
        }
    }

}
