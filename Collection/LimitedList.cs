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
using C5;

namespace CashLib.Collection
{
	/// <summary>
	/// This stores a limited number of items, De-queuing them automatically once the limit has been met.
	/// </summary>
	public class LimitedList<T> : ArrayBase<T>
	{
		private int _limit;
        private T _default;
		public int Limit{
            get{return _limit;}
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Value must be greater than 0");
                if (value == _limit)
                    return;
                if (value < _limit)
                {
                    Array.Copy(base.array, 0, base.array, 0, value);
                }
                else
                {
                    T[] newArray = new T[value];
                    Array.Copy(base.array, 0, newArray, 0, _limit);
                    for (int i = _limit; i < value; i++)
                        newArray[i] = _default;

                    base.array = newArray;
                }
                _limit = value;
                return;
            }
        }
		public LimitedList(int limit, T fillWith): base(limit - 1, EqualityComparer<T>.Default, MemoryType.Normal)
		{
			_limit = limit;
            _default = fillWith;
			for(int i = 0; i < limit; i++)
				base.array[i] = fillWith;
			base.size = limit;
		}
		
		public void Add(T item)
		{
			if (this.isReadOnlyBase)
			{
				throw new ReadOnlyCollectionException();
			}
			//Shift it down one, cutting off the last item
			Array.Copy(base.array, 0, base.array, 1, _limit - 1);
			base.array[0] = item;
		}

        public override System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return base.GetEnumerator();
        }
	}
}
