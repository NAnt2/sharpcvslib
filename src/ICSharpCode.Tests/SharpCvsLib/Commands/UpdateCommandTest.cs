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

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Commands {
    /// <summary>
    ///     Tests that if a file is removed it will be pulled back down
    ///         with an update.
    /// </summary>
	[TestFixture]
	public class UpdateCommandTest	{
		private ILog LOGGER = 
			LogManager.GetLogger (typeof(CheckoutModuleCommandTest));
	    
	    String rootDir;
	    String checkFile;
	    String moduleDir;	    
	    
	    private Manager manager;	    
		/// <summary>
		/// Constructor for customer db test.
		/// </summary>
		public UpdateCommandTest () {
		}
		
		/// <summary>
		///     Checkout the sharpcvslib module so we have something
		///         to test the update command with.
		/// </summary>
		[SetUp]
		public void SetUp () {
		    this.moduleDir = TestConstants.MODULE;
		    this.rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, this.moduleDir);
		    this.checkFile = 
		        Path.Combine (rootDir, TestConstants.TARGET_FILE);
        }

        /// <summary>Wrapper for the checkout command test checkout method.</summary>        
        private void Checkout () {
            this.Checkout (null);
        }
        
        /// <summary>Wrapper for the checkout command test checkout method.</summary>
        private void Checkout (String revision) {
            this.Checkout (revision, null);
        }
        
        private void Checkout (String revision, String overrideDirectory) {
            this.manager = new Manager ();
            CheckoutModuleCommandTest checkout =
                new CheckoutModuleCommandTest ();
            checkout.Checkout (revision, overrideDirectory);
        }

		/// <summary>
		///     Remove the local path directory that we were testing with.
		/// </summary>
		[TearDown]
        public void TearDown () {
            this.CleanUp ();
        }

        private void CleanUp () {
            if (Directory.Exists(TestConstants.LOCAL_PATH)) {
                Directory.Delete (TestConstants.LOCAL_PATH, true);
            }            
        }

		/// <summary>
		///     Test that the update command brings the check file back
		///         down after it is deleted.
		/// </summary>
		[Test]
		public void UpdateTest () {
		    this.Checkout ();
		    File.Delete (checkFile);
		    
		    Assertion.Assert ("File should be gone now.  file=[" + checkFile + "]", !File.Exists (checkFile));
		    this.UpdateAllRecursive (rootDir);
		    Assertion.Assert ("Should have found the file.  file=[" + 
		                      checkFile + "]", File.Exists (checkFile));
		    
		    ICvsFile[] entries = 
		        this.manager.Fetch(rootDir, Factory.FileType.Entries);
            int found = 0;	
		    
		    String[] files = 
		        Directory.GetFiles (rootDir);
		    String[] directories = 
		        Directory.GetDirectories (rootDir);
		    // Minus the cvs directory
		    int total = files.Length + directories.Length - 1;
		    Assertion.Assert ("Count of directories and files should be equal to " +
		                      "the entries in the CVS/Entries file.  They are not.  " +
		                      "entriesCount=[" + entries.Length + "]" + 
		                      "files=[" + files.Length + "]" +
		                      "directories=[" + directories.Length + "]" +
		                      "total=[" + total + "]", 
		                      entries.Length == total);
		    foreach (ICvsFile cvsEntry in entries) {
		        Entry entry = (Entry)cvsEntry;
		        
		        System.Console.WriteLine ("entry=[" + entry + "]");
		        if (entry.Name.Equals (TestConstants.TARGET_FILE)) {
		            found++;
		        }
		    }
		    
		    Assertion.Assert ("Build file should have a cvs entry.", found == 1);
		    
		    // Had some problems with an extra module directory appearing under
		    //    the main working folder.
		    String doubleModuleDir = Path.Combine (rootDir, TestConstants.MODULE);
		    Assertion.Assert ("Should not be a module directory under root folder=[" + doubleModuleDir + "]",
		                      !Directory.Exists (doubleModuleDir));
		}
		
		private void UpdateAllRecursive (String rootDir, String overrideDirectory) {
            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        TestConstants.LOCAL_PATH, 
                                        TestConstants.MODULE);

            working.OverrideDirectory = overrideDirectory;
		    
            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
		    
            ICommand command = new UpdateCommand2 (working);
            Assertion.AssertNotNull ("Should have a command object.", command);
		    
            connection.Connect (working, TestConstants.PASSWORD_VALID);

            // Update all files...
            LOGGER.Debug ("Fetching all files from rootDir=[" + rootDir + "]");
            working.FoldersToUpdate = 
                this.manager.FetchFilesToUpdate (rootDir);
            
            command.Execute (connection);
            connection.Close ();		    
		}

        private void UpdateAllRecursive (String rootDir) {
            this.UpdateAllRecursive (rootDir, null);
        }
        
        /// <summary>
        ///     Test that a directory checked out as a revision is updated
        ///         successfully.
        /// </summary>
        [Test]
        public void UpdateRevisionTest () {
            this.Checkout (TestConstants.Revision.TAG_1);
            this.UpdateAllRecursive (this.rootDir);
            
            CheckoutModuleCommandTest.AssertFileContentsEqualString (this.checkFile, TestConstants.Revision.CONTENT_1);
        }
        
        /// <summary>
        ///     Test that a directory checked out with an override directory is
        ///         updated correctly.
        /// </summary>
        [Test]
        public void UpdateOverrideDirectoryTest () {
		    this.moduleDir = TestConstants.OVERRIDE_DIRECTORY;
		    this.rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, this.moduleDir);
		    this.checkFile = 
		        Path.Combine (rootDir, TestConstants.TARGET_FILE);

            this.Checkout (null, TestConstants.OVERRIDE_DIRECTORY);
		    File.Delete (checkFile);
		    
		    Assertion.Assert ("File should be gone now.  file=[" + checkFile + "]", !File.Exists (checkFile));
		    this.UpdateAllRecursive (rootDir, TestConstants.OVERRIDE_DIRECTORY);
		    Assertion.Assert ("Should have found the file.  file=[" + 
		                      checkFile + "]", File.Exists (checkFile));

        }
	}
}
