/*
    Copyright (c) Alice Cash 2017, 
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
using System.Threading;
using StormLib.Threading;

namespace StormLib.Tasks
{
    public class Scheduler: Invoker, IThreadTask
    {
        List<CronTask> _tasks;
        DateTime _nextTick = DateTime.Now.AddSeconds(1);


        public Scheduler(string name) : base("Scheduler: " + name)
        {
            _tasks = new List<CronTask>();
            DateTime now = DateTime.Now;
            _nextTick = new DateTime(now.Year, now.Month,now.Day,now.Hour,now.Minute, 0);
        }

        public void Start()
        { 

        }

        public void Stop(bool force)
        {

        }

        public void ReflectionFromFile(string filename)
        {


        }


       [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void AddTask(string input, Action command)
        {
            AddTask(new CronTask(input, command));
        }


        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void AddTask(string minute, string hour, string day, string month, string weekday, Action command)
        {
            AddTask(new CronTask(minute, hour, day, month, weekday, command));
        }

        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void AddTask(CronTask task)
        {
            this.InvokeMethod(() => { _tasks.Add(task); });
        }

        public void RunTask()
        {
            if(DateTime.Now >= _nextTick)
            {
                _nextTick = _nextTick.AddSeconds(1);
                foreach (CronTask task in _tasks)
                {
                    task.CheckTaskTime();
                }
            }
        }  
    }
}
