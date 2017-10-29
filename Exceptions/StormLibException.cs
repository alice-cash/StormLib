using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Exceptions
{
    public abstract class StormLibException : Exception
    {
        public StormLibException()
        {
        }

        public StormLibException(string message)
            : base(message)
        {
        }

        public StormLibException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
