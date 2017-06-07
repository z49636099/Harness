using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    class BackendSocketAccept_61850 : BackendSocketAccept
    {

        private Unc unc = new Unc();

        private atop61850Base P61850 = new atop61850Base();

        public BackendSocketAccept_61850()
        {
            unc.Connect(Global.UncPath, Global.Domain, Global.User, Global.Passowrd);
        }

        protected override string Echo(string message)
        {
            try
            {
                string[] Command;
                string ServerName = string.Empty;
                string TagName = string.Empty;
                string DataType = string.Empty;
                string ReturnData = string.Empty;
                string[] Data = message.Split('-');
                switch (Data[0].Trim())
                {
                    case "LoadSCL":
                        string[] FI = Directory.GetFiles(@"\\10.0.0.187\Document\International Link\PG59XX\QA\eNode Database\MB-50\MBES-50EC\M61-P14-ed2-t5-server");

                        string[] database = Data[1].Trim().Replace("\r\n", "").Replace("\0", "").Split(' ');

                        foreach (string item in database)
                        {
                            foreach (string cidFile in FI)
                            {
                                FileInfo _fileInfo = new FileInfo(cidFile);
                                string FileIndex = _fileInfo.Name.Split('-').Last().Replace("server", "").Replace(".cid", "").PadLeft(2, '0');
                                if (item.Trim().Replace("ServerIED", "").PadLeft(2, '0') == FileIndex)
                                {
                                    //0 = successuful -1 = fail
                                    ReturnData = LoadCidFile(cidFile, item) ? "0" : "-1";
                                    break;
                                }
                            }
                        }
                        break;
                    case "Read":
                        Command = Data[1].Split(' ');
                        ServerName = Command[0];
                        TagName = Command[1];
                        DataType = Command[2].Replace("\r\n", "");
                        ////取得真實的內容 submit.sh
                        //GetRealValue(ServerName, TagName);
                        //取得資料
                        ReturnData = P61850.GetValue(ServerName, TagName, DataType);
                        break;
                    case "Update":
                        Command = Data[1].Replace("\r\n", "").Split(' ');
                        ServerName = Command[0];
                        TagName = Command[1];
                        DataType = Command[2];
                        string Value = Command[3];
                        //更新Xales的資料庫 0 = successuful -1 = fail
                        if (P61850.UPdateValue(ServerName, TagName, DataType, Value))
                        {
                            ReturnData = "0";
                        }
                        else
                        {
                            ReturnData = "-1";
                        }
                        break;
                }
                return ReturnData;
            }
            catch (Exception exError)
            {
                WriteLog(exError.Message);
                return string.Empty;
            }
        }

        //private bool UPdateValue(string ServerName, string TagName, string Datatype, string Value)
        //{
        //    OperationCygwin($"./update.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName} {Command_61850.GetDataType(Datatype)} {Value}", "update.sh");

        //    OperationCygwin($"./submit.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName} {Command_61850.GetDataType(Datatype)} {Value}", "submit.sh");

        //    while (true)
        //    {
        //        string OutputData = OperationCygwin($"./show2.sh sample root xelas123 {ServerName} SETDATAVALUES {TagName}", "show2.sh");
        //        int Count = OutputData.Split('\n').Count();
        //        string[] returnData = OutputData.Split('\n')[Count - 2].Split('|');
        //        if (returnData[6].Trim().Equals("COMPLETED"))
        //            break;
        //        else if (returnData[6].Trim().Equals("ERROR"))
        //            continue;
        //    }
        //    return true;
        //}

        //public bool GetRealValue(string ServerName, string TagName)
        //{
        //    try
        //    {
        //        //61850 Get Real Value to Client Database
        //        OperationCygwin($"./submit.sh sample root xelas123 {ServerName} GETDATAVALUES {TagName}", "submit.sh");

        //        while (true)
        //        {
        //            string OutputData = OperationCygwin($"./show2.sh sample root xelas123 {ServerName} GETDATAVALUES {TagName}", "show2.sh");
        //            int Count = OutputData.Split('\n').Count();
        //            string[] returnData = OutputData.Split('\n')[Count - 2].Split('|');
        //            if (returnData[6].Trim().Equals("COMPLETED"))
        //                break;
        //            else if (returnData[6].Trim().Equals("ERROR"))
        //                continue;
        //        }
        //        return true;
        //    }
        //    catch (Exception exError)
        //    {
        //        WriteLog(exError.ToString());
        //        return false;
        //    }
        //}

        //public bool GetRealValue(List<DataStruct61850> Data)
        //{
        //    try
        //    {
        //        for (int i = 0; i < Data.Count(); i++)
        //        {
        //            //61850 Get Real Value to Client Database
        //            OperationCygwin($"./submit.sh sample root xelas123 {Data[i].IEC61850_SERVERNAME} GETDATAVALUES {Data[i].IEC61850_TAGNAME}", "submit.sh");

        //            while (true)
        //            {
        //                string OutputData = OperationCygwin($"./show2.sh sample root xelas123 {Data[i].IEC61850_SERVERNAME} GETDATAVALUES {Data[i].IEC61850_TAGNAME}", "show2.sh");
        //                int Count = OutputData.Split('\n').Count();
        //                string[] returnData = OutputData.Split('\n')[Count - 2].Split('|');
        //                if (returnData[6].Trim().Equals("COMPLETED"))
        //                    break;
        //                else if (returnData[6].Trim().Equals("ERROR"))
        //                    break;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception exError)
        //    {
        //        WriteLog(exError.ToString());
        //        return false;
        //    }
        //}

        public bool LoadCidFile(string Path, string ServerIED)
        {
            bool ReturnData = false;

            //if (ImportSCL(ServerIED, Path) && StartSCL(ServerIED) && Associate(ServerIED))
            if (StartSCL(ServerIED) && Associate(ServerIED))
                ReturnData = true;
            else
                ReturnData = false;
            return ReturnData;
        }

        public bool StartSCL(string ServerIED)
        {
            string ReturnMessage = P61850.OperationCygwin($"./start_server.sh sample root xelas123 {ServerIED}", "start_server.sh");
            WriteLog($"Run Server {ReturnMessage}");
            ReturnMessage = P61850.OperationCygwin($"./start_client.sh sample root xelas123 C_{ServerIED}", "start_client.sh");
            WriteLog($"Run client {ReturnMessage}");
            return true;
        }

        public bool Associate(string ServerIED)
        {
            string ReturnMessage = P61850.OperationCygwin($"./connect.sh sample root xelas123 {ServerIED}", "connect.sh");
            WriteLog($"Associate {ReturnMessage}");
            return true;
        }

        //private string GetValue(string ServerName, string TagName, string DataType)
        //{
        //    string ReturnData = string.Empty;
        //    Dictionary<string, string> TempData = new Dictionary<string, string>();
        //    string[] OutputData = OperationCygwin($"./show.sh sample root xelas123 {ServerName} ACSI_DATA_ATTR \"type = '{Command_61850.GetDataType(DataType)}'\" dataRef,type,{Command_61850.GetCommandType(Command_61850.GetDataType(DataType))}", "show.sh").Split('\n');
        //    foreach (var item in OutputData)
        //    {
        //        if (TempData.Count > 0)
        //        {
        //            if (TempData.ContainsKey(TagName))
        //            {
        //                ReturnData = TempData[TagName];
        //                break;
        //            }
        //            else
        //            {
        //                if (item != string.Empty && item.Substring(0, 1) == "|")
        //                {
        //                    string[] RealData = item.Split('|');
        //                    TempData.Add(RealData[1].Trim(), RealData[3]);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (item != string.Empty && item.Substring(0, 1) == "|")
        //            {
        //                string[] RealData = item.Split('|');
        //                TempData.Add(RealData[1].Trim(), RealData[3]);
        //            }
        //        }
        //    }
        //    return ReturnData;
        //}

        //private string OperationCygwin(string Command, string ShellCommand)
        //{
        //    try
        //    {
        //        System.Diagnostics.Process Carried_Out = new System.Diagnostics.Process();
        //        System.Diagnostics.ProcessStartInfo Carried_Out_Info = new System.Diagnostics.ProcessStartInfo(@"C:\cygwin64\bin\bash.exe");
        //        //設定要執行的指令
        //        Console.WriteLine(Command);
        //        Carried_Out_Info.Arguments = Command;
        //        string Folder = Command_61850.GetCommandFolder(ShellCommand);
        //        Carried_Out_Info.WorkingDirectory = @"C:\cygwin64\opt\xelas\iec61850\client\" + Folder;
        //        Carried_Out_Info.RedirectStandardOutput = true;
        //        Carried_Out_Info.RedirectStandardError = true;
        //        Carried_Out_Info.UseShellExecute = false;
        //        Carried_Out.StartInfo = Carried_Out_Info;
        //        //執行程式
        //        Carried_Out.Start();
        //        string OutputData = Carried_Out.StandardOutput.ReadToEnd();
        //        Console.WriteLine(OutputData);
        //        Carried_Out.WaitForExit();
        //        return OutputData;
        //    }
        //    catch (Exception exError)
        //    {
        //        atopLog.WriteLog(atopLogMode.SystemError, exError.Message);
        //        return string.Empty;
        //    }
        //}
        private void WriteLog(string Msg)
        {
            atopLog.WriteLog(atopLogMode.SocketInfo, Msg);
        }
    }
    public struct DataStruct61850
    {
        public string IEC61850_SERVERNAME;
        public string IEC61850_TAGNAME;
        public string IEC61850_DATATYPE;
        public string IEC61850_EXCHANGETYPE;
        public string IEC61850_FC;
        public string IEC61850_VALUE;
        public int Frontend_ADDRESS;
        public string Frontend_FUNCTION;
        public string Frontend_RANGE;
    }
    
}
