using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessFrontendSocketAccept
    {
        public TcpClient clientSocket;
        string clNo;
        public void startClient(TcpClient inClientSocket, string clineNo)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }
        private void doChat()
        {

            NetworkStream networkStream = clientSocket.GetStream();
            try
            {
                while (clientSocket.Connected)
                {
                    byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom).Replace("\0", "");

                    if (dataFromClient.Trim() == "")
                    {
                        Send("\0", false);
                    }


                    //Send();
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, clientSocket.Client.LocalEndPoint + " Control accept : " + ex.Message);
            }
            Close();
        }

        private void Send(string Data, bool NewLine = true)
        {
            if (NewLine && !Data.EndsWith("\r\n"))
            {
                Data += "\r\n";
            }
            NetworkStream networkStream = clientSocket.GetStream();
            byte[] sendBytes = Encoding.ASCII.GetBytes(Data);
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();
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
