using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Tasks.Cron
{
    ///  <summary>
    ///  Represents the DAY field in a cron entry
    ///  </summary>
    ///  <remarks>
    ///  </remarks>
    internal class Day : CronField
    {
        public override int MinimumValue { get { return 1; } }

        public override int MaximumValue { get { return 31; } }

        public Day(string data)
        {
            Data = ProccessEntry(data);
        }
    }
}
