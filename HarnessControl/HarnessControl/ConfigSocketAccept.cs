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

        public event Action<string, string> ReceiveEvent;

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
                    if (dataFromClient.Trim() == "")
                    {
                        Send("\0", false);
                    }


                    ReceiveSocket(dataFromClient);

                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, clientSocket.Client.LocalEndPoint + " Control accept : " + ex.Message);
            }
            Close();
        }


        public void Send(string Data, bool NewLine = true)
        {
            if (NewLine && !Data.EndsWith("\r\n"))
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
                // Data = Data.Replace("\r\n", "\n");
                // string[] lineArray = Data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                ConfigData += Data;
                if (ConfigData.StartsWith("Setting\r\n"))
                {
                    if (ConfigData.ToUpper().Contains("\r\nEND\r\n"))
                    {
                        ConfigData = ConfigData.Remove(ConfigData.Length - 7);
                        TriggerEvent("Config", ConfigData);
                        //ReceiveEvent?.BeginInvoke("Config", ConfigData, new AsyncCallback((target) =>
                        // {
                        //     ConfigData = "";
                        // }), null);
                        return;
                    }
                }
                else if (ConfigData.StartsWith("Setup"))
                {
                    TriggerEvent("Setup", ConfigData.Replace("Setup ", "").Replace("\r\n", ""));
                }
                else if (ConfigData.StartsWith("Test"))
                {
                    TriggerEvent("Test", ConfigData.Replace("Test ", "").Replace("\r\n", ""));
                }
                else
                {
                    ConfigData = "";
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, clientSocket.Client.LocalEndPoint + ": Config format fail : " + ex.Message);
                Send("Error");
            }
        }

        private void TriggerEvent(string Status, string Data)
        {
            ReceiveEvent?.BeginInvoke(Status, Data, new AsyncCallback((target) =>
            {
                ConfigData = "";
            }), null);
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
