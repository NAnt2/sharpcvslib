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
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft.MSDNMag.AsyncUtils;

using Sporadicism.Builder.Event;

namespace Sporadicism.Builder.Runner {
	/// <summary>
	/// Summary description for ProcessRunner.
	/// </summary>
    public class NAntRunner : AsyncOperation {
        /// <summary>
        /// Output message event.
        /// </summary>
        public event TargetEventHandler TargetOutputEvent;

        private static FileInfo _nantPath;
        private string _target;
        private Process _process;
        private int sleepTime = 50;
        private bool _verbose;
        private bool _debug;

        private static readonly string slash = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// NAnt target to execute.
        /// </summary>
        public string Target {
            get { return this._target; }
            set { this._target = value; }
        } 

        /// <summary>
        /// Used to toggle debug (more verbose) output from NAnt.  Corresponds to the
        /// <code>-debug</code> property on NAnt.
        /// </summary>
        public bool Debug {
            get { return this._debug; }
            set { this._debug = value; }
        }

        /// <summary>
        /// Used to toggle the verbose output from NAnt.  Corresponds to the 
        /// <code>-verbose</code> property on NAnt.
        /// </summary>
        public bool Verbose {
            get { return this._verbose; }
            set { this._verbose = value; }
        }

        public static FileInfo NAntPath {
            get {
                if (null == _nantPath) {
                    _nantPath = new FileInfo(Directory.GetCurrentDirectory() + slash +
                        ".." + slash + ".." + slash + ".." + slash + ".." + slash + "build" + slash + "nant.bat");
                    if (!_nantPath.Exists) {
                        string root = Path.GetPathRoot(Directory.GetCurrentDirectory());
                        _nantPath = Find(new DirectoryInfo(root), "nant.bat");
                    }
                }
                return _nantPath;
            }
        }
  
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="syncInvoke"></param>
		public NAntRunner(ISynchronizeInvoke syncInvoke) : base(syncInvoke) {
		}

        public static string ProjectHelp(FileInfo buildFile) {
            Process process = new Process();
            process.StartInfo.FileName = NAntPath.FullName;
            process.StartInfo.WorkingDirectory = NAntPath.Directory.FullName;
            process.StartInfo.Arguments = 
                string.Format("-buildfile:{0} -projecthelp", buildFile.FullName);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();

            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();
            return output;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="syncInvoke"></param>
        /// <param name="target"></param>
        public NAntRunner(ISynchronizeInvoke syncInvoke, string target) : this(syncInvoke) {
            this._target = target;
        }

        private static FileInfo Find(DirectoryInfo currentDir, string fileName) {
            try {
                foreach (FileInfo file in currentDir.GetFiles(fileName)) {
                    return file;
                }
            } catch (Exception) {
                // ignore security exceptions
            }


            foreach (DirectoryInfo dir in currentDir.GetDirectories()) {
                Find(dir, fileName);
            }
            return null;
        }

        /// <summary>
        /// Implement the asynchronous <see cref="DoWork"/> method.
        /// </summary>
        protected override void DoWork () {
            this.StartProcess();

            // Wait for the process to end, or cancel it
            while (! _process.HasExited) {
                Thread.Sleep(sleepTime); // sleep
                if (CancelRequested) {
                    // Not a very nice way to end a process,
                    // but effective.
                    _process.Kill();
                    AcknowledgeCancel();
                }
            }
        }

        public void Stop() {
            this.Cancel();
        }

        private void StartProcess() {
            _process = new Process();
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.CreateNoWindow = true;

            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;

            _process.StartInfo.FileName = NAntPath.FullName;
            _process.StartInfo.WorkingDirectory = NAntPath.DirectoryName;

            string args = string.Empty;
            if (this.Verbose) {
                args += "-verbose ";
            }

            if (this.Debug) {
                args += "-debug ";
            }
            args += this.Target;

            _process.StartInfo.Arguments = args;

            _process.Start();

            new MethodInvoker(ReadStandardOutput).BeginInvoke(null, null);
        }

        private void ReadStandardOutput() {
            string line;
            while ((line = _process.StandardOutput.ReadLine()) != null) {
                FireAsync(TargetOutputEvent, this, new TargetEventArgs(this._target, line));
            }
        }
	}
}
