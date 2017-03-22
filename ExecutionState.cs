/*
 * Copyright 2013 Matthew Cash. All rights reserved.
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

namespace CashLib
{
    /// <summary>
    /// This class is used for the Return value Functions or Methods, when the possibility of failure is there. 
    /// This class represents a Void Type.
    /// </summary>
    public struct ExecutionState
    {


        /// <summary>
        /// This returns a Succeeded result.
        /// </summary
        public static ExecutionState Succeeded() { return new ExecutionState(true); }
        /// <summary>
        /// This returns a Failed result.
        /// </summary>
        public static ExecutionState Failed()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            return new ExecutionState(false);
        }
        /// <summary>
        /// This returns a Succeeded result. It allows for a human readable string to be passed along.
        /// </summary
        public static ExecutionState Succeeded(string reason) { return new ExecutionState(true, reason); }
        /// <summary>
        /// This returns a Failed result. It allows for a human readable string to be passed along.
        /// </summary>
        public static ExecutionState Failed(string reason)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            return new ExecutionState(false, reason);
        }

        public bool Sucess;
        public string Reason;

        public ExecutionState(bool sucess)
        {
            Sucess = sucess;
            Reason = "";
        }
        public ExecutionState(bool sucess, string reason)
        {
            Sucess = sucess;
            Reason = reason;
        }

        public static bool operator true(ExecutionState exestate)
        {
            return exestate.Sucess;
        }
        public static bool operator false(ExecutionState exestate)
        {
            return !exestate.Sucess;
        }

        public static bool operator !(ExecutionState exestate)
        {
            return !exestate.Sucess;
        }
    }

    /// <summary>
    /// This class is used for the Return value of Functions or Methods, when the possibility of failure is there.
    /// This class allows a Result to be passed through it so it may replace non void types.
    /// </summary>
    public struct ExecutionState<T>
    {
        /// <summary>
        /// This returns a Succeeded result.
        /// </summary>
        public static ExecutionState<T> Succeeded(T result) { return new ExecutionState<T>(true, result); }
        /// <summary>
        /// This returns a Failed result, using the default of T as the result's value.
        /// </summary>
        public static ExecutionState<T> Failed()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            return new ExecutionState<T>(false, default(T));
        }
        /// <summary>
        /// This returns a Succeeded result. It allows for a human readable string to be passed along.
        /// </summary>
        public static ExecutionState<T> Succeeded(T result, string reason) { return new ExecutionState<T>(true, result, reason); }
        /// <summary>
        /// This returns a Failed result, using the default of T as the result's value. It allows for a human readable string to be passed along.
        /// </summary>
        public static ExecutionState<T> Failed(string reason)
        {
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            return new ExecutionState<T>(false, default(T), reason);
        }

        public bool Sucess;
        public T Result;
        public string Reason;

        public ExecutionState(bool sucess, T result)
        {
            Sucess = sucess;
            Result = result;
            Reason = "";
        }

        public ExecutionState(bool sucess, T result, string reason)
        {
            Sucess = sucess;
            Result = result;
            Reason = reason;
        }


        public static bool operator true(ExecutionState<T> exestate)
        {
            return exestate.Sucess;
        }
        public static bool operator false(ExecutionState<T> exestate)
        {
            return !exestate.Sucess;
        }

        public static bool operator !(ExecutionState<T> exestate)
        {
            return !exestate.Sucess;
        }
    }
}
