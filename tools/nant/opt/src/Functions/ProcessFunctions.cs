#region "Copyright"
/*
The MIT License

Copyright (c) 2004-2005 Clayton Harbour

Permission is hereby granted, free of charge, to any person obtaining a copy of this 
software and associated documentation files (the "Software"), to deal in the Software 
without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.

 */
#endregion "Copyright"

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Functions;

using Sporadicism.NAntExtras.Runner;

namespace Sporadicism.NAntExtras.Functions {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[FunctionSet("Process", "ProcessFunctions")]
	public class ProcessFunctions : FunctionSetBase {
		public ProcessFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
		}

        /// <summary>
        /// Start the given process.
        /// </summary>
        /// <param name="target">The name of the process to start.</param>
        /// <returns>The process id of the given process.</returns>
        [Function("Start")]
        public static string Start(string target) {
            return Start(target, false);
        }

        /// <summary>
        /// Start the given process.
        /// </summary>
        /// <param name="target">The name of the process to start.</param>
        /// <param name="waitForExit"><see langword="true"/> if program execution should
        ///     wait for the given process to terminate.  Defaults to <see langword="false"/>.</param>
        /// <returns>The process id of the given process.</returns>
        [Function("Start")]
        public static string Start(string target, bool waitForExit) {
            Process process = StartRunner.Instance.Run(target);

            if (waitForExit) {
                process.WaitForExit();
            }

            try {
                FileInfo processPath;
                if (target.EndsWith(".lnk")) {
                    processPath = new FileInfo(ShortcutFunctions.GetTarget(target));
                } else {
                    processPath = 
                        new FileInfo(target);
                    processPath = new FileInfo(target);
                }
                return GetIds(Path.GetFileNameWithoutExtension(processPath.FullName));
            } catch (Exception) {
                return string.Format("Unable to find process id(s) for {0}",
                    target);
            }
        }

        /// <summary>
        /// Kill all processes with the given name.
        /// </summary>
        /// <param name="processName">Name of the processes to kill.</param>
        /// <returns>Count of the number of processes killed.</returns>
        [Function("Kill")]
        public static string Kill(string processName) {
            int killed = 0;
            foreach(Process process in Process.GetProcessesByName(processName)) {
                process.Kill();
                killed++;
            }
            return killed.ToString();
        }

        /// <summary>
        /// Kill process with the given id.
        /// </summary>
        /// <param name="id">Id of the process to kill.</param>
        /// <returns>Count of the number of processes killed, or in this case 1.</returns>
        [Function("KillId")]
        public static string KillId(int id) {
            Process process = Process.GetProcessById(id);
            process.Kill();
            return "1";
        }

        /// <summary>
        /// Get a comma seperated list of id's for the given process name.
        /// </summary>
        /// <param name="processName">Name of process to query.</param>
        /// <returns>A comma seperated list of processes.</returns>
        public static string GetIds(string processName) {
            return GetIds(processName, ",");
        }

        /// <summary>
        /// Get a comma seperated list of id's for the given process name.
        /// </summary>
        /// <param name="processName">Name of process to query.</param>
        /// <param name="delimiter">The delimeter to use for the return id's.</param>
        /// <returns>A comma seperated list of processes.</returns>
        public static string GetIds(string processName, string delimiter) {
            StringBuilder builder = new StringBuilder();
            foreach (Process process in Process.GetProcessesByName(processName)) {
                if (process.ProcessName == processName) {
                    if (builder.Length > 0) {
                        builder.Append(delimiter);
                    }
                    builder.Append(process.Id);         
                }
            }
            return builder.ToString();
        }
	}
}
