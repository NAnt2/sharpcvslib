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
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;
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
        private const string CVS_PASS_REGEX = @"[\w]*" + CvsRoot.CVSROOT_REGEX + @"[\s]([^\r\n]*)";
        private readonly ILog LOGGER = LogManager.GetLogger(typeof(LoginCommand));

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
            set {this.password = PasswordScrambler.Scramble(value);}
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
        public void Execute (ICommandConnection connection){
            string password = this.GetPassword();

            if (null == password) {
                // Is there a password file?
                //     yes, get password for this username
                //     no, prompt user for password to use
                System.Console.Write("CVS password for {0}: ", this.cvsRoot.User);
                password = System.Console.ReadLine();

                password = PasswordScrambler.Scramble(password);

                // once we have a password then put it in the .cvspass file.
                // this file is either in the HOME directory or in the root of the 
                // current folder.
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
                    System.Console.WriteLine(String.Format("Unable to find passfile: {0}", passwordFile));
                    Environment.Exit(-1);
                }
                System.Console.WriteLine(String.Format("Using passfile: {0}.", passwordFile.FullName));
                StreamWriter writer = passwordFile.AppendText();
                writer.WriteLine(String.Format("{0} {1}", 
                    connection.Repository.CvsRoot, password));
                writer.Close();
            }
        }

        /// <summary>
        /// Lookup the password for the given file
        /// </summary>
        /// <returns></returns>
        public string GetPassword () {
            string [] passFileLocations = { 
                 System.Environment.GetEnvironmentVariable(ENV_HOME),
                    System.Environment.GetEnvironmentVariable(ENV_CVS_HOME),
                    Path.GetPathRoot(Environment.CurrentDirectory)};

            string password = null;
            foreach (string passFilePath in passFileLocations) {
                LOGGER.Debug(String.Format("Looking for passfile: {0}", passFilePath));
                password = this.ReadPassword(passFilePath);
                if (null != password) {
                    return password;
                }
            }
            return null;
        }

        private string ReadPassword (string fullPath) {
            FileInfo passFile = new FileInfo(fullPath);
            if (!passFile.Exists) {
                passFile = new FileInfo(Path.Combine(fullPath, CVS_PASSFILE));
                if (!passFile.Exists) {
                    return null;
                }
            }
            System.Console.WriteLine(String.Format("Looking for passfile: {0}", passFile.FullName));

            StreamReader passStream = 
                new StreamReader(passFile.FullName);

            string passLine = passStream.ReadToEnd();

            System.Console.WriteLine(String.Format("PassLine: {0}.", passLine));

            //Regex regex = new Regex(CVS_PASS_REGEX, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
/*            MatchCollection matches = Regex.Matches(passLine, 
                CVS_PASS_REGEX, RegexOptions.IgnorePatternWhitespace);

            System.Console.WriteLine(String.Format("Matches count: {0}.", matches.Count));
            System.Console.WriteLine(String.Format("Match Group count: {0}.", 
                Regex.Match(passLine, CVS_PASS_REGEX, RegexOptions.IgnorePatternWhitespace).Groups.Count));
            System.Console.WriteLine(String.Format("Regex: {0}.", CVS_PASS_REGEX));
            System.Console.WriteLine(String.Format("Attempting to match: {0}.", this.CvsRoot.ToString()));
            if (matches.Count <= 0) {
                return null;
            }
*/
            Regex regex = new Regex(CVS_PASS_REGEX, 
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(passLine);
//            System.Console.WriteLine(String.Format("Capture count: {0}.", groups.Count));

            foreach (Match match in matches) {
//                string cvsRootTemp = group.Captures[1].Value +
//                    group.Captures[3].Value + group.Captures[4].Value +
//                    group.Captures[5].Value;

                try {
                    CvsRoot cvsRootTemp = new CvsRoot(match.Value);
                    System.Console.WriteLine(String.Format("cvsRootTemp: {0}.", cvsRootTemp));
                    if (this.CvsRoot.Equals(cvsRootTemp)) {
                        return match.Groups[6].Value;
                    }
                } catch (ICSharpCode.SharpCvsLib.Misc.CvsRootParseException) {

                }
            }
            return null;
        }

    }
}
