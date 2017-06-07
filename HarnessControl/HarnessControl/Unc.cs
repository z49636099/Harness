using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HarnessControl
{
    public class Unc : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct USE_INFO_2
        {
            internal string ui2_local;
            internal string ui2_remote;
            internal string ui2_password;
            internal UInt32 ui2_status;
            internal UInt32 ui2_asg_type;
            internal UInt32 ui2_refcount;
            internal UInt32 ui2_usecount;
            internal string ui2_username;
            internal string ui2_domainname;
        }

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern UInt32 NetUseAdd(string UncServerName, UInt32 Level, ref USE_INFO_2 Buf, out UInt32 ParmError);

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern UInt32 NetUseDel(string UncServerName, string UseName, UInt32 ForceCond);

        private string _UncPath;
        private int _LastError;
        private bool _IsConnect;

        public bool Connect(string UncPath, string Domain, string User, string Password)
        {
            try
            {
                var useinfo = new USE_INFO_2
                {
                    ui2_remote = UncPath,
                    ui2_domainname = Domain,
                    ui2_username = User,
                    ui2_password = Password,
                    ui2_asg_type = 0,
                    ui2_usecount = 1
                };
                _UncPath = UncPath;
                uint parmError;
                uint Code = NetUseAdd(null, 2, ref useinfo, out parmError);
                _LastError = (int)Code;
                _IsConnect = (Code == 0);
                return _IsConnect;
            }
            catch
            {
                _LastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                uint Code = NetUseDel(null, _UncPath, 2);
                _LastError = (int)Code;
                return (Code == 0);
            }
            catch
            {
                _LastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        public bool IsConnect
        {
            get { return _IsConnect; }
        }

        public int LastError
        {
            get { return _LastError; }
        }

        public void Dispose()
        {
            if (_IsConnect)
                Disconnect();
            GC.SuppressFinalize(this);
        }
    }
}
