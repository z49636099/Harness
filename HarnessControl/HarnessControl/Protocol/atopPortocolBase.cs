﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public abstract class atopPortocolBase
    {
        public HarnessSession Session { get; set; }

        public event Action<string> StatusEvent;

        public List<HarnessTCPClient> SocketClientList = new List<HarnessTCPClient>();

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
        public Dictionary<int, string> GetPointValueList(string Value)
        {
            Dictionary<int, string> Dic = new Dictionary<int, string>();
            string[] ValueSplit = Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ValueSplit.Length; i += 2)
            {
                Dic.Add(int.Parse(ValueSplit[i]), ValueSplit[i + 1]);
            }
            return Dic;
        }

        public int ToInt(string Value)
        {
            if (Value.Contains("0x"))
            {
                return Convert.ToInt32(Value, 16);
            }

            return int.Parse(Value);
        }

        public bool SetRandomValueToServer(ConfigMappingItem Item)
        {
            try
            {
                HarnessTCPClient Client = SocketClientList[Item.BackendIndex - 1];
                int[] SetValue = PointValueRange.GetRandomValue(Item.BackendDataType, Item.BackendCount);
                string SetValueStr = string.Join(",", SetValue);
                string Cmd = HarnessCommand.GetServerCommand(Item.BackendDataType, Item.BackendProtocolType);
                Client.Send(string.Format("Set {0} {1} {2} {3}", Cmd, Item.BackendStart, Item.BackendCount, SetValueStr), 5000);

                int TryCount = 15;
                while (TryCount-- > 0)
                {
                    bool IsUpdateFinish = true;
                    Thread.Sleep(100);
                    string Data = Client.Send(string.Format("Get {0} {1} {2}", Cmd, Item.BackendStart, Item.BackendCount));
                    Dictionary<int, string> Dic = GetPointValueList(Data);
                    for (int i = 0; i < SetValue.Length; i++)
                    {
                        int Index = i + Item.BackendStart;
                        int ServerValue = int.Parse(Dic[Index]);
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

        public virtual void CheckStatVariable(HarnessTCPClient Client, string Response, string Command)
        {
            if (Command.Contains("statVariable"))
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    string Stat = Client.Send("get $::stat").Replace("\r\n", "").Trim();
                    switch (Stat)
                    {
                        case "-1":
                            throw new TestException("vwait stat is timeout; command = " + Command);
                        case "0":
                            return;
                        case "1":
                            break;
                        default:
                            throw new TestException($"cmd error : status = {Stat} , {Command}");
                    }
                }
            }
        }



    }

    public class TestException : Exception
    {
        public TestException(string Msg) : base(Msg)
        {

        }
    }
}
