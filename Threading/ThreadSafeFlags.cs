using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashLib.Threading
{
    /// <summary>
    /// Flag state for the ThreadSafeAttribute class.
    /// </summary>
    [Flags]
    public enum ThreadSafeFlags
    {
        /// <summary>
        /// Function is thread safe and may or may not block calls.
        /// </summary>
        ThreadSafe,
        /// <summary>
        /// Function is not thread safe and will throw an exception if 
        /// called in an unsafe manor.
        /// </summary>
        ThreadSafeEnforced,
        /// <summary>
        /// Function is not thread safe and may have undefined behavior
        /// when called in an unsafe manor.
        /// </summary>
        ThreadUnsafe,
        /// <summary>
        /// Function is thread safe and Asynchronous, calls will not 
        /// block but may not run immediatly.
        /// </summary>
        ThreadSafeAsynchronous,
        /// <summary>
        /// Function is thread safe and is Synchronous, calls will block
        /// until the parent thread runs if called in an unsafe manor.
        /// </summary>
        ThreadSafeSynchronous
    }
}
