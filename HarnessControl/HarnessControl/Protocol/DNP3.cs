using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HarnessControl
{
    public class DNP3 : atopPortocolBase
    {
        public HarnessTCPClient FrontendClient { get; set; }

        public List<PointVarInfo> PolledChangeVar = new List<PointVarInfo>();
        public List<PointVarInfo> PolledStaticVar = new List<PointVarInfo>();
        public List<PointVarInfo> PolledControlVar = new List<PointVarInfo>();


        public DNP3()
        {
            FrontendClient = Session.SocketClient;
            PolledChangeVar.Add(new PointVarInfo { Type = "BI", Num = 2, Var = new int[] { 0, 1, 2, 3 } });
            PolledChangeVar.Add(new PointVarInfo { Type = "DI", Num = 5, Var = new int[] { 0, 1, 2, 3 } });
            PolledChangeVar.Add(new PointVarInfo { Type = "CT", Num = 22, Var = new int[] { 0, 1, 2, 5, 6 } });
            PolledChangeVar.Add(new PointVarInfo { Type = "AI", Num = 32, Var = new int[] { 0, 1, 2, 3, 4, 5, 7 } });

            PolledStaticVar.Add(new PointVarInfo { Type = "BI", Num = 1, Var = new int[] { 0, 1, 2 } });
            PolledStaticVar.Add(new PointVarInfo { Type = "DI", Num = 3, Var = new int[] { 0, 1, 2 } });
            PolledStaticVar.Add(new PointVarInfo { Type = "CT", Num = 20, Var = new int[] { 0, 1, 2, 5, 6 } });
            PolledStaticVar.Add(new PointVarInfo { Type = "AI", Num = 30, Var = new int[] { 0, 1, 2, 3, 4, 5 } });

            PolledControlVar.Add(new PointVarInfo { Type = "BO", Num = 10, Var = new int[] { 0, 1, 2 } });
            PolledControlVar.Add(new PointVarInfo { Type = "AO", Num = 40, Var = new int[] { 0, 1, 2, 3 } });
        }


        public override void Patten(string PattenPath)
        {
            throw new NotImplementedException();
        }

        public override void PollChange()
        {
            FrontendClient.Send("mdnpmodifysession autoClassPollIIN false");
            Thread.Sleep(10000);
            int LoopCount = 0;
            DateTime EndTime = DateTime.Now.AddSeconds(10);
            while (EndTime > DateTime.Now)
            {
                LoopCount++;
                atopLog.WriteLog(atopLogMode.TestInfo, "Start Run Polled Change operation ...");
                FrontendClient.Send("mdnpevent statVariable stat");
                FrontendClient.Send("vwait ::stat");
                foreach (ConfigMappingItem Item in Session.MappingItemList)
                {
                    PointVarInfo EventVar = PolledChangeVar.Where(a => a.Type == Item.FrontendDataType).FirstOrDefault();
                    if (EventVar == null)
                    { continue; }
                    atopLog.WriteLog(atopLogMode.TestInfo, "Poll change " + Item.MappingString);
                    foreach (int EVar in EventVar.Var)
                    {
                        DownloadReadQualifier(EventVar.Num, EVar, Item, true);
                    }
                }
            }
        }


        public override void PollControl()
        {
            throw new NotImplementedException();
        }
        public override void PollSataic()
        {
            throw new NotImplementedException();
        }


        private void DownloadReadQualifier(int ObjectNum, int eVar, ConfigMappingItem item, bool IsPollChange)
        {
            if (IsPollChange)
            {
                int[] QualifierCode = { 6, 7, 8 };
                foreach (int QUA in QualifierCode)
                {
                    FrontendClient.Send("mdnpevent statVariable stat");
                    FrontendClient.Send("vwait ::stat");
                    Thread.Sleep(3000);
                    SetRandomValueToServer(item);
                    DataCompare(ObjectNum, eVar, QUA, item);
                }
            }
            else
            {
                int[] QualifierCode = { 0, 1, 6, 17, 28 };
                foreach (int QUA in QualifierCode)
                {
                    DataCompare(ObjectNum, eVar, QUA, item);
                }
            }
        }

        private void DataCompare(int objectNum, int eVar, int Qualifier, ConfigMappingItem item)
        {
            string QualifierCMD = GetQualifierCMD(Qualifier);
        }

        private Dictionary<string, string> SendHarnessCommand(string Command)
        {
            string DataVariable = null;
            Dictionary<string, string> DicDataVariable = new Dictionary<string, string>();
            try
            {
                HarnessTCPClient Client = Session.SocketClient;
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




                DataVariable = Response;
                if (Command.Contains("dataVariable"))
                {
                    DataVariable = Client.Send("get [array get ::data]").Trim();
                }

                DicDataVariable = GetDataVariableDic(DataVariable);

                if (DicDataVariable.ContainsKey("PARSINGSTATUS"))
                {
                    if (DicDataVariable["PARSINGSTATUS"] != "Success")
                    {
                        throw new TestException($"Parsing Status Fail , Status = {DicDataVariable["PARSINGSTATUS"]} ,Expected : Success");
                    }
                }

                if (Help.Contains("variation") && HCmd.Contains("mdnpbin"))
                {
                    string HelpVarLine = Help.Split('\n').Where(a => a.Contains("variation")).First();
                    Regex R = new Regex(@"\[(.*)\]");
                    Match M = R.Match(HelpVarLine);

                }

                string Qual, Grp;
                int[] Value;
                Dictionary<string, string> DicCommandPara = GetDataVariableDic(string.Join(" ", CommandArr.Skip(1)));
                switch (HCmd)
                {
                    case "mdnpread":
                        Qual = "All";
                        if (DicDataVariable.ContainsKey("object"))
                        {
                            Grp = DicDataVariable["object"];
                        }
                        else if (DicDataVariable.ContainsKey("group"))
                        {
                            Grp = DicDataVariable["group"];
                        }
                        break;
                    case "mdnpbincmd":
                        Qual = "16bitindex";
                        Grp = "12";
                        if(DicCommandPara.ContainsKey("start"))
                        {
                            //Value = 
                        }
                        break;
                    case "mdnpanlgcmd":
                        break;
                }


            }
            catch (HarnessSocketException ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, ex.Message);
                return null;
            }
            catch (TestException ex)
            {
                atopLog.WriteLog(atopLogMode.TestFail, ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
                return null;
            }
            return DicDataVariable;
        }

        private int GetValueInt (string Value)
        {
            switch(Value)
            {
                case "lon":
                    return 3;
                case "loff":
                    return 4;
                case "on":
                    return 3;
                case "off":
                    return 4;
                default:
                    return ToInt(Value);
            }
        }


        private string GetQualifierCMD(int Qualifier)
        {
            switch (Qualifier)
            {
                case 0:
                    return "8bitstartstop";
                case 1:
                    return "16bitstartstop";
                case 6:
                    return "all";
                case 7:
                    return "8bitlimited";
                case 8:
                    return "16bitlimited";
                case 17:
                    return "8bitindex";
                case 28:
                    return "16bitindex";
                default:
                    throw new TestException("No find Qualifier CMD : " + Qualifier);
            }
        }
    }

    public class PointVarInfo
    {
        public int Num { get; set; }
        public string Type { get; set; }
        public int[] Var { get; set; }
    }
}
