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
    /// Add file(s) in the cvs repository.
    /// </summary>
    public class AddCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private string fileNames;
        private string localDirectory;
        private string unparsedOptions;
        private string message;
        private string kflag; // could be enumeration
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(AddCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }
        /// <summary>
        /// Add file(s) from a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="adOptions">Options</param>
        public AddCommand(string cvsroot, string fileNames, string adOptions) : 
            this(new CvsRoot(cvsroot), fileNames, adOptions){
        }

        /// <summary>
        /// Add file(s) in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="fileNames">Files to remove</param>
        /// <param name="adOptions">Options</param>
        public AddCommand(CvsRoot cvsroot, string fileNames, string adOptions) {
            this.cvsRoot = cvsroot;
            this.fileNames = fileNames;
            this.unparsedOptions = adOptions;
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
            ICSharpCode.SharpCvsLib.Commands.AddCommand addCommand;
            this.ParseOptions(this.unparsedOptions);
            try {
                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
                }
                currentWorkingDirectory = new WorkingDirectory( this.cvsRoot,
                    localDirectory, fileNames);
                // If fileNames has a wild card (*) like '*.txt'
                ICSharpCode.SharpCvsLib.FileSystem.Entry newEntry = 
                    new ICSharpCode.SharpCvsLib.FileSystem.Entry(fileNames, null);
                currentWorkingDirectory.AddEntry(localDirectory, newEntry);
                // Create new AddCommand object
                addCommand = new ICSharpCode.SharpCvsLib.Commands.AddCommand(
                                 this.currentWorkingDirectory);
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            return addCommand;
        }
 
        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="adOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (String adOptions) {
            int endofOptions = 0;
            for (int i = 0; i < adOptions.Length; i++) {
                if (adOptions[i]== '-' && adOptions[i+1] == 'm') {
                    i += 2;
                    // get message to attach to files 
                    if (adOptions.IndexOf(" -", i, adOptions.Length - i) == -1) {
                        endofOptions = adOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = adOptions.IndexOf(" -", i, adOptions.Length - i) - 2;
                    }
                    message = adOptions.Substring(i, endofOptions);
                }
                if (adOptions[i]== '-' && adOptions[i+1] == 'k') {
                    i += 2;
                    // get rcs-kflag to attach to files 
                    if (adOptions.IndexOf(" -", i, adOptions.Length - i) == -1) {
                        endofOptions = adOptions.Length - i - 1;
                    }
                    else {
                        endofOptions = adOptions.IndexOf(" -", i, adOptions.Length - i) - 2;
                    }
                    kflag = adOptions.Substring(i, endofOptions);
                }
            }
        }
    }
}