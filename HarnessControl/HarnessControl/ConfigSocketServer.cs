using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class ConfigSocketServer
    {
        public TcpListener Listener;
        public ConfigSocketAccept Client { get; set; }

        public event Action<string> ReceiveEvent;
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
                    Client?.Close();
                    Client = new ConfigSocketAccept();
                    Client.ReceiveEvent += Client_ReceiveEvent;
                    Client.startClient(clientSocket);
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server Exception :127.0.0.1:" + Port + "==>" + ex.Message);
            }
            Client?.Close();
            Listener.Stop();
            atopLog.WriteLog(atopLogMode.SocketInfo, "Socket Server is stop :127.0.0.1:" + Port);
        }

        private void Client_ReceiveEvent(string obj)
        {
            ReceiveEvent?.BeginInvoke(obj, null, null);
        }
    }


}
