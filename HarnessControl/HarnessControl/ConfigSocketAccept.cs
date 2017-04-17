using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public class ConfigSocketAccept
    {
        public TcpClient clientSocket;
        private string ConfigData { get; set; }

        public event Action<string> ReceiveEvent;

        public ConfigSocketAccept()
        {
            ConfigData = "";
        }

        public void startClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {
            Byte[] sendBytes = null;

            NetworkStream networkStream = clientSocket.GetStream();
            try
            {
                while (clientSocket.Connected)
                {
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom).Replace("\0", "");

                    ReceiveSocket(dataFromClient);

                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, "Control accept : " + ex.Message);
            }
            Close();
        }


        private void Send(string Data)
        {
            if(!Data.EndsWith("\r\n"))
            {
                Data += "\r\n";
            }
            NetworkStream networkStream = clientSocket.GetStream();
            var sendBytes = Encoding.ASCII.GetBytes(Data);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();
        }
        private void ReceiveSocket(string Data)
        {
            try
            {
                Data = Data.Replace("\r\n", "\n");
                string[] lineArray = Data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lineArray)
                {
                    if (line.ToUpper() == "END")
                    {
                        ReceiveEvent?.BeginInvoke(ConfigData, new AsyncCallback((target) =>
                        {
                            ConfigData = "";
                            Send("Harness is ready");
                        }), null);
                    }
                    ConfigData += line + Environment.NewLine;
                }
            }catch(Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, clientSocket.Client.LocalEndPoint + ": Config format fail : " + ex.Message);
                Send("Error");
            }
        }

        public void Close()
        {
            try
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, "Server accept is disconnect:" + clientSocket.Client.LocalEndPoint);
                clientSocket.Close();
            }
            catch { }
        }
    }
}
