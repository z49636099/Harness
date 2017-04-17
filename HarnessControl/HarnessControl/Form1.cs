using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        HarnessTCPClient client = new HarnessTCPClient();
        HarnessController Controller = new HarnessController();

        ConfigSocketServer ControlServer = new ConfigSocketServer();
        private void Form1_Load(object sender, EventArgs e)
        {
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
                Task.Factory.StartNew(new Action(() => { ControlServer.Start(8000); }));
                GetHarnessNetInfo();
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
            }

        }

        private void ControlServer_ReceiveEvent(string obj)
        {
            try
            {
                Controller.ParsingConfig(obj);
                Controller.Setup();
                ControlServer.Client.Send("Harness is ready");
            }
            catch (Exception ex)
            {
                atopLog.WriteLog(atopLogMode.SystemError, ex.Message);
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
                    HarnessInfo Info = new HarnessInfo();
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
    }
}
