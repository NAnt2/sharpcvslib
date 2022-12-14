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
using System.IO;
using System.Text;

using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands{
    /// <summary>
    /// Check out module files from a cvs repository.
    /// </summary>
    public class CheckoutCommand{
        private CvsRoot cvsRoot;
        private string repository;
        private string revision;
        private string localDirectory;
        private DateTime date;

        private string unparsedOptions;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(CheckoutCommand));
        private WorkingDirectory currentWorkingDirectory;
        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }

        /// <summary>
        /// Check out module files from a cvs repository.
        /// </summary>
        /// <param name="cvsroot">User information</param>
        /// <param name="repositoryName">Repository</param>
        /// <param name="coOptions">Options</param>
        public CheckoutCommand(string cvsroot, string repositoryName, string coOptions) : 
            this(new CvsRoot(cvsroot), repositoryName, coOptions){
        }

        /// <summary>
        /// Create a new checkout command, initialize the variables that are used
        ///     in a checkout.
        /// </summary>
        /// <param name="cvsRoot">The cvs root to use for this checkout.</param>
        /// <param name="repositoryName">Name of the local repository path.</param>
        /// <param name="coOptions">All unparsed checkout options.</param>
        public CheckoutCommand (CvsRoot cvsRoot, string repositoryName, string coOptions) {
            this.cvsRoot = cvsRoot;
            repository = repositoryName;
            this.unparsedOptions = coOptions;
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
            CheckoutModuleCommand checkoutCommand;
            try {
                this.ParseOptions(this.unparsedOptions);
                // create CvsRoot object parameter
//FIXME: Local directory was not being set correctly.
//                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
//                }

                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder();
                    msg.Append(Environment.NewLine).Append("Creating working directory:");
                    msg.Append(Environment.NewLine).Append("\t cvsroot=[").Append(this.cvsRoot);
                    msg.Append(Environment.NewLine).Append("\t localDirectory=[").Append(this.localDirectory);
                    msg.Append(Environment.NewLine).Append("\t repository=[").Append(this.repository);

                    LOGGER.Debug(msg);
                }
                this.currentWorkingDirectory = new WorkingDirectory(this.cvsRoot,
                    localDirectory, repository);
                if (revision != null) {
                    this.currentWorkingDirectory.Revision = revision;
                }
                if (!date.Equals(DateTime.MinValue)) {
                    this.currentWorkingDirectory.Date = date;
                }
                // Create new CheckoutModuleCommand object
                checkoutCommand = new CheckoutModuleCommand(this.currentWorkingDirectory);
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            return checkoutCommand;
        }

        /// <summary>
        /// Parse the command line options/ arguments and populate the command
        ///     object with the arguments.
        /// </summary>
        /// <param name="coOptions">A string value that holds the command
        ///     line options the user has selected.</param>
        private void ParseOptions (String coOptions) {
            int endofOptions = 0;
            // get Checkout Options and parameters
            for (int i = 0; i < coOptions.Length; i++){
                if (coOptions[i]== '-' && coOptions[i+1] == 'r'){
                    i += 2;
                    // get revision of files to checkout
                    if (coOptions.IndexOf(" -", i, coOptions.Length - i) == -1){
                        endofOptions = coOptions.Length - i - 1;
                    }
                    else{
                        endofOptions = coOptions.IndexOf(" -", i, coOptions.Length - i) - 2;
                    }
                    revision = coOptions.Substring(i, endofOptions);
					i = i + endofOptions;
				}
                if (coOptions[i]== '-' && coOptions[i+1] == 'd'){
                    i += 2;
                    // get location to place files locally
                    if (coOptions.IndexOf(" -", i, coOptions.Length - i) == -1){
                        endofOptions = coOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else{
                        endofOptions = coOptions.IndexOf(" -", i, coOptions.Length - i) - 2;
                    }
                    localDirectory = coOptions.Substring(i, endofOptions);
					i = i + endofOptions;
				}
                if (coOptions[i]== '-' && coOptions[i+1] == 'D'){
                    i += 2;
                    // get date of files to checkout
                    // Date format needs to be the short date pattern as stated in the 
                    // Control Panel -> Regional Options -> see Date tab
                    if (coOptions.IndexOf(" -", i, coOptions.Length - i) == -1){
                        endofOptions = coOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else{
                        endofOptions = coOptions.IndexOf(" -", i, coOptions.Length - i) - 2;
                    }
                    try{
                        // Parse string to DateTime format
                        string datepar = coOptions.Substring(i, endofOptions);
						i = i + endofOptions;
						date = System.Convert.ToDateTime(datepar, DateTimeFormatInfo.CurrentInfo);
                    }
                    catch{
                        StringBuilder msg = new StringBuilder ();
                        msg.Append("The -D checkout option parameter is not ");
                        msg.Append("in correct format of ");
                        msg.Append(DateTimeFormatInfo.CurrentInfo.ShortDatePattern);
                        msg.Append(".");
                        throw new ApplicationException (msg.ToString());
                    }
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'A'){
                    String msg = "The -A checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'N'){
                    String msg = "The -N checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'P'){
                    String msg = "The -P checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'R'){
                    String msg = "The -R checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'c'){
                    String msg = "The -c checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'f'){
                    String msg = "The -f checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'l'){
                    String msg = "The -l checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'n'){
                    String msg = "The -n checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 'p'){
                    String msg = "The -p checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
                if (coOptions[i]== '-' && coOptions[i+1] == 's'){
                    String msg = "The -s checkout option is not  " +
                        "implemented.";
                    throw new NotImplementedException (msg);
                }
            }
        }
    }
}
