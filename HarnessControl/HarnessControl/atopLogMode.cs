using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarnessControl
{
    public enum atopLogMode
    {
        /// <summary></summary>
        SocketInfo,

        SystemError,

        ProcessInfo,

        TestFail,

        TestSuccess,
        
        TestInfo,
    }
}
