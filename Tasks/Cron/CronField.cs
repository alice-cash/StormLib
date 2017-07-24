/*
    Copyright (c) Matthew Cash 2017, 
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashLib.Tasks.Cron
{
    internal abstract class CronField
    {
        abstract public int MinimumValue { get; }
        abstract public int MaximumValue { get; }

        public int[] Data;

        protected int[] ProccessEntry(string data)
        {
            // We split by commas and process each as its own unit
            List<int> returnData = new List<int>();
            string[] split;
            if (data.Contains(','))
            {
                split = data.Split(',');
                foreach (string s in split)
                    returnData.AddRange(ProccessEntry(s));
            } else
            {
                int[] subrange;
                int divisor = 1;
                // Pull any divisior off
                if (data.Contains('/'))
                {
                    split = data.Split('/');
                    if (split.Length != 2) throw new ArgumentException("Entry contains more than 1 / symbol!");
                    divisor = int.Parse(split[1]);
                    data = split[0];
                }
                // Fill with either the one value or range of values
                if (data.Contains('-'))
                {
                    int min, max;
                    split = data.Split('/');
                    if (split.Length != 2) throw new ArgumentException("Entry contains more than 1 - symbol!");
                    min = int.Parse(split[0]);
                    max = int.Parse(split[1]);
                    subrange = GetRange(min, max);
                }
                else if (data == "*")
                    subrange = GetRange(MinimumValue, MaximumValue);
                else
                    subrange = new int[] { int.Parse(data) };

                // null out any values that don't match the divisor.
                if(divisor != 1)
                {
                    if (divisor < 1) throw new ArgumentException("Invalid divisor!");
                    for(int i = 0; i < subrange.Length; i++)
                    {
                        subrange[i] = subrange[i] % divisor == 0 ? subrange[i] : -1;
                    }
                }
                // Don't add any values which are outside the range
                for (int i = 0; i < subrange.Length; i++)
                {
                    if (subrange[i] > MinimumValue && subrange[i] < MaximumValue)
                        returnData.Add(subrange[i]);
                    
                }
            }
            return returnData.ToArray();
        }

        private int[] GetRange(int start, int end)
        {
            if (start > end) throw new ArgumentException("Range start is bigger than range end!");
            int[] returnData = new int[end - start];
            for (int i = start; i < end; i++)
                returnData[i - start] = start;
            return returnData;
        }
    }
}
