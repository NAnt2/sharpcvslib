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
		    this.manager = new Manager ();
		    CheckoutModuleCommandTest checkout =
		        new CheckoutModuleCommandTest ();
		    checkout.CheckoutTest ();
		}
		
		/// <summary>
		///     Test that the update command brings the build file back
		///         down after it is deleted.
		/// </summary>
		[Test]
		public void UpdateTest () {
		    string rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE);
		    string buildFile = 
		        Path.Combine (rootDir, TestConstants.TARGET_FILE);
		    
		    //Assertion.Assert ("File should be there, have not deleted it yet.  " +
		    //                  "file=[" + buildFile + "]",
		    //                  File.Exists (buildFile));
		    
		    File.Delete (buildFile);
		    
		    Assertion.Assert ("File should be gone now.  file=[" + buildFile + "]", !File.Exists (buildFile));
            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        TestConstants.LOCAL_PATH, 
                                        TestConstants.MODULE);

            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
		    
            ICommand command = new UpdateCommand2 (working);
            Assertion.AssertNotNull ("Should have a command object.", command);
		    
            connection.Connect (working, TestConstants.PASSWORD_VALID);

            // Update all files...
            working.FoldersToUpdate = 
                this.manager.FetchFilesToUpdate (rootDir);
            
            command.Execute (connection);
            connection.Close ();
		    
		    Assertion.Assert ("Should have found the build file.  file=[" + 
		                      buildFile + "]", File.Exists (buildFile));
		    
		    ICvsFile[] entries = 
		        this.manager.Fetch(rootDir, Entry.FILE_NAME);
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
		}
		
		/// <summary>
		///     Remove the local path directory that we were testing with.
		/// </summary>
		[TearDown]
		public void TearDown () {
		    Directory.Delete (TestConstants.LOCAL_PATH, true);
		}


	}
}
