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
using System.Threading;

using Sporadicism.Builder.Runner;

namespace Sporadicism.Builder.ThreadHandler {
	/// <summary>
	/// Summary description for BrowserThread.
	/// </summary>
	public class StartThread {
        private string _target;
        private string _args;
        private Thread _thread;

        /// <summary>
        /// Start a ne thread on the given target.
        /// </summary>
        /// <param name="target"></param>
		public StartThread(string target) : this(target, null) {
		}

        /// <summary>
        /// Start a ne thread on the given target.
        /// </summary>
        /// <param name="target"></param>
        public StartThread(string target, string args) {
            this._target = target;
            this._args = args;
            this._thread = new Thread(new ThreadStart(Run));
            this._thread.Start();
        }

        private void Run () {
            if (null == this._args) {
                StartRunner.Instance.Run(this._target);
            } else {
                StartRunner.Instance.Run(this._target, this._args);
            }
        }
	}
}
