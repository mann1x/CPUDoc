using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUDoc
{
    public class RebootAppEventArgs : EventArgs
    {
        public RebootAppEventArgs(Reason reasonGiven) => this.ReasonGiven = reasonGiven;

        public Reason ReasonGiven { get; }
    }
}
