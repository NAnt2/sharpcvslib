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
//    <credit>Credit to Dick Grune, Vrije Universiteit, Amsterdam, for writing
//    the shell-script CVS system that this is based on.  In addition credit
//    to Brian Berliner and Jeff Polk for their work on the cvsnt port of
//    this work. </credit>
//    <author>Steve Kenzell</author>
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Text;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;

using ICSharpCode.SharpCvsLib.Console.Commands;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console {

    /// <summary>The main driver/ entry point into the program.</summary>
    public class ConsoleMain {
        private ILog LOGGER = LogManager.GetLogger(typeof(ConsoleMain));

        /// <summary>Constructor.
        ///     TODO: Fill in more of a usage/ explanation.</summary>
        public ConsoleMain () {
        }

        /// <summary>
        /// Driver for console application.
        ///
        /// TODO: Write a better description :-)
        /// </summary>
        public void Execute (String[] args) {
            CommandLineParser parser = new CommandLineParser (args);

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder();
                msg.Append(Environment.NewLine).Append("Using arguments:");
                foreach (String arg in args) {
                    msg.Append(Environment.NewLine).Append("\t arg1=[").Append(arg).Append("]");
                }
                LOGGER.Debug(msg);
            }
            ICommand command = null;
            try {
                command = parser.Execute ();
            } catch (CommandLineParseException e) {
                LOGGER.Debug (e);
                System.Console.WriteLine(Usage.General);
                return;
            }

            if (null != command) {
                // might need to move this up to the library, make working
                //  directory a public property??  Not sure.
                WorkingDirectory workingDirectory = parser.CurrentWorkingDirectory;

                string password = "";

                // Create CVSServerConnection object that has the ICommandConnection
                CVSServerConnection serverConn = new CVSServerConnection();
                try{
                    // try connecting with empty password for anonymous users
                    serverConn.Connect(workingDirectory, password);
                }
                catch (AuthenticationException eDefault){
                    LOGGER.Info("Authentication failed using empty password, trying .cvspass file.", eDefault);
                    try{
                        //string scrambledpassword;
                        // check to connect with password from .cvspass file
                        // check for .cvspass file and get password
                        //password = PasswordScrambler.Descramble(scrambledpassword);
                        serverConn.Connect(workingDirectory, password);
                    }
                    catch (AuthenticationException eCvsPass){
                        try {
                            LOGGER.Info("Authentication failed using .cvspass file, prompting for password.", eCvsPass);
                            // prompt user for password by using login command?
                            LoginCommand login = new LoginCommand(workingDirectory.CvsRoot);
                            serverConn.Connect(workingDirectory, login.Password);
                            throw eCvsPass;
                        } catch (AuthenticationException e) {
                            StringBuilder msg = new StringBuilder();
                            msg.Append("Fatal error, aborting.");
                            msg.Append("cvs [login aborted]: ")
                                .Append(workingDirectory.CvsRoot.User)
                                .Append(": unknown user or bad password.");
                            LOGGER.Error(msg, e);
                            System.Console.WriteLine(msg.ToString());
                            Environment.Exit(-1);
                        }
                    }
                }
                // Execute the command on cvs repository.
                command.Execute(serverConn);
                serverConn.Close();
            }
        }
    }
}
