/*
 * Copyright 2012 Matthew Cash. All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 * 
 *    1. Redistributions of source code must retain the above copyright notice, this list of
 *          conditions and the following disclaimer.
 *          
 *    2. Redistributions in binary form must reproduce the above copyright notice, this list
 *          of conditions and the following disclaimer in the documentation and/or other materials
 *                provided with the distribution.
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
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using C5;
using CashLib;
using CashLib.Collection;
using CashLib.Localization;
using CashLib.Exceptions;


namespace CashLib
{
    public enum ConsoleCommandSucess
    {
        Sucess,
        Failure
    }

    public struct ConsoleFunction
    {
        public System.Func<string[], ConsoleResponce> Function;
        public string HelpInfo;
    }
    public struct ConsoleResponce
    {
        public string Value;
        public ConsoleCommandSucess Sucess;

        public static ConsoleResponce NewSucess(string data)
        {
            return new ConsoleResponce() { Sucess = ConsoleCommandSucess.Sucess, Value = data };
        }
        public static ConsoleResponce NewFailure(string data)
        {
            return new ConsoleResponce() { Sucess = ConsoleCommandSucess.Failure, Value = data };
        }
    }
    public struct ConsoleVarable
    {
        public string Value;
        public string HelpInfo;
        public Func<string, ExecutionState> ValidCheck;

        public override string ToString()
        {
            return Value;
        }


        public static ConsoleVarable OnOffVarable(string HelpInfo)
        {
            return new ConsoleVarable() { Value = "0", ValidCheck = ValidCheckMethod_0_1, HelpInfo = HelpInfo };
        }

        public static ExecutionState ValidCheckMethod_0_1(string t)
        {
            t = t.Trim();
            if (t == "0" || t == "1")
                return ExecutionState.Succeeded();
            return ExecutionState.Failed("Value must be 0 or 1.");
        }

    }



    /// <summary>
    /// This is the back end for the Console system.
    /// </summary>
    public static class TConsole
    {
        private static HashDictionary<string, ConsoleVarable> _varables;
        private static HashDictionary<string, ConsoleFunction> _functions;

        private static LimitedList<string> _consoleBacklog;
        private static string _backlogLine;


        public static void Init(int backlogLength = 500)
        {
            _varables = new HashDictionary<string, ConsoleVarable>();
            _functions = new HashDictionary<string, ConsoleFunction>();
            _consoleBacklog = new LimitedList<string>(backlogLength, string.Empty);
            _backlogLine = "";

            ConsoleFunction ConsoleHelp = new ConsoleFunction() {
                Function = ConsoleHelpFunc,
                HelpInfo = DefaultLanguage.Strings.GetString("Console_Help_Help")
            };
            
            SetFunc("help", ConsoleHelp);

        }

        static ConsoleResponce ConsoleHelpFunc(string[] data)
        {
            ConsoleResponce cr = new ConsoleResponce()
            {
                Sucess = ConsoleCommandSucess.Sucess,
                Value = ""
            };

            if (data.Length == 0)
            {
                foreach (string s in _functions.Keys)
                    cr.Value += s + "\n";
                foreach (string s in _varables.Keys)
                    cr.Value += s + "\n";
            }
            else
            {
                foreach (string s in _varables.Keys)
                    if (s == data[0])
                        cr.Value = _varables[s].HelpInfo;
                foreach (string s in _functions.Keys)
                    if (s == data[0])
                        cr.Value = _functions[s].HelpInfo;
            }
            
            return cr;
        }

        public static void WriteToBacklog(string message, bool newline)
        {
            if (!newline)
                _backlogLine += message;
            if (newline && _backlogLine == "")
                _consoleBacklog.Add(message);
            else
            {
                _consoleBacklog.Add(_backlogLine + message);
                _backlogLine = "";
            }
        }

        public static ConsoleVarable GetValue(string name)
        {
            lock (_varables)
                if (ValueContains(name))
                {
                    return _varables[name];
                }
            //this is called by normal code, so throw a normal exception!
            throw new InvalidVarableNameExceptions(string.Format("{0} does not exist", name));
        }


        public static void SetValue(string name, ConsoleVarable value)
        {
            lock (_varables)
                if (_varables.Contains(name))
                {
                    _varables[name] = value;
                }
                else
                {
                    _varables.Add(name, value);
                }
        }

        public static void SetIfNotExsistValue(string name, ConsoleVarable value)
        {
            lock (_varables)
                if (!_varables.Contains(name))
                {
                    _varables.Add(name, value);
                }
        }

        public static bool ValueContains(string name)
        {
            lock (_varables)
                return _varables.Contains(name);
        }

        public static ConsoleResponce ExecuteFunc(string name, params string[] args)
        {
            lock (_varables)
            {
                ConsoleResponce cr = new ConsoleResponce()
                {
                    Sucess = ConsoleCommandSucess.Failure,
                    Value = ""
                };

                try
                {
                    if (_functions.Contains(name))
                    {
                        return _functions[name].Function(args);
                    }
                }
                catch (ConsoleException ex)
                {
                    cr.Value = DefaultLanguage.Strings.GetFormatedString("Console_Function_Exception", ex);
                }
                return cr;
            }
        }



        public static void SetFunc(string name, ConsoleFunction func)
        {
            lock (_varables)
                if (_varables.Contains(name))
                {
                    _functions[name] = func;
                }
                else
                {
                    _functions.Add(name, func);
                }
        }

        public static bool FuncContains(string name)
        {
            lock (_varables)
                return _functions.Contains(name);
        }

        public static ConsoleResponce ProcessLine(string line)
        {
            line = line.Trim();
            lock (_varables)
            {
                ConsoleResponce cr = new ConsoleResponce()
                {
                    Sucess = ConsoleCommandSucess.Sucess,
                };
                
                if (line == "")
                {
                    cr.Value = "";
                    return cr;
                }

                string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string name = split[0], value = "";
                if (split.Length != 1)
                {
                    string[] sptmp = new string[split.Length - 1];
                    Array.Copy(split, 1, sptmp, 0, split.Length - 1);
                    split = sptmp;
                }
                else
                {
                    split = new string[0];
                }

                if (_varables.Contains(name) && split.Length == 0)
                {
                    cr.Value = string.Format("{0} = {1}", name, _varables[name]);
                    return cr;
                }
                value = String.Join(" ", split);
                if (_varables.Contains(name))
                {
                    if (value.StartsWith("=") || value.StartsWith("\\"))
                    {
                        value = value.Remove(0, 1);
                        value = value.Trim();
                    }
                    if (_varables[name].ValidCheck(value))
                    {
                        ConsoleVarable cv = _varables[name];
                        cv.Value = value;
                        _varables[name] = cv;
                        cr.Value = string.Format("{0} = {1}", name, _varables[name]);

                        return cr;
                    }
                    else
                    {
                        cr.Sucess = ConsoleCommandSucess.Failure;
                        cr.Value = DefaultLanguage.Strings.GetFormatedString("Console_Validation_Failure", name, value);
                        return cr;
                    }
                }
                if (_functions.Contains(name))
                {
                    return ExecuteFunc(name, ArgSplit(value, true));
                }

                cr.Sucess = ConsoleCommandSucess.Failure;
                cr.Value = DefaultLanguage.Strings.GetFormatedString("Console_Unknown_Varable", name);

                return cr;
            }
        }

        public static void ProcessFile(string fileName)
        {
            lock (_varables)
            {
                if (!File.Exists(fileName))
                    throw new FileNotFoundException("Could not find {0}", fileName);

                string line, varableName, VarableArguments;
                int pos;
                foreach (string l in File.ReadAllLines(fileName))
                {
                    line = l.Trim();
                    if (line.StartsWith("#")) continue;
                    if (!line.Contains(" ") && !line.Contains("\t")) continue;

                    //We need to stop at the first space or \t.
                    for (pos = 0; pos < line.Length; pos++)
                    {
                        if (line[pos] == ' ' || line[pos] == '\t')
                            break;
                    }

                    varableName = line.Substring(0, pos);

                    VarableArguments = line.Substring(pos);
                    VarableArguments = VarableArguments.Trim();
                    if (_varables.Contains(varableName))
                    {
                        if (_varables[varableName].ValidCheck != null && _varables[varableName].ValidCheck(VarableArguments).Sucess)
                        {
                            ConsoleVarable cv = _varables[varableName];
                            cv.Value = VarableArguments;
                            _varables[varableName] = cv;
                        }

                    }
                    else if (_functions.Contains(varableName))
                    {
                        var result = ExecuteFunc(varableName, ArgSplit(VarableArguments, true));
                        if (result.Sucess == ConsoleCommandSucess.Failure)
                            System.Diagnostics.Debug.WriteLine(result.Value);
                    }
                }

            }
        }

        private static string[] ArgSplit(string args, bool ignoreEmprt = false)
        {
            bool instring = false;
            int escape = 0;
            List<string> tmpArgs = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (char c in args)
            {
                if (escape > 0) escape--;
                if (c == ' ' && !instring)
                {
                    if (sb.Length != 0 && ignoreEmprt == true || ignoreEmprt == false)
                    {
                        tmpArgs.Add(sb.ToString());
                    }
                    sb = new StringBuilder();
                    continue;
                }
                if (c == '\\')
                {
                    escape += 2;
                    continue;
                }
                if (c == '"' && escape == 0)
                {
                    instring = !instring;
                    continue;
                }

                sb.Append(c);

            }

            if (sb.Length != 0 && ignoreEmprt == true || ignoreEmprt == false)
            {
                tmpArgs.Add(sb.ToString());
            }

            return tmpArgs.ToArray();
        }


        public static string GetBacklog(bool IncludeEmptyLines = true)
        {
            StringBuilder sb = new StringBuilder(_consoleBacklog.Limit);
            foreach (string s in _consoleBacklog)
            {
                if (!IncludeEmptyLines && s != String.Empty)
                    sb.AppendLine(s);
            }
            return sb.ToString();
        }

        public static void Write(string s)
        {
            Trace.Write(s);
        }

        public static void Write(string s, params string[] format)
        {
            Trace.Write(string.Format(s, format));
        }

        public static void WriteLine(string s)
        {
            Trace.WriteLine(s);
        }

        public static void WriteLine(string s, params string[] format)
        {
            Trace.WriteLine(string.Format(s, format));
        }




    }
}
