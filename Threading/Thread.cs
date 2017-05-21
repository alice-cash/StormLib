using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CashLib.Tasks;
using CashLib.Threading;

namespace CashLib.Threading
{
    public class Thread : InvokeManager
    {

        List<IThreadTask> _tasks;

        System.Threading.Thread thread;
        bool _threadRunning = false;
        public bool ThreadRunning { get { return _threadRunning; } }

        public Thread(string name) : base("Thread: " + name)
        {
            _tasks = new List<IThreadTask>();
            thread = new System.Threading.Thread(ThreadLoop);
        }

        [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
        public void Start()
        {
            thread.Start();
        }

        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void AddTask(IThreadTask task)
        {
            this.InvokeMethod(() => { _tasks.Add(task); task.Start(); });
        }

        /// <summary>
        /// Performs an asynchronous thread stop. 
        /// Does not immediatly stop the thread, the thread finishes executing its current task.
        /// </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void Stop()
        {
            this.InvokeMethod(() => { _threadRunning = false; SendStop(); });
        }

        private void SendStop()
        {
            foreach (IThreadTask task in _tasks)
            {
                task.Stop();
            }
        }

        private void ThreadLoop()
        {
            //We need to take control of our own thread enforcer before running.
            base.ChangeThreadOwner();

            _threadRunning = true;

            DateTime nextTick = DateTime.Now.AddSeconds(1);

            while (_threadRunning)
            {
                //Handle invoked commands
                this.PollInvokes();

                foreach(IThreadTask task in _tasks)
                {
                    task.RunTask();
                }

                System.Threading.Thread.Yield();
            }
        }
    }
}
