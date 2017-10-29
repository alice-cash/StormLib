using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormLib.Threading
{
    ///  <summary>
    ///  Attribute indicating ThreadSafty of a Method, Constructor, Class or Interface.
    ///  </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property)]
    
    public class ThreadSafeAttribute : System.Attribute
    {
        public readonly ThreadSafeFlags Flags;

        private ThreadSafeAttribute()
        {

        }

        public ThreadSafeAttribute(ThreadSafeFlags Flags) 
        {
            this.Flags = Flags;
        }
        

        
    }
}
