/*
 * Created by SharpDevelop.
 * User: Matthew
 * Date: 8/129/2010
 * Time: 3:45 AM
 * 
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
using C5;


namespace CashLib.Collection
{
	/// <summary>
	/// A sorted LinkedList.
	/// </summary>
	class SortedList<T>:LinkedList<T>
	{
		System.Collections.Generic.IComparer<T> _comparer;
		public SortedList(System.Collections.Generic.IComparer<T> comparer)
		{
			_comparer = comparer;
			base.FIFO = false;
		}
		
		public override void Enqueue(T item)
		{
			int pos = 0;
			while(pos+1 <= Count && _comparer.Compare(this[pos], item) < 0)pos++;
			base.Insert(pos, item);
		}
		
		public override bool Add(T item)
		{
			Enqueue(item);
			return true;
		}
		/*
		public void AddAll<U>(System.Collections.Generic.IEnumerable<U> items)
		{
			foreach(U item in items)
				Enqueue(item as T);
		}*/
		
        public override void Insert(int i, T item)
        {
            throw new NotSupportedException();
        }
		
        /*public override void InsertAll<U>(int i, System.Collections.Generic.IEnumerable<U> items)
		{
			throw new NotSupportedException();
		}*/
		
		public override void InsertFirst(T item)
		{
			throw new NotSupportedException();
		}
		
		public override void InsertLast(T item)
		{
			throw new NotSupportedException();
		}
		
		public override void Shuffle()
		{
			throw new NotSupportedException();
		}
		
		public override void Shuffle(Random rnd)
		{
			throw new NotSupportedException();
		}
		
		public override void Sort()
		{
			base.Sort(_comparer);
		}
		
		public override bool FIFO {
			get { return base.FIFO; }
			set { /*do nothing as it should always be false*/ }
		}
	}
}