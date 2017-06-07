using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HarnessControl
{
    public class ProcessController
    {
        #region Harness
        //將視窗移動到最上層
        [DllImport("USER32.DLL")] //引用User32.dll
        public static extern bool SetForgroundWindow(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        public static extern bool CloseWindow(IntPtr hWnd);
        //尋找視窗
        [DllImport("USER32.DLL")] //引用User32.dll
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int SendMessage(
            IntPtr hWnd,   // handle to destination window
            int Msg,    // message
            int wParam, // first message parameter
            int lParam // second message parameter
        );

        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_CLOSE = 0x10;
        public Process HarnessProcess { get; set; }

        public void HarnessProcessStart(string ProgramName, string Parameter)
        {
            HarnessProcess = Process.Start(ProgramName, Parameter);
            CloseHarnessDialog();
        }

        private static void CloseHarnessDialog()
        {
            int Count = 100;
            int Action = 0;
            while (true)
            {
                Thread.Sleep(100);
                IntPtr player = IntPtr.Zero;
                switch (Action)
                {
                    case 0:
                        player = FindWindow("#32770", "Demo will Expire");
                        if (player != IntPtr.Zero)
                        {
                            SendMessage(player, SC_CLOSE, 0, 0);
                            Action = 1;
                        }
                        if (--Count > 0)
                        {
                            continue;
                        }
                        Action = 1;
                        Count = 50;
                        break;
                    case 1:
                        player = FindWindow("#32770", "Open Workspace");
                        if (player != IntPtr.Zero)
                        {
                            SendMessage(player, SC_CLOSE, 0, 0);
                            Action = -1;
                        }
                        if (--Count <= 0)
                        {
                            Action = -1;
                        }
                        break;
                    default:
                        return;
                }
            }
        }
        #endregion

        #region 61850
        
        public static string SendCommand_61850(string Command, string Shell_Command)
        {
            Process Carried_Out = new Process();
            ProcessStartInfo Carried_Out_Info = new System.Diagnostics.ProcessStartInfo(@"C:\cygwin64\bin\bash.exe");
            Carried_Out_Info.Arguments = Command;
            string Carried_Folder = Command_61850.GetCommandFolder(Shell_Command);
            Carried_Out_Info.WorkingDirectory = @"C:\cygwin64\opt\xelas\iec61850\client\" + Carried_Folder;
            Carried_Out_Info.RedirectStandardOutput = true;
            Carried_Out_Info.RedirectStandardError = true;
            Carried_Out_Info.UseShellExecute = false;
            Carried_Out.StartInfo = Carried_Out_Info;
            //執行程式
            Carried_Out.Start();
            string OutputData = Carried_Out.StandardOutput.ReadToEnd();
            //Console.WriteLine(OutputData);
            Carried_Out.WaitForExit();
            return OutputData;
        }

        #endregion 
    }
}
