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
using System.IO;
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

        public CheckoutCommand(string cvsroot, string repositoryName){
            string password = "";
            try{
                // create CvsRoot object parameter 
                CvsRoot root = new CvsRoot(cvsroot);
                // need CvsRoot object and two strings to 
                //create new WorkingDirectory object parameter
                // CvsRoot cvsroot,
                // string localdirectory, < use current directory >
                //string repositoryname) < name of the module> example is sharpcvslib
                // ++++++++++++ process coOptions +++++++++++++++
                string localDirectory = Environment.CurrentDirectory;
                workingDirectory = new WorkingDirectory( root, 
                    localDirectory, repositoryName);
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
                    LoginCommand login = new LoginCommand(cvsroot);
                    serverConn.Connect(workingDirectory, login.Password);
                }
            }
            // run the execute method.
            getCommand.Execute(serverConn);
            serverConn.Close();
        }
    }
}
