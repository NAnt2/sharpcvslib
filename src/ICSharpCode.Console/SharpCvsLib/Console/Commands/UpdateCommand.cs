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
    /// Update modules in the cvs repository.
    /// </summary>
    public class UpdateCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private string repository;
        private string revision;
        private string localDirectory;
        private DateTime date;
        private string unparsedOptions;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(UpdateCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }
        /// <summary>
        /// Update module files from a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="repositoryName">Repository</param>
        /// <param name="upOptions">Options</param>
        public UpdateCommand(string cvsroot, string repositoryName, string upOptions) : 
            this(new CvsRoot(cvsroot), repositoryName, upOptions){
        }

        /// <summary>
        ///    Update modules or files in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="repositoryName">Repository</param>
        /// <param name="upOptions">Options</param>
        public UpdateCommand(CvsRoot cvsroot, string repositoryName, string upOptions) {
            this.cvsRoot = cvsroot;
            repository = repositoryName;
            this.unparsedOptions = upOptions;
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
            UpdateCommand2 updateCommand;
            try {
                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
                }
                currentWorkingDirectory = new WorkingDirectory( this.cvsRoot,
                    localDirectory, repository);
                if (revision != null) {
                    currentWorkingDirectory.Revision = revision;
                }
                if (date.Equals(null)) {
                    currentWorkingDirectory.Date = date;
                }
                // Create new UpdateCommand2 object
                updateCommand = new UpdateCommand2(this.currentWorkingDirectory);
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            this.ParseOptions(updateCommand, this.unparsedOptions);
         
            return updateCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="updateCommand">A update command that is to be
        ///     populated.</param>
        /// <param name="upOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (ICommand updateCommand, String upOptions) {
            int endofOptions = 0;
            for (int i = 0; i < upOptions.Length; i++) {
                if (upOptions[i]== '-' && upOptions[i+1] == 'r') {
                    i += 2;
                    // get revision of files to update
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) { 
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'D') {
                    i += 2;
                    // get date of files to update
                    // Date format needs to be the short date pattern as stated in the 
                    // Control Panel -> Regional Options -> see Date tab
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    try {
                        // Parse string to DateTime format
                        string datepar = upOptions.Substring(i, endofOptions);
                        date = System.Convert.ToDateTime(datepar, DateTimeFormatInfo.CurrentInfo);
                    }
                    catch {
                        StringBuilder msg = new StringBuilder ();
                        msg.Append("The -D update option parameter is not ");
                        msg.Append("in correct format of ");
                        msg.Append(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        msg.Append(".");
                        throw new ApplicationException (msg.ToString());
                    }
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'd') {
                    i += 2;
                    // get revision of files 
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    localDirectory = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'j') {
                    i += 2;
                    // get revision of files 
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    // Set revision attribute for update command
                    //revisionTo = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'k') {
                    i += 2;
                    // get kopt
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    // Set kopt attribute for update command
                    //????? = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'I') {
                    i += 2;
                    // get More file to ignore 
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    //set attribute for this command 
                    //????? = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'W') {
                    i += 2;
                    // get wrapper specification line
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1) {
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    //revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'A') {
                    String msg = "The -A update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'P') {
                    String msg = "The -P update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'C') {
                    String msg = "The -C update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'f') {
                    String msg = "The -f update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'l') {
                    String msg = "The -l update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'R') {
                    String msg = "The -R update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'l') {
                    String msg = "The -l update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'p') {
                    String msg = "The -p update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'b') {
                    String msg = "The -b update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'm') {
                    String msg = "The -m update option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
            }
        }
    }
}