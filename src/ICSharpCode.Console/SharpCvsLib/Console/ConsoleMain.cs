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
                // run the execute checkout command on cvs repository.
                command.Execute(serverConn);
                serverConn.Close();
            }

/*  This code is duplicated and I was not sure what should be moved to the 
 *      CommandLineParser and what was already there.
            switch (parser.Command){
            case "add":
            case "ad":
            case "new":
                break;
            case "checkout":
            case "co":
            case "get":
                CheckoutCommand coCommand =
                new CheckoutCommand(parser.Cvsroot, parser.Repository,
                                    parser.Options);
                coCommand.Execute();
                break;
            case "commit":
            case "ci":
            case "com":
                break;
            case "login":
            case "logon":
            case "lgn":
                // login to server
                LoginCommand login = new LoginCommand(parser.Cvsroot);
                login.Execute();
                break;
            case "passwd":
            case "password":
            case "setpass":
                // add to .cvspass file
                // scramble password
                // write to file
                break;
            case "remove":
            case "rm":
            case "delete":
                break;
            case "up":
            case "upd":
            case "update":
                UpdateCommand upCommand =
                    new UpdateCommand(parser.Cvsroot, parser.Repository,
                    parser.Options);
                upCommand.Execute();
                break;
            case "--help":
                System.Console.WriteLine (Usage.General);
                break;
            case "--help-options":
                System.Console.WriteLine (Usage.Options);
                break;
            case "--help-commands":
                System.Console.WriteLine (Usage.Commands);
                break;
            case "--help-synonyms":
                System.Console.WriteLine (Usage.Synonyms);
                break;
            default:
                System.Console.WriteLine ("Not a valid comand.");
                break;
            }
            System.Console.WriteLine ("Thanks for using the command line tool.");
            */
        }
        
    }
}
