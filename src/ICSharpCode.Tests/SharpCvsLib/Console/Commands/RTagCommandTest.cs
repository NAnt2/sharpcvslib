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
using System.Diagnostics;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;

using ICSharpCode.SharpCvsLib.Config.Tests;
using ICSharpCode.SharpCvsLib.Console.Parser;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Console.Commands {
    
    /// <summary>
    ///     Test the rtag command object for valid ones
    ///         and test invalid ones.
    /// </summary>
    [TestFixture]
    public class RTagCommandTest {

        private TestSettings settings = new TestSettings();
        private readonly ILog LOGGER = LogManager.GetLogger(typeof(RTagCommandTest));
        /// <summary>
        ///     Constructory for test case.
        /// </summary>
        public RTagCommandTest ()
        {
        }

        /// <summary>
        ///     Create a RTagCommand object.
        ///
        /// </summary>
        [Test]
        public void MakeRTagCommandTest () {
            Directory.CreateDirectory( settings.Config.LocalPath);
            Environment.CurrentDirectory = settings.Config.LocalPath;

            String commandLine = "-d:pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib rt sharpcvslib";
            String [] commandLineArgs = commandLine.Split(' ');
            // Test Creating a RTagCommand object
            ConsoleMain consoleMain = new ConsoleMain();
            consoleMain.Execute(commandLineArgs);
            Assertion.AssertNotNull ("Should have a command object.", consoleMain);
        }
        /// <summary>
        ///     RTag files based on revision specified in -r option.
        ///
        /// </summary>
        [Test]
        public void MinusrOptionRTagFilesBasedOnRevision (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-rv0_3_1 ";
            // Test Creating a RTagCommand object
            RTagCommand rtagCommand = new RTagCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", rtagCommand);
            //rtagCommand.Execute();
        }
        /// <summary>
        ///     RTag files to specified description
        ///     with the -m option
        /// </summary>
        [Test]
        public void MinusmOptionRTagFileDescription (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-mnewdescription ";
            // Test Creating a RTagCommand object
            RTagCommand rtagCommand = new RTagCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", rtagCommand);
            //rtagCommand.Execute();
            //Assertion.Assert(Directory.Exists("newlocation"));
        }
        /// <summary>
        ///     RTag files no earlier than the specified Date 
        ///     with the -D option
        /// </summary>
        [Test]
        public void MinusDOptionRTagByCertainDate (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-D01.28.03 ";
            // Test Creating a RTagCommand object
            RTagCommand rtagCommand = new RTagCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", rtagCommand);
            //rtagCommand.Execute();
            // Find a file that should be prior to date specified 
            //Assertion.Assert ("Should have found the rtag file.  file=[" +
            //    checkFile + "]", File.Exists (checkFile));

            // Find a file that should be after the date specified
            //Assertion.Assert ("Should have not found the rtag file.  file=[" +
            //    checkFile + "]", File.Exists (checkFile));
        }
    }
}
