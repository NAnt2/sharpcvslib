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
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Misc {
	/// <summary>
	/// Summary description for CustomDBTest.
	/// </summary>
	[TestFixture]
	public class CvsFileManagerTest	{
		private ILog LOGGER = 
			LogManager.GetLogger (typeof(CvsFileManagerTest));
	    
	    private String[] cvsEntries = 
	        {
	            "/CvsFileManager.cs/1.1/Sun May 11 08:02:05 2003//",
	            "/SharpCvsLib.build/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.cmbx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.prjx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.Tests.prjx/1.1/Sun May 11 18:02:05 2003//",
                "D/conf////"
	        };
	    
	    private CvsFileManager manager;
		
		/// <summary>
		/// Constructor for customer db test.
		/// </summary>
		public CvsFileManagerTest () {
		}

        /// <summary>
        ///     Perform setup operations for the test.  Create a new
        ///         file manager object.
        /// </summary>
        [SetUp]
        public void SetUp () {
            this.manager = new CvsFileManager ();
        }
        
        [TearDown]
        public void TearDown () {
//            string cvsFile = 
//                Path.Combine (TestConstants.LOCAL_PATH, this.manager.ENTRIES);
//            File.Delete (cvsFile);
        }
        
		/// <summary>
		/// Test that a cvs entry is added to the correct location and 
		///     contains the correct data.  This tests read and write 
		///     functionality.
		/// </summary>
		[Test]
		public void EntryWriteReadTest ()	{
		    Entry entry = new Entry (cvsEntries[0]);
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, entry);

		    string entryFile = 
		        Path.Combine (TestConstants.LOCAL_PATH, this.manager.ENTRIES);
		    Assertion.Assert ("Missing file=[" + entryFile + "]", 
		                      File.Exists (entryFile));
		    
		    ICollection entries;
		    entries = this.manager.ReadEntries (TestConstants.LOCAL_PATH);
		    Assertion.Assert ("There should only be 1 entry, found=[" + entries.Count + "]",
		                      entries.Count == 1);

            IEnumerator entryEnumerator = entries.GetEnumerator ();
            entryEnumerator.MoveNext ();
            Entry readEntry = (Entry)entryEnumerator.Current;
            Assertion.Assert ("Cvs entry should match the entryString.", 
                              readEntry.CvsEntry.Equals (cvsEntries[0]));
		}
		
		// TODO: Implement this method when I start doing cvs add and removes, etc.
		public void EntryLogWriteReadTest () {
		    Entry entry = new Entry (this.cvsEntries[0]);
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, entry);

		    string entryFile = 
		        Path.Combine (TestConstants.LOCAL_PATH, this.manager.ENTRIES);
		    Assertion.Assert ("Missing file=[" + entryFile + "]", 
		                      File.Exists (entryFile));

            const string addEntryString = 
                "A/SharpCvsLib.build/1.1/Sun May 11 09:07:28 2003//";
		    Entry logEntry = new Entry (addEntryString);
		    this.manager.AddLogEntry (TestConstants.LOCAL_PATH, logEntry);
		}
				
		[Test]
		public void WriteDirectoryEntriesFromPathTest () {
		    String[] directories = {"/sharpcvslib-tests/conf",
		                            "/sharpcvslib-tests/doc",
		                            "/sharpcvslib-tests/lib",
		                            "/sharpcvslib-tests/src"};
		    
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, 
		                           this.manager.CreateDirectoryEntryFromPath (directories[0]));
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, 
		                           this.manager.CreateDirectoryEntryFromPath (directories[1]));
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, 
		                           this.manager.CreateDirectoryEntryFromPath (directories[2]));
		    this.manager.AddEntry (TestConstants.LOCAL_PATH, 
		                           this.manager.CreateDirectoryEntryFromPath (directories[3]));
		    
		    ICollection currentEntries = 
		        this.manager.ReadEntries (TestConstants.LOCAL_PATH);
		    
		    Assertion.Assert ("Current entries should = directory count, which is 4", 
		                      currentEntries.Count == 4);
		    
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
		
		private void verifyEntryCount (String path, int entriesExpected) {
		    ICollection currentEntries =
		        this.manager.ReadEntries (path);
		    
		    int entriesFound = currentEntries.Count;
		    Assertion.Assert ("Should have found " + entriesExpected + 
		                      "entr(y)(ies) in the file " +
		                      "for each entry in our entries array.  " +
		                      "Instead found=[" + entriesFound + "]" +
		                      "but was expecting=[" + entriesExpected + "]", 
		                      entriesFound == entriesExpected);
		}
	}
}
