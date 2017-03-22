using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashLib.Exceptions
{
    public abstract class CashLibException : Exception
    {
        public CashLibException()
        {
        }

        public CashLibException(string message)
            : base(message)
        {
        }

        public CashLibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
