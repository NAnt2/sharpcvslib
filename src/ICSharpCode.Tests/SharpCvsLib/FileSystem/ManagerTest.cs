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
//    Author: Clayton Harbour
//     claytonharbour@sporadicism.com
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    
    /// <summary>
    ///     Test the functions of the file system manager.
    /// </summary>
    [TestFixture]
    public class ManagerTest {
        
	    private String[] cvsEntries = 
	        {
	            "/CvsFileManager.cs/1.1/Sun May 11 08:02:05 2003//",
	            "/SharpCvsLib.build/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.cmbx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.prjx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.Tests.prjx/1.1/Sun May 11 18:02:05 2003//",
                "D/conf////"
	        };

        private readonly String ROOT_ENTRY = 
            ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (ManagerTest));
        
        /// <summary>
        ///     Constructor.
        /// </summary>
        public ManagerTest () {
            
        }
        /// <summary>
        ///     Test that an entry file can be correctly determined and added
        ///         to the correct file.
        /// </summary>
        [Test]
        public void AddRootTest () {
            ICvsFile root = new Root (TestConstants.LOCAL_PATH, this.ROOT_ENTRY);
            
            Manager manager = new Manager ();
            manager.Add (root);
            
            String cvsPath = Path.Combine (TestConstants.LOCAL_PATH, "CVS");
            String file = Path.Combine (cvsPath, root.Filename);
            Assertion.Assert ("File does not exist=[" + file + "]", 
                              File.Exists (file));
            String shouldNotExist = Path.Combine (cvsPath, "CVS");
            System.Console.WriteLine (shouldNotExist);
            Assertion.Assert ("File should not exist=[" + 
                                  shouldNotExist + "]",
                              !Directory.Exists (shouldNotExist));
        }
        
		/// <summary>
		///     Insert a number of entries into the cvs entries file.
		///     
		///     Verify that all entries were added correctly.
		/// 
		///     Add another entry to the file that has the same name as another
		///         entry.  
		/// 
		///     Verify that there are not duplicate entries.
		/// </summary>
		[Test]
		public void WriteManyEntriesThenAddOneSame () {
		    Manager manager = new Manager ();
		    ArrayList entries = new ArrayList ();
		    
		    String path = TestConstants.LOCAL_PATH;
		    
		    LOGGER.Debug ("Enter write many");
		    foreach (String cvsEntry in this.cvsEntries) {
	            LOGGER.Debug ("cvsEntry=[" + cvsEntry + "]");
		        entries.Add (new Entry (path, cvsEntry));
		    }
		    
		    manager.Add ((ICvsFile[])entries.ToArray (typeof (ICvsFile)));
		    		    
		    this.verifyEntryCount (TestConstants.LOCAL_PATH, 
		                           this.cvsEntries.Length);
		    
		    string newEntry = 
		        "/MyNewFile.cs/1.1/Sun May 11 09:07:28 2003//";
		    
		    manager.Add (new Entry (path, newEntry));
		    
		    this.verifyEntryCount (TestConstants.LOCAL_PATH,
		                           this.cvsEntries.Length + 1);
		    
		}
		
		/// <summary>
		///     Verify that the count of entries that is expected is found in
		///         the entries file.  If not then throw an assertion exception.
		/// </summary>
		/// <param name="path">The path to check for the entries in.</param>
		/// <param name="entriesExpected">The number of entries expected.</param>
		private void verifyEntryCount (String path, int entriesExpected) {
		    Manager manager = new Manager ();
		    ICvsFile[] currentEntries =
		        manager.Fetch (path, Entry.FILE_NAME);
		    
		    int entriesFound = currentEntries.Length;
		    Assertion.Assert ("Should have found " + entriesExpected + 
		                      "entr(y)(ies) in the file " +
		                      "for each entry in our entries array.  " +
		                      "Instead found=[" + entriesFound + "]" +
		                      "but was expecting=[" + entriesExpected + "]", 
		                      entriesFound == entriesExpected);
		}
		
		/// <summary>
		///     Remove the test directory that we were working with.
		/// </summary>
		[TearDown]
		public void TearDown () {
		    // TODO: Uncomment this when the fear of deleting something wrong goes away.
		    //Directory.Delete (TestConstants.LOCAL_PATH);
		}
		
		[Test]
		public void WriteDirectoryEntriesFromPathTest () {
		    Manager manager = new Manager ();
		    String rootDir = Path.Combine (TestConstants.LOCAL_PATH, 
		                                   TestConstants.MODULE);
		    
		    LOGGER.Debug ("rootDir=[" + rootDir + "]");
		    String[] directories = {"conf",
		                            "doc",
		                            "lib",
		                            "src"};
		    
		    manager.Add (manager.CreateDirectoryEntry (Path.Combine (rootDir,
		                                                             directories[0])));
		    manager.Add (manager.CreateDirectoryEntry (Path.Combine (rootDir,
		                                                             directories[1])));
		    manager.Add (manager.CreateDirectoryEntry (Path.Combine (rootDir, 
		                                                             directories[2])));
		    manager.Add (manager.CreateDirectoryEntry (Path.Combine (rootDir,
		                                                             directories[3])));
		    
		    ICvsFile[] currentEntries = 
		        manager.Fetch (rootDir, Entry.FILE_NAME);
		    
		    Assertion.Assert ("Current entries should = directory count, which is 4", 
		                      currentEntries.Length == 4);
		    
		    int found = 0;
		    foreach (Entry entry in currentEntries) {
		        foreach (String directory in directories) {
		            string dirName = 
		                directory.Substring (directory.LastIndexOf ('/') + 1);
		            if (entry.Name.Equals (dirName)) {
		                found++;
		            }
		        }
		    }
		    Assertion.Assert ("Did not find all directory names in entries file." +
		                      "looking for=[4] and found=[" + found + "]", 
		                      4 == found);
		                      
		}


    }
}
