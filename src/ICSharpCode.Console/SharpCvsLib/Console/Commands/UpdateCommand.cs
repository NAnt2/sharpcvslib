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

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Update modules in the cvs repository.
    /// </summary>
    public class UpdateCommand {

        private ICommand updateCommand;
        private WorkingDirectory workingDirectory;
        private string upcvsroot;
        private string repository;
        private string revision;
        private string localDirectory;
        private DateTime date;

        /// <summary>
        ///    Update modules or files in the cvs repository
        /// </summary>
        /// <param name="cvsroot">User Information</param>
        /// <param name="repositoryName">Repository</param>
        /// <param name="upOptions">Options</param>
        public UpdateCommand(string cvsroot, string repositoryName, string upOptions) {
            int endofOptions = 0;
            upcvsroot = cvsroot;
            repository = repositoryName;
            // get Checkout Options and parameters
            for (int i = 0; i < upOptions.Length; i++){
                if (upOptions[i]== '-' && upOptions[i+1] == 'r'){
                    i += 2;
                    // get revision of files to update
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1){
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else{
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'D'){
                    i += 2;
                    // get date of files to update
                    // Date format needs to be the short date pattern as stated in the 
                    // Control Panel -> Regional Options -> see Date tab
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1){
                        endofOptions = upOptions.Length - i - 1;  // minus one so not to
                        // include last space
                    }
                    else{
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    try{
                        // Parse string to DateTime format
                        string datepar = upOptions.Substring(i, endofOptions);
                        date = System.Convert.ToDateTime(datepar, DateTimeFormatInfo.CurrentInfo);
                    }
                    catch{
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
                    //revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'k') {
                    i += 2;
                    // get kopt
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1){
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else{
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    //revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'I'){
                    i += 2;
                    // get More file to ignore 
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1){
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else{
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    //revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'W'){
                    i += 2;
                    // get wrapper specification line
                    if (upOptions.IndexOf(" -", i, upOptions.Length - i) == -1){
                        endofOptions = upOptions.Length - i - 1;
                    }
                    else{
                        endofOptions = upOptions.IndexOf(" -", i, upOptions.Length - i) - 2;
                    }
                    //revision = upOptions.Substring(i, endofOptions);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'A'){
                    String msg = "The -A update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'P'){
                    String msg = "The -P update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'C'){
                    String msg = "The -C update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'f'){
                    String msg = "The -f update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'l'){
                    String msg = "The -l update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'R'){
                    String msg = "The -R update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'l'){
                    String msg = "The -l update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'p'){
                    String msg = "The -p update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'b'){
                    String msg = "The -b update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
                if (upOptions[i]== '-' && upOptions[i+1] == 'm'){
                    String msg = "The -m update option is not  " +
                        "implemented.";
                    throw new ApplicationException (msg);
                }
            }
        }

        /// <summary>
        /// Process the update command with cvs library API calls
        /// </summary>
        public void Execute () {
            string password = "";
            try 
            {
                // create CvsRoot object parameter
                CvsRoot root = new CvsRoot(upcvsroot);
                if (localDirectory == null) {
                    localDirectory = Environment.CurrentDirectory;
                }
                workingDirectory = new WorkingDirectory( root,
                    localDirectory, repository);
                if (revision != null) {
                    workingDirectory.Revision = revision;
                }
                // Create new UpdateCommand2 object
                updateCommand = new UpdateCommand2(workingDirectory);
            }
            catch {
            }
            // Create CVSServerConnection object that has the ICommandConnection
            CVSServerConnection serverConn = new CVSServerConnection();
            try {
                // try connecting with empty password for anonymous users
                serverConn.Connect(workingDirectory, password);
            }
            catch {
                try {
                    //string scrambledpassword;
                    // check to connect with password from .cvspass file
                    // check for .cvspass file and get password
                    //password = PasswordScrambler.Descramble(scrambledpassword);
                    serverConn.Connect(workingDirectory, password);
                }
                catch {
                    // prompt user for password by using login command?
                    LoginCommand login = new LoginCommand(upcvsroot);
                    serverConn.Connect(workingDirectory, login.Password);
                }
            }
            // run the execute checkout command on cvs repository.
            updateCommand.Execute(serverConn);
            serverConn.Close();
        }
    }
}