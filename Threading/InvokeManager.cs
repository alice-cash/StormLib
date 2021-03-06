﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Threading
{
    ///  <summary>
    ///  Provides a managed way to process and transfer Invoker objects and offers thread transfer options.
    ///  </summary>
    public class InvokeManager: Invoker
    {
        private List<Invoker> _invokers;

        public InvokeManager(string name): base(name)
        {
            _invokers = new List<Invoker>();
        }


        ///  <summary>
        ///  Register an Invoker object, typically for a collection or group of objects.
        ///  </summary>
        ///  <param name="item"></param>
        public void RegisterInvoker(Invoker item)
        {
            InvokeMethod(() => { _invokers.Add(item); });
        }

        ///  <summary>
        ///  Performs a Synchronous call to the target Manager to transfer an Invoker. This blocks until the recieving thread can accept it.
        ///  </summary>
        ///  <param name="target"></param>
        ///  <param name="item"></param>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void GiveInvokerTo(InvokeManager target, Invoker item)
        {
            // If an invoke is not required for the target then it is running on the same thread as us.
            if(target.InvokeRequired())
                this.InvokeMethod(() => { SynchronousGiveInvokerTo(target, item); });
            else
            {
                // We can just reassign it since the thread runs both objects.
                target._invokers.Add(item);
                this._invokers.Remove(item);
            }
        }

        ///  <summary>
        ///  This performs a safe, Synchronous transfer between Managers.
        ///  </summary>
        ///  <param name="target"></param>
        ///  <param name="item"></param>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeSynchronous)]
        private void SynchronousGiveInvokerTo(InvokeManager target, Invoker item)
        {
            if (_invokers.Contains(item))
                target.SynchronousInvokeMethod(() => 
                {
                    target._invokers.Add(item);
                    this._invokers.Remove(item);
                    item.ChangeThreadOwner();
                });
            else
                throw new InvalidOperationException(string.Format("Invoker {0} is not owned by {1}.", item.ToString(), this.ToString()));
        }

        public override void PollInvokes()
        {
            base.PollInvokes();
            foreach (Invoker i in _invokers)
                i.PollInvokes();
        }

        [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
        internal override void ChangeThreadOwner()
        {
            foreach (Invoker item in _invokers)
                item.ChangeThreadOwner();
            base.ChangeThreadOwner();
        }
    }
}
