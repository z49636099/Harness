using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    public static class Global
    {
        public static string LocalIP { get; set; }

        public const int MainServerPort = 8000;

        #region 網路磁碟連線資訊
        public static string UncPath = @"\\10.0.0.187\Document\International Link\PG59XX\QA\eNode Database\MB-50\MBES-50EC\M61-P14-ed2-t5-server";
        public static string Domain = "localhost";        //網域
        public static string User = "binghuang";          //帳號
        public static string Passowrd = "104046";         //密碼
        #endregion

    }
}
