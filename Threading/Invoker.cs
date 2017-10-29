/*
 * Copyright 2012 Matthew Cash. All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 * 
 *    1. Redistributions of source code must retain the above copyright notice, this list of
 *       conditions and the following disclaimer.
 * 
 *    2. Redistributions in binary form must reproduce the above copyright notice, this list
 *       of conditions and the following disclaimer in the documentation and/or other materials
 *       provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY Matthew Cash ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Matthew Cash OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Matthew Cash.
 */
using System;
using System.Collections.Generic;

using System.Threading;

namespace StormLib.Threading
{
    ///  <summary>
    ///  The invoker class is used to Invoke methods across threads.
    ///  </summary>
    [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
    public class Invoker
    {
        ThreadSafetyEnforcer _threadSafety;

        [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
        Queue<InvokeItem> _invokeList;

        private class SynchronousLocker
        {

            bool locker;
            InvokeItem invokeTarget;

            public SynchronousLocker(System.Action<object> methodToInvoke, object userData)
            {
                invokeTarget = new InvokeItem(methodToInvoke, userData);
                locker = false;
            }

            public void Wait(int timeout)
            {
                DateTime dieTime = DateTime.Now.AddMilliseconds(timeout);
                while (!locker)
                {
                    System.Threading.Thread.Yield();
                    if (dieTime < DateTime.Now)
                        throw new InvalidProgramException("Thread invoker timeout! A thread may have been terminated or is stuck! This should not occur.");
                }
            }            

            public void InvokeProxy(object nulldata)
            {
                invokeTarget.Action(invokeTarget.UserData);
                locker = true;
                
            }
        }

        ///  <summary>
        ///  Creates a new instance of the Invoker Class, which is used to Invoke methods across threads.
        ///  </summary>
        ///  <param name="threadSafety">A pre-configured ThreadSafetyEnforcer.</param>
        ///  <exception cref="ArgumentNullException">threadSafety is null</exception>
        ///  <exception cref="ArgumentNullException">threadSafety is null</exception>
        public Invoker(ThreadSafetyEnforcer threadSafety)
        {
            if (threadSafety == null)
                throw new ArgumentNullException();
            if (!threadSafety.EnforcingThreadSafety)
                throw new ArgumentException("Invoker requires an enforcing Enforcer object.");

            _threadSafety = threadSafety;
            _invokeList = new Queue<InvokeItem>();
        }

        ///  <summary>
        ///  Creates a new instance of the Invoker Class, which is used to Invoke methods across threads. Thready Safety is enforced.
        ///  </summary>
        ///  <param name="Identifier">An Identifier for Exception calls.</param>
        public Invoker(String Identifier)
        {
            _threadSafety = new ThreadSafetyEnforcer(Identifier, true);
            _invokeList = new Queue<InvokeItem>();
        }

        ///  <summary>
        ///  Creates a new instance of the Invoker Class, which is used to Invoke methods across threads.  Thready Safety is enforced.
        ///  </summary>
        public Invoker()
        {
            _threadSafety = new ThreadSafetyEnforcer("Generic-StormLib.Threading.Invoker");
            _invokeList = new Queue<InvokeItem>();
        }

        ///  <summary>
        ///  Attempt to enqueue the provided method to be called by the parent thread.
        ///  </summary>
        ///  <param name="methodToInvoke">A method or delegate to call.</param>
        ///  <param name="data">An object with information sent to the method.</param>
        ///  <remarks>
        ///  This is an asynchronous and does not block. If the calling thread 
        ///  </remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void InvokeMethod(System.Action<object> methodToInvoke, object data)
        {
            lock (_invokeList)
            {
                _invokeList.Enqueue(new InvokeItem(methodToInvoke, data));
            }
        }

        ///  <summary>
        ///  Attempt to enqueue the provided method to be called by the parent thread.
        ///  </summary>
        ///  <param name="methodToInvoke">A method or delegate to call.</param>
        ///  <param name="userData">An object with information sent to the method.</param>
        ///  <remarks>
        ///  This is an asynchronous and does not block. If the calling thread 
        ///  </remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void InvokeMethod(System.Action methodToInvoke)
        {
            lock (_invokeList)
            {
                _invokeList.Enqueue(new InvokeItem((object e) => { methodToInvoke(); }, null));
            }
        }

        ///  <summary>
        ///  Attempt to enqueue the provided method to be called by the parent thread. This blocks
        ///  until the method is invoked. If called from the parent thread the method is executed immediately.
        ///  Timeout occurs after 2 minutes, after which an exception is thrown.
        ///  </summary>
        ///  <param name="methodToInvoke">A method or delegate to call.</param>
        ///  <param name="userData">An object with information sent to the method.</param>
        ///  <exception cref="InvalidProgramException">A InvalidProgramException is thrown when the application waits more than 2 minutes. 
        ///  In the event this occurs the parent thread may have been terminated or is stuck.</exception>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeSynchronous)]
        public void SynchronousInvokeMethod(System.Action<object> methodToInvoke, object userData)
        {
            SynchronousInvokeMethod(methodToInvoke, userData, (2 * 60 * 1000));
        }

        ///  <summary>
        ///  Attempt to enqueue the provided method to be called by the parent thread. This blocks
        ///  until the method is invoked. If called from the parent thread the method is executed immediately.
        ///  Timeout occurs after 2 minutes, after which an exception is thrown.
        ///  </summary>
        ///  <param name="methodToInvoke">A method or delegate to call.</param>
        ///  <exception cref="InvalidProgramException">A InvalidProgramException is thrown when the application waits more than 2 minutes. 
        ///  In the event this occurs the parent thread may have been terminated or is stuck.</exception>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeSynchronous)]
        public void SynchronousInvokeMethod(System.Action methodToInvoke)
        {
            SynchronousInvokeMethod((object e) => { methodToInvoke(); }, null, (2 * 60 * 1000));
        }

        ///  <summary>
        ///  Attempt to enqueue the provided method to be called by the parent thread. This
        ///  blocks until the method is invoked. Tf called from the parent thread the method is
        ///  executed immediately.
        ///  </summary>
        ///  <param name="methodToInvoke">A method or delegate to call.</param>
        ///  <param name="userData">An object with information sent to the method.</param>
        ///  <param name="timeout">Timeout in milliseconds.</param>
        ///  <exception cref="InvalidProgramException">A InvalidProgramException is thrown when
        ///  the application waits more than the provided timeout. In the event this occurs the
        ///  parent thread may have been terminated or is stuck.</exception>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeSynchronous)]
        public void SynchronousInvokeMethod(System.Action<object> methodToInvoke, object userData, int timeout)
        {
            if (InvokeRequired())
            {
                SynchronousLocker synchronousInvoke = new SynchronousLocker(methodToInvoke, userData);

                InvokeMethod(synchronousInvoke.InvokeProxy, synchronousInvoke);

                synchronousInvoke.Wait(timeout);
            }
            else
            {
                methodToInvoke(userData);
            }
        }

        ///  <summary>
        ///  Returns true if we need to invoke a method.
        ///  </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
        public bool InvokeRequired()
        {
            return !_threadSafety.IsSameThread();
        }

        ///  <summary>
        ///  Retrieves the current number of waiting items. The result is not guaranteed to be accurate due to cross thread access.
        ///  </summary>
        ///  <remarks>
        ///  This is ThreadSafe Enforced and should only be called from the parent thread. If this count is required by another thread that request should be invoked.
        ///  </remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeEnforced)]
        public int InvokeCount()
        {
            _threadSafety.EnforceThreadSafety();
            lock (_invokeList)
            {
                return _invokeList.Count;
            }
        }

        ///  <summary>
        ///  Retrieves the current number of waiting items which are causing blocks. The result is not guaranteed to be accurate due to cross thread access.
        ///  </summary>
        ///  <remarks>
        ///  This is ThreadSafe Enforced and should only be called from the parent thread. If this count is required by another thread that request should be invoked.
        ///  </remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeEnforced)]
        public int InvokeSyncCount()
        {
            _threadSafety.EnforceThreadSafety();
            int result = 0;
            lock (_invokeList)
            {
                foreach (InvokeItem item in _invokeList)
                    if (item.UserData is SynchronousLocker)
                        result++;
            }
            return result;
        }

        ///  <summary>
        ///  To be called by the parent thread, This checks the invoke list and runs any methods in it.
        ///  </summary>
        ///  <remarks>
        ///  This is ThreadSafe Enforced and should only be called from the parent thread.
        ///  </remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeEnforced)]
        public virtual void PollInvokes()
        {
            _threadSafety.EnforceThreadSafety();

            int count = 0;
            lock (_invokeList)
            {
                count = _invokeList.Count;
            }

            InvokeItem item;

            while (count > 0)
            {
                lock (_invokeList)
                {
                    // Ensure this hasn't changed to zero since our last lock. If so break.
                    // This should typically not happen unless PollInvokes is called from
                    // the delegated function below.
                    if (_invokeList.Count == 0)
                        break;
                    item = _invokeList.Dequeue();
                    count = _invokeList.Count;
                }

                item.Action(item.UserData);
            }
        }

        ///  <summary>
        ///  Transfers ownership of the object to a new thread, typically used by the InvokeManager when shifting workloads.
        ///  </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
        internal virtual void ChangeThreadOwner()
        {
            _threadSafety.ChangeThreadOwner();
        }
    }
}
