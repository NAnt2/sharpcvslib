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

using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;

using ICSharpCode.SharpCvsLib.Console.Commands;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    ///     Parse the command line parameters and create a new Console
    ///     command object for the current parameters passed in.
    /// </summary>
    public class CommandLineParser {
        private readonly ILog LOGGER = LogManager.GetLogger(typeof (CommandLineParser));

        private String[] arguments;

        private string cvsroot;
        private string command;
        private string options;
        private string repository;
        private string singleOptions;

        private WorkingDirectory currentWorkingDirectory;
        /// <summary>
        /// Get the current working directory, parsed from the command line.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }

            /// <summary>
            /// Value of the cvsroot to use as a string.  This will be passed
            ///     into the CvsRoot object which will know how to parse it.
            /// </summary>
            public String Cvsroot {
            get {return this.cvsroot;}
        }

        /// <summary>
        /// The text value of the command that will be executed.  This should be
        ///     translated into one of the public API command objects.
        /// </summary>
        public String Command {
            get {return this.command;}
        }

        /// <summary>
        /// Option to pass into the command.
        ///
        /// TODO: There may need to be an options collection to handle options,
        ///     either that or handle them as an attribute of the individual commands...
        /// </summary>
        public String Options {
            get {return this.options;}
        }

        /// <summary>
        /// Value of the repository to use as a string.  This will be passed
        ///     into the CheckoutCommand object which will know which files to get.
        /// </summary>
        public String Repository {
            get {return this.repository;}
        }

        /// <summary>Create a new instance of the command line parser and
        ///     initialize the arguments object.</summary>
        /// <param name="args">A collection of strings that represent the command
        ///     line arguments sent into the program.</param>
        public CommandLineParser (String[] args) {
            this.arguments = args;

            // TODO: Remove this hack when add method to set options.
            this.options = String.Empty;
        }

        /// <summary>
        ///      Parse the command line options.
        /// </summary>
        /// <returns>A command object from the library which will be used to 
        ///     access the repsository.</returns>
        /// <exception cref="CommandLineParseException">If there is a problem
        ///     parsing the command line arguments (i.e. if invalid arguments
        ///     are entered.</exception>
        public ICommand Execute () {
            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append("\n Command line arguments:");
                foreach (String argument in this.arguments) {
                    msg.Append("\n\t argument=[").Append(argument).Append("]");
                }
                LOGGER.Debug(msg);
            }
            // TODO: Remove = null when all other code paths return a value,
            //      this was just put in so it would compile.
            ICommand command = null;
            if (arguments.Length < 1) {
                System.Console.WriteLine (Usage.General);
            }

            for (int i = 0; i < arguments.Length; i++) {
                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder ();
                    msg.Append("arguments[").Append(i).Append("]=[").Append(arguments[i]).Append("]");
                    LOGGER.Debug(msg);
                }
                if (arguments[i].IndexOf ("-d", 0, 2) >= 0) {
                    cvsroot = arguments[i].Substring (2);
                    i++;
                    if (arguments.Length < i) {
                        throw new CommandLineParseException("Only specified a cvsroot, need to specify a command.");
                    }
                }
                switch (arguments[i]) {
                    case "checkout":
                    case "co":
                    case "get":
                        singleOptions = "ANPRcflnps";
                        this.command = arguments[i++];
                        // get rest of arguments which is options on the checkout command.
                        while (arguments[i].IndexOf("-", 0, 1) >= 0){
                            // Get options with second parameters?
                            if (arguments[i].IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0){
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ){
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            }
                            else{
                                this.options = this.options + arguments[i];       // Yes
                                this.options = this.options + arguments[i] + " ";
                            }
                            i++;
                        }
                        if (arguments.Length > i){
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.repository = arguments[i];
                        } else {
                            this.repository = String.Empty;
                        }
                        try {
                            CheckoutCommand checkoutCommand = 
                                new CheckoutCommand(cvsroot, repository, options);
                            command = checkoutCommand.CreateCommand ();
                            this.currentWorkingDirectory = 
                                checkoutCommand.CurrentWorkingDirectory;
                        } catch (Exception e) {
                            LOGGER.Error(e);
                            throw new CommandLineParseException("Unable to create checkout command.", e);
                        }
                        break;
                    case "login":
                        // login to server
                        this.command = arguments[i];
                        break;
                    case "passwd":
                        this.command = arguments[i];
                        break;
                    case "up":
                    case "upd":
                    case "update":
                        singleOptions = "ACPRbdfmp";
                        this.command = arguments[i++];
                            // get rest of arguments which is options on the update command.
                        while (arguments[i].IndexOf("-", 0, 1) >= 0) {
                            // Get options with second parameters?
                            if (arguments[i].IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0) {
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ) {
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            } else {
                                this.options = this.options + arguments[i];       // Yes
                                this.options = this.options + arguments[i] + " ";
                            }
                            i++;
                        }
                        if (arguments.Length > i)
                        {
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.repository = arguments[i++];
                        } 
                        else 
                        {
                            this.repository = String.Empty;
                        }
                        break;
                    case "--help":
                        System.Console.WriteLine(Usage.General);
                        break;
                    case "--help-options":
                        System.Console.WriteLine(Usage.Options);
                        break;
                    case "--help-commands":
                        System.Console.WriteLine(Usage.Commands);
                        break;
                    case "--help-synonyms":
                        System.Console.WriteLine(Usage.Synonyms);
                        break;
                    default:
                        StringBuilder msg = new StringBuilder ();
                        msg.Append("Unknown command entered.  ");
                        msg.Append("command=[").Append(arguments[i]).Append("]");
                        throw new CommandLineParseException(msg.ToString());
                    }
                }
            return command;
        }
    }
}
