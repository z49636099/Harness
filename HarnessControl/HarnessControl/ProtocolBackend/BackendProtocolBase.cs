using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    public abstract class BackendProtocolBase
    {
        public abstract string AddPoint(string[] Para);
        public abstract string GetData(string[] Para);
        public abstract string SetData(string[] Para);
        public abstract string DeletePoint(string[] Para);
    }
}
