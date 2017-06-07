using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarnessControl
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();
        }

        SocketClient client = new SocketClient();
        ProtocolController Controller = new ProtocolController();

        ConfigSocketServer ControlServer = new ConfigSocketServer();
        private AutomationControl.Form1 FormAutomation = new AutomationControl.Form1();
        private string LogFolder { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            Global.LocalIP = GetLocalIP();
            //Log Show Action
            atopLog.ShowMsg = new Action<string>((str) =>
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    txtMsg.AppendText(str + Environment.NewLine);
                }));
            });

            try
            {
                ControlServer.ReceiveEvent += ControlServer_ReceiveEvent;
                Task.Factory.StartNew(new Action(() => { ControlServer.Start(Global.MainServerPort); }));
                GetHarnessNetInfo();
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
            }
        }

        private void NewMethod(string Msg)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtMsg.AppendText(Msg);
            }));
        }

        private void ControlServer_ReceiveEvent(string Status, string obj)
        {
            try
            {
                switch (Status)
                {
                    case "Config":
                        Controller.ParsingConfigXml(obj);
                        atopLog.WriteLog(atopLogMode.TestInfo, obj);
                        ControlServer.Client.Send("Config is received");
                        break;
                    case "Setup":
                        Controller.Setup(obj);
                        ControlServer.Client.Send("Harness is ready");
                        break;
                    case "Test":
                        ControlServer.Client.Send(obj + " is Start");
                        StartTest(obj);
                        ControlServer.Client.Send("Test is finally");
                        CopyLogFolder();
                        break;
                    case "LogFolder":
                        LogFolder = obj;
                        CopyLogFolder();
                        break;
                }
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
                ControlServer.Client.Send("Error : " + ex.Message);
            }
        }

        private void StartTest(string obj)
        {
            switch (obj.ToUpper())
            {
                case "RELIABILITY":
                    Controller.Frontend.Reliability();
                    break;
                case "POLLCONTROL":
                    Controller.Frontend.PollControl();
                    break;
                case "POLLCHANGE":
                    Controller.Frontend.PollChange();
                    break;
                case "POLLSTATIC":
                    Controller.Frontend.PollSataic();
                    break;
                default:
                    throw new Exception("command error : " + obj);
            }
        }

        private void CopyLogFolder()
        {
            try
            {
                if (LogFolder == null)
                    return;
                if (!Directory.Exists(LogFolder))
                    return;
                DirectoryCopy(Application.StartupPath + "/App_Data/Logs", LogFolder, true);
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, "Copy Folder fail: " + ex.Message);
            }
        }

        private void GetHarnessNetInfo()
        {
            using (StreamReader sr = new StreamReader(Application.StartupPath + " /SettingInfo.txt"))
            {
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.ToLower();
                    string[] lineArray = line.Split(',');
                    PCNetworkInfo Info = new PCNetworkInfo();
                    if (line.StartsWith("backend"))
                        Info.Type = EnumEnd.Backend;
                    else if (line.StartsWith("frontend"))
                        Info.Type = EnumEnd.Frontend;
                    else
                        continue;

                    Match M = Regex.Match(lineArray[0].Trim(), @"^[a-zA-Z]*(\d*)$");
                    Info.Index = int.Parse(M.Groups[1].Value);

                    foreach (var Para in lineArray)
                    {
                        string[] ParaInfo = Para.Split('=').Select(a => a.Trim()).ToArray();
                        switch (ParaInfo[0].ToLower())
                        {
                            case "ip":
                                Info.IPAddress = ParaInfo[1];
                                break;
                            case "port":
                                Info.Port = int.Parse(ParaInfo[1]);
                                break;
                        }
                    }
                    Controller.HarnessInfoList.Add(Info);
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ControlServer.Listener?.Stop();
        }

        private void btnClientController_Click(object sender, EventArgs e)
        {
            FormAutomation.Show();
        }

        private string GetLocalIP()
        {
            return "192.168.4.82";
            // 取得本機名稱
            String strHostName = Dns.GetHostName();

            // 取得本機的 IpHostEntry 類別實體
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            foreach (var IP in iphostentry.AddressList)
            {
                if (IP.AddressFamily == AddressFamily.InterNetwork)
                {
                    return IP.ToString();
                }
            }
            throw new Exception("Get local ip fail.");
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
