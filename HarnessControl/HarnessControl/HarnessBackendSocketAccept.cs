using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class HarnessBackendSocketAccept
    {
        public TcpClient clientSocket;
        string clNo;
        public HarnessTCPClient HarnessSocket { get; set; }
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
                    string serverResponse = Echo(dataFromClient);

                    Send(serverResponse);
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

        public string Echo(string line)
        {
            try
            {
                string[] Data = line.Split(' ');
                EnumProtocolType Type = GetProtocolType(Data[1]);
                switch (Data[0])
                {
                    case "Get":
                        return GetData(Type, Data[1], Data[2], Data[3]);
                    case "Set":
                        return SetData(Type, Data[1], Data[2], Data[3], Data[4]);
                    case "Add":
                        return AddPoint(Type, Data);
                    case "Delete":
                        return DeletePoint(Type, Data);
                    default:
                        return "-1";
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, clientSocket.Client.LocalEndPoint + " Error :" + ex.Message);
                return "Error : " + ex.Message;
            }
        }

        private string DeletePoint(EnumProtocolType Type, string[] Para)
        {
            int Quantity = 0;
            int StartPoint = 0;
            string Cmd = Para[1];
            string CmdFormat = "";
            if (Type == EnumProtocolType.DNP3)
            {
                Quantity = int.Parse(Para[2]);
            }
            else
            {
                StartPoint = int.Parse(Para[2]);
                Quantity = int.Parse(Para[3]);
            }

            switch (Type)
            {
                case EnumProtocolType.DNP3:
                    CmdFormat = "{0} remove session 0";
                    break;
                case EnumProtocolType.Modbus:
                    CmdFormat = "{0} remove point {1}";
                    break;
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                    CmdFormat = "{0} remove ioa {1}";
                    break;
            }
            int EndPoint = StartPoint + Quantity - 1;
            for (int i = StartPoint; i <= EndPoint; i++)
            {
                int DataIndex = i - StartPoint;
                string SocketCmd = string.Format(CmdFormat, Cmd, i);
                string PointData = HarnessSocket.Send(SocketCmd).Replace("\r\n", "");
            }
            return "0";
        }


        private string AddPoint(EnumProtocolType Type, string[] Para)
        {
            int Quantity = 0;
            int StartPoint = 0;
            string Cmd = Para[1];
            string CmdFormat = "";
            string[] Value;
            if (Type == EnumProtocolType.DNP3)
            {
                Quantity = int.Parse(Para[2]);
                Value = SplitValue(Para[3], Quantity);
            }
            else
            {
                StartPoint = int.Parse(Para[2]);
                Quantity = int.Parse(Para[3]);
                Value = SplitValue(Para[4], Quantity);
            }

            switch (Type)
            {
                case EnumProtocolType.DNP3:
                    CmdFormat = "{0} add value {2}";
                    break;
                case EnumProtocolType.Modbus:
                    CmdFormat = "{0} add point {1} value {2}";
                    break;
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                    CmdFormat = "{0} add ioa {1} value {2}";
                    break;
            }
            int EndPoint = StartPoint + Quantity - 1;
            for (int i = StartPoint; i <= EndPoint; i++)
            {
                int DataIndex = i - StartPoint;
                string SocketCmd = string.Format(CmdFormat, Cmd, i, Value[DataIndex]);
                string PointData = HarnessSocket.Send(SocketCmd).Replace("\r\n", "");
            }
            return "0";
        }

        private string SetData(EnumProtocolType Type, string Cmd, string point, string Quantity, string Data)
        {
            int StartPoint = int.Parse(point);
            int Count = int.Parse(Quantity);
            int EndPoint = StartPoint + Count - 1;
            string[] DataArray = SplitValue(Data, Count);
            string CmdFormat = "";
            switch (Type)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.Modbus:
                    CmdFormat = "{0} set point {1} value {2}";
                    break;
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                    CmdFormat = "{0} get ioa {1} value {2}";
                    break;
            }
            for (int i = StartPoint; i <= EndPoint; i++)
            {
                int DataIndex = i - StartPoint;
                string SocketCmd = string.Format(CmdFormat, Cmd, i, DataArray[DataIndex]);
                string PointData = HarnessSocket.Send(SocketCmd,-1).Replace("\r\n", "");
            }
            return "0";
        }

        private string GetData(EnumProtocolType Type, string Cmd, string point, string Quantity)
        {
            int StartPoint = int.Parse(point);
            int EndPoint = StartPoint + int.Parse(Quantity) - 1;
            string CmdFormat = "";
            string Data = "";
            switch (Type)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.Modbus:
                    CmdFormat = "{0} get point {1} value";
                    break;
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                    CmdFormat = "{0} get ioa {1} value";
                    break;
            }
            for (int i = StartPoint; i <= EndPoint; i++)
            {
                string SocketCmd = string.Format(CmdFormat, Cmd, i);
                string PointData = HarnessSocket.Send(SocketCmd).Replace("\r\n", "");
                Data += i.ToString() + " " + PointData + " ";
            }
            return Data;
        }

        private string[] SplitValue(string Value, int Count)
        {
            List<string> DataList = Value.Split(',').ToList();
            if (DataList.Count == 1)
            {
                for (int i = 1; i < Count; i++)
                {
                    DataList.Add(DataList[0]);
                }
            }
            return DataList.ToArray();
        }

        private EnumProtocolType GetProtocolType(string Cmd)
        {
            if (Cmd.StartsWith("sdnp"))
            {
                return EnumProtocolType.DNP3;
            }
            if (Cmd.StartsWith("smb"))
            {
                return EnumProtocolType.Modbus;
            }
            if (Cmd.StartsWith("s101"))
            {
                return EnumProtocolType.IEC101;
            }
            if (Cmd.StartsWith("s104"))
            {
                return EnumProtocolType.IEC104;
            }
            throw new Exception("No support cmd : " + Cmd);
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
