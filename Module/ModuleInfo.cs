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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;


namespace StormLib.Module
{
	///  <summary>
	///  Description of ModuleInfo.
	///  </summary>
	public static class ModuleInfo
	{
        ///  <summary>
        ///  Loads Modules in the assembly and optionally the SharedLibrary.
        ///  </summary>
		public static void LoadModules(Assembly LookIn, bool SharedLibrary)
		{
            LoadAssemblyModules(LookIn);
            if (SharedLibrary)
            {
                
                LoadAssemblyModules(Assembly.GetExecutingAssembly());
            }
		}

        public static void LoadModules()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asp in loadedAssemblies)
                LoadAssemblyModules(asp);
        }

        private static void LoadAssemblyModules(Assembly target)
        {
            //We simply use reflection to call a loading class for each module.
            foreach (Type type in target.GetTypes())
            {
                if (InterfaceTest(type))
                {
                    IModuleLoader ml = Activator.CreateInstance(type) as IModuleLoader;
                    if (ml == null) continue;
                    Debug.WriteLine(string.Format("Found Module \"{0}\", Version {1}", ml.Name, ml.Version));
                    ml.Load();
                }
            }
        }

        private static bool InterfaceTest(Type type)
        {
            foreach (Type @interface in type.GetInterfaces())
                if (@interface == typeof(IModuleLoader))
                    return true;
            return false;
        }
	}
}
