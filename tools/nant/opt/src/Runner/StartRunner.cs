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

namespace Sporadicism.NAntExtras.Runner {
	/// <summary>
	/// Summary description for StartRunner.
	/// </summary>
	public class StartRunner {

        /// <summary>
        /// Static accessor for the StartRunner instance.
        /// </summary>
        public static StartRunner Instance = new StartRunner();

		private StartRunner() {
		}

        /// <summary>
        /// Start the given executable target.
        /// </summary>
        /// <param name="target">The target file to start.</param>
        /// <returns></returns>
        public Process Run (string target) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo(target);
            process.StartInfo = startInfo;
            process.Start();
            return process;
        }
	}
}
