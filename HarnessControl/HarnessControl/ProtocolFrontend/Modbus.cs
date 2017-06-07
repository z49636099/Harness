
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace HarnessControl
{
    public class Modbus : atopProtocolHarness
    {
        private string[] PolledChaangeDataType = { "Coil", "Disc", "IReg", "HReg" };
        private string[] PolledControlDataType = { "WCoi", "WDis", "WIRe", "WHRe" };

        public override void Patten(string PattenPath)
        {
            throw new NotImplementedException();
        }

        public Modbus(CommunicationBase FC) : base(FC)
        {

        }

        public override void PollChange()
        {
            int Loop_Count = 0;
            DateTime EndTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < EndTime)
            {
                foreach (ConfigMappingItem Item in FrontendCommunication.MappingItemList)
                {
                    if (Array.IndexOf(PolledChaangeDataType, Item.FrontendDataType) < 0)
                    {
                        continue;
                    }
                    atopLog.WriteLog(atopLogMode.TestInfo, "Polled_Change " + Item.MappingString);
                    string Command = HarnessControl.Command_Harness.GetMasterCommand(Item.FrontendDataType, EnumProtocolType.Modbus);
                    int SlaveID = Item.BackendSlaveID;

                    #region 單次讀取

                    if (!SetRandomValueToServer(Item))
                    {
                        continue;
                    }
                    Thread.Sleep(5000);
                    int Quantity = 0;
                    Dictionary<string, string> DataVariable;
                    if (Item.FrontendCount * 2 == Item.BackendCount)
                        Quantity = 2;
                    else if (Item.FrontendCount == Item.BackendCount)
                        Quantity = 1;
                    else
                    {
                        atopLog.WriteLog(atopLogMode.TestFail, "Config quantity fail : " + Item.MappingString);
                        continue;
                    }

                    for (int Index = 0; Index < Item.FrontendCount; Index++)
                    {
                        int PointAddress = Item.FrontendStart + Index;
                        DataVariable = SendHassionCmd($"{Command} start {PointAddress} quantity {Quantity}", SlaveID);
                        if (DataVariable == null) { continue; }
                        DataCompare(DataVariable, PointAddress, Item);
                        if (Quantity == 2) { Index++; }
                    }

                    #endregion

                    #region 批量讀取
                    if (!SetRandomValueToServer(Item))
                    {
                        continue;
                    }
                    Thread.Sleep(5000);
                    DataVariable = SendHassionCmd($"{Command} start {Item.FrontendStart} quantity {Item.FrontendCount}", SlaveID);
                    if (DataVariable == null) { continue; }
                    DataCompare(DataVariable, Item.FrontendStart, Item);
                    #endregion
                }
                Loop_Count++;
                atopLog.WriteLog(atopLogMode.TestInfo, "PollChange Loop:" + Loop_Count);
            }
        }

        //private string GetSlaveName(ConfigMappingItem Item)
        //{
        //    var Communication = BackendCommunication.Where(a => a.Index == Item.BackendIndex).First();
        //    var Session = Communication as Communication_Harness;
        //    if (Session == null)
        //        return "";
        //    else
        //        return Session.SlaveName;
        //}

        public override void PollControl()
        {
            int Loop_Count = 0;
            DateTime EndTime = DateTime.Now.AddSeconds(10);
            while (DateTime.Now < EndTime)
            {
                foreach (var Item in FrontendCommunication.MappingItemList)
                {
                    if (Array.IndexOf(PolledControlDataType, Item.FrontendDataType) < 0)
                    {
                        continue;
                    }
                    int SlaveName = Item.BackendSlaveID;
                    atopLog.WriteLog(atopLogMode.TestInfo, "Polled_Control " + Item.MappingString);
                    string Command = HarnessControl.Command_Harness.GetMasterCommand(Item.FrontendDataType, EnumProtocolType.Modbus);
                    int[] RandomValue = PointValueRange.GetRandomValue(Item.BackendDataType, Item.BackendCount);
                    for (int Index = 0; Index < Item.FrontendCount; Index++)
                    {
                        int PointIndex = Index + Item.FrontendStart;
                        int PointValue = RandomValue[Index];
                        SendHassionCmd($"{Command} start {PointIndex} value {PointValue}", SlaveName);
                    }
                    Thread.Sleep(2000);
                    DataCompare_Control(RandomValue, Item);
                }
                Loop_Count++;
                atopLog.WriteLog(atopLogMode.TestInfo, "PollControl Loop:" + Loop_Count);
            }
        }

        public override void PollSataic()
        {
        }



        private void DataCompare_Control(int[] randomValue, ConfigMappingItem item)
        {
            switch (item.BackendProtocolType)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    Dictionary<int, int> DicPointValue = new Dictionary<int, int>();
                    for (int Index = 0; Index < item.FrontendCount; Index++)
                    {
                        DicPointValue.Add(item.FrontendStart + Index, randomValue[Index]);
                    }
                    CheckData(DicPointValue, item);
                    break;
                case EnumProtocolType.IEC61850:
                    break;
            }
        }

        public void DataCompare(Dictionary<string, string> DicDataVariable, int StartPoint, ConfigMappingItem Item)
        {
            //Dictionary<string, string> DicDataVariable = GetDataVariableDic(DataVariable);
            Dictionary<int, int> DicPointValue = new Dictionary<int, int>();
            string DataType = "";
            switch (ToInt(DicDataVariable["FC"]))
            {
                case 1: DataType = "Coil"; break;
                case 2: DataType = "Disc"; break;
                case 3: DataType = "HReg"; break;
                case 4: DataType = "IReg"; break;
                default:
                    atopLog.WriteLog(atopLogMode.TestFail, "Error FC :" + DicDataVariable["FC"]);
                    return;
            }
            foreach (var Variable in DicDataVariable)
            {
                if (Variable.Key.Contains("DATA"))
                {
                    int Key = int.Parse(Variable.Key.Replace("DATA", ""));
                    DicPointValue.Add(Key + StartPoint, ToInt(Variable.Value));
                }
            }
            CheckData(DicPointValue, Item);
        }

        private void CheckData(Dictionary<int, int> DicPointValue, ConfigMappingItem Item)
        {
            int VariableMaxPoint = DicPointValue.Select(a => a.Key).Max();
            int VariableMinPoint = DicPointValue.Select(a => a.Key).Min();
            string ClientCommand = Command_Harness.GetMasterCommand(Item.FrontendDataType, EnumProtocolType.Modbus);
            //foreach (var Item in Session.MappingItemList)
            //{
            int tmpMaxPoint = -1;
            int tmpMinPoint = -1;
            int ItemStart = Item.FrontendStart;
            int ItemEnd = Item.FrontendStart + Item.FrontendCount - 1;
            //if (Item.FrontendDataType != DataType)
            //{
            //    continue;
            //}
            if (VariableMinPoint <= ItemStart && VariableMaxPoint >= ItemStart)
            {
                tmpMinPoint = ItemStart;
                tmpMaxPoint = VariableMaxPoint > ItemEnd ? ItemEnd : VariableMaxPoint;
            }
            else if (VariableMaxPoint <= ItemEnd && VariableMaxPoint >= ItemEnd)
            {
                tmpMaxPoint = ItemEnd;
                tmpMinPoint = VariableMinPoint > ItemStart ? VariableMinPoint : ItemStart;
            }
            else if (VariableMinPoint > ItemStart && VariableMaxPoint < ItemEnd)
            {
                tmpMinPoint = VariableMinPoint;
                tmpMaxPoint = VariableMaxPoint;
            }
            //else { continue; }
            switch (Item.BackendProtocolType)
            {
                case EnumProtocolType.DNP3:
                case EnumProtocolType.IEC101:
                case EnumProtocolType.IEC104:
                case EnumProtocolType.Modbus:
                    Dictionary<int, int> DicBackendVariable = GetBackendData(Item, tmpMinPoint, tmpMaxPoint);
                    if (DicBackendVariable == null) { return; }
                    DataCompare1(DicPointValue, DicBackendVariable, Item);
                    break;
                case EnumProtocolType.IEC61850:
                    break;
            }
            //}
        }

        /// <summary>DNP3 Modbus 101 104</summary>
        private void DataCompare1(Dictionary<int, int> DicFrontVariable, Dictionary<int, int> DicBackendVariable, ConfigMappingItem Item)
        {
            int QuantityType = DicFrontVariable.Count - DicBackendVariable.Count;
            int MaxIndex = DicFrontVariable.Keys.Max();
            for (int i = DicFrontVariable.Keys.Min(); i <= MaxIndex; i++)
            {
                if (DicFrontVariable.Count == 0)
                    return;
                if (QuantityType == 0)
                {
                    int BackendPoint = i - Item.FrontendStart + Item.BackendStart;
                    var Variable = DicFrontVariable[i];
                    if (Variable != DicBackendVariable[BackendPoint])
                    {
                        atopLog.WriteLog(atopLogMode.TestFail, $"DataType: {Item.FrontendDataType}, Point: {i} Frontend value {Variable}, Backend Value: {DicBackendVariable[BackendPoint]}");
                    }
                    DicFrontVariable.Remove(i);
                }
                else if (QuantityType > 0)
                {
                    int BackendPoint = (i - Item.FrontendStart + Item.BackendStart) / 2;
                    int LowValue = DicFrontVariable[i + 1];
                    int HighValue = DicFrontVariable[i];
                    int FrontendValue = 0;
                    switch (Item.FrontendDataType)
                    {
                        case "Coil":
                        case "Disc":
                        case "WCoi":
                        case "WDis":
                            FrontendValue = LowValue * 2 + HighValue;
                            break;
                        default:
                            FrontendValue = LowValue * 65536 + HighValue;
                            break;
                    }
                    if (FrontendValue != DicBackendVariable[BackendPoint])
                    {
                        atopLog.WriteLog(atopLogMode.TestFail, $"DataType: {Item.FrontendDataType}, Point: {i} Frontend High Value: {HighValue} Low Value: {LowValue}, Backend Value: {DicBackendVariable[BackendPoint]}");
                    }
                    DicFrontVariable.Remove(i);
                    DicFrontVariable.Remove(i + 1);
                }
            }
            if (DicFrontVariable.Count != 0)
            {
                string PointString = string.Join(",", DicFrontVariable.Select(a => a.Key));
                atopLog.WriteLog(atopLogMode.TestFail, $"DataType : {Item.FrontendDataType},No Such point : {PointString}");
            }
        }

        private Dictionary<int, int> GetBackendData(ConfigMappingItem Item, int FrontendStartPoint, int FrontendEndPoint)
        {
            SocketClient Client = SocketClientList[Item.BackendIndex - 1];
            Dictionary<int, int> DicBackendVariable = new Dictionary<int, int>();
            string ServerCommand = Command_Harness.GetSlaveCommand(Item.FrontendDataType, Item.BackendProtocolType);
            int QuantityType = Item.FrontendCount - Item.BackendCount;
            int BackendStart = -1;
            int BackendCount = -1;
            if (QuantityType == 0)
            {
                BackendStart = Item.BackendStart + FrontendStartPoint - Item.FrontendStart;
                BackendCount = FrontendEndPoint - FrontendStartPoint + 1;
            }
            else if (QuantityType < 0)
            {
                atopLog.WriteLog(atopLogMode.TestFail, "No support : " + Item.MappingString);
                return null;
                //BackendStart = Item.BackendStart + ((tmpMinPoint - Item.FrontendStart) * 2);
                //BackendCount = (tmpMaxPoint - tmpMinPoint + 1) * 2;
            }
            else
            {
                BackendStart = Item.BackendStart + ((FrontendStartPoint - Item.FrontendStart) / 2);
                BackendCount = (FrontendEndPoint - FrontendStartPoint + 1) / 2;
            }
            string Data = Client.Send(string.Format("Get {0} {1} {2} {3}", ServerCommand, BackendStart, BackendCount,Item.BackendSlaveID));
            DicBackendVariable = GetPointValueList(Data);
            return DicBackendVariable;
        }

        public Dictionary<string, string> SendHassionCmd(string Command, int SlaveName)
        {
            FrontendSession.SocketClient.Send("mmbmodifysession address " + SlaveName);
            return SendHassionCmd(Command);
        }

        public override Dictionary<string, string> SendHassionCmd(string Command)
        {
            Dictionary<string, string> DicDataVariable = null;
            try
            {
                DicDataVariable = base.SendHassionCmd(Command);
                string[] CommandArr = Command.Split(' ');
                string HCmd = CommandArr[0];

                if (ToInt(DicDataVariable["FC"]) != GetFunctionCode(HCmd))
                {
                    throw new TestException($"FunctionCode Fail , FC = {DicDataVariable["FC"]} ,Expected : { GetFunctionCode(HCmd)}");
                }

                if (DicDataVariable.ContainsKey("BYTECNT"))
                {
                    double ByteLen = 0;
                    if (HCmd.Contains("coil") || HCmd.Contains("readdinput"))
                    {
                        ByteLen = 0.125;
                    }
                    else
                    {
                        ByteLen = 2;
                    }
                    int DataCount = 0;
                    foreach (var dic in DicDataVariable)
                    {
                        if (dic.Key.StartsWith("DATA"))
                        {
                            DataCount++;
                        }
                    }
                    double DataByte = ByteLen * DataCount;
                    int ByteCnt = ToInt(DicDataVariable["BYTECNT"]);
                    if (ByteCnt < DataByte || ByteCnt - 1 >= DataByte)
                    {
                        throw new TestException($"Check Byte Count Fail , Byte =  {ByteCnt} ,Expected : {ByteLen}");
                    }
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



        public override void CheckStatVariable(SocketClient Client, string Response, string Command)
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
                        default:
                            throw new TestException($"cmd error : status = {Stat} , {Command}");
                    }
                }
            }
        }

        public int GetFunctionCode(string Cmd)
        {
            switch (Cmd)
            {
                case "mmbreadcoils":
                    return 0x1;
                case "mmbreaddinputs":
                    return 0x2;
                case "mmbreadhregs":
                    return 0x3;
                case "mmbreadiregs":
                    return 0x4;
                case "mmbwritecoil":
                    return 0x5;
                case "mmbwritehreg":
                    return 0x6;
                case "mmbwritemulticoils":
                    return 0x15;
                case "mmbwritemultihregs":
                    return 0x16;
                default:
                    throw new TestException("Function code fail , No support the command " + Cmd);
            }
        }

    }
}
