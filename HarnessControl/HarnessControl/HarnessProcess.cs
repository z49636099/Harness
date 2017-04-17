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
    public class HarnessProcess
    {
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

        public void ProcessStart(string ProgramName, string Parameter)
        {
            Process.Start(ProgramName, Parameter);
            CloseHarnessDialog();
        }

        private static void CloseHarnessDialog()
        {
            int Count = 50;
            int Action = 0;
            while (true)
            {
                Thread.Sleep(100);
                IntPtr player = IntPtr.Zero;
                switch (Action)
                {
                    case 0:
                        player = FindWindow("#32770", "Demo will Expire");
                        atopLog.WriteLog(atopLogMode.ProcessInfo, "Find windows:Demo will Expire");
                        if (player != IntPtr.Zero)
                        {
                            atopLog.WriteLog(atopLogMode.ProcessInfo, "Find windows:Demo will Expire success");
                            SendMessage(player, SC_CLOSE, 0, 0);
                            Action = 1;
                        }
                        break;
                    case 1:
                        player = FindWindow("#32770", "Open Workspace");
                        atopLog.WriteLog(atopLogMode.ProcessInfo, "Find windows:Open Workspace");
                        if (player != IntPtr.Zero)
                        {
                            atopLog.WriteLog(atopLogMode.ProcessInfo, "Find windows:Open Workspace success");
                            SendMessage(player, SC_CLOSE, 0, 0);
                            Action = -1;
                        }
                        if(--Count <=0)
                        {
                            Action = -1;
                        }
                        break;
                    default:
                        return;
                }
            }
        }



    }
}
