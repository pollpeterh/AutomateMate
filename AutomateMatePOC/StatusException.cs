using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomateMatePOC
{
    class StatusException : Exception
    {
        new string Message;
        bool Critical;

        public StatusException(string message, bool critical)
        {
            Message = message;
            Critical = critical;
        }
    }
}
