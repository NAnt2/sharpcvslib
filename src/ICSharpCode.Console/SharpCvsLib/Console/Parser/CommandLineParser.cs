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
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;

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

        private CvsRoot cvsRoot;
        private string commandTxt;
        private string options = string.Empty;
        private string repository;
        private string singleOptions;
        private string files;
        bool _readOnly;

        private static bool _verbose = false;

        private const string REGEX_LOG_LEVEL = @"-log:(?<Level>(debug|info|warn|error))";
        private const String ENV_CVS_ROOT = "CVS_ROOT";

        private WorkingDirectory currentWorkingDirectory;
        /// <summary>
        /// Get the current working directory, parsed from the command line.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }

        /// <summary>
        /// <code>true</code> if the server response and requests should be sent to the appropriate
        /// logger (usually standard out); otherwise <code>false</code>.
        /// </summary>
        public static bool IsVerbose {
            get { return _verbose; }
        }

        public bool ReadOnly {
            get { return _readOnly; }
            set { _readOnly = value; }
        }

        /// <summary>
        /// Value of the cvsroot to use as a string.  This will be passed
        ///     into the CvsRoot object which will know how to parse it.
        /// </summary>
        public CvsRoot CvsRoot {
            get {return this.cvsRoot;}
        }

        /// <summary>
        /// The text value of the command that will be executed.  This should be
        ///     translated into one of the public API command objects.
        /// </summary>
        public String Command {
            get {return this.commandTxt;}
        }
        /// <summary>
        /// Value of the repository to use as a string.  This will be passed
        ///     into the RemoveCommand object which will know which files to get.
        /// </summary>
        public String Files {
            get {return this.files;}
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

        /// <summary>
        /// Return the commandline string collection as a single string.
        /// </summary>
        public string CommandLine {
            get {
                StringBuilder msg = new StringBuilder ();
                foreach (string arg in this.arguments) {
                    msg.Append(String.Format("{0} ", arg));
                }
                return msg.ToString();;

            }
        }

        private string password;
        /// <summary>
        /// The password passed in on the commandline, or null if none.
        /// </summary>
        public string Password {
            get {return this.password;}
        }

        /// <summary>Create a new instance of the command line parser and
        ///     initialize the arguments object.</summary>
        /// <param name="args">A collection of strings that represent the command
        ///     line arguments sent into the program.</param>
        public CommandLineParser (String[] args) {
            this.arguments = args;
        }

        /// <summary>
        /// Parse the command line options.  There are two (2) general sweeps
        ///     at parsing the command line.  The first sweep looks for command line
        ///     help switches, denoted by -- parameters.     
        /// </summary>
        /// <returns>A command object from the library which will be used to 
        ///     access the repsository.</returns>
        /// <exception cref="CommandLineParseException">If there is a problem
        ///     parsing the command line arguments (i.e. if invalid arguments
        ///     are entered.</exception>
        public ICommand Execute () {
            ICommand command = null;
            for (int i = 0; i < arguments.Length; i++) {
                string arg = arguments[i];

                // stop when we reach the command
                if (!arg.StartsWith("-")) {
                    break;
                }

                if (arg.StartsWith("-d:")) {
                    string tempRoot = arg.Substring(2, arg.Length - 2);
                    this.cvsRoot = new CvsRoot(tempRoot);
                    continue;
                }

                Match passwordMatch = 
                    Regex.Match(arg, ICSharpCode.SharpCvsLib.Console.Commands.LoginCommand.REGEX_PASSWORD);
                if (passwordMatch.Success) {
                    this.password = passwordMatch.Groups["Password"].Value;
                }

                Match logMatch = Regex.Match(arg, REGEX_LOG_LEVEL);
                if (logMatch.Success) {
                    string newLevelString = logMatch.Groups["Level"].Value;

                    if (null != newLevelString && newLevelString.Length != 0) {
                        log4net.Core.LevelMap map = log4net.LogManager.GetRepository().LevelMap;
                        log4net.Core.Level newLevel = map[newLevelString];
                        log4net.LogManager.GetRepository().Threshold = newLevel;
                    }
                }


                switch (arg) {
                    case "-verbose":
                        _verbose = true;
                        break;
                    case "-H":
                        // show help
                        break;
                    case "--help":
                        if (i+1 < arguments.Length) {
                            string commandName = arguments[++i];
                            ICommandParser commandParser =
                                CommandParserFactory.GetCommandParser(commandName);
                            System.Console.WriteLine(commandParser.Usage);
                            return null;
                        } else {
                            System.Console.WriteLine(Usage.General);
                            return null;
                        }
                    case "--help-options":
                        System.Console.WriteLine(Usage.Options);
                        return null;
                    case "--help-commands":
                        System.Console.WriteLine(Usage.Commands);
                        return null;
                    case "--help-synonyms":
                        System.Console.WriteLine(Usage.Synonyms);
                        return null;
                    case "-v":
                    case "--version":
                        System.Console.WriteLine(Usage.Version);
                        return null;
                    case "-Q":
                        // really quiet
                        break;
                    case "-q":
                        // somewhat quiet
                        break;
                    case "-r":
                        this.ReadOnly = true;
                        break;
                    case "-w":
                        // read-write
                        break;
                    case "-n":
                        throw new NotSupportedException("Unsupported option -n");
                        break;
                    case "-t":
                        throw new NotSupportedException("Unsupported option -t");
                        break;
                    case "-T":
                        throw new NotSupportedException("Unsupported option -T");
                        break;
                    case "-e":
                        throw new NotSupportedException("Unsupported option -e");
                        break;
                    case "-d":
                        this.cvsRoot = new CvsRoot(arguments[++i]);
                        break;
                    case "-f":
                        throw new NotSupportedException("Unsupported option -f");
                        break;
                    case "-F":
                        throw new NotSupportedException("Unsupported option -F");
                        break;
                    case "-z":
                        throw new NotSupportedException("Unsupported option -z");
                        break;
                    case "-x":
                        throw new NotSupportedException("Unsupported option -x");
                        break;
                    case "-y":
                        throw new NotSupportedException("Unsupported option -y");
                        break;
                    case "-a":
                        throw new NotSupportedException("Unsupported option -a");
                        break;
                    case "-N":
                        throw new NotSupportedException("Unsupported option -N");
                        break;
                    case "-s":
                        throw new NotSupportedException("Unsupported option -s");
                        break;
                    case "-o":
                        throw new NotSupportedException("Unsupported option -o");
                        break;
                    case "-O":
                        throw new NotSupportedException("Unsupported option -O");
                        break;
                    case "--encrypt":
                        throw new NotSupportedException("Unsupported option --encryp");
                        break;
                    case "--authenticate":
                        throw new NotSupportedException("Unsupported option --authenticate");
                        break;
                }
            }

            for (int i = 0; i < arguments.Length; i++) {
                string commandString = arguments[i].Trim();
                ICommandParser parser;
                switch (commandString) {
                    case "add":
                    case "ad":
                    case "new":
                        i++;
                        string [] tempAddArgs = new string[arguments.Length - i];
                        Array.Copy(arguments, i, tempAddArgs, 0, arguments.Length - i);
                        AddCommandParser addCommand = 
                            new AddCommandParser(this.CvsRoot, tempAddArgs);
                        command = addCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            addCommand.CurrentWorkingDirectory;
                        i = arguments.Length;
                        break;
                    case "commit":
                    case "ci":
                    case "com":
                        i++;
                        string [] tempCommitArgs = new string[arguments.Length - i];
                        Array.Copy(arguments, i, tempCommitArgs, 0, arguments.Length - i);
                        CommitCommandParser commitCommand = 
                            new CommitCommandParser(this.CvsRoot, tempCommitArgs);
                        command = commitCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            commitCommand.CurrentWorkingDirectory;
                        i = arguments.Length;
                        break;
                    case "checkout":
                    case "co":
                    case "get":
                        singleOptions = "ANPRcflnps";
                        this.commandTxt = arguments[i];
                        i++;
                        // get rest of arguments which is options on the checkout command.
                        while (arguments.Length > i && arguments[i].Trim().IndexOf("-") == 0){
                            // Get options with second parameters?
                            if (arguments[i].Trim().IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0){
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ){
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            }
                            else{
                                this.options = this.options + arguments[i++];       // Yes
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
                        CheckoutCommandParser checkoutCommand = 
                            new CheckoutCommandParser(this.CvsRoot, this.Repository, options);
                        command = checkoutCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            checkoutCommand.CurrentWorkingDirectory;
                        break;
                    case "import":
                    case "imp":
                    case "im":
                        i++;
                        string [] tempImportArgs = new string[arguments.Length - i];
                        Array.Copy(arguments, i, tempImportArgs, 0, arguments.Length - i);
                        ImportCommandParser importCommand = 
                            new ImportCommandParser(this.CvsRoot, tempImportArgs);
                        command = importCommand.CreateCommand();
                        this.currentWorkingDirectory =
                            importCommand.CurrentWorkingDirectory;
                        i = arguments.Length;
                        break;
                    case "init":
                        this.commandTxt = arguments[i];
                        InitCommandParser initCommand = new InitCommandParser(this.CvsRoot);
                        command = initCommand.CreateCommand ();
                        this.currentWorkingDirectory = initCommand.CurrentWorkingDirectory;
                        break;
                    case "log":
                    case "lo":
                        this.commandTxt = arguments[i++];
                        string[] logArgs = new string[arguments.Length - i];
                        Array.Copy(arguments, i, logArgs, 0, arguments.Length - i);
                        LogCommandParser logCommandParser = 
                            new LogCommandParser(this.CvsRoot, logArgs);
                        command = logCommandParser.CreateCommand();
                        this.currentWorkingDirectory = logCommandParser.CurrentWorkingDirectory;
                        i = arguments.Length;
                        break;
                    case "login":
                    case "logon":
                    case "lgn":
                        // login to server
                        this.commandTxt = arguments[i];
                        ICSharpCode.SharpCvsLib.Console.Commands.LoginCommand loginCommand = 
                            new ICSharpCode.SharpCvsLib.Console.Commands.LoginCommand(this.CvsRoot, this.currentWorkingDirectory);
                        loginCommand.Args = arguments;
                        this.currentWorkingDirectory = loginCommand.CurrentWorkingDirectory;
                        command = loginCommand;
                        break;
                    case "dir":
                    case "list":
                    case "ls":
                        parser = CommandParserFactory.GetCommandParser("ls");
                        i = arguments.Length;
                        command = parser.CreateCommand();
                        this.currentWorkingDirectory = 
                            parser.CurrentWorkingDirectory;
                        break;
                    case "passwd":
                    case "password":
                    case "setpass":
                        this.commandTxt = arguments[i];
                        break;
                    case "remove":
                    case "delete":
                    case "rm":
                        singleOptions = "Rfl";
                        this.commandTxt = arguments[i];
                        i++;
                        // get rest of arguments which is options on the update command.
                        while (arguments.Length > i && arguments[i].IndexOf("-", 0, 1) >= 0) {
                            // Get options with second parameters?
                            if (arguments[i].IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0) {
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ) {
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            } 
                            else {
                                this.options = this.options + arguments[i];       // Yes
                                this.options = this.options + arguments[i] + " ";
                            }
                            i++;
                        }
                        if (arguments.Length > i) {
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.files = arguments[i++];
                        } 
                        else {
                            this.files = String.Empty;
                        }
                        RemoveCommandParser removeCommand = 
                            new RemoveCommandParser(this.CvsRoot, files, options);
                        command = removeCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            removeCommand.CurrentWorkingDirectory;
                        break;
                    case "rt":
                    case "rtag":
                    case "rtfreeze":
                        singleOptions = "abBdfFlMnR";
                        this.commandTxt = arguments[i++];
                        // get rest of arguments which is options on the rtag command.
                        while (arguments.Length > i && arguments[i].IndexOf("-", 0, 1) >= 0) {
                            // Get options with second parameters?
                            if (arguments[i].IndexOfAny( singleOptions.ToCharArray(), 1, 1) >= 0) {
                                for ( int cnt=1; cnt < arguments[i].Length; cnt++ ) {
                                    this.options = this.options + "-" + arguments[i][cnt] + " "; // No
                                }
                            } 
                            else {
                                this.options = this.options + arguments[i];       // Yes
                                this.options = this.options + arguments[i] + " ";
                            }
                            i++;
                        }
                        if (arguments.Length > i) {
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.repository = arguments[i++];
                        } 
                        else {
                            this.repository = String.Empty;
                        }
                        RTagCommandParser rtagCommand = 
                            new RTagCommandParser(this.CvsRoot, repository, options);
                        command = rtagCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            rtagCommand.CurrentWorkingDirectory;
                        break;
                    case "st":
                    case "stat":
                    case "status":
                        string[] commandArgs = new string[arguments.Length - i];
                        Array.Copy(arguments, i, commandArgs, 0, arguments.Length - i);
                        parser = 
                            CommandParserFactory.GetCommandParser("status");
                        i = arguments.Length;
                        command = parser.CreateCommand();
                        this.currentWorkingDirectory = 
                            parser.CurrentWorkingDirectory;
                        break;
                    case "up":
                    case "upd":
                    case "update":
                        singleOptions = "ACPRbdfmp";
                        this.commandTxt = arguments[i++];
                        // get rest of arguments which is options on the update command.
                        while (arguments.Length > i && arguments[i].IndexOf("-", 0, 1) >= 0) {
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
                        if (arguments.Length > i) {
                            // Safely grab the module, if not specified then
                            //  pass null into the repository...the cvs command
                            //  line for cvsnt/ cvs seems to bomb out when
                            //  it sends to the server
                            this.repository = arguments[i++];
                        } 
                        else {
                            this.repository = String.Empty;
                        }
                        UpdateCommandParser updateCommand = 
                            new UpdateCommandParser(this.CvsRoot, repository, options);
                        command = updateCommand.CreateCommand ();
                        this.currentWorkingDirectory = 
                            updateCommand.CurrentWorkingDirectory;
                        break;
                    case "xml":
                        parser  = CommandParserFactory.GetCommandParser("xml");
                        i = arguments.Length;
                        command = parser.CreateCommand();
                        this.currentWorkingDirectory = 
                            parser.CurrentWorkingDirectory;

                        break;
                }
            }
            if (command == null) {
                ConsoleMain.ExitError("Use --help-commands to list commands", Usage.General);
            }
            this.currentWorkingDirectory.ReadOnly = this.ReadOnly;
            return command;
        }
    }
}
