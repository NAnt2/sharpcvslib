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

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>
    ///     Parse the command line parameters and create a new Console
    ///     command object for the current parameters passed in.
    /// </summary>
    public class CommandLineParser {
    
        private String[] arguments;
        
        private string cvsroot;
        private string command;
        private string options;

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
        
        /// <summary>Create a new instance of the command line parser and 
        ///     initialize the arguments object.</summary>
        /// <param name="args">A collection of strings that represent the command
        ///     line arguments sent into the program.</param>
        public CommandLineParser (String[] args) {
            this.arguments = args; 
            
            // TODO: Remove this hack when add method to set options.
            this.options = String.Empty;
        }
        
        /// <summary>Parse the command line options.</summary>
        public void Execute () {
            if (arguments.Length < 1) {
                System.Console.WriteLine (Usage.General);
            }

            for (int i = 0; i < arguments.Length; i++) {
                switch (arguments[i]) {
                    case "checkout":
                    case "co": 
                        this.command = arguments[i];
                        break;
                    case "update":
                        this.command = arguments[i];
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
                    case "-d":
                        cvsroot = arguments[i];
                        break;
                    default:
                        System.Console.WriteLine (Usage.General);
                        throw new System.Exception ("not known");
                }
            }
            System.Console.WriteLine ("Thanks for using the command line tool.");

        }
    
    }
    
}
