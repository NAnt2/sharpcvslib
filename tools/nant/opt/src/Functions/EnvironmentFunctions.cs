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
using System.Runtime.InteropServices;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Functions;

using Sporadicism.NAntExtras.Runner;

namespace Sporadicism.NAntExtras.Functions {
	[FunctionSet("Environment", "EnvironmentFunctions")]
	public class EnvironmentFunctions : FunctionSetBase {
		public EnvironmentFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
		}

        /// <summary>
        /// Start the given process.
        /// </summary>
        /// <param name="target">The name of the process to start.</param>
        /// <returns>The process id of the given process.</returns>
        [Function("SetVar")]
        public static string Start(string varName, string varValue) {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT ||
                Environment.OSVersion.Platform == PlatformID.Win32S ||
                Environment.OSVersion.Platform == PlatformID.Win32Windows) {
                SetEnvironmentVariable(varName, varValue);            
            } else {
                setenv(varName, varValue, 0);
            }
            return string.Format("{0}={1}", varName, varValue);
        }

        /// <summary>
        /// Code borrowed from NAnt <setenv/> task:
        /// 
        /// http://nant.sourceforge.net/nightly/latest/help/tasks/setenv.html
        /// 
        /// Win32 DllImport for the SetEnvironmentVariable function.
        /// </summary>
        /// <param name="lpName"></param>
        /// <param name="lpValue"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool SetEnvironmentVariable(string lpName, string lpValue);
                
        /// <summary>
        /// Code borrowed from NAnt <setenv/> task:
        /// 
        /// http://nant.sourceforge.net/nightly/latest/help/tasks/setenv.html
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        [DllImport("libc")]
        private static extern int setenv(string name, string value, int overwrite);
	}
}
