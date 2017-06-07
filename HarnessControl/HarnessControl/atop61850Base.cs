using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    
    public class atop61850Base
    {
        [STAThread]
        public virtual bool ImportSCL(string ServerName, string Path)
        {
            string ReturnMessage = OperationCygwin($"./import.sh sample root xelas123 {ServerName} \"{Path}\"", "import.sh");
            /* 確認回傳資料的內容，有哪些需要判斷 */
            string[] Data = (ReturnMessage.Trim().Split('\n'));
            if (Data[Data.Length - 1].Contains("Import completed."))
                return true;
            else
                return false;
        }

        public virtual bool StartSCLServer(string ServerIED)
        {
            bool Flag = true;
            /* 必須要判斷字元是不是成功 */
            string ReturnMessage = OperationCygwin($"./start_server.sh sample root xelas123 {ServerIED}", "start_server.sh");
            string[] Data = ReturnMessage.Trim().Split('\n');
            foreach (string line in Data)
            {
                if (line .Contains("Error"))
                {
                    Flag = false;
                    break;
                }
            }
            if (Flag)
                atopLog.WriteLog(atopLogMode.XelasCommandInfo, ReturnMessage);
            else
                atopLog.WriteLog(atopLogMode.XelasCommandError, ReturnMessage);
            return Flag;
        }

        public virtual bool StartSCLClient(string ClientIED)
        {
            bool Flag = true;
            /* 必須要判斷字元是不是成功 */
            string ReturnMessage = OperationCygwin($"./start_client.sh sample root xelas123 C_{ClientIED}", "start_client.sh");
            string[] Data = ReturnMessage.Trim().Split('\n');
            foreach (string line in Data)
            {
                if (line.Contains("Error"))
                {
                    Flag = false;
                    break;
                }
            }

            if (Flag)
                atopLog.WriteLog(atopLogMode.XelasCommandInfo, ReturnMessage);
            else
                atopLog.WriteLog(atopLogMode.XelasCommandError, ReturnMessage);

            return Flag;
        }

        public virtual bool Associate(string ServerIED)
        {
            /* 必須要判斷字元是不是成功 */
            string ReturnMessage = OperationCygwin($"./connect.sh sample root xelas123 {ServerIED}", "connect.sh");
            return true;
        }

        public virtual bool SetData(string ServerName, string TagName, string DataType, string Value)
        {
            try
            {
                /* submit set data -> Check execute condition -> submit.sh get data -> show.sh*/
                OperationCygwin($"./submit.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName} {DataType} {Value}", "submit.sh");

                /* submit set data success*/
                if (Process_Status(ServerName, TagName))
                {
                    /* submit get datat*/
                    string Server_Value = GetValue(ServerName, TagName, DataType);
                    if (Value == ServerName)
                    {
                        atopLog.WriteLog(atopLogMode.XelasCommandInfo, $"Wirte to Server : {ServerName} , TagName : {TagName} , Value : {Value} is Success");
                        return true;
                    }
                    else
                    {
                        atopLog.WriteLog(atopLogMode.XelasCommandInfo, $"Wirte to Server : {ServerName} , TagName : {TagName} , Value : {Value} is Fail");
                        return false;
                    }
                }
                else
                {
                    atopLog.WriteLog(atopLogMode.XelasCommandInfo, $"Wirte to Server : {ServerName} , TagName : {TagName} , Value : {Value} is Fail");
                    return false;
                }
            }
            catch (Exception exError)
            {
                atopLog.WriteLog(atopLogMode.SystemError, exError.Message);
                return false;
            }
        }

        private bool Process_Status(string ServerName, string TagName)
        {
            try
            {
                DateTime Curry_Time = DateTime.Now;
                bool Flag = false;
                while ((Curry_Time.Second - DateTime.Now.Second) < 3)
                {
                    string OutputData = OperationCygwin($"./show2.sh sample root xelas123 {ServerName} GETDATAVALUES {TagName}", "show2.sh");
                    int Count = OutputData.Split('\n').Count();
                    string[] returnData = OutputData.Split('\n')[Count - 2].Split('|');
                    /* 這邊需要修改，在確定一下如果有Error的話要怎麼處理 */
                    if (returnData[6].Trim() =="COMPLETED")
                    {
                        Flag = true;
                        break;
                    }
                    else if (returnData[6].Trim() == ("ERROR"))
                    {
                        atopLog.WriteLog(atopLogMode.TestInfo, "Submit Command : Error");
                        Flag = false;
                        break;
                    }
                }
                //if ((Curry_Time.Second - DateTime.Now.Second) > 5)
                //{
                //    atopLog.WriteLog(atopLogMode.SystemInformation, "Submit Command : Timeout");
                //    Flag = false;
                //}
                return Flag;
            }
            catch (Exception exError)
            {
                atopLog.WriteLog(atopLogMode.SystemError, exError.Message);
                return false;
            }
        }

        private bool getRealValue(string ServerName, string TagName)
        {
            try
            {
                OperationCygwin($"./submit.sh sample root xelas123 {ServerName} GETDATAVALUES {TagName}", "submit.sh");

                return Process_Status(ServerName, TagName);
            }
            catch (Exception exError)
            {
                atopLog.WriteLog(atopLogMode.SystemError, exError.Message);
                return false;
            }
        }

        public virtual string GetValue(string ServerName, string TagName, string DataType)
        {
            try
            {
                /* 取得實際資料失敗 */
                if (!getRealValue(ServerName, TagName))
                {
                    return string.Empty;
                }

                string ReturnData = string.Empty;
                Dictionary<string, string> TempData = new Dictionary<string, string>();
                string[] OutputData = OperationCygwin($"./show.sh sample root xelas123 {ServerName} ACSI_DATA_ATTR \"type = '{DataType}'\" dataRef,type,{getCommandType(DataType)}", "show.sh").Split('\n');

                for (int i = 0; i < OutputData.Length; i++)
                {
                    if (TempData.Count > 0)
                    {
                        if (TempData.ContainsKey(TagName))
                        {
                            ReturnData = TempData[TagName];
                            return ReturnData;
                        }
                        else
                        {
                            if (OutputData[i] != string.Empty && OutputData[i].StartsWith("|"))
                            {
                                string[] RealData = OutputData[i].Split('|');
                                TempData.Add(RealData[1].Trim(), RealData[3]);
                            }
                        }
                    }
                    else
                    {
                        if (OutputData[i] != string.Empty && OutputData[i].StartsWith("|"))
                        {
                            string[] RealData = OutputData[i].Split('|');
                            TempData.Add(RealData[1].Trim(), RealData[3]);
                        }
                    }
                }
                atopLog.WriteLog(atopLogMode.XelasCommandInfo, $"Xelas Rutrn Data {ReturnData}");
                return ReturnData;
            }
            catch (Exception exError)
            {
                atopLog.WriteLog(atopLogMode.SystemError, exError.Message);
                return string.Empty;
            }
        }

        public bool UPdateValue(string ServerName, string TagName, string Datatype, string Value)
        {
            OperationCygwin($"./update.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName} {Command_61850.GetDataType(Datatype)} {Value}", "update.sh");

            OperationCygwin($"./submit.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName} {Command_61850.GetDataType(Datatype)} {Value}", "submit.sh");

            while (true)
            {
                string OutputData = OperationCygwin($"./show2.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName}", "show2.sh");
                int Count = OutputData.Split('\n').Count();
                string[] returnData = OutputData.Split('\n')[Count - 2].Split('|');
                if (returnData[6].Trim().Equals("COMPLETED"))
                    break;
                else if (returnData[6].Trim().Equals("ERROR"))
                    continue;
            }
            return true;
        }

        private string getCommandType(string DataType)
        {
            string Value = string.Empty;
            switch (DataType)
            {
                case "INTEGER":
                case "UNSIGNED":
                case "BOOLEAN":
                    Value = "int_val";
                    break; ;
                case "BITSTRING":
                case "TIMESTAMP":
                case "STRING":
                case "OCTETSTRING":
                case "MMSSTRING":
                    Value = "string_val";
                    break;
                case "FLOAT":
                    Value = "float_val";
                    break; ;
            }
            return Value;
        }

        public string GetRandomValue(string DataType)
        {
            Random _rd = new Random();
            switch (DataType)
            {
                case "Coil":
                case "WCoi":
                case "Disc":
                    return _rd.Next(0, 2).ToString();
                case "HReg":
                case "WHRe":
                case "IReg":
                    return _rd.Next(0, 255).ToString();
            }
            return string.Empty;
        }

        public string OperationCygwin(string Command, string Shell_Command)
        {
            return ProcessController.SendCommand_61850(Command, Shell_Command);
        }
    }
}
