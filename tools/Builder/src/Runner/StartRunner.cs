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

namespace Sporadicism.Builder.Runner {
	/// <summary>
	/// Provides a quick, easy way to start a new process.
	/// </summary>
	public class StartRunner {

        /// <summary>
        /// Static accessor for the <see cref="StartRunner"/> class.
        /// </summary>
        public static StartRunner Instance = new StartRunner();

		private StartRunner() {
		}

        /// <summary>
        /// Create a new process that will handle the given target.
        /// </summary>
        /// <param name="target">This can either be an application or an associated file
        ///     type such as <code>.jpg</code> or <code>.html</code>.  The default 
        ///     application will be launched to handle the request.</param>
        public void Run (string target) {
            System.Diagnostics.Process.Start(target);
        }

        /// <summary>
        /// Launch the given target and pass in the args specified.
        /// </summary>
        /// <param name="target"></param>
        public void Run (string target, string args) {
            System.Diagnostics.Process.Start(target, args);
        }

	}
}
