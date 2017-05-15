using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HarnessControl
{
    public class IEC_10X : atopPortocolBase
    {

        string[] PolledChangeDataType = { "MSP", "MDP", "MST", "MBS", "MVN", "MVS", "MVF" };
        string[] PolledControlDataType = { "CSCNA", "CDCNA", "CRCNA", "CBONA", "CSENA", "CSENB", "CSENC" };
        private string ASDU { get; set; }

        public override HarnessSession Session
        {
            get { return base.Session; }

            set
            {
                base.Session = value;
                switch (Session.Protocol)
                {
                    case EnumProtocolType.IEC101:
                        if (Session.ConnectionType == EnumConnectionType.ETH)
                        {
                            ASDU = Session.SettingInfo[5];
                        }
                        else if (Session.ConnectionType == EnumConnectionType.COM)
                        {
                            ASDU = Session.SettingInfo[9];
                        }
                        break;
                    case EnumProtocolType.IEC104:
                        ASDU = Session.SettingInfo[5];
                        break;
                }
            }
        }

        public override void Patten(string PattenPath)
        {
            throw new NotImplementedException();
        }

        public override void PollSataic()
        {
        }

        public override void PollChange()
        {
            int Loop_Count = 0;
            DateTime EndTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < EndTime)
            {
                foreach (var Item in Session.MappingItemList)
                {
                    if (Array.IndexOf(PolledChangeDataType, Item.FrontendDataType) < 0)
                    {
                        continue;
                    }
                    atopLog.WriteLog(atopLogMode.TestInfo, "Polled_Change " + Item.MappingString);
                    string Command = HarnessCommand.GetMasterCommand(Item.FrontendDataType, Item.FrontendProtocolType);

                    //單次讀取
                    if (!SetRandomValueToServer(Item))
                    {
                        continue;
                    }
                    Thread.Sleep(10000);
                    for (int Index = 0; Index < Item.FrontendCount; Index++)
                    {
                        int IOA = Index + Item.FrontendStart;
                        if (Item.FrontendDataType == "MIT")
                        {
                            Dictionary<string, string> DataVariable = GetMITValue(IOA);
                            DataCompare(DataVariable);
                        }
                        else
                        {
                            string CMD = HarnessCommand.GetMasterCommand("crdna", Item.FrontendProtocolType);
                            var DataVariable = SendHassionCmd($"{CMD} get ioa {IOA} value");
                            if (!CheckResponse(DataVariable))
                            {
                                continue;
                            }
                            DataCompare(DataVariable);
                        }
                    }
                }
                Loop_Count++;
                atopLog.WriteLog(atopLogMode.TestInfo, "Polled Change Loop:" + Loop_Count);
            }

        }

        public override void PollControl()
        {
            int Loop_Count = 0;
            DateTime EndTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < EndTime)
            {
                foreach (ConfigMappingItem Item in Session.MappingItemList)
                {
                    if (Array.IndexOf(PolledControlDataType, Item.FrontendDataType) < 0)
                    {
                        continue;
                    }
                    atopLog.WriteLog(atopLogMode.TestInfo, "Polled_Control " + Item.MappingString);
                    string Command = HarnessCommand.GetMasterCommand(Item.FrontendDataType, Item.FrontendProtocolType);
                    string Mode = GetMode(Command);
                    for (int Index = 0; Index < Item.FrontendCount; Index++)
                    {
                        int IOA = Index + Item.FrontendStart;
                        int Value = PointValueRange.GetRandomValue(Item.FrontendDataType);
                        var DataVariable = SendHassionCmd($"{Command} ioa {IOA} value {Value} mode {Mode}");
                        if (!CheckResponse(DataVariable))
                            continue;
                        DataCompare(DataVariable);
                    }

                }
                Loop_Count++;
                atopLog.WriteLog(atopLogMode.TestInfo, "Polled Control Loop:" + Loop_Count);
            }
        }

        private Dictionary<string, string> GetMITValue(int IOA)
        {
            string CMD = HarnessCommand.GetMasterCommand("mit", Session.Protocol);
            var Value = Session.SocketClient.Send($"{CMD} get ioa {IOA} value");
            Dictionary<string, string> DataVariable = new Dictionary<string, string>();
            DataVariable.Add("OBJ0,IOA", IOA.ToString());
            DataVariable.Add("Value", Value);
            return DataVariable;
        }

        private void DataCompare(Dictionary<string, string> DataVariable)
        {
            try
            {
                if (!DataVariable.ContainsKey("OBJ0,IOA"))
                {
                    throw new TestException("no such OBJ0,IOA");
                }
                int IOA = ToInt(DataVariable["OBJ0,IOA"]);
                foreach (var Item in Session.MappingItemList)
                {
                    if (IOA < Item.FrontendStart || IOA > Item.FrontendStart + Item.FrontendCount)
                    {
                        continue;
                    }
                    double FrontendValue = ToDouble(DataVariable[GetValuekey(Item.FrontendDataType)]);
                    int[] BackendValueArray = GetBackendData(Item);
                    double BackendValue = ToBackendValue(Item, BackendValueArray, IOA);
                    if (FrontendValue != BackendValue)
                    {
                        if (Item.BackendCount == 1)
                        {
                            throw new TestException($"Check Data Fail: IOA = {IOA} , Frontend value = {FrontendValue} , Backend value = {BackendValue}");
                        }
                        else
                        {
                            int BackendIndex = (IOA - Item.FrontendStart) * 2 + Item.BackendStart;
                            throw new TestException($"Check Data Fail: IOA = {IOA} , Frontend value = {FrontendValue} , Backend high value = {BackendValueArray[BackendIndex]}, low value = {BackendValueArray[BackendIndex + 1]}");
                        }
                    }
                }

            }
            catch (HarnessSocketException ex)
            {
                atopLog.WriteLog(atopLogMode.SocketInfo, ex.Message);
            }
            catch (TestException ex)
            {
                atopLog.WriteLog(atopLogMode.TestFail, ex.Message);
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
            }
        }

        public int[] GetBackendData(ConfigMappingItem Item)
        {
            string BackendCommand = HarnessCommand.GetSlaveCommand(Item.BackendDataType, Item.BackendProtocolType);
            int[] Value = new int[Item.BackendCount];
            switch (Item.BackendProtocolType)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    HarnessTCPClient Client = SocketClientList[Item.BackendIndex - 1];
                    string BackendData = Client.Send($"Get {BackendCommand} {Item.BackendStart} {Item.BackendCount}");
                    var DicBackendData = GetPointValueList(BackendData);
                    for (int i = 0; i < Item.BackendCount; i++)
                    {
                        Value[i] = DicBackendData[i + Item.BackendStart];
                    }
                    break;
                case EnumProtocolType.IEC61850:
                    break;
            }
            return Value;
        }



        private double ToBackendValue(ConfigMappingItem Item, int[] BackendValueArray, int IOA)
        {
            double CheckData = 0;
            switch (Item.BackendProtocolType)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    if (Item.BackendCount == 1)
                    {
                        int BackendIndex = IOA - Item.FrontendStart + Item.BackendStart;
                        switch (Item.FrontendDataType)
                        {
                            case "MST":
                                CheckData = BackendValueArray[BackendIndex] % 256;
                                break;
                            default:
                                CheckData = BackendValueArray[BackendIndex];
                                break;
                        }
                    }
                    else
                    {
                        int BackendIndex = (IOA - Item.FrontendStart) * 2 + Item.BackendStart;
                        int HighValue = BackendValueArray[BackendIndex];
                        int LowValue = BackendValueArray[BackendIndex + 1];

                        switch (Item.FrontendDataType)
                        {
                            case "MDP":
                            case "CDCNA":
                                CheckData = HighValue * 2 + LowValue;
                                break;
                            case "MVF":
                            case "CSENC":
                                CheckData = IEEE754(HighValue * 65536 + LowValue);
                                break;
                            default:
                                CheckData = HighValue * 65536 + LowValue;
                                break;
                        }
                    }
                    break;
                case EnumProtocolType.IEC61850:
                    break;
            }
            return CheckData;
        }

        private double IEEE754(int value)
        {
            string Bin = Convert.ToString(value, 2).PadLeft(32, '0');
            int S = Convert.ToInt32(Bin.Substring(0, 1));
            int E = Convert.ToInt32(Bin.Substring(1, 8), 2);
            double M = 0;
            for (int i = 1; i < 24; i++)
            {
                int BinIndex = i + 8;
                M += Math.Pow(2, -1 * i) * Convert.ToInt32(Bin[BinIndex].ToString());
            }
            if (E == 0 && M == 0) { return 0; }
            if (E == 255 && M != 0) { throw new TestException("IEEE754 Fail : 1.#QNAN , value = " + value); }
            if (E == 255 && M == 0 && S == 0) { throw new TestException("IEEE754 Fail : 1.#INF , value = " + value); }
            if (E == 255 && M == 0 && S == 1) { throw new TestException("IEEE754 Fail : -1.#INF , value = " + value); }
            int Sing = S * 2 + 1;
            int Index = E - 126;
            double Mantissa = M;
            if (E > 0)
            {
                Index = E - 127;
                Mantissa = M + 1;
            }
            string Num = (Sing * Math.Pow(2, Index) * Mantissa).ToString("0.#####E+0");
            return ToDouble(Num);
        }

        public override Dictionary<string, string> SendHassionCmd(string Command)
        {
            Dictionary<string, string> DicDataVariable = null;
            try
            {
                DicDataVariable = base.SendHassionCmd(Command);
                Dictionary<string, string> DicCommandPara = GetCommandPara(Command);
                string[] CommandArr = Command.Split(' ');
                string HCmd = CommandArr[0];


                if (!DicDataVariable.ContainsKey("OBJ0,IOA"))
                {
                    throw new TestException("No such data(OBJ0,IOA)");
                }
                int IOA = ToInt(DicDataVariable["OBJ0,IOA"]);

                #region Check Type
                if (!HCmd.Contains("crdna"))
                {
                    int CMDType = GetCMDType(HCmd);
                    if (ToInt(DicDataVariable["TYPE"]) != CMDType)
                    {
                        throw new TestException($"Check Type Fail : ioa = {IOA} ,Command = {HCmd}, type = {DicDataVariable["TYPE"]} ,Expected = {CMDType}");
                    }
                }
                #endregion

                #region Check COT

                if (!DicDataVariable.ContainsKey("COT"))
                {
                    throw new TestException($"IOA = {IOA}, no such COT");
                }
                else
                {
                    if (DicCommandPara.ContainsKey("mode"))
                    {
                        int COT = ToInt(DicDataVariable["COT"]);
                        int CheckCOT = -1;
                        switch (DicCommandPara["mode"])
                        {
                            case "auto":
                                COT = -1;
                                break;
                            case "select":
                                CheckCOT = 7;
                                break;
                            case "execute":
                            case "activate":
                                CheckCOT = 10;
                                break;
                            default:
                                throw new TestException($"COT ");

                        }
                        if (CheckCOT != COT)
                        {
                            throw new TestException($"COT Fail : IOA = {IOA} , COT = {COT} ,Expected = {CheckCOT}");
                        }
                    }
                }
                #endregion

                #region Check Value
                if (DicCommandPara.ContainsKey("value"))
                {
                    string ValueKey = GetValuekey(HCmd);
                    string CheckValue = DicCommandPara["value"].Trim();
                    if (!DicDataVariable.ContainsKey(ValueKey))
                    {
                        throw new TestException("no such variable : " + ValueKey);
                    }
                    int Value = ToInt(DicDataVariable[ValueKey]);
                    int ValueFormat = 0;
                    if (HCmd.Contains("cscna"))
                    {
                        CheckValue = CheckValue.Replace("off", "0").Replace("on", "1");
                        ValueFormat = 0x80;
                    }
                    else if (HCmd.Contains("cdcna"))
                    {
                        CheckValue = CheckValue.Replace("1", "2").Replace("0", "1").Replace("off", "1").Replace("on", "2");
                        ValueFormat = 0x80;
                    }
                    else if (HCmd.Contains("crcna"))
                    {
                        CheckValue = CheckValue.Replace("up", "2").Replace("down", "1");
                        ValueFormat = 0x80;
                    }
                    int CheckValueNum = ToInt(CheckValue);
                    if (DicCommandPara.ContainsKey("mode") && DicCommandPara["mode"] == "select")
                    {
                        CheckValueNum += ValueFormat;
                    }
                    if (CheckValueNum != Value)
                    {
                        throw new TestException($"Check Value Fail : IOA = {IOA}, ValueName = {ValueKey} , {Value} != {CheckValue}");
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

        public bool CheckResponse(Dictionary<string, string> DicDataVariable)
        {
            try
            {
                int IOA = ToInt(DicDataVariable["OBJ0,IOA"]);

                #region Check ASDU
                string DataASDU = DicDataVariable["ASDU"];
                if (ASDU != DataASDU)
                {
                    throw new TestException($"IOA = {IOA} , ASDU ={DataASDU} , Expected = {ASDU}");
                }
                #endregion

                #region Check QDS
                if (DicDataVariable.ContainsKey("OBJ0,QDS"))
                {
                    int QDS = ToInt(DicDataVariable["OBJ0,QDS"]);
                    if (QDS != 0)
                    {
                        throw new TestException($"IOA = {IOA} , QDS = {QDS}, Expected : 0");
                    }
                }
                #endregion

                #region Check QDP
                if (DicDataVariable.ContainsKey("OBJ0,QDP"))
                {
                    int QDP = ToInt(DicDataVariable["OBJ0,QDP"]);
                    if (QDP != 0)
                    {
                        throw new TestException($"IOA = {IOA} , QDP = {QDP}, Expected : 0");
                    }
                }
                #endregion

                #region Check Originator
                int Originator = ToInt(DicDataVariable["ORIGINATOR"]);
                if (Originator != 4)
                {
                    throw new TestException($"IOA = {IOA}, Originator = {Originator}, Expected : 4");
                }
                #endregion

                #region Check QTY
                int QTY = ToInt(DicDataVariable["QTY"]);
                if (QTY != 1)
                {
                    throw new TestException($"IOA = {IOA}, QTY = {QTY}, Expected : 1");
                }
                #endregion

                #region Check SQ
                int SQ = ToInt(DicDataVariable["SQ"]);
                if (SQ != 0)
                {
                    throw new TestException($"IOA = {IOA}, SQ = {SQ}, Expected : 0");
                }
                #endregion
            }
            catch (TestException ex)
            {
                atopLog.WriteLog(atopLogMode.TestFail, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
                return false;
            }
            return true;
        }

        private int GetCMDType(string CMD)
        {
            if (CMD.Contains("cscna")) return 45;
            if (CMD.Contains("cdcna")) return 46;
            if (CMD.Contains("crcna")) return 47;
            if (CMD.Contains("csena")) return 48;
            if (CMD.Contains("csenb")) return 49;
            if (CMD.Contains("csenc")) return 50;
            if (CMD.Contains("cbona")) return 51;
            throw new TestException("Check Type no suppor Command : " + CMD);
        }

        private string GetValuekey(string CMD)
        {
            CMD = CMD.ToUpper();
            if (CMD.Contains("MIT")) { return "Value"; }
            if (CMD.Contains("MSP")) { return "OBJ0,SIQ"; }
            if (CMD.Contains("MDP")) { return "OBJ0,DIQ"; }
            if (CMD.Contains("MST")) { return "OBJ0,VTI"; }
            if (CMD.Contains("MBS")) { return "OBJ0,BSI"; }
            if (CMD.Contains("MVN")) { return "OBJ0,NVA"; }
            if (CMD.Contains("MVS")) { return "OBJ0,SVA"; }
            if (CMD.Contains("MVF")) { return "OBJ0,FVA"; }
            if (CMD.Contains("CSCNA")) { return "OBJ0,SCO"; }
            if (CMD.Contains("CDCNA")) { return "OBJ0,DC"; }
            if (CMD.Contains("CBONA")) { return "OBJ0,RCO"; }
            if (CMD.Contains("CSENA")) { return "OBJ0,NVA"; }
            if (CMD.Contains("CSENB")) { return "OBJ0,SVA"; }
            if (CMD.Contains("CSENC")) { return "OBJ0,FVA"; }
            if (CMD.Contains("CRCNA")) { return "OBJ0,RCO"; }

            throw new TestException("no support command : " + CMD);
        }

        private string GetMode(string CMD)
        {
            if (CMD.EndsWith("cbona"))
                return "activate";
            return "execute";
        }

        private double ToDouble(string Num)
        {
            double D = 0;
            if (double.TryParse(Num, out D))
            {
                return D;
            }
            return base.ToInt(Num);
        }
    }
}
