#region "Copyright"
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.
//
//  <author>Mike Krueger</author>
//  <author>Clayton Harbour</author>
//
#endregion

using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using ICSharpCode.SharpCvsLib.Attributes;
using ICSharpCode.SharpCvsLib.Config;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Protocols {
    /// <summary>
    /// Handle connect and authentication for the pserver protocol.
    /// </summary>
    [Author("Mike Krueger", "mike@icsharpcode.net", "2001")]
    [Author("Clayton Harbour", "claytonharbour@sporadicism.com", "2003-2005")]
    [Protocol("ext")]
    public class ExtProtocol : AbstractProtocol {
        private const string VERSION_ONE = "-1";
        private const string VERSION_TWO = "-2";

        private const string EnvCvsRsh = "CVS_RSH";

        private readonly ILog LOGGER =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Process _process = null;

        /// <summary>
        /// Create a new instance of the ext (ssh) protocol.
        /// </summary>
        public ExtProtocol() {
        }   

        /// <summary>
        /// Connect to the cvs server using the ext method.
        /// </summary>
        public override void Connect() {
            this.HandleExtAuthentication();
        }

        /// <summary>
        /// Disconnect from the cvs server.
        /// </summary>
        public override void Disconnect() {
            if (_process != null && !_process.HasExited) {
                _process.Kill();
                _process.WaitForExit();
                _process = null;
            }
        }


        private void HandleExtAuthentication () {
            if (null == _process) {
                _process = StartProcess();
            }

            BufferedStream errstream = new BufferedStream(_process.StandardError.BaseStream);
            StreamWriter streamWriter  = _process.StandardInput;
            StreamReader streamReader = _process.StandardOutput;

            SetInputStream(new CvsStream (streamReader.BaseStream));
            SetOutputStream(new CvsStream (streamWriter.BaseStream));
        }

        private Process StartProcess() {
            ProcessStartInfo startInfo =
                this.GetProcessInfo(this.Config.Shell, VERSION_ONE);

            Process process = new Process();
            try {
                process.StartInfo = startInfo;
                process.StartInfo.RedirectStandardError  = true;
                process.StartInfo.RedirectStandardInput  = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute        = false;

                process.Exited += new EventHandler(this.ExitShellEvent);

                LOGGER.Info(string.Format("{0} {1}",
                    process.StartInfo.FileName, process.StartInfo.Arguments));

                process.Start();
            } catch (Exception) {
                try {
                    process.StartInfo = this.GetProcessInfo(this.Config.Shell, VERSION_TWO);
                    process.StartInfo.RedirectStandardError  = true;
                    process.StartInfo.RedirectStandardInput  = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute        = false;

                    process.Start();
                } catch (Exception e) {
                    throw new ICSharpCode.SharpCvsLib.Exceptions.ExecuteShellException(
                        string.Format("{0} {1}",
                        this.Config.Shell, _process.StartInfo.Arguments), e);
                }
            }

            System.Threading.Thread.Sleep(100);

            return process;
        }

        private ProcessStartInfo GetProcessInfo (string program, string version) {
            string tProgram = Path.GetFileNameWithoutExtension(program);
            ProcessStartInfo startInfo;
            switch (tProgram) {
                case "plink": {
                    startInfo = this.GetPlinkProcessInfo(tProgram, version);
                    break;
                }
                case "ssh": {
                    startInfo = this.GetSshProcessInfo(tProgram, version);
                    break;
                }
                default:
                    throw new ArgumentException(string.Format("Unknown ssh program specified ( {0} )",
                        this.Config.Shell));
            }
            return startInfo;
        }

        private ProcessStartInfo GetPlinkProcessInfo (string program, string version) {
            StringBuilder processArgs = new StringBuilder ();
            processArgs.Append (string.Format(" -l \"{0}\"",
                this.Repository.CvsRoot.User));
            processArgs.Append(version);
            if (this.Password != null && this.Password != string.Empty) {
                processArgs.Append(string.Format(" -pw {0} ", this.Password));
            }
            processArgs.Append (" ").Append (this.Repository.CvsRoot.Host);
            processArgs.Append (" cvs server ");

            ProcessStartInfo startInfo =
                new ProcessStartInfo(program, processArgs.ToString ());

            return startInfo;
        }

        private ProcessStartInfo GetSshProcessInfo (string program, string version) {
            StringBuilder processArgs = new StringBuilder ();
            processArgs.Append (string.Format(" -l \"{0}\"",
                this.Repository.CvsRoot.User));
            processArgs.Append (" ").Append (this.Repository.CvsRoot.Host);
            processArgs.Append (" cvs server ");

            ProcessStartInfo startInfo =
                new ProcessStartInfo(program, processArgs.ToString());

            return startInfo;
        }

        private void ExitShellEvent(object sender, EventArgs e) {
            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug("Process EXITED");
            }

            if (_process.ExitCode != 0) {
                throw new AuthenticationException();
            }
        }

    }
}
