/*
    Copyright (c) Matthew Cash 2017, 
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CashLib.Tasks.Cron;

namespace CashLib.Tasks
{
    public class Task
    {
        /*
         * This works like a cron entry, e.g.
         *  ┌───────────── minute (0 - 59)
         *  │ ┌───────────── hour (0 - 23)
         *  │ │ ┌───────────── day of month (1 - 31)
         *  │ │ │ ┌───────────── month (1 - 12)
         *  │ │ │ │ ┌───────────── day of week (0 - 6) (Sunday to Saturday;
         *  │ │ │ │ │                                       7 is also Sunday)
         *  │ │ │ │ │
         *  │ │ │ │ │
         *  * * * * * 
         * 
         * */

        Minute minute;
        Hour hour;
        Day day;
        Month month;
        Weekday weekday;

        Action command;

        public Task(string input, Action command)
        {
            string[] data = input.Split(' ');
            if (data.Length != 5) throw new ArgumentException("Argument does not have 5 parts.");
            this.minute = new Minute(data[0]);
            this.hour = new Hour(data[1]);
            this.day = new Day(data[2]);
            this.month = new Month(data[3]);
            this.weekday = new Weekday(data[4]);
            this.command = command;
        }


        public void CheckTaskTime()
        {
            DateTime now = DateTime.Now;
            if( minute.Data.Contains(now.Minute) &&
                hour.Data.Contains(now.Hour) &&
                day.Data.Contains(now.Day) &&
                month.Data.Contains(now.Month) &&
                weekday.Data.Contains((int)now.DayOfWeek))
            {
                this.command();
            }
        }
    }
}
