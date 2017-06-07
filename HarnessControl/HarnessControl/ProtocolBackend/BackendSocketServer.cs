using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class BackendSocketServer
    {
        public TcpListener Listener;
        public List<BackendSocketAccept> ClientList = new List<BackendSocketAccept>();
        public List<ConfigMappingItem> MappingItemList = new List<ConfigMappingItem>();
        public SocketClient HarnessSocket { get; set; }

        public void Start<T>(int Port) where T: BackendSocketAccept, new()
        {
            Listener = new TcpListener(IPAddress.Parse(Global.LocalIP), Port);
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
                    BackendSocketAccept client = new T();
                    client.HarnessSocket = HarnessSocket;
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
        
    }

}
