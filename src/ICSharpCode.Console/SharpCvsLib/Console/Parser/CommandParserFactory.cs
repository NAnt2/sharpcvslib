#region "Copyright"
//
// Copyright (C) 2004 Clayton Harbour
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
//
//    <author>Clayton Harbour</author>
#endregion


using System;
using System.Collections;

using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;

namespace ICSharpCode.SharpCvsLib.Console.Parser {
	/// <summary>
	/// Summary description for CommandFactory.
	/// </summary>
	public class CommandParserFactory {

        private bool showUsage;
        /// <summary>
        /// <code>true</code> if the help/usage information should be displayed for this command.
        /// </summary>
        public bool ShowUsage {
            get {return this.showUsage;}
            set {this.showUsage = value;}
        }

        private string command;
        private string[] args;
        private CvsRoot cvsRoot;
        private WorkingDirectory workingDirectory;

        /// <summary>
        /// Creates a new instance of the command parser.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="cvsRoot"></param>
        /// <param name="workingDirectory"></param>
		public CommandParserFactory(string command, string[] args,
            CvsRoot cvsRoot, WorkingDirectory workingDirectory){
            this.command = command;
            this.args = GetArgsAfterCommandName(args);
            this.cvsRoot = cvsRoot;
            this.workingDirectory = workingDirectory;
		}

        private string[] GetArgsAfterCommandName (string[] args) {
            ArrayList subArgs = new ArrayList();
            bool add = false;
            foreach (string argument in args) {
                if (add) {
                    subArgs.Add(argument);
                }
                if (argument.Equals("xml")) {
                    add = true;
                }
            }

            return (string[])subArgs.ToArray(typeof(String));
        }

        /// <summary>
        /// Create a new instance of the command parser that matches the command name specified
        /// in the constructor.
        /// </summary>
        /// <returns>A new instance of the specified command parser that implements the 
        /// <see cref="ICommandParser"/> interface.</returns>
        public ICommandParser GetCommandParser () {
            ICommandParser parser = null;
            switch (command) {
                case "xml":
                    parser = XmlLogCommandParser.GetInstance();
                    break;
                case "co":
                    parser = CheckoutCommandParser.GetInstance();
                    break;
                default:
                    throw new ArgumentException(
                        String.Format("Unknown command: {0}.", this.command));
            }
            parser.Args = this.args;
            parser.CvsRoot = this.cvsRoot;
            parser.CurrentWorkingDirectory = this.workingDirectory;

            try {
                parser.ParseOptions();
            } catch (CommandLineParseException e) {
                string msg = 
                    String.Format("{0}{1}{2}",
                    e.Message, Environment.NewLine, parser.Usage);
                System.Console.WriteLine(msg);
            }
            
            return parser;
        }
	}
}
