/*
 * Copyright 2013 Alice Cash. All rights reserved.
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
 * THIS SOFTWARE IS PROVIDED BY Alice Cash ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Alice Cash OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Alice Cash.
 */
using System;
using C5;

namespace StormLib.Collection
{
	/// <summary>
	/// This stores a limited number of items, De-queuing them automatically once the limit has been met.
	/// </summary>
	public class LimitedStack<T> : ArrayBase<T>
	{
		private int _limit;
        private int _index;

        public override int Count { get
            {
                return _index + 1;
            }
        }

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
                    if (_index + 1 > value) _index = value - 1;
                }
                else
                {
                    T[] newArray = new T[value];
                    Array.Copy(base.array, 0, newArray, 0, _limit);

                    base.array = newArray;
                }
                _limit = value;
                return;
            }
        }

		public LimitedStack(int limit): base(limit - 1, EqualityComparer<T>.Default, MemoryType.Normal)
		{
			_limit = limit;
			base.size = limit;
		}
		
		public void Add(T item)
		{
			if (this.isReadOnlyBase)
			{
				throw new ReadOnlyCollectionException();
			}

            if (_index < _limit - 1)
            {
                _index++;
                base.array[_index] = item;
            }
            else
            {
                // Shift it up one, cutting off the oldest item
                Array.Copy(base.array, 1, base.array, 0, _limit - 1);
                base.array[_index] = item;
            }
		}
        
        public T Pop()
        {
            if (_index == 0) throw new InvalidOperationException("Stack is empty!");
            T item = base.array[_index];
            // Shift it up one, cutting off the oldest item
            Array.Copy(base.array, 1, base.array, 0, _limit - 1);
            _index--;
            return item;
        }

        public void Push(T item)
        {
            Add(item);
        }

        public override System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return base.GetEnumerator();
        }
	}
}
