#region "Copyright"
//
// Copyright (C) 2003 Steve Kenzell
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
//    <author>Steve Kenzell</author>
#endregion
using System;
using System.Globalization;
using System.Text;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Remove file(s) in the cvs repository.
    /// </summary>
    public class RTagCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private string fileNames;
        private string localDirectory;
        private string revision;
        private DateTime date;
        private string unparsedOptions;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(RTagCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }
        /// <summary>
        /// RTags a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="rtOptions">Options</param>
        public RTagCommand(string cvsroot, string fileNames, string rtOptions) : 
            this(new CvsRoot(cvsroot), fileNames, rtOptions) {
        }

        /// <summary>
        ///    RTags in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="rtOptions">Options</param>
        public RTagCommand(CvsRoot cvsroot, string fileNames, string rtOptions) {
            this.cvsRoot = cvsroot;
            this.fileNames = fileNames;
            this.unparsedOptions = rtOptions;
        }

        /// <summary>
        /// Create the command object that will be used to act on the repository.
        /// </summary>
        /// <returns>The command object that will be used to act on the
        ///     repository.</returns>
        /// <exception cref="Exception">TODO: Make a more specific exception</exception>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        public ICommand CreateCommand () {
            ICSharpCode.SharpCvsLib.Commands.RTagCommand rtagCommand;
            try {
                this.ParseOptions(this.unparsedOptions);
                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
                }
                currentWorkingDirectory = new WorkingDirectory( this.cvsRoot,
                    localDirectory, fileNames);
                // Create new RemoveCommand object
                rtagCommand = new ICSharpCode.SharpCvsLib.Commands.RTagCommand(
                                 this.currentWorkingDirectory );
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            return rtagCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="rtOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (String rtOptions) {
            int endofOptions = 0;
            for (int i = 0; i < rtOptions.Length; i++) {
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'm') {
                    i += 2;
                    // get location to place files locally
                    if (rtOptions.IndexOf(" -", i, rtOptions.Length - i) == -1) {
                        endofOptions = rtOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else {
                        endofOptions = rtOptions.IndexOf(" -", i, rtOptions.Length - i) - 2;
                    }
                    localDirectory = rtOptions.Substring(i, endofOptions);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'r') {
                    i += 2;
                    // get revision of files to tag
                    if (rtOptions.IndexOf(" -", i, rtOptions.Length - i) == -1) {
                        endofOptions = rtOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else {
                        endofOptions = rtOptions.IndexOf(" -", i, rtOptions.Length - i) - 2;
                    }
                    revision = rtOptions.Substring(i, endofOptions);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'D') {
                    i += 2;
                    // get date of files for rtag
                    // Date format needs to be the short date pattern as stated in the 
                    // Control Panel -> Regional Options -> see Date tab
                    if (rtOptions.IndexOf(" -", i, rtOptions.Length - i) == -1) {
                        endofOptions = rtOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else {
                        endofOptions = rtOptions.IndexOf(" -", i, rtOptions.Length - i) - 2;
                    }
                    try {
                        // Parse string to DateTime format
                        string datepar = rtOptions.Substring(i, endofOptions);
                        date = System.Convert.ToDateTime(datepar, DateTimeFormatInfo.CurrentInfo);
                    }
                    catch {
                        StringBuilder msg = new StringBuilder ();
                        msg.Append("The -D rtag option parameter is not ");
                        msg.Append("in correct format of ");
                        msg.Append(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        msg.Append(".");
                        throw new ApplicationException (msg.ToString());
                    }
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'a') {
                    String msg = "The -a rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'b') {
                    String msg = "The -b rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'B') {
                    String msg = "The -B rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'd') {
                    String msg = "The -d rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'F') {
                    String msg = "The -F rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'f') {
                    String msg = "The -f rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'M') {
                    String msg = "The -M rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'l') {
                    String msg = "The -l rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'n') {
                    String msg = "The -n rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rtOptions[i]== '-' && rtOptions[i+1] == 'R') {
                    String msg = "The -R rtag option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
            }
        }
    }
}