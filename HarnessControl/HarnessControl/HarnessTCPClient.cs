using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessTCPClient
    {
        private bool? WaitResult = null;
        public string ReceiveData { get; set; }
        public string ShowData { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }

        public event Action<string> DataReceived;
        public TcpClient TelnetClinet = new TcpClient();

        protected void _ReceivedData(string _Msg)
        {
            DataReceived?.Invoke(_Msg);
        }

        public bool IsOpen
        {
            get
            {
                bool open = true;
                try
                {
                    if (TelnetClinet.Connected && TelnetClinet.Client.Poll(0, SelectMode.SelectRead))
                    {
                        open = TelnetClinet.Client.Receive(new byte[1], SocketFlags.Peek) != 0;
                    }
                }
                catch
                {
                    open = false;
                }
                return open;
            }
        }

        public void Connect(string _IPAddress, int _Port)
        {
            try
            {
                Port = _Port;
                IPAddress = _IPAddress;

                if (TelnetClinet != null && TelnetClinet.Connected)
                {
                    TelnetClinet.Close();
                }
                ShowData = "";
                TelnetClinet = new TcpClient();
                TelnetClinet.Connect(IPAddress, Port);
                Task.Factory.StartNew(WaitReceive);
            }
            catch
            {
                throw new Exception("Connect Fail!");
            }
        }

        public void Disconnect()
        {
            try
            {
                TelnetClinet.Close();
            }
            catch { }
        }
        private void WaitReceive()
        {
            while (TelnetClinet.Connected)
            {
                try
                {
                    Thread.Sleep(10);
                    byte[] bytes = new byte[TelnetClinet.ReceiveBufferSize];
                    int numBytesRead = TelnetClinet.GetStream().Read(bytes, 0, TelnetClinet.ReceiveBufferSize);
                    //int numBytesRead = TelnetClinet.Client.Receive()
                    if (numBytesRead == 0)
                    {
                        continue;
                    }
                    string _Data = Encoding.ASCII.GetString(bytes).Replace("\0", "");

                    ReceiveData += _Data;
                    //ShowData += _Data;

                    _ReceivedData(ShowData);
                    if (ReceiveData.Contains("\r\n"))
                    {
                        WaitResult = true;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            WaitResult = false;
        }

        public string Send(string Str, int timeout = 5000)
        {
            ReceiveData = "";
            ClearKeyword();

            _WriteData(Str);

            while (timeout > 0 && WaitResult == null)
            {
                Thread.Sleep(10);
                timeout -= 10;
            }
            if (timeout == 0 )
            {
                throw new HarnessSocketException("[Receive Timeout] Command : " + Str);
            }
            if (ReceiveData == "-1" || ReceiveData.StartsWith("Error :"))
            {
                throw new HarnessSocketException("[Harness Exception] " + ReceiveData.Replace("Error :", ""));
            }
            if(ReceiveData.EndsWith("\r\n"))
            {
                ReceiveData = ReceiveData.Remove(ReceiveData.Length - 2);
            }
            return ReceiveData;
        }


        private void _WriteData(string _Data)
        {
            byte[] cmd = Encoding.ASCII.GetBytes(_Data+ "\r\n");
            TelnetClinet.GetStream().Write(cmd, 0, cmd.Length);
        }

        private void ClearKeyword()
        {
            WaitResult = null;
            ReceiveData = "";
        }
    }
    public class HarnessSocketException : Exception
    {
        public HarnessSocketException(string Msg) : base(Msg)
        {

        }
    }
}
