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
        private string _branch;
        private string _logFile;
        private string _message;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommitCommandParser () {

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
        /// Create a new instance of the <see cref="CommitCommandParser"/>.
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
            this.ParseOptions();
            string cvsFolder = Path.Combine(Environment.CurrentDirectory, "CVS");

            FileParser parser = new FileParser(this.Args);

            CurrentWorkingDirectory.Folders = parser.Folders;
            // Create new CommitCommand2 object
            commitCommand = new ICSharpCode.SharpCvsLib.Commands.CommitCommand2(
                this.CurrentWorkingDirectory );

            // set public properties on the commit command
            commitCommand.LogMessage = _message;
            commitCommand.Branch = this._branch;
            if (null != this._logFile) {
                commitCommand.LogFile = new FileInfo(this._logFile);
            }
        
            return commitCommand;
        }

        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        public override void ParseOptions () {
            for (int i = 0; i < this.Args.Length; i++) {
                string arg = this.Args[i];
                if (arg.IndexOf("-") > -1) {
                    switch (arg) {
                        case "-r":
                            _branch = this.Args[++i];
                            break;
                        case "-F":
                            _logFile = this.Args[++i];
                            break;
                        case "-m":
                            _message = this.Args[++i];
                            break;
                        case "-c":
                            throw new NotImplementedException (
                                "The -c commit option is not implemented.");
                        case "-D":
                            throw new NotImplementedException (
                                "The -D commit option is not implemented.");
                        case "-f":
                            throw new NotImplementedException (
                                "The -f commit option is not implemented.");
                        case "-l":
                            throw new NotImplementedException (
                                "The -l commit option is not implemented.");
                        case "-n":
                            throw new NotImplementedException (
                                "The -n commit option is not implemented.");
                        case "-R":
                            throw new NotImplementedException (
                                "The -R commit option is not implemented.");
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