using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
 
namespace CashLib.Diagnostics
{
    public class ConsoleTraceListiner : TraceListener
    {

        public override void Write(string message)
        {
            Console.WriteToBacklog(message, false);
        }

        public override void WriteLine(string message)
        {
            Console.WriteToBacklog(message, true);
        }
    }
}
