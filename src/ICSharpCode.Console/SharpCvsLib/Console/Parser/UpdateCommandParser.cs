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
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.IO;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    /// Update modules in the cvs repository.
    /// </summary>
    public class UpdateCommandParser : AbstractCommandParser {
        private string fileNames;
        private string revision;
        private DateTime date;
        private string unparsedOptions;

        /// <summary>
        /// Create a new instance of the <see cref="UpdateCommandParser"/>.
        /// </summary>
        /// <returns></returns>
        public static ICommandParser GetInstance() {
            return GetInstance(typeof(UpdateCommandParser));
        }

        /// <summary>
        /// Name of the command being parsed.
        /// </summary>
        public override string CommandName {
            get {return "update";}
        }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public override string CommandDescription {
            get {return "Bring work tree in sync with repository";}
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UpdateCommandParser () {

        }

        /// <summary>
        /// Update module files from a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="fileNames">Files</param>
        /// <param name="upOptions">Options</param>
        public UpdateCommandParser(string cvsroot, string fileNames, string upOptions) : 
            this(new CvsRoot(cvsroot), fileNames, upOptions){
        }

        /// <summary>
        ///    Update modules or files in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="fileNames">Files</param>
        /// <param name="upOptions">Options</param>
        public UpdateCommandParser(CvsRoot cvsroot, string fileNames, string upOptions) {
            this.CvsRoot = cvsroot;
            this.fileNames = fileNames;
            this.unparsedOptions = upOptions;

            // HACK: This is just done until I can make this look like the other parsers
            this.Args = fileNames.Split(' ');
        }

        /// <summary>
        /// Nicknames for the add command.
        /// </summary>
        public override ICollection Nicks {
            get {
                if (0 == commandNicks.Count) {
                    commandNicks.Add("up");
                    commandNicks.Add("upd");
                }
                return commandNicks;
            }
        }

        /// <summary>
        /// The add command is implemented in the library and commandline parser.
        /// </summary>
        public override bool Implemented {
            get {return true;}
        }

        /// <summary>
        /// Create the command object that will be used to act on the repository.
        /// </summary>
        /// <returns>The command object that will be used to act on the
        ///     repository.</returns>
        /// <exception cref="Exception">TODO: Make a more specific exception</exception>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        public override ICommand CreateCommand () {
            DirectoryInfo dir = 
                new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "CVS"));

            UpdateCommand2 updateCommand;

            this.ParseOptions(this.unparsedOptions);

            FileParser parser = new FileParser(this.Args);
            CurrentWorkingDirectory.Folders = parser.Folders;
            // Create new UpdateCommand2 object
            updateCommand = new UpdateCommand2(CurrentWorkingDirectory);

            if (revision != null) {
                updateCommand.Revision = revision;
            }
            if (!date.Equals(DateTime.MinValue)) {
                updateCommand.Date = date;
            }

            return updateCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="upOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        private void ParseOptions (String upOptions) {
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
					i = i + endofOptions;
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
						i = i + endofOptions;
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
                    this.SetLocalDirectory(upOptions.Substring(i, endofOptions));
					i = i + endofOptions;
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
					i = i + endofOptions;
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
					i = i + endofOptions;
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
					i = i + endofOptions;
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
					i = i + endofOptions;
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

        /// <summary>
        /// Output the command usage and arguements.
        /// </summary>
        public override string Usage {
            get {
                string usage = 
@"Usage: cvs update [-APCdflRpbm] [-k kopt] [-r rev] [-D date] [-j rev]
    [-I ign] [-W spec] [files...]
        -A      Reset any sticky tags/date/kopts.
        -P      Prune empty directories.
        -C      Overwrite locally modified files with clean repository copies.
        -d      Build directories, like checkout does.
        -f      Force a head revision match if tag/date not found.
        -l      Local directory only, no recursion.
        -R      Process directories recursively.
        -p      Send updates to standard output (avoids stickiness).
        -k kopt Use RCS kopt -k option on checkout. (is sticky)
        -r rev  Update using specified revision/tag (is sticky).
        -D date Set date to update from (is sticky).
        -j rev  Merge in changes made between current revision and rev.
        -b      Perform -j merge from branch point.
        -m      Perform -j merge from last merge point (default).
        -I ign  More files to ignore (! to reset).
        -W spec Wrappers specification line.
        -3      Produce 3-way conflicts.
        -S      Use case insensitive update to select between conflicting names.
(Specify the --help global option for a list of other help options)";

                return usage;
            }
        }

    }
}