using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public abstract class atopPortocolBase
    {
        public virtual HarnessSession Session { get; set; }

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
            try
            {
                HarnessTCPClient Client = SocketClientList[Item.BackendIndex - 1];
                int[] SetValue = PointValueRange.GetRandomValue(Item.BackendDataType, Item.BackendCount);
                string SetValueStr = string.Join(",", SetValue);
                string Cmd = HarnessCommand.GetSlaveCommand(Item.BackendDataType, Item.BackendProtocolType);
                Client.Send(string.Format("Set {0} {1} {2} {3}", Cmd, Item.BackendStart, Item.BackendCount, SetValueStr), 5000);

                int TryCount = 15;
                while (TryCount-- > 0)
                {
                    bool IsUpdateFinish = true;
                    Thread.Sleep(100);
                    string Data = Client.Send(string.Format("Get {0} {1} {2}", Cmd, Item.BackendStart, Item.BackendCount));
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
        public virtual Dictionary<string, string> SendHassionCmd(string Command)
        {
            var Client = Session.SocketClient;
            string[] CommandArr = Command.Split(' ');
            string HCmd = CommandArr[0];
            string Help = Client.Send(HCmd + " ?");
            if (Help.Contains("feedback") && !Command.Contains("feedback"))
            {
                Command += " feedback false";
            }
            if (Help.Contains("statVariable") && !Command.Contains("statVariable"))
            {
                Command += " statVariable stat";
            }
            if (Help.Contains("dataVariable") && !Command.Contains("dataVariable"))
            {
                Command += " dataVariable data";
            }
            string Response = Client.Send(Command);
            CheckStatVariable(Client, Response, Command);
            string DataVariable = Response;
            if (Command.Contains("dataVariable"))
            {
                DataVariable = Client.Send("get [array get ::data]").Trim();
            }

            var DicDataVariable = GetDataVariableDic(DataVariable);
            if (DicDataVariable.ContainsKey("PARSINGSTATUS"))
            {
                if (DicDataVariable["PARSINGSTATUS"] != "Success")
                {
                    throw new TestException($"Parsing Status Fail , Status = {DicDataVariable["PARSINGSTATUS"]} ,Expected : Success");
                }
            }

            return DicDataVariable;
        }

        public Dictionary<string, string> GetCommandPara(string Command)
        {
            Dictionary<string, string> DicPara = new Dictionary<string, string>();
            string CMD = string.Join(" ", Command.Split(' ').Skip(1));
            string[] SplitMark = CMD.Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> CmdPara = new List<string>();
            for (int i = 0; i < SplitMark.Length; i++)
            {
                if (i % 2 == 0)
                {
                    CmdPara.AddRange(SplitMark[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    CmdPara.Add(SplitMark[i]);
                }
            }
            for (int i = 0; i < CmdPara.Count; i += 2)
            {
                DicPara.Add(CmdPara[i], CmdPara[i + 1]);
            }
            return DicPara;
        }

    }

    public class TestException : Exception
    {
        public TestException(string Msg) : base(Msg)
        {

        }
    }
}
