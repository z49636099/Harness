using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public abstract class atopProtocolHarness : atopPortocolBase
    {
        public Communication_Harness FrontendSession { get; set; }

        public atopProtocolHarness(CommunicationBase FC) : base(FC)
        {
            FrontendSession = (FC as Communication_Harness);
        }

        public virtual Dictionary<string, string> SendHassionCmd(string Command)
        {
            
               var Client = FrontendSession.SocketClient;
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
        public virtual void CheckStatVariable(SocketClient Client, string Response, string Command)
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

        /// <summary> ex: Command = m104msp ioa {IOA} value {Value} mode {Mode}
        /// return : [ioa,{IOA}] [value,{value}] [mode,{Mode}]
        /// </summary>
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
}
