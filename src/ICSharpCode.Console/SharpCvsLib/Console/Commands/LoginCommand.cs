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
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Console;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Login to a cvs repository.
    /// </summary>
    public class LoginCommand : ICommand {
        private CvsRoot cvsRoot;
        private string password;
        private  WorkingDirectory workingDirectory;

        private const string ENV_HOME = "HOME";
        private const string ENV_CVS_PASSFILE = "CVS_PASSFILE";
        private const string ENV_CVS_HOME = "CVS_HOME";
        private const string CVS_PASSFILE = ".cvspass";
        private const string REGEX_CVS_PASSFILE = @"[\w]*" + CvsRoot.CVSROOT_REGEX + @"[\s]([^\s]*)";
        /// <summary>Regular expression to parse out the password prompt from the commandline</summary>
        public const string REGEX_PASSWORD = @"[-]*pwd:([^\s]*)";
        private readonly ILog LOGGER = LogManager.GetLogger(typeof(LoginCommand));

        private const string OPT_PASSWORD = "pwd";
        private string[] args;

        private ConsoleWriter writer;

        private ConsoleWriter Writer {
            get {
                if (null == this.writer) {
                    this.writer = new ConsoleWriter();
                }
                return this.writer;
            }
        }

        /// <summary>
        /// Commandline arguments.
        /// </summary>
        public string[] Args {
            get {return this.args;}
            set {this.args = value;}
        }

        /// <summary>
        /// Get the command line arguments as a string.
        /// </summary>
        public string CommandLine {
            get {
                StringBuilder msg = new StringBuilder ();
                foreach (string arg in this.args) {
                    msg.Append(String.Format("{0} ", arg));
                }
                return msg.ToString();
            }
        }

        /// <summary>
        /// The text value of the password that will be used to login.  This should be
        ///     translated into one of the public API command objects.
        /// </summary>
        public CvsRoot CvsRoot {
            get {return this.cvsRoot;}
        }

        /// <summary>
        /// Get the password.
        /// </summary>
        public string Password {
            get {return this.password;}
            set {this.password = value;}
        }

        /// <summary>
        /// Login to a cvs repository.
        /// </summary>
        /// <param name="cvsRoot">User information</param>
        public LoginCommand(string cvsRoot) : this(new CvsRoot(cvsRoot)) {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cvsRoot"></param>
        public LoginCommand (CvsRoot cvsRoot) {
            this.cvsRoot = cvsRoot;
        }

        /// <summary>
        /// Login to a cvs repository with workDirectory object
        /// </summary>
        /// <param name="cvsRoot">The repository root.</param>
        /// <param name="workingDirectory">User information</param>
        public LoginCommand(CvsRoot cvsRoot, WorkingDirectory workingDirectory){
            this.cvsRoot = cvsRoot;
            this.workingDirectory = workingDirectory;
            // Is there a password file?
            //     yes, get password for this username
            //     no, prompt user for password to use
        }

        /// <summary>
        /// Process the login command with cvs library API calls.
        /// </summary>
        public void Execute () {
            if (null != this.CvsRoot && this.CvsRoot.Protocol != 
                ICSharpCode.SharpCvsLib.Misc.CvsRoot.HostProtocol.PSERVER) {
                return;
            }
            this.password = this.GetPassword();
        }

        /// <summary>
        /// Process the login command with cvs library API calls.
        /// </summary>
        public void Execute (ICommandConnection connection) {
            this.Execute();
        }

        /// <summary>
        /// Lookup the password for the given file
        /// </summary>
        /// <returns></returns>
        private string GetPassword () {
            string thePassword = null;
            if (null != this.args && this.args.Length > 0) {
                Regex regex = new Regex(REGEX_PASSWORD);
                Match match = regex.Match(this.CommandLine);
                string pwd = match.Groups[0].Value;
                LOGGER.Debug(String.Format("password: {0}", pwd));
                thePassword = pwd;
            } else {
                string [] passFileLocations = { 
                    System.Environment.GetEnvironmentVariable(ENV_HOME),
                    System.Environment.GetEnvironmentVariable(ENV_CVS_HOME),
                    Path.GetPathRoot(Environment.CurrentDirectory)};

                foreach (string passFilePath in passFileLocations) {
                    LOGGER.Debug(String.Format("Looking for passfile: {0}", passFilePath));
                    thePassword = this.ReadPassword(passFilePath);
                    if (null != thePassword && String.Empty != thePassword) {
                        return PasswordScrambler.Descramble(thePassword);
                    }
                }

                if (thePassword == null) {
                    Writer.WriteLine(String.Format("Logging in to {0}", this.CvsRoot));
                    Writer.Write(String.Format("CVS password: "));
                    thePassword = System.Console.ReadLine();

                    password = PasswordScrambler.Scramble(thePassword);

                    this.WritePassword(password);
                }
            }

            return thePassword;
        }

        private string ReadPassword (string fullPath) {
            string pwd = String.Empty;
            if (null == fullPath || String.Empty == fullPath) {
                pwd = null;    
            } else {
                FileInfo passFile = new FileInfo(fullPath);

                if (!passFile.Exists) {
                    LOGGER.Debug(String.Format("Passfile {0} does not exist.", passFile.FullName));
                    passFile = new FileInfo(Path.Combine(fullPath, CVS_PASSFILE));
                }

                if (!passFile.Exists) {
                    LOGGER.Debug(String.Format("Passfile {0} does not exist.", passFile.FullName));
                    pwd = null;
                } else {
                    LOGGER.Debug(String.Format("Found passfile: {0}", passFile.FullName));

                    string passLine = null;
                    using (StreamReader passStream = new StreamReader(passFile.FullName)) {
                        passLine = passStream.ReadToEnd();
                        passStream.Close();
                    }

                    LOGGER.Debug(String.Format("PassLine: {0}.", passLine));

                    Regex regex = new Regex(REGEX_CVS_PASSFILE, 
                        RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                    MatchCollection matches = regex.Matches(passLine);

                    LOGGER.Debug(String.Format("Number of matches for {0}: {1}",
                        REGEX_CVS_PASSFILE, matches.Count));

                    foreach (Match match in matches) {
                        try {
                            CvsRoot cvsRootTemp = new CvsRoot(match.Value);
                            LOGGER.Debug(String.Format("cvsRootTemp: {0}.", cvsRootTemp));
                            if (this.CvsRoot.Equals(cvsRootTemp)) {
                                pwd = match.Groups[match.Groups.Count - 1].Value;
                                LOGGER.Debug(String.Format("Found password: {0}.", pwd));
                                break;
                            } else {
                                LOGGER.Debug(String.Format("Cvsroot: [{0}] is not equal to cvsRootTemp: [{1}].",
                                    this.CvsRoot.ToString(), cvsRootTemp));
                            }
                        } catch (ICSharpCode.SharpCvsLib.Misc.CvsRootParseException) {
                            LOGGER.Debug(String.Format("Invalid cvsroot: {0}.", match.Value));
                        }
                    }
                }
            }
            return pwd;
        }

        private void WritePassword(string thePassword) {
            FileInfo passwordFile = 
                new FileInfo(Environment.GetEnvironmentVariable(ENV_CVS_HOME));

            if (!passwordFile.Exists) {
                passwordFile = 
                    new FileInfo(
                    Path.Combine(Environment.GetEnvironmentVariable(ENV_CVS_HOME), CVS_PASSFILE));
            }

            if (null == passwordFile) {
                passwordFile = new FileInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            }

            if (null == passwordFile) {
                passwordFile = new FileInfo(
                    Path.DirectorySeparatorChar.ToString());
            }

            if (null == passwordFile) {
                LOGGER.Debug(String.Format("Unable to find passfile: {0}", passwordFile));
                Environment.Exit(-1);
            }
            LOGGER.Debug(String.Format("Using passfile: {0}.", passwordFile.FullName));
            try {
                using (StreamWriter writer = passwordFile.AppendText()) {
                    writer.WriteLine(String.Format("{0} {1}", 
                        this.CvsRoot, thePassword));
                    writer.Close();
                }
            } catch (IOException e) {
                LOGGER.Warn(String.Format("Unable to write password to file: {0}.", 
                    passwordFile.FullName), e);
            }

        }

    }
}
