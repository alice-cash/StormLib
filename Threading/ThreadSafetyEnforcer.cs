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
using System.Threading;

namespace CashLib.Threading
{
    /// <summary>
    /// Manage and verify thread safety, throwing an exception when a violation has occurred.
    /// </summary>
    [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
    public sealed class ThreadSafetyEnforcer
    {
       
        readonly  bool _enforceThreadSafeCalls;
        int _safeThreadID;
        string _identifier;

        /// <summary>
        /// Returns if the instance is enforcing safe calls.
        /// </summary>
        public bool EnforcingThreadSafety
        {
            get { return _enforceThreadSafeCalls; }
        }

        /// <summary>
        /// Takes the thread ID used to create it and uses it when CheckThreadSafety and EnforceThreadSafety are called. EnforcingThreadSafety is enabled.
        /// </summary>
        /// <param name="identifier">This is used when throwing exceptions.</param>
        public ThreadSafetyEnforcer(string identifier)
            : this(identifier, true)
        {
        }

        /// <summary>
        /// Takes the thread ID used to create it and uses it when CheckThreadSafety and EnforceThreadSafety are called.
        /// </summary>
        /// <param name="identifier">This is used when throwing exceptions.</param>
        /// <param name="enforceThreadSafety">
        /// This enables or disables enforcement of thread safety in the EnforceThreadSafety method.
        /// When set to false, EnforceThreadSafety will never throw an exception.
        /// </param>
        public ThreadSafetyEnforcer(string identifier, bool enforceThreadSafety)
        {
            _enforceThreadSafeCalls = enforceThreadSafety;
            _safeThreadID = GetManagedThreadId();
            _identifier = identifier;

        }

        /// <summary>
        /// Throws an exception when the Thread Check fails.
        /// </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeEnforced)]
        public void EnforceThreadSafety()
        {
            if (_enforceThreadSafeCalls && !IsSameThread())
                throw new InvalidOperationException(string.Format("Cross-thread Access to '{0}' is not permitted from this scope.", _identifier));
        }

        /// <summary>
        /// Returns true if the current thread is the same as the Original thread.
        /// </summary>
        [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
        public bool IsSameThread()
        {
            return _safeThreadID == GetManagedThreadId();
        }

        /// <summary>
        /// Retrieve the current ThreadID.
        /// </summary>
        private int GetManagedThreadId()
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Changes the thread thread owner to the current thread.
        /// </summary>
        /// <remarks>This is only threadsafe when the old thread has invoked the new thread 
        /// to transfer Ownership. Even in the event a third thread reads it during a write, 
        /// it will be for an "IsSameThread" or "EnforceThreadSafty" call which will fail in 
        /// any case, thread 3 != Thread 1 or Thread 2.</remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
        public void ChangeThreadOwner()
        {
            _safeThreadID = GetManagedThreadId();
        }
    }
}
