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
//    <author>Clayton Harbour</author>
#endregion
using System;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Text;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Console.Parser;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Produce an xml log report.
    /// </summary>
    public class XmlLogCommand {
        private WorkingDirectory currentWorkingDirectory;
        private CvsRoot cvsRoot;
        private CvsRoot CvsRoot {
            get {return this.cvsRoot;}
            set {
//                System.Console.WriteLine(String.Format("CvsRoot: {0}.", value));
                this.cvsRoot = value;}
        }

        private string moduleName;
        private string ModuleName {
            get {return this.moduleName;}
            set {
//                System.Console.WriteLine(String.Format("Module name: {0}.", value));
                this.moduleName = value;}
        }
        private string localDirectory;
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof(XmlLogCommand));

        /// <summary>
        /// The current working directory.
        /// </summary>
        public WorkingDirectory CurrentWorkingDirectory {
            get {return this.currentWorkingDirectory;}
        }

        /// <summary>
        /// Name of the command being parsed.
        /// </summary>
        public static string CommandName {
            get {return "xml";}
        }

        private const string OPT_DATE = "-D";
        private const string OPT_DAYS = "-Ds";
        private const string OPT_OUTPUT_XML_FILENAME = "-o";
        private const string OPT_OUTPUT_XSL_FILENAME = "-style";
        private const string OPT_NAME_MAP = "-nm";

        private DateTime startDate;
        private DateTime StartDate {
            get {return this.startDate;}
            set {this.startDate = value;}
        }

        private DateTime endDate;
        private DateTime EndDate {
            get {return this.endDate;}
            set {this.endDate = value;}
        }

        private int lastNDays;
        private int LastNDays {
            get {return this.lastNDays;}
            set {this.lastNDays = value;}
        }

        private string xmlFilename;
        private string XmlFilename {
            get {return this.xmlFilename;}
            set {
//                System.Console.WriteLine(String.Format("Setting XmlFilename: {0}.", 
//                    value));
                this.xmlFilename = value;}
        }

        private string xslFilename;
        private string XslFilename {
            get {return this.xslFilename;}
            set {
//                System.Console.WriteLine(String.Format("Setting XslFilename: {0}.",
//                    value));
                this.xslFilename = value;
            }
        }

        /// <summary>
        /// Produce an xml log report.
        /// </summary>
        public XmlLogCommand(CvsRoot cvsroot, string[] args) {
//            System.Console.WriteLine(String.Format("Number of arguments: {0}.", args.Length));
            this.cvsRoot = cvsroot;
            this.moduleName = args[args.Length - 1];
            int i = 0;
            while (i < args.Length - 1) {
                string arg = args[i];
                switch (arg) {
                    case OPT_DATE: 
                        i++;
                        arg = args[i];
                        DateTime date = Util.DateParser.ParseCvsDate(arg);

                        if (DateTime.MinValue == this.StartDate) {
                            this.StartDate = date;
                        } else {
                            this.EndDate = date;
                        }
                        break;
                    case OPT_DAYS:
                        i++;
                        arg = args[i];
                        int numDays = Convert.ToInt32(arg);
                        this.LastNDays = numDays;
                        break;
                    case OPT_OUTPUT_XML_FILENAME:
                        i++;
                        arg = args[i];
                        this.XmlFilename = arg;
                        break;
                    case OPT_OUTPUT_XSL_FILENAME:
                        i++;
                        arg = args[i];
                        this.XslFilename = arg;
                        break;
                    default:
                        throw new ArgumentException(String.Format("Unknown option: {0}.",
                            arg));
                }
                i++;
            }
        }

        /// <summary>
        /// Create a new <see cref="ICSharpCode.SharpCvsLib.Commands.XmlLogCommand"/>.
        /// </summary>
        /// <returns></returns>
        public ICommand CreateCommand () {
            ICSharpCode.SharpCvsLib.Commands.XmlLogCommand xmlLogCommand;

            try {
                // create CvsRoot object parameter
                if (localDirectory == null || localDirectory == String.Empty) {
                    localDirectory = Environment.CurrentDirectory;
                }
                this.currentWorkingDirectory = new WorkingDirectory(this.cvsRoot,
                    localDirectory, this.moduleName);
                xmlLogCommand = 
                    new ICSharpCode.SharpCvsLib.Commands.XmlLogCommand(this.currentWorkingDirectory, this.moduleName);
                xmlLogCommand.StartDate = this.StartDate;
                xmlLogCommand.EndDate = this.EndDate;
                xmlLogCommand.XmlFilename = this.XmlFilename;
                xmlLogCommand.XslFilename = this.XslFilename;
            }
            catch (Exception e) {
                LOGGER.Error (e);
                throw e;
            }
            return xmlLogCommand;        } 
    }
}