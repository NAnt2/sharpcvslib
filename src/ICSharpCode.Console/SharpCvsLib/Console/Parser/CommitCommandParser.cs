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
using System.Globalization;
using System.Collections;
using System.IO;
using System.Text;

using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    /// Commit changes in the cvs repository.
    /// </summary>
    public class CommitCommandParser : AbstractCommandParser {
        private string fileNames;
        private string unparsedOptions;
        private string revision;
        private string logFile;
        private string message;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommitCommandParser () {

        }

        /// <summary>
        /// Commit changes to a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="ciOptions">Options</param>
        public CommitCommandParser(string cvsroot, string fileNames, string ciOptions) : 
            this(new CvsRoot(cvsroot), fileNames, ciOptions){
        }

        /// <summary>
        ///    Commit changes in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="ciOptions">Options</param>
        public CommitCommandParser(CvsRoot cvsroot, string fileNames, string ciOptions) {
            this.CvsRoot = cvsroot;
            this.fileNames = fileNames;
            this.unparsedOptions = ciOptions;
        }

        /// <summary>
        ///    Commit changes in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="ciOptions">Options</param>
        public CommitCommandParser(CvsRoot cvsroot, string[] ciOptions) {
            this.CvsRoot = cvsroot;
            this.Args = ciOptions;
        }

        /// <summary>
        /// Create a new instance of the <see cref="XmlLogCommandParser"/>.
        /// </summary>
        /// <returns></returns>
        public static ICommandParser GetInstance() {
            return GetInstance(typeof(CommitCommandParser));
        }

        /// <summary>
        /// Name of the command being parsed.
        /// </summary>
        public override string CommandName {
            get {return "commit";}
        }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public override string CommandDescription {
            get {return "Check files into the repository";}
        }

        /// <summary>
        /// Nicknames for the add command.
        /// </summary>
        public override ICollection Nicks {
            get {
                if (0 == commandNicks.Count) {
                    commandNicks.Add("ci");
                    commandNicks.Add("com");
                }
                return commandNicks;
            }
        }

/* Crap from the CommandLineParser

                        singleOptions = "DRcfln";
                        this.commandTxt = arguments[i];
                        i++;
                        // get rest of arguments which is options on the commit command.
                        while (arguments.Length > i && arguments[i].IndexOf("-", 0, 1) >= 0) {
                            LOGGER.Debug("Parsing arguments.  Argument[" + i + "]=[" + arguments[i]);
                            // Get options with second parameters?
                            if (arguments[i].IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0) {
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ) {
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            }
                            else {
                                this.options = this.options + arguments[i++];       // Yes
                                this.options = this.options + arguments[i] + " ";
                            }
                            i++;
                        }
                        if (arguments.Length > i) {
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.repository = arguments[i];
                        } 
                        else {
                            this.repository = String.Empty;
                        }
*/

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
            ICSharpCode.SharpCvsLib.Commands.CommitCommand2 commitCommand;
            this.ParseOptions(this.unparsedOptions);
            string cvsFolder = Path.Combine(Environment.CurrentDirectory, "CVS");

            if (fileNames == null || fileNames.Length == 0) {
                fileNames = Environment.CurrentDirectory;
            }

            FileParser parser = new FileParser(this.Args);

            CurrentWorkingDirectory.Folders = parser.Folders;
            // Create new CommitCommand2 object
            commitCommand = new ICSharpCode.SharpCvsLib.Commands.CommitCommand2(
                this.CurrentWorkingDirectory );

            // set public properties on the commit command
            if (message != null) {
                commitCommand.LogMessage = message;
            }
        
            return commitCommand;
        }

        private void GetFilesRecursive(DirectoryInfo dir, ArrayList files) {
            if (!(dir.Name.IndexOf("CVS") > -1)) {
                foreach (FileInfo file in dir.GetFiles()) {
                    files.Add(file);
                }

                foreach (DirectoryInfo dirInfo in dir.GetDirectories()) {
                    this.GetFilesRecursive(dirInfo, files);
                }
            }
        }

        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="ciOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        private void ParseOptions (String ciOptions) {
            int endofOptions = 0;
            for (int i = 0; i < this.Args.Length; i++) {
                string arg = this.Args[i];
                if (arg.IndexOf("-") > -1) {
                    switch (arg) {
                        case "-r":
                            revision = this.Args[++i];
                            break;
                        case "-F":
                            logFile = this.Args[++i];
                            break;
                        case "-m":
                            message = this.Args[++i];
                            break;
                        case "-c":
                            throw new NotImplementedException (
                                "The -c commit option is not implemented.");
                            break;
                        case "-D":
                            throw new NotImplementedException (
                                "The -D commit option is not implemented.");
                            break;
                        case "-f":
                            throw new NotImplementedException (
                                "The -f commit option is not implemented.");
                            break;
                        case "-l":
                            throw new NotImplementedException (
                                "The -l commit option is not implemented.");
                            break;
                        case "-n":
                            throw new NotImplementedException (
                                "The -n commit option is not implemented.");
                            break;
                        case "-R":
                            throw new NotImplementedException (
                                "The -R commit option is not implemented.");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Output the command usage and arguements.
        /// </summary>
        public override string Usage {
            get {
                string usage = 
@"Usage: cvs commit [-DnRlf] [-m msg | -F logfile] [-r rev] files...
    -D          Assume all files are modified.
    -n          Do not run the module program (if any).
    -R          Process directories recursively.
    -l          Local directory only (not recursive).
    -f          Force the file to be committed; disables recursion.
    -F logfile  Read the log message from file.
    -m msg      Log message.
    -r branch   Commit to specific branch or trunk.
    -c          Check for valid edits before committing.
(Specify the --help global option for a list of other help options)";

                return usage;
            }
        }
    }
}