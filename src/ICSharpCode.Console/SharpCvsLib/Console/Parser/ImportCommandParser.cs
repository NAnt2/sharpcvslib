#region "Copyright"
//
// Copyright (C) 2004 Clayton Harbour
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
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    /// Initialize the cvs repository.
    /// </summary>
    public class ImportCommandParser : AbstractCommandParser {
        private string[] unparsedOptions;

        private string message;
        private string vendor = "tcvs-vendor";
        private string release = "tcvs-release";
        private string branch;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImportCommandParser () {

        }

        /// <summary>
        /// Initialize a cvs command.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="args">Commandline arguments.</param>
        public ImportCommandParser(string cvsroot, string[] args) : 
            this(new CvsRoot(cvsroot), args){
        }

        /// <summary>
        /// Initialize a cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="args">Commandline arguments.</param>
        public ImportCommandParser(CvsRoot cvsroot, string[] args) {
            this.CvsRoot = cvsroot;
            this.Args = args;
        }

        /// <summary>
        /// Create a new instance of the <see cref="ImportCommandParser"/>.
        /// </summary>
        /// <returns></returns>
        public static ICommandParser GetInstance() {
            return GetInstance(typeof(ImportCommandParser));
        }

        /// <summary>
        /// Name of the command being parsed.
        /// </summary>
        public override string CommandName {
            get {return "import";}
        }

        /// <summary>
        /// Nicknames for the add command.
        /// </summary>
        public override ICollection Nicks {
            get {
                if (commandNicks.Count == 0) {
                    commandNicks.Add("import");
                    commandNicks.Add("im");
                    commandNicks.Add("imp");
                }

                return commandNicks;
            }
        }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public override string CommandDescription {
            get {return "Import sources into CVS, using vendor branches";}
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
            ImportModuleCommand importCommand;
            this.ParseOptions();
            Manager manager = new Manager(Environment.CurrentDirectory);
            this.CurrentWorkingDirectory = 
                new WorkingDirectory(this.CvsRoot,
                this.CurrentDir.FullName, this.Module);

            DirectoryInfo importDir = 
                new DirectoryInfo(Environment.CurrentDirectory);

            if (!importDir.Exists) {
                ConsoleMain.ExitProgram("Import directory does not exist.");
            }

            importCommand = 
                new ImportModuleCommand(CurrentWorkingDirectory, this.message);

            importCommand.VendorString = this.vendor;
            importCommand.ReleaseString = this.release;
            importCommand.LogMessage = this.message;
            return importCommand;
        }

        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        public override void ParseOptions () {
            int noDashIndex = 0;
            for (int i = 0; i < this.Args.Length; i++) {
                string arg = Args[i];
                if (arg.StartsWith("-")) {
                    switch (arg) {
                        case "-C":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-d":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-f":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-n":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-k":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-I":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                        case "-b":
                            this.branch = this.Args[++i];
                            break;
                        case "-m":
                            this.message = this.Args[++i];
                            break;
                        case "-W":
                            throw new NotImplementedException(string.Format("Argument not implemented {0}.", arg));
                    }
                } else {
                    if (0 == noDashIndex) {
                        this.Module = this.Args[i];
                        noDashIndex++;
                    } else if (1 == noDashIndex) {
                        this.vendor = this.Args[i];
                        noDashIndex++;
                    } else if (2 == noDashIndex) {
                        this.release = this.Args[i];
                        noDashIndex++;
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
@"Usage: cvs import [-C] [-d] [-f] [-k subst] [-I ign] [-m msg] [-b branch]
    [-W spec] [-n] repository [vendor-tag] [release-tags...]
        -C      Create CVS directories while importing.
        -d      Use the file's modification time as the time of import.
        -f      Overwrite existing release tags.
        -k sub  Set default RCS keyword substitution mode.
        -I ign  More files to ignore (! to reset).
        -b bra  Vendor branch id.
        -m msg  Log message.
        -W spec Wrappers specification line.
        -n      Don't create vendor branch or release tags.
(Specify the --help global option for a list of other help options)";
                return usage;
            }
        }
    }
}