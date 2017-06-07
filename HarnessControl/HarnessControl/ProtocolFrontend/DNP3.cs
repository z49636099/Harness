using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HarnessControl
{
    public class DNP3 : atopProtocolHarness
    {
        public SocketClient FrontendClient { get; set; }

        public List<PointVarInfo> PolledChangeVar = new List<PointVarInfo>();
        public List<PointVarInfo> PolledStaticVar = new List<PointVarInfo>();
        public List<PointVarInfo> PolledControlVar = new List<PointVarInfo>();

        public DNP3(CommunicationBase FC) : base(FC)
        {
            FrontendClient = FrontendSession.SocketClient;
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
                foreach (ConfigMappingItem Item in FrontendCommunication.MappingItemList)
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

        public override Dictionary<string, string> SendHassionCmd(string Command)
        {
            Dictionary<string, string> DicDataVariable = new Dictionary<string, string>();
            try
            {
                DicDataVariable = base.SendHassionCmd(Command);
                string[] CommandArr = Command.Split(' ');
                string HCmd = CommandArr[0];


                //if (Help.Contains("variation") && HCmd.Contains("mdnpbin"))
                //{
                //    string HelpVarLine = Help.Split('\n').Where(a => a.Contains("variation")).First();
                //    Regex R = new Regex(@"\[(.*)\]");
                //    Match M = R.Match(HelpVarLine);

                //}

                string Qual, Grp, Var = "";
                int Start = 0, Stop = 0;
                int[] Value = null;
                Dictionary<string, string> DicCommandPara = GetCommandPara(Command);
                string VarName = DicDataVariable.Where(a => Regex.IsMatch(a.Key, "OBJ.*,VAR")).Select(a => a.Key).FirstOrDefault();
                if (VarName != null)
                {
                    Var = DicDataVariable[VarName];
                }
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
                        else
                        {
                            Grp = "1";
                        }
                        break;
                    case "mdnpbincmd":
                        Qual = "16bitindex";
                        Grp = "12";
                        if (DicCommandPara.ContainsKey("start"))
                        {
                            Value = DicCommandPara["value"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(a => GetValueInt(a))
                                                           .ToArray();
                            Start = ToInt(DicCommandPara["start"]);
                            Stop = ToInt(DicCommandPara["stop"]);
                        }
                        else if (DicCommandPara.ContainsKey("point"))
                        {
                            Value = DicCommandPara["control"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(a => GetValueInt(a))
                                                           .ToArray();
                            string[] PointIndexBin = DicCommandPara["point"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            Start = ToInt(PointIndexBin[0]);
                            Stop = ToInt(PointIndexBin[PointIndexBin.Length - 1]);
                        }
                        break;
                    case "mdnpanlgcmd":
                        Qual = "16bitindex";
                        Grp = "41";
                        string[] PointIndexAnlg = DicCommandPara["point"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Start = ToInt(PointIndexAnlg[0]);
                        Stop = ToInt(PointIndexAnlg[PointIndexAnlg.Length - 1]);
                        break;
                    default:
                        throw new TestException("no support this command : " + HCmd);
                }

                #region Check Qual
                if (DicCommandPara.ContainsKey("qualifier"))
                {
                    Qual = DicCommandPara["qualifier"];
                }
                string[] QualNames = DicDataVariable.Where(a => Regex.IsMatch(a.Key, "OBJ.*,QUAL")).Select(a => a.Key).ToArray();
                foreach (var QName in QualNames)
                {
                    int QualNum = GetQualifierNum(Qual);
                    if (QualNum == 0x06)
                        continue;
                    if (QualNum != ToInt(DicDataVariable[QName]))
                    {
                        throw new TestException($"QUAL Fail : Qual = {DicDataVariable[QName]} , expected = {QualNum}");
                    }
                }
                #endregion

                #region check GRP
                string[] GRPNames = DicDataVariable.Where(a => Regex.IsMatch(a.Key, "OBJ.*,GRP")).Select(a => a.Key).ToArray();
                foreach (var GRPName in GRPNames)
                {
                    if (ToInt(DicDataVariable[GRPName]) != ToInt(Grp))
                    {
                        throw new TestException($"GRP Fail ,GRP = {DicDataVariable[GRPName]} , Expected = {Grp}");
                    }
                }
                #endregion

                #region Check AC
                int AC = ToInt(DicDataVariable["AC"]);
                if (AC > 0xcf || AC < 0xc0)
                {
                    throw new TestException($"AC Fail : GRP = {Grp} , VAR = {Var} ,AC = {AC}");
                }
                #endregion

                #region Check FC
                int FC = ToInt(DicDataVariable["FC"]);
                if (FC != 0x81)
                {
                    throw new TestException($"FC Fail : GRP = {Grp} , VAR = {Var} ,FC = {FC}");
                }
                #endregion

                #region Check IIN
                int IIN = ToInt(DicDataVariable["IIN"]);
                if (IIN != 0 && IIN != 0x1000)
                {
                    throw new TestException($"IIN Fail : GRP = {Grp} , VAR = {Var} ,IIN = {IIN}");
                }
                #endregion

                #region Check OBJ.*,STATUS
                string[] StatusNames = DicDataVariable.Keys.Where(a => Regex.IsMatch(a, "OBJ.*STATUS")).ToArray();
                foreach (string Name in StatusNames)
                {
                    if (ToInt(DicDataVariable[Name]) != 0)
                    {
                        throw new TestException($"Status Fail : GRP = {Grp} ,VAR = {Var} , {Name} = {DicDataVariable[Name]} ,Expected = 0");
                    }
                }
                #endregion

                #region Check Data
                if (Value != null)
                {
                    var DicPointData = GetPointValue(DicDataVariable);
                    for (int i = Start; i <= Stop; i++)
                    {
                        int PointValue = Value[0];
                        if (Value.Length != 0)
                        {
                            PointValue = Value[i - Start];
                        }
                        if (DicPointData[i] != PointValue)
                        {
                            throw new TestException($"Point Value Fail : Point = {i} , {DicPointData} != {PointValue}");
                        }
                    }
                }

                #endregion
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

        //private void CheckFlag(Dictionary<string, string> DicDataVariable)
        //{

        //}

        //private int GetFlag(string GPoint)
        //{
            
        //}

        private Dictionary<int, int> GetPointValue(Dictionary<string, string> DicDataVarible)
        {
            Dictionary<int, int> DicData = new Dictionary<int, int>();
            string[] DataNames = DicDataVarible.Keys.Where(a => Regex.IsMatch(a, "OBJ.*,DATA.*|OBJ.*,CCODE.*")).ToArray();
            foreach (string Name in DataNames)
            {
                string PointName = Name.Replace("DATA", "POINT").Replace("CCODE", "POINT");
                DicData.Add(ToInt(DicDataVarible[PointName]), ToInt(DicDataVarible[Name]));
            }
            return DicData;
        }




        private int GetValueInt(string Value)
        {
            switch (Value)
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

        private int GetQualifierNum(string Name)
        {
            switch (Name)
            {
                case "8bitstartstop":
                    return 0;
                case "16bitstartstop":
                    return 1;
                case "all":
                    return 6;
                case "8bitlimited":
                    return 7;
                case "16bitlimited":
                    return 8;
                case "8bitindex":
                    return 0x17;
                case "16bitindex":
                    return 0x28;
                default:
                    return 0;
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
