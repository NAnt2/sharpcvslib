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
    ///     Test the update command object for valid ones
    ///         and test invalid ones.
    /// </summary>
    [TestFixture]
    public class UpdateCommandTest {

        private TestSettings settings = new TestSettings();
        private readonly ILog LOGGER = LogManager.GetLogger(typeof(UpdateCommandTest));
        /// <summary>
        ///     Constructory for test case.
        /// </summary>
        public UpdateCommandTest ()
        {
        }

        /// <summary>
        ///     Create a UpdateCommand object.
        ///
        /// </summary>
        [Test]
        public void MakeUpdateCommandTest () {
            Directory.CreateDirectory( settings.Config.LocalPath);
            Environment.CurrentDirectory = settings.Config.LocalPath;

            String commandLine = "-d:pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib up sharpcvslib";
            String [] commandLineArgs = commandLine.Split(' ');
            // Test Creating a UpdateCommand object
            ConsoleMain consoleMain = new ConsoleMain();
            consoleMain.Execute(commandLineArgs);
            Assertion.AssertNotNull ("Should have a command object.", consoleMain);
        }
        /// <summary>
        ///     Update files based on revision specified in -r option.
        ///
        /// </summary>
        [Test]
        public void MinusrOptionUpdateFilesBasedOnRevision (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-rv0_3_1 ";
            // Test Creating a UpdateCommand object
            UpdateCommand updateCommand = new UpdateCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", updateCommand);
            //updateCommand.Execute();
        }
        /// <summary>
        ///     Update files to specified local location instead of current local location
        ///     with the -d option
        /// </summary>
        [Test]
        public void MinusdOptionUpdateFileIntoDir (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-dnewlocation ";
            // Test Creating a UpdateCommand object
            UpdateCommand updateCommand = new UpdateCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", updateCommand);
            //updateCommand.Execute();
            //Assertion.Assert(Directory.Exists("newlocation"));
        }
        /// <summary>
        ///     Update files no earlier than the specified Date 
        ///     with the -D option
        /// </summary>
        [Test]
        public void MinusDOptionUpdateByCertainDate (){
            String root = ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
            String repository = "sharpcvslib";
            String options = "-D01.28.03 ";
            // Test Creating a UpdateCommand object
            UpdateCommand updateCommand = new UpdateCommand(root, repository, options);
            Assertion.AssertNotNull ("Should have a command object.", updateCommand);
            //updateCommand.Execute();
            // Find a file that should exist 
            //Assertion.Assert ("Should have found the update file.  file=[" +
            //    checkFile + "]", File.Exists (checkFile));

            // Find a file that should not exist
            //Assertion.Assert ("Should have found the update file.  file=[" +
            //    checkFile + "]", File.Exists (checkFile));
        }
    }
}
