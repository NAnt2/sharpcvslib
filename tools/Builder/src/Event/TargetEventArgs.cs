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

namespace Sporadicism.Builder.Event {
	/// <summary>
	/// The <see cref="TargetEventArgs"/> class is used to provide feedback on the output
	/// or progress of a build target.
	/// </summary>
	public class TargetEventArgs : EventArgs {
        private string _target;
        private string _output;

        /// <summary>
        /// Target that was executed.
        /// </summary>
        public string Target {
            get { return this._target; }
        }

        /// <summary>
        /// The output of the build target.
        /// </summary>
        public string Output {
            get { return this._output; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">Target that was executed.</param>
        /// <param name="output">Output of the target.</param>
		public TargetEventArgs(string target, string output) {
            this._target = target;
            this._output = output;
		}
	}
}
