#region "Copyright"
// Copyright (C) 2004 Mike Krueger, Clayton Harbour
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

using ICSharpCode.SharpCvsLib.Config;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Protocols
{
	/// <summary>
	/// Handle connect and authentication for the pserver protocol.
	/// </summary>
	public class ExtProtocol : AbstractProtocol {
        private readonly ILog LOGGER =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Process p = null;

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
            if (p != null && !p.HasExited) {
                p.Kill();
                p.WaitForExit();
                p = null;
            }
        }


        private void HandleExtAuthentication () {
            StringBuilder processArgs = new StringBuilder ();
            processArgs.Append ("-l ").Append (this.Repository.CvsRoot.User);
            processArgs.Append (" -q ");  // quiet
            processArgs.Append (" ").Append (this.Repository.CvsRoot.Host);
            processArgs.Append (" \"cvs server\"");

            try {

                ProcessStartInfo startInfo =
                    new ProcessStartInfo(this.Config.Shell, processArgs.ToString ());
                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder ();
                    msg.Append("Process=[").Append(this.Config.Shell).Append("]");
                    msg.Append("Process Arguments=[").Append(processArgs).Append("]");
                    LOGGER.Debug(msg);
                }
                startInfo.RedirectStandardError  = true;
                startInfo.RedirectStandardInput  = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute        = false;

                p = new Process();

                p.StartInfo = startInfo;
                p.Exited += new EventHandler(this.ExitShellEvent);
                p.Start();
            } catch (Exception e) {
                if (LOGGER.IsDebugEnabled) {
                    LOGGER.Debug(e);
                }
                throw new ExecuteShellException(this.Config.Shell + processArgs.ToString ());
            }
            BufferedStream errstream = new BufferedStream(p.StandardError.BaseStream);
            //inputstream  = new CvsStream(new BufferedStream(p.StandardOutput.BaseStream));
            //outputstream = new CvsStream(new BufferedStream(p.StandardInput.BaseStream));
            StreamWriter streamWriter  = p.StandardInput;
            StreamReader streamReader = p.StandardOutput;

            //streamWriter.AutoFlush = true;
            //streamReader.ReadToEnd ();
            //streamWriter.WriteLine(password);

            SetInputStream(new CvsStream (streamReader.BaseStream));
            SetOutputStream(new CvsStream (streamWriter.BaseStream));
        }

        private void ExitShellEvent(object sender, EventArgs e) {
            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug("Process EXITED");
            }

            if (p.ExitCode != 0) {
                throw new AuthenticationException();
            }
        }

	}
}
