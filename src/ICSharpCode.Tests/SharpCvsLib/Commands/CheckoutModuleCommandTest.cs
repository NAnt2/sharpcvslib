#region "Copyright"
// Copyright (C) 2003 Clayton Harbour
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
//    Author:    Clayton Harbour
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Config.Tests;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Commands {
    /// <summary>
    ///     Test that checkout module command fetches files from the 
    ///         remote repository.  All files are not verified, only 
    ///         a select few are checked.  After the files are checked
    ///         the entries are verified in the <code>CVS/Entries</code>
    ///         folder.
    /// </summary>
    [TestFixture]
    public class CheckoutModuleCommandTest  {
        string rootDir;
        string checkFile;
        
        private TestSettings settings = new TestSettings ();
        
        Manager manager;

        private ILog LOGGER = 
            LogManager.GetLogger (typeof(CheckoutModuleCommandTest));
                
        /// <summary>
        /// Constructor for customer db test.
        /// </summary>
        public CheckoutModuleCommandTest () {
        }
        
        /// <summary>
        ///     
        /// </summary>
        [SetUp]
        public void SetUp () { 
            this.rootDir =
                Path.Combine (this.settings.Config.LocalPath, 
                              this.settings.Config.Module);
            this.checkFile =
                Path.Combine (rootDir, this.settings.Config.TargetFile);
            this.manager = new Manager (); 
        }
        
        /// <summary>
        ///     Remove the local path directory that we were testing with.
        /// </summary>
        [TearDown]
        public void TearDown () {
           this.CleanTempDirectory ();
        }

        
        /// <summary>
        ///     Test that a checkout with all parameters is successful.
        /// </summary>
        [Test]
        public void CheckoutTest () {
            this.Checkout ();
            
            Assertion.Assert ("Should have found the check file.  file=[" + 
                              checkFile + "]", File.Exists (checkFile));
            
            ICvsFile[] entries = 
                manager.Fetch (rootDir, Factory.FileType.Entries);
            int foundFileEntry = 0; 
            int foundDirectoryEntry = 0;
            
            foreach (ICvsFile cvsEntry in entries) {
                Entry entry = (Entry)cvsEntry;
                System.Console.WriteLine ("entry=[" + entry + "]");
                if (entry.Name.Equals (this.settings.Config.TargetFile)) {
                    foundFileEntry++;
                }
                
                if (entry.Name.Equals (this.settings.Config.TargetDirectory)) {
                    foundDirectoryEntry++;
                }
            }
            
            Assertion.Assert ("Build file should have a cvs entry.", foundFileEntry == 1);
            Assertion.Assert (this.settings.Config.TargetDirectory + " directory should have a cvs entry.", foundDirectoryEntry == 1);
            Assertion.Assert ("Should not have a cvs directory above module path.", 
                              !Directory.Exists (Path.Combine (this.settings.Config.LocalPath, manager.CVS)));
            Assertion.Assert ("Should not have a cvs directory in the current execution path.  ",
                              !Directory.Exists (Path.Combine (this.settings.Config.Module, manager.CVS)));
            
            
        }
        
        /// <summary>
        ///     Test that specifying a revision produces a checkout of the specific
        ///     revision tag and creates a tag file in the cvs folder.
        /// </summary>
        [Test]
        public void CheckoutRevisionTest_Revision_1 () {
            this.CheckoutRevisionTest (this.settings.Config.Tag1, 
                                       this.settings.Config.Content1);
        }
        
        /// <summary>
        ///     Test that specifying a revision produces a checkout of the specific
        ///     revision tag and creates a tag file in the cvs folder.
        /// </summary>
        [Test]
        public void CheckoutRevisionTest_Revision_2 () {
            this.CheckoutRevisionTest (this.settings.Config.Tag2, 
                                       this.settings.Config.Tag2);
        }
        
        /// <summary>
        ///     Test that specifying a revision produces a checkout of the specific
        ///     revision tag and creates a tag file in the cvs folder.
        /// </summary>
        /// <param name="revision">The revision tag to checkout.</param>
        /// <param name="expectedContent">The file contents that are expected.</param>
        private void CheckoutRevisionTest (String revision, String expectedContent) {
            this.Checkout (revision, null);
            Assertion.Assert ("Should have found the check file.  file=[" + 
                              checkFile + "]", File.Exists (checkFile));
            
            ICvsFile[] entries = 
                manager.Fetch (rootDir, Factory.FileType.Entries);
            int foundFileEntry = 0; 
            int foundDirectoryEntry = 0;
            
            foreach (ICvsFile cvsEntry in entries) {
                Entry entry = (Entry)cvsEntry;
                System.Console.WriteLine ("entry=[" + entry + "]");
                if (entry.Name.Equals (this.settings.Config.TargetFile)) {
                    foundFileEntry++;
                }
                
                if (entry.Name.Equals (this.settings.Config.TargetDirectory)) {
                    foundDirectoryEntry++;
                }
            }
            
            Assertion.Assert ("Build file should have a cvs entry.", foundFileEntry == 1);
            Assertion.Assert (this.settings.Config.TargetDirectory + " directory should have a cvs entry.", foundDirectoryEntry == 1);
            Assertion.Assert ("Should not have a cvs directory above module path.", 
                              !Directory.Exists (Path.Combine (this.settings.Config.LocalPath, manager.CVS)));
            Assertion.Assert ("Should not have a cvs directory in the current execution path.  ",
                              !Directory.Exists (Path.Combine (this.settings.Config.Module, manager.CVS))); 

            String tagFile = 
                Path.Combine (Path.Combine (this.settings.Config.Module, manager.CVS), Tag.FILE_NAME);
            Assertion.Assert ("Should not have a cvs directory in the current execution path.  ",
                              !Directory.Exists (tagFile)); 
            
            AssertFileContentsEqualString (checkFile, expectedContent);
        }

        /// <summary>
        ///     Assert that the expected file contents match the contents actually
        ///         in the given file.
        /// </summary>        
        public static void AssertFileContentsEqualString (String filename, String expectedContent) {
            StreamReader reader = new StreamReader (filename);
            String actualContent = reader.ReadToEnd ();
            
            // Note the read to end method appends a carriage return (^M)/ line feed (^F)
            //    to the string read so this is removed manually:
            actualContent = actualContent.Substring (0, actualContent.Length -2);
            reader.Close ();
            Assertion.AssertEquals ("Files should be equal.", 
                                    expectedContent,
                                    actualContent);            
        }

        
        /// <summary>
        ///     Test that specifying a revision produces a checkout of the specific
        ///     revision tag and creates a tag file in the cvs folder.
        /// </summary>
        [Test]
        public void CheckoutOverrideDirectoryTest () {
            this.rootDir =
                Path.Combine (this.settings.Config.LocalPath, this.settings.Config.OverrideDirectory);
            this.checkFile =
                Path.Combine (rootDir, this.settings.Config.TargetFile);

            this.Checkout (null, this.settings.Config.OverrideDirectory);
            Assertion.Assert ("Should have found the check file.  file=[" + 
                              checkFile + "]", File.Exists (checkFile));
            
            ICvsFile[] entries = 
                manager.Fetch (rootDir, Factory.FileType.Entries);
            int foundFileEntry = 0; 
            int foundDirectoryEntry = 0;
            
            foreach (ICvsFile cvsEntry in entries) {
                Entry entry = (Entry)cvsEntry;
                System.Console.WriteLine ("entry=[" + entry + "]");
                if (entry.Name.Equals (this.settings.Config.TargetFile)) {
                    foundFileEntry++;
                }
                
                if (entry.Name.Equals (this.settings.Config.TargetDirectory)) {
                    foundDirectoryEntry++;
                }
            }
            
            Assertion.Assert ("Build file should have a cvs entry.", foundFileEntry == 1);
            Assertion.Assert (this.settings.Config.TargetDirectory + " directory should have a cvs entry.", foundDirectoryEntry == 1);
            Assertion.Assert ("Should not have a cvs directory above module path.", 
                              !Directory.Exists (Path.Combine (this.settings.Config.LocalPath, manager.CVS)));
            Assertion.Assert ("Should not have a cvs directory in the current execution path.  ",
                              !Directory.Exists (Path.Combine (this.settings.Config.Module, manager.CVS))); 

            String tagFile = 
                Path.Combine (Path.Combine (this.settings.Config.Module, manager.CVS), Tag.FILE_NAME);
            Assertion.Assert ("Should not have a cvs directory and tag file in the current execution path.  ",
                              !Directory.Exists (tagFile)); 
        }
                
        /// <summary>
        ///     Check if the temporary directory exists.  If it does then
        ///         remove the directory.
        /// </summary>
        private void CleanTempDirectory () {
            if (Directory.Exists(this.settings.Config.LocalPath)) {
                Directory.Delete (this.settings.Config.LocalPath, true);
            }            
        }
        
        /// <summary>
        ///     Perform a checkout command.
        /// </summary>
        public void Checkout () {
            this.Checkout (null);
        }

        /// <summary>
        ///     Perform a checkout command using the values in the 
        ///         The revision tag
        ///         (if specified) is also used to select the code
        ///         to checkout.
        /// </summary>
        /// <param name="revision">The specific revision of the module
        ///     to checkout from the repository.  If <code>null</code> 
        ///     is specified then the default revision, usually the 
        ///     <code>HEAD</code> is checked out.</param>
        /// <param name="overrideDirectory">The override directory to 
        ///     checkout the repository to.  If <code>null</code>
        ///     is specified then the directory is not overridden
        ///     and the module name is used.</param>
        public void Checkout (String revision, String overrideDirectory) {
            CvsRoot root = new CvsRoot (this.settings.Config.Cvsroot);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        this.settings.Config.LocalPath, 
                                        this.settings.Config.Module);
            
            System.Console.WriteLine (this.settings.Config.LocalPath);

            working.Revision = revision;
            working.OverrideDirectory = overrideDirectory;
            
            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
            
            ICommand command = new CheckoutModuleCommand (working);
            Assertion.AssertNotNull ("Should have a command object.", command);
            
            try {
                connection.Connect (working, this.settings.Config.ValidPassword);
            } catch (AuthenticationException) {
                Assertion.Assert ("Failed to authenticate with server.", true);
            }

            command.Execute (connection);
            connection.Close ();            
        }
        
        /// <summary>
        ///     Perform a checkout command.  The revision tag
        ///         (if specified) is also used to select the code
        ///         to checkout.
        /// </summary>
        /// <param name="revision">The specific revision of the module
        ///     to checkout from the repository.  If <code>null</code> 
        ///     is specified then the default revision, usually the 
        ///     <code>HEAD</code> is checked out.</param>
        public void Checkout (String revision) {
            this.Checkout (revision, null);
        }
        
    }
}
