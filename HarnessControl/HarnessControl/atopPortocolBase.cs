using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    public abstract class atopPortocolBase
    {        
        public HarnessSession Session { get; set; }

        public event Action<string> StatusEvent;

        public void Reliability ()
        {
            /* 1 day */
            PollSataic();
            /* 3 day */
            PollControl();
            /* 5 day */
            PollChange();
        }

        public abstract void Patten(string PattenPath);

        public abstract void PollControl();
        public abstract void PollChange();
        public abstract void PollSataic();


        public void SendStatus(string StatusMessage)
        {
            StatusEvent?.Invoke(StatusMessage);
        }
    }


    public abstract class DNP3:atopPortocolBase
    {

    }
}
