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
    /// Commit changes in the cvs repository.
    /// </summary>
    public class CommitCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private string repository;
        private string localDirectory;
        private string unparsedOptions;
        private string revision;
        private string logFile;
        private string message;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(CommitCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }
        /// <summary>
        /// Commit changes to a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="repository">Files to remove</param>
        /// <param name="ciOptions">Options</param>
        public CommitCommand(string cvsroot, string repository, string ciOptions) : 
            this(new CvsRoot(cvsroot), repository, ciOptions){
        }

        /// <summary>
        ///    Commit changes in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="repository">Files to remove</param>
        /// <param name="ciOptions">Options</param>
        public CommitCommand(CvsRoot cvsroot, string repository, string ciOptions) {
            this.cvsRoot = cvsroot;
            this.repository = repository;
            this.unparsedOptions = ciOptions;
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
            ICSharpCode.SharpCvsLib.Commands.CommitCommand2 commitCommand;
            try {
                this.ParseOptions(this.unparsedOptions);
                // set properties before creation of CommitCommand2
                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
                }
                currentWorkingDirectory = new WorkingDirectory( this.cvsRoot,
                    localDirectory, repository);
                // Create new CommitCommand2 object
                commitCommand = new ICSharpCode.SharpCvsLib.Commands.CommitCommand2(
                                 this.currentWorkingDirectory );
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            // set public properties on the commit command
            if (message != null) {
                commitCommand.LogMessage = message;
            }
         
            return commitCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="ciOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (String ciOptions) {
            int endofOptions = 0;
            for (int i = 0; i < ciOptions.Length; i++) {
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'r') {
                    i += 2;
                    // get revision of files to commit
                    if (ciOptions.IndexOf(" -", i, ciOptions.Length - i) == -1) {
                        endofOptions = ciOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = ciOptions.IndexOf(" -", i, ciOptions.Length - i) - 2;
                    }
                    revision = ciOptions.Substring(i, endofOptions);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'F') {
                    i += 2;
                    // get filename to get message from
                    if (ciOptions.IndexOf(" -", i, ciOptions.Length - i) == -1) {
                        endofOptions = ciOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = ciOptions.IndexOf(" -", i, ciOptions.Length - i) - 2;
                    }
                    logFile = ciOptions.Substring(i, endofOptions);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'm') {
                    i += 2;
                    // get message to attach to files 
                    if (ciOptions.IndexOf(" -", i, ciOptions.Length - i) == -1) {
                        endofOptions = ciOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = ciOptions.IndexOf(" -", i, ciOptions.Length - i) - 2;
                    }
                    message = ciOptions.Substring(i, endofOptions);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'c') {
                    String msg = "The -c commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'D') {
                    String msg = "The -D commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'f') {
                    String msg = "The -f commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'l') {
                    String msg = "The -l commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'n') {
                    String msg = "The -n commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (ciOptions[i]== '-' && ciOptions[i+1] == 'R') 
                {
                    String msg = "The -R commit option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
            }
        }
    }
}