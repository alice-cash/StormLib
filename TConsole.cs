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
    public enum ConsoleCommandState
    {
        Sucess,
        Failure
    }

    public struct ConsoleFunction
    {
        public System.Func<string[], ConsoleResponse> Function;
        public string HelpInfo;
        public delegate TabData GetTabCompletionValues(string line);
        public GetTabCompletionValues TabFunction;
    }
    public struct TabData
    {
        public bool Result { get; set; }
        public string Line { get; set; }
        public string[] TabStrings { get; set; }

        public static TabData Failue()
        {
            return new TabData() { Result = false };
        }
    }

    public struct ConsoleResponse
    {
        public string Value;
        public ConsoleCommandState State;

        public static ConsoleResponse NewSucess(string data)
        {
            return new ConsoleResponse() { State = ConsoleCommandState.Sucess, Value = data };
        }
        public static ConsoleResponse NewFailure(string data)
        {
            return new ConsoleResponse() { State = ConsoleCommandState.Failure, Value = data };
        }
        public static ConsoleResponse NewFailure()
        { 
            return new ConsoleResponse() { State = ConsoleCommandState.Failure, Value = "" };
        }
    }
    public struct ConsoleVarable
    {
        public string Value;
        public string HelpInfo;
        public Func<string, ExecutionState> ValidCheck;
        public delegate TabData GetTabCompletionValues(string line);
        public GetTabCompletionValues TabFunction;

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
                HelpInfo = DefaultLanguage.Strings.GetString("Console_Help_Help"),
                TabFunction = ConsoleHelpTab                
            };
            
            SetFunc("help", ConsoleHelp);

        }

        static TabData ConsoleHelpTab(string line)
        {
            string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string subLine = "";
            if (split.Length > 2) return TabData.Failue();
            if (split.Length == 2) subLine = split[1];

            TabData data = TabInput(subLine);
            data.Line = split[0] + " " + data.Line;
            return data;
        }

        static ConsoleResponse ConsoleHelpFunc(string[] data)
        {
            ConsoleResponse cr = new ConsoleResponse()
            {
                State = ConsoleCommandState.Sucess,
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

        public static ConsoleResponse GetValue(string name)
        {
            lock (_varables)
                if (ValueContains(name))
                {
                    return ConsoleResponse.NewSucess(_varables[name].Value);
                }
            return ConsoleResponse.NewFailure();
        }

        public static ConsoleVarable GetVariable(string name)
        {
            lock (_varables)
                if (ValueContains(name))
                {
                    return _varables[name];
                }
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

        public static ConsoleResponse ExecuteFunc(string name, params string[] args)
        {
            lock (_varables)
            {
                ConsoleResponse cr = new ConsoleResponse()
                {
                    State = ConsoleCommandState.Failure,
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

        public static TabData TabInput(string line)
        {
            string[] matches;
            lock (_varables)
            {
                line = line.TrimStart();
                if (line.Contains(" "))
                {
                    TabData subResponse;
                    string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string trimline = split[0];

                    //Get possible results from module. 
                    if (_varables.Contains(trimline))
                        if (_varables[trimline].TabFunction == null)
                            return TabData.Failue();
                        else
                            subResponse = _varables[trimline].TabFunction(line);
                    else if (_functions.Contains(trimline))
                        if (_functions[trimline].TabFunction == null)
                            return TabData.Failue();
                        else
                            subResponse = _functions[trimline].TabFunction(line);
                    else return TabData.Failue();

                    if (subResponse.Result == false)
                        return TabData.Failue();

                    line = subResponse.Line;

                    if (subResponse.TabStrings == null || subResponse.TabStrings.Length == 0)
                        return new TabData() { Result = true, Line = line };
                    if (subResponse.TabStrings.Length == 1)
                        return new TabData() { Result = true, Line = subResponse.TabStrings[0] };

                    matches = subResponse.TabStrings;
                }
                else
                {
                    SortedList<string> sortedMatches = new SortedList<string>(StringComparer.CurrentCultureIgnoreCase);
                    foreach (string name in _varables.Keys)
                        if (name.StartsWith(line))
                            sortedMatches.Add(name);
                    foreach (string name in _functions.Keys)
                        if (name.StartsWith(line))
                            sortedMatches.Add(name);

                    if (sortedMatches.Count == 0)
                        return new TabData() { Result = false };
                    if (sortedMatches.Count == 1)
                        return new TabData() { Result = true, Line = sortedMatches[0] }; ;

                    //Expand it out to match the closest match of any.
                    matches = sortedMatches.ToArray();
                }
            }

            int length = line.Length;

            char c;
            while (matches[0].Length > length)
            {
                //length is always 1 more than the index,
                //this means a length of 1 will attempt to grab an index of 2;
                c = matches[0][length];
                foreach (string entry in matches)
                {
                    if (entry.Length == length || entry[length] != c)
                        goto @break; //no point in continuing                        
                }
                line += c;
                length++;
            }
            @break:;

            return new TabData() { Result = true, Line = line, TabStrings = matches };

        }

        public static ConsoleResponse ProcessLine(string line)
        {
            line = line.Trim();
            lock (_varables)
            {
                ConsoleResponse cr = new ConsoleResponse()
                {
                    State = ConsoleCommandState.Sucess,
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
                    if (_varables[name].ValidCheck == null || _varables[name].ValidCheck(value).Sucess)
                    {
                        ConsoleVarable cv = _varables[name];
                        cv.Value = value;
                        _varables[name] = cv;
                        cr.Value = string.Format("{0} = {1}", name, _varables[name]);

                        return cr;
                    }
                    else
                    {
                        cr.State = ConsoleCommandState.Failure;
                        cr.Value = DefaultLanguage.Strings.GetFormatedString("Console_Validation_Failure", name, value);
                        return cr;
                    }
                } else if (_functions.Contains(name))
                {
                    return ExecuteFunc(name, ArgSplit(value, true));
                }

                cr.State = ConsoleCommandState.Failure;
                cr.Value = DefaultLanguage.Strings.GetFormatedString("Console_Unknown_Varable", name);

                return cr;
            }
        }

        public static void SaveToFile(string filename, string[] variables, bool isFullPath = false, bool hardfail = false)
        {
            if (!isFullPath)
                filename = Environment.CurrentDirectory + "/" + filename;
            lock (_varables)
            {
                FileStream fileHandle;
                if (!File.Exists(filename))
                {
                    try
                    {
                        fileHandle = File.Create(filename);
                    }
                    catch (Exception e)
                    {
                        if (hardfail)
                            throw new FileNotFoundException("Could not open file {0}.", e.ToString());
                        else
                        {
                            WriteLine("Could not open file {0}.", e.ToString());
                            return;
                        }
                    }
                } else {
                    fileHandle = File.OpenWrite(filename);
                }

                StreamWriter sw = new StreamWriter(fileHandle);
                ConsoleResponse currentValue;
                foreach (string name in variables)
                {
                    currentValue = GetValue(name);
                    if (currentValue.State == ConsoleCommandState.Failure)
                        if (hardfail)
                            throw new InvalidVarableNameExceptions(string.Format("{0} does not exist", name));
                        else
                            continue;
                    sw.WriteLine("{0}={1}", name, currentValue.Value);
                }
                sw.Flush();
                sw.Close();
            }
        }

        public static void ProcessFile(string filename, bool isFullPath = false, bool hardfail = false)
        {
            if (!isFullPath)
                filename = Environment.CurrentDirectory + "/" + filename;
            lock (_varables)
            {
                if (!File.Exists(filename))
                    if (hardfail)
                        throw new FileNotFoundException("Could not load file.", filename);
                    else
                    {
                        WriteLine("Could not find {0}.", filename);
                                return;
                    }
                WriteLine("Loading file {0}", filename);

                string line, varableName, VarableArguments;
                int pos;
                foreach (string l in File.ReadAllLines(filename))
                {
                    line = l.Trim();
                    if (line.StartsWith("#")) continue;
                    if (!line.Contains(" ") && !line.Contains("\t")) continue;

                    //We need to stop at the first space, \t, or =.
                    for (pos = 0; pos < line.Length; pos++)
                    {
                        if (line[pos] == ' ' || line[pos] == '\t' || line[pos] == '=')
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
                        if (result.State == ConsoleCommandState.Failure)
                            Debug.WriteLine(result.Value);
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
