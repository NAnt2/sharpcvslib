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
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    /// Add file(s) in the cvs repository.
    /// </summary>
    public class AddCommandParser : AbstractCommandParser {
        private string _message;
        private string _kflag; // could be enumeration

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AddCommandParser () {

        }

        /// <summary>
        ///    Commit changes in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="args">Options</param>
        public AddCommandParser(CvsRoot cvsroot, string[] args) {
            this.CvsRoot = cvsroot;
            this.Args = args;
        }

        /// <summary>
        /// Create a new instance of the <see cref="AddCommandParser"/>.
        /// </summary>
        /// <returns></returns>
        public static ICommandParser GetInstance() {
            return GetInstance(typeof(AddCommandParser));
        }

        /// <summary>
        /// Name of the command being parsed.
        /// </summary>
        public override string CommandName {
            get {return "add";}
        }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public override string CommandDescription {
            get {return "Add a new file/directory to the repository";}
        }

        /// <summary>
        /// Nicknames for the add command.
        /// </summary>
        public override ICollection Nicks {
            get {
                if (commandNicks.Count == 0) {
                    commandNicks.Add("ad");
                    commandNicks.Add("new");
                }

                return commandNicks;
            }
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
            ICSharpCode.SharpCvsLib.Commands.AddCommand addCommand;
            this.ParseOptions(this.Args);

            FileParser parser = new FileParser(this.Args);

            CurrentWorkingDirectory.Folders = parser.Folders;
            addCommand = new ICSharpCode.SharpCvsLib.Commands.AddCommand(
                this.CurrentWorkingDirectory);

            // set public properties on the commit command
            addCommand.Folders = parser.Folders;
            addCommand.Kflag = this._kflag;
            addCommand.Message = this._message;

            return addCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="adOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (string[] args) {
            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];
                if (arg.StartsWith("-")) {
                    switch (arg) {
                        case "-m":
                            string _message = args[++i];
                            break;
                        case "-k":
                            string _kflag = args[++i];
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
@"Usage: cvs add [-k rcs-kflag] [-m message] files...
        -k      Use ""rcs-kflag"" to add the file with the specified kflag.
        -m      Use ""message"" for the creation log.
(Specify the --help global option for a list of other help options)";

                return usage;
            }
        }
    }
}