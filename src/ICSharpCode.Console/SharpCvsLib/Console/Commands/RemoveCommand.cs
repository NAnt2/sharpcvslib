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
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Remove file(s) in the cvs repository.
    /// </summary>
    public class RemoveCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private string fileNames;
        private string unparsedOptions;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(RemoveCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }
        /// <summary>
        /// Remove file(s) from a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="rmOptions">Options</param>
        public RemoveCommand(string cvsroot, string fileNames, string rmOptions) : 
            this(new CvsRoot(cvsroot), fileNames, rmOptions){
        }

        /// <summary>
        ///    Remove file(s) in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="rmOptions">Options</param>
        public RemoveCommand(CvsRoot cvsroot, string fileNames, string rmOptions) {
            this.cvsRoot = cvsroot;
            this.fileNames = fileNames;
            this.unparsedOptions = rmOptions;
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
            ICSharpCode.SharpCvsLib.Commands.RemoveCommand removeCommand;
            this.ParseOptions(this.unparsedOptions);
            try {
                String currentDirectory = Environment.CurrentDirectory;
                // Open the Repository file in the CVS directory
                Manager manager = new Manager(currentDirectory);
                Repository repository = manager.FetchRepository(currentDirectory); 
                // If this fails error out and state the user
                //    is not in a CVS repository directory tree.
                currentWorkingDirectory = new WorkingDirectory( this.cvsRoot,
                    currentDirectory, repository.FileContents);
                // Create new RemoveCommand object
                removeCommand = new ICSharpCode.SharpCvsLib.Commands.RemoveCommand(
                                 this.currentWorkingDirectory, currentDirectory, 
                                 new ICSharpCode.SharpCvsLib.FileSystem.Entry( currentDirectory, fileNames));
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            return removeCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="rmOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        /// <exception cref="NotImplementedException">If the command argument
        ///     is not implemented currently.  TODO: Implement the argument.</exception>
        private void ParseOptions (String rmOptions) 
        {
            for (int i = 0; i < rmOptions.Length; i++) {
                if (rmOptions[i]== '-' && rmOptions[i+1] == 'f') {
                    String msg = "The -f remove option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rmOptions[i]== '-' && rmOptions[i+1] == 'l') {
                    String msg = "The -l remove option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (rmOptions[i]== '-' && rmOptions[i+1] == 'R') {
                    String msg = "The -R remove option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
            }
        }
    }
}