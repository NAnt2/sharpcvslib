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
using System.IO;
using System.Text;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;

namespace ICSharpCode.SharpCvsLib.Console.Commands{

/// <summary>
/// Check out module files from a cvs repository.
/// </summary>
public class CheckoutCommand{
    private ICommand getCommand;
    private WorkingDirectory workingDirectory;
    private string cocvsroot;
    private string repository;
    private string revision;
    private string localDirectory;
    private DateTime date;

    /// <summary>
    /// Check out module files from a cvs repository.
    /// </summary>
    /// <param name="cvsroot">User information</param>
    /// <param name="repositoryName">Repository</param>
    /// <param name="coOptions">Options</param>
    public CheckoutCommand(string cvsroot, string repositoryName, string coOptions){
        int endofOptions = 0;
        cocvsroot = cvsroot;
        repository = repositoryName;
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
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'N'){
                String msg = "The -N checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'P'){
                String msg = "The -P checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'R'){
                String msg = "The -R checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'c'){
                String msg = "The -c checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'f'){
                String msg = "The -f checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'l'){
                String msg = "The -l checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'n'){
                String msg = "The -n checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 'p'){
                String msg = "The -p checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
            if (coOptions[i]== '-' && coOptions[i+1] == 's'){
                String msg = "The -s checkout option is not  " +
                    "implemented.";
                throw new ApplicationException (msg);
            }
        }
    }
    /// <summary>
    /// Process the checkout command with cvs library API calls
    /// </summary>
    public void Execute () {
        string password = "";
        try{
            // create CvsRoot object parameter
            CvsRoot root = new CvsRoot(cocvsroot);
            if (localDirectory == null){
                localDirectory = Environment.CurrentDirectory;
            }
            workingDirectory = new WorkingDirectory( root,
                               localDirectory, repository);
            if (revision != null){
                workingDirectory.Revision = revision;
            }
            // Create new CheckoutModuleCommand object
            getCommand = new CheckoutModuleCommand(workingDirectory);
        }
        catch{
        }

        // Create CVSServerConnection object that has the ICommandConnection
        CVSServerConnection serverConn = new CVSServerConnection();
        try{
            // try connecting with empty password for anonymous users
            serverConn.Connect(workingDirectory, password);
        }
        catch{
            try{
                //string scrambledpassword;
                // check to connect with password from .cvspass file
                // check for .cvspass file and get password
                //password = PasswordScrambler.Descramble(scrambledpassword);
                serverConn.Connect(workingDirectory, password);
                }
                catch{
                    // prompt user for password by using login command?
                    LoginCommand login = new LoginCommand(cocvsroot);
                    serverConn.Connect(workingDirectory, login.Password);
                }
            }
            // run the execute checkout command on cvs repository.
            getCommand.Execute(serverConn);
        serverConn.Close();
    }
}
}
