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
using System.IO;
using System.Text;
using System.Xml;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;

using ICSharpCode.SharpCvsLib.Console.Commands;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;
using log4net.Config;

namespace ICSharpCode.SharpCvsLib.Console {

    /// <summary>The main driver/ entry point into the program.</summary>
    [Serializable]
    public class ConsoleMain {
        private ILog LOGGER;

        private string[] _args;

        private const string DEFAULT_CONFIG = 
            @"
<?xml version='1.0' encoding='utf-8' ?> 
<configuration>
    <configSections>
        <section name='log4net' type='log4net.Config.Log4NetConfigurationSectionHandler,log4net'/>
    </configSections>       
    <log4net debug='false'>
        <appender name='ConsoleAppender' type='log4net.Appender.ConsoleAppender'>
            <layout type='log4net.Layout.PatternLayout'>
                <param name='ConversionPattern' value='[%c{2}:%m  - [%x] &lt;%X{auth}&gt;]%n' />
            </layout>
        </appender>
        <appender name='RollingLogFileAppender' type='log4net.Appender.RollingFileAppender'>
            <param name='File' value='cvs.log' />
            <param name='AppendToFile' value='true' />
            <param name='MaxSizeRollBackups' value='10' />
            <param name='MaximumFileSize' value='1000000' />
            <param name='RollingStyle' value='Size' />
            <param name='StaticLogFileName' value='true' />
            <layout type='log4net.Layout.PatternLayout'>
                <param name='Header' value='[Header]\r\n' />
                <param name='Footer' value='[Footer]\r\n' />
                <param name='ConversionPattern' value='%d [%t] %-5p %c [%x] - %m%n' />
            </layout>
        </appender>
        <root>
            <level value='INFO' />
            <!--<appender-ref ref='RollingLogFileAppender' />-->
            <appender-ref ref='ConsoleAppender' />
        </root>
    </log4net>
</configuration>
";

        /// <summary>
        /// Command line arguments.
        /// </summary>
        public string[] Args {
            get {return this._args;}
            set {this._args = value;}
        }

        /// <summary>Constructor.
        ///     TODO: Fill in more of a usage/ explanation.</summary>
        public ConsoleMain () {
            try {
                LOGGER = LogManager.GetLogger(typeof(ConsoleMain));
            } catch (Exception) {
                try {
                    DOMConfigurator.Configure(WriteDefaultConfig());
                } catch (Exception) {
                    BasicConfigurator.Configure();
                }
            }
        }

        private MemoryStream WriteDefaultConfig () {
            MemoryStream stream = new MemoryStream();
            try {
                stream.Write(Encoding.UTF8.GetBytes(DEFAULT_CONFIG), 0, DEFAULT_CONFIG.Length);
            }catch (Exception e) {
                System.Console.WriteLine(e);
            }
            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void Execute(string[] args) {
            this.Args = args;
            this.Execute();
        }

        /// <summary>
        /// Driver for console application.
        ///
        /// TODO: Write a better description :-)
        /// </summary>
        public void Execute () {
            string[] args = this._args;
            CommandLineParser parser = new CommandLineParser (args);

//            if (LOGGER.IsDebugEnabled) {
//                StringBuilder msg = new StringBuilder();
//                msg.Append(Environment.NewLine).Append("Using arguments:");
//                foreach (String arg in args) {
//                    msg.Append(Environment.NewLine).Append("\t arg1=[").Append(arg).Append("]");
//                }
//                LOGGER.Debug(msg);
//            }
            ICommand command = null;
            try {
                command = parser.Execute ();
            } catch (CommandLineParseException) {
//                LOGGER.Debug (e);
                System.Console.WriteLine(Usage.General);
                return;
            }

//            LOGGER.Debug("command type: " + command.GetType());

            if (null != command) {
                // might need to move this up to the library, make working
                //  directory a public property??  Not sure.
                WorkingDirectory workingDirectory = parser.CurrentWorkingDirectory;

                string password = "";

                // Create CVSServerConnection object that has the ICommandConnection
                CVSServerConnection serverConn = new CVSServerConnection(workingDirectory);

                if (null == serverConn) {
                    System.Console.WriteLine("Unable to connect to server.");
                    Environment.Exit(-1);
                }

                if (command.GetType() == typeof (LoginCommand)) {
                    command.Execute (serverConn);
                } else {
                    try{
                        // try connecting with empty password for anonymous users
                        serverConn.Connect(workingDirectory, password);
                    }
                    catch (AuthenticationException){
//                        LOGGER.Info("Authentication failed using empty password, trying .cvspass file.", eDefault);
                        try{
                            //string scrambledpassword;
                            // check to connect with password from .cvspass file
                            // check for .cvspass file and get password
                            //password = PasswordScrambler.Descramble(scrambledpassword);
                            serverConn.Connect(workingDirectory, password);
                        }
                        catch (AuthenticationException eCvsPass){
                            try {
//                                LOGGER.Info("Authentication failed using .cvspass file, prompting for password.", eCvsPass);
                                // prompt user for password by using login command?
                                LoginCommand login = new LoginCommand(workingDirectory.CvsRoot);
                                serverConn.Connect(workingDirectory, login.Password);
                                throw eCvsPass;
                            } catch (AuthenticationException) {
                                StringBuilder msg = new StringBuilder();
                                msg.Append("Fatal error, aborting.");
                                msg.Append("cvs [login aborted]: ")
                                    .Append(workingDirectory.CvsRoot.User)
                                    .Append(": unknown user or bad password.");
//                                LOGGER.Error(msg, e);
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
}
