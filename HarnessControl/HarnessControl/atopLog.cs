using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using NLog;
namespace HarnessControl
{
    public static class atopLog
    {
        public static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Action<string> ShowMsg { get; set; }



        public static void WriteLog(atopLogMode LogMode, string Message)
        {
            ShowMsg(LogMode.ToString() + ":" + Message);
            switch (LogMode)
            {
                //case atopLogMode.TestFail:
                //    Display(ConsoleColor.Black, ConsoleColor.Red, Message);
                //    logger.Info(Message);
                //    break;
                //case atopLogMode.TestSuccess:
                //    Display(ConsoleColor.Black, ConsoleColor.Blue, Message);
                //    logger.Info(Message);
                //    break;
                //case atopLogMode.XelasCommandError:
                //    Display(ConsoleColor.Black, ConsoleColor.Yellow, Message);
                //    logger.Error(Message);
                //    break;
                //case atopLogMode.XelasCommandInfo:
                //    Display(ConsoleColor.Black, ConsoleColor.Blue, Message);
                //    logger.Info(Message);
                //    break;
                //case atopLogMode.SystemInformation:
                //    Display(ConsoleColor.Black, ConsoleColor.Blue, Message);
                //    logger.Info(Message);
                //    break;
                case atopLogMode.ProcessInfo:
                case atopLogMode.SocketInfo:
                    logger.Info(Message);
                    break;
                case atopLogMode.SystemError:
                    logger.Error(Message);
                    break;
            }
        }

        /// <summary>
        /// Display to Form
        /// </summary>
        /// <param name="Message"></param>
        public static void Display(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor, string Message)
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
            Console.WriteLine(Message);
        }
    }
}
