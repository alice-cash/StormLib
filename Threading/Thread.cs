using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StormLib.Tasks;
using StormLib.Threading;

namespace StormLib.Threading
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
            thread.Name = name;
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

        ///  <summary>
        ///  Performs an asynchronous thread stop. 
        ///  Does not immediatly stop the thread, Gives the thread the specified seconds to terminate.
        ///  </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void Stop(int stopTimeout = 5)
        {
            this.InvokeMethod(() => { _threadRunning = false; SendStop(false); });
            WaitForStop(stopTimeout);
        }

        private void SendStop(bool force)
        {
            foreach (IThreadTask task in _tasks)
            {
                task.Stop(force);
            }
        }

        private void WaitForStop(int stopTimeout)
        {
            DateTime EndTime = DateTime.Now.AddSeconds(stopTimeout);
            while(DateTime.Now <= EndTime)
            {
                System.Threading.Thread.Yield();
                if (!thread.IsAlive)
                    return;
            }
            thread.Abort();
            SendStop(true);
        }

        private void ThreadLoop()
        {
            // We need to take control of our own thread enforcer before running.
            base.ChangeThreadOwner();

            _threadRunning = true;

            DateTime nextTick = DateTime.Now.AddSeconds(1);

            while (_threadRunning)
            {
                // handle invoked commands
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
