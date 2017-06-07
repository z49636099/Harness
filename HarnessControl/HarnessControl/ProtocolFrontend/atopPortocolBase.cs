using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public abstract class atopPortocolBase
    {
        public atopPortocolBase(CommunicationBase FC)
        {
            FrontendCommunication = FC;
        }

        public virtual CommunicationBase FrontendCommunication { get; set; }

        public event Action<string> StatusEvent;

        public List<SocketClient> SocketClientList = new List<SocketClient>();

        public List<CommunicationBase> BackendCommunication = new List<CommunicationBase>();

        public void Reliability()
        {
            /* 1 day */
            PollSataic();
            /* 3 day */
            PollControl();
            /* 5 day */
            PollChange();
        }

        public abstract void Patten(string PattenPath);

        public abstract void PollControl();
        public abstract void PollChange();
        public abstract void PollSataic();


        public void SendStatus(string StatusMessage)
        {
            StatusEvent?.Invoke(StatusMessage);
        }

        /// <summary>Harness DataVariable to Dictionary</summary>
        public Dictionary<string, string> GetDataVariableDic(string DataVariable)
        {
            Dictionary<string, string> Dic = new Dictionary<string, string>();
            string[] ValueSplit = DataVariable.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ValueSplit.Length; i += 2)
            {
                Dic.Add(ValueSplit[i], ValueSplit[i + 1]);
            }
            return Dic;
        }

        /// <summary>TCL Array to Dictionary</summary>
        public Dictionary<int, int> GetPointValueList(string Value)
        {
            Dictionary<int, int> Dic = new Dictionary<int, int>();
            string[] ValueSplit = Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ValueSplit.Length; i += 2)
            {
                Dic.Add(int.Parse(ValueSplit[i]), ToInt(ValueSplit[i + 1]));
            }
            return Dic;
        }

        public int ToInt(string Value)
        {
            Value = Value.Trim();
            if (Value.Contains("0x"))
            {
                return Convert.ToInt32(Value, 16);
            }

            return int.Parse(Value);
        }

        public bool SetRandomValueToServer(ConfigMappingItem Item)
        {
            switch (Item.BackendProtocolType)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    return SetRandomValueToServer_Harness(Item);
                case EnumProtocolType.IEC61850:
                    return SetRandomValueToServer_61850(Item);
                default:
                    throw new TestException("SetRandomValueToServer Fail.");
            }
        }

        public bool SetRandomValueToServer_61850(ConfigMappingItem Item)
        {
            //TODO 一頭霧水中...
            throw new Exception("還沒寫");
        }

        public bool SetRandomValueToServer_Harness(ConfigMappingItem Item)
        {
            try
            {
                SocketClient Client = SocketClientList[Item.BackendIndex - 1];
                int[] SetValue = PointValueRange.GetRandomValue(Item.BackendDataType, Item.BackendCount);
                string SetValueStr = string.Join(",", SetValue);
                string Cmd = Command_Harness.GetSlaveCommand(Item.BackendDataType, Item.BackendProtocolType);
                Client.Send(string.Format("Set {0} {1} {2} {3} {4}", Cmd, Item.BackendStart, Item.BackendCount, SetValueStr,Item.BackendSlaveID), 5000);

                int TryCount = 15;
                while (TryCount-- > 0)
                {
                    bool IsUpdateFinish = true;
                    Thread.Sleep(100);
                    string Data = Client.Send(string.Format("Get {0} {1} {2} {3}", Cmd, Item.BackendStart, Item.BackendCount,Item.BackendSlaveID));
                    Dictionary<int, int> Dic = GetPointValueList(Data);
                    for (int i = 0; i < SetValue.Length; i++)
                    {
                        int Index = i + Item.BackendStart;
                        int ServerValue = Dic[Index];
                        if (ServerValue != SetValue[i])
                        {
                            IsUpdateFinish = false;
                            break;
                        }
                    }
                    if (IsUpdateFinish)
                        return true;
                }
            }
            catch (HarnessSocketException ex)
            {
                atopLog.WriteLog(atopLogMode.TestFail, Item.BackendName + " : " + ex.Message);
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
            }
            atopLog.WriteLog(atopLogMode.TestFail, "Set Random Value Fail.");
            return false;
        }
    }

    public class TestException : Exception
    {
        public TestException(string Msg) : base(Msg)
        {

        }
    }
}
