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
// 
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;

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
        
	    private String[] directories = 
	        {
                "conf",
	            "doc",
                "lib",
                "src"
	        };


        private readonly String ROOT_ENTRY = 
            ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        private readonly String REPOSITORY_ENTRY =
            "sharpcvslib";
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
            String path = Path.Combine (TestConstants.LOCAL_PATH, 
                            TestConstants.MODULE);
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Root.FILE_NAME);
        }
        
        /// <summary>
        ///     The cvs root entry was appearing twice (and more) times in the
        ///         cvs Root file.
        /// </summary>
        [Test]
        public void AddRootTwiceTest () {
            String path = TestConstants.LOCAL_PATH;
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Root.FILE_NAME);
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Root.FILE_NAME);
            
            this.verifyEntryCount (path, Root.FILE_NAME, 1);
        }

        /// <summary>
        ///     You should not be able to add a different root once you have
        ///         added a root.
        /// </summary> 
        [Test]       
        public void AddDiffRootTest () {
            String path = TestConstants.LOCAL_PATH;
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Root.FILE_NAME);
            this.AddCvsFileTest (path, this.ROOT_ENTRY + "/changed", Root.FILE_NAME);
            
            this.verifyEntryCount (path, Root.FILE_NAME, 1);            
        }
        
        /// <summary>
        ///     Test that a repository file can be created successfully.
        /// </summary>
        [Test]
        public void AddRepositoryTest () {
            String path = Path.Combine (TestConstants.LOCAL_PATH, 
                            TestConstants.MODULE);
            this.AddCvsFileTest (path, 
                                 this.REPOSITORY_ENTRY, 
                                 Repository.FILE_NAME);
        }
        
        private void AddCvsFileTest (String path, 
                                     String line, 
                                     String filename) {
            Factory factory = new Factory ();
            
            ICvsFile cvsEntry = factory.CreateCvsObject (path, filename, line);
            
            Manager manager = new Manager ();
            manager.Add (cvsEntry);
            
            String cvsPath = Path.Combine (path, manager.CVS);
            String file = Path.Combine (cvsPath, cvsEntry.Filename);
            
            Assertion.Assert ("File does not exist=[" + file + "]", 
                              File.Exists (file));
            String cvsUnderCvs = Path.Combine (cvsPath, manager.CVS);
            System.Console.WriteLine (cvsUnderCvs);
            Assertion.Assert ("File should not exist=[" + 
                                  cvsUnderCvs + "]",
                              !Directory.Exists (cvsUnderCvs));

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
		    
		    String path = Path.Combine (TestConstants.LOCAL_PATH, 
		                                TestConstants.MODULE);
		    
		    LOGGER.Debug ("Enter write many");
		    
		    this.WriteTestEntries (path);
		    		    
		    this.verifyEntryCount (path,
		                           Entry.FILE_NAME,
		                           this.cvsEntries.Length);
		    
		    string newEntry = 
		        "/MyNewFile.cs/1.1/Sun May 11 09:07:28 2003//";
		    
		    manager.Add (new Entry (path, newEntry));
		    
		    this.verifyEntryCount (path,
		                           Entry.FILE_NAME,
		                           this.cvsEntries.Length + 1);
		    
		}
		
		private void WriteTestEntries (String path) {
		    ArrayList entries = new ArrayList ();
		    Manager manager = new Manager ();
		    foreach (String cvsEntry in this.cvsEntries) {
	            LOGGER.Debug ("cvsEntry=[" + cvsEntry + "]");
		        entries.Add (new Entry (path, cvsEntry));
		    }
		    
		    manager.Add ((ICvsFile[])entries.ToArray (typeof (ICvsFile)));
		}
		
		private void WriteTestDirectoryEntries (String path) {
		    Manager manager = new Manager ();
            LOGGER.Debug ("path=[" + path + "]");
		    
		    Directory.CreateDirectory (Path.Combine (path, manager.CVS));
		    this.CreateDirAndCvsEntry (path, directories[0]);
		    this.CreateDirAndCvsEntry (path, directories[1]);
		    this.CreateDirAndCvsEntry (path, directories[2]);
		    this.CreateDirAndCvsEntry (path, directories[3]);
		}
		
		private void CreateDirAndCvsEntry (String path, String dirEntry) {
		    Manager manager = new Manager ();
		    String entryDir = Path.Combine (path, dirEntry);
		    String entryCvsDir = Path.Combine (entryDir, manager.CVS);
		    
		    Directory.CreateDirectory (entryCvsDir);
            this.AddCvsFileTest (entryDir, 
                                 this.REPOSITORY_ENTRY, 
                                 Repository.FILE_NAME);
		    this.AddCvsFileTest (entryDir,
		                         this.cvsEntries[0],
		                         Entry.FILE_NAME);
		                         
		    Entry entry = manager.CreateDirectoryEntry (entryDir);
		    manager.Add (entry);
		}
		
		/// <summary>
		///     Verify that the count of entries that is expected is found in
		///         the entries file.  If not then throw an assertion exception.
		/// </summary>
		/// <param name="path">The path to check for the entries in.</param>
		/// <param name="entriesExpected">The number of entries expected.</param>
		private void verifyEntryCount (String path,
		                               String cvsFilename,
		                               int entriesExpected) {
		    Manager manager = new Manager ();
		    ICvsFile[] currentEntries =
		        manager.Fetch (path, cvsFilename);
		    
		    int entriesFound = currentEntries.Length;
		    Assertion.Assert ("Should have found " + entriesExpected + 
		                      "entr(y)(ies) in the file " +
		                      "for each entry in our entries array.  " +
		                      "Instead found=[" + entriesFound + "]" +
		                      "but was expecting=[" + entriesExpected + "]", 
		                      entriesFound == entriesExpected);
		}

        /// <summary>
        ///     Tests that directory entries are created given the full local
        ///         path to the directory.
        /// </summary>		
		[Test]
		public void WriteDirectoryEntriesFromPathTest () {
		    Manager manager = new Manager ();
		    String rootDir = Path.Combine (TestConstants.LOCAL_PATH, 
		                                   TestConstants.MODULE);
		    
		    this.WriteTestDirectoryEntries (rootDir);
		    
		    ICvsFile[] currentEntries = 
		        manager.Fetch (rootDir, Entry.FILE_NAME);
		    
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
		
		[Test]
		public void AddDirectoryEntriesFromPath () {
		    const String NEW_DIRECTORY = "test";
		    Manager manager = new Manager ();
		    String path = Path.Combine (TestConstants.LOCAL_PATH, 
		                                TestConstants.MODULE);
		    
		    String newDirectory = Path.Combine (path, NEW_DIRECTORY);
		    Directory.CreateDirectory (newDirectory);
		    
		    manager.AddDirectories (path);
		    
		    try {
		        manager.Find (path, NEW_DIRECTORY);
		        Assertion.Assert ("If program got here a directory was found.  " +
		                          "Because there should not be a cvs entry under " +
		                          "the new directory this should not happen.",
		                          !true);
		    } catch (EntryNotFoundException) {}
            this.WriteTestEntries (path);
		    Directory.CreateDirectory (Path.Combine (newDirectory, manager.CVS));
		    
		    manager.AddDirectories (path);		    
		    Assertion.Assert ("Should contain the directory entry.", 
		                      null != manager.Find (path, NEW_DIRECTORY));
		    Assertion.Assert ("There should be no cvs entry above the root directory.",
		                      !Directory.Exists (Path.Combine (TestConstants.LOCAL_PATH,
		                                                       manager.CVS)));
		                      
		}
		
		/// <summary>
		///     Find all of the working folders in the cvs entries files and
		///         add them to the folders to update collection on the working
		///         directory object.
		/// </summary>
		[Test]
		public void FindAllWorkingFolders () {
		    Manager manager = new Manager ();
		    string rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE);

            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        rootDir, 
                                        TestConstants.MODULE);
		    
		    this.WriteTestDirectoryEntries (rootDir);
            this.AddCvsFileTest (rootDir, 
                                 this.REPOSITORY_ENTRY, 
                                 Repository.FILE_NAME);
            working.FoldersToUpdate = manager.FetchFilesToUpdate (rootDir);
		    
		    Assertion.Assert ("Working folders count should be greater than 1.",
		                      working.FoldersToUpdate.Length > 1);
		}
		
		/// <summary>
		///     Find all the working folders after a checkout.  Determine if all
		///         of the files have been found or if the find recursive missed some.
		/// </summary>
		[Test]
		public void FindAllWorkingFoldersAfterCheckout () {
		    Manager manager = new Manager ();
		    
		    String DEEP_REPOS_ENTRY = "Logger/src/com/sporadicism";
		    
		    String TARGET_ENTRIES_PATH = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE) + 
		        @"\src\com\sporadicism\util\logger";
		    
		    Entry[] TARGET_ENTRIES = {
		                        new Entry (TARGET_ENTRIES_PATH, 
		                                   "/EventLogger.cs/1.1/Wed Jan 08 06:26:41 2003//"),
                                new Entry (TARGET_ENTRIES_PATH,
                                           "/Log.cs/1.1/Wed Jan 08 06:26:41 2003//"),
                                new Entry (TARGET_ENTRIES_PATH,
                                           "/LogFactory.cs/1.1/Wed Jan 08 06:26:41 2003//"),
                                new Entry (TARGET_ENTRIES_PATH,
                                           "/LogName.cs/1.1/Wed Jan 08 06:26:41 2003//")
		    };
		    int TARGET_ENTRIES_COUNT = 4;
		    
		    this.Checkout ();
		    
		    String updateDir = Path.Combine (TestConstants.LOCAL_PATH, 
		                                     TestConstants.MODULE);
		    Folder[] folders = 
		        manager.FetchFilesToUpdate (updateDir);
		    
		    bool found = false;
		    int entriesFound = 0;
		    foreach (Folder folder in folders) {
	            if (DEEP_REPOS_ENTRY.Equals (folder.Repos.FileContents)) {
	                found = true;
	            }
		        foreach (Entry entry in folder.Entries) {
		            if (this.IsInEntries (entry, TARGET_ENTRIES)) {
		                entriesFound++;
		            }
                    System.Console.WriteLine ("entry=[" + entry + "]");		        
		        }
		    }
		    
		    Assertion.Assert ("Did not find file=[" + DEEP_REPOS_ENTRY + "]",
		                      found);
		    Assertion.AssertEquals ("Did not find entries count=[" + TARGET_ENTRIES_COUNT + "]",
		                      TARGET_ENTRIES_COUNT, entriesFound);
		    
		}
		
		private bool IsInEntries (Entry entry, Entry[] entries) {
		    foreach (Entry currentEntry in entries) {
		        if (currentEntry.Equals (entry)) {
		            return true;
		        }
		    }
		    return false;
		}
		
		private void Checkout () {
            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        TestConstants.LOCAL_PATH, 
                                        TestConstants.MODULE);

            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
		    
            ICommand command = new CheckoutModuleCommand (working);
            Assertion.AssertNotNull ("Should have a command object.", command);
		    
            connection.Connect (working, TestConstants.PASSWORD_VALID);

            command.Execute (connection);
            connection.Close ();
		}
		
		/// <summary>
		/// Test that a file not found exception does not propogate up 
		///     during a fetch.  This should be trapped and then keep on
		///     reading or return control gracefully.  All sub-folders
		///     cannot be expected to be under cvs control.
		/// </summary>
		[Test]
		public void NoBlowUpOnFileNotFoundEntries () {
		    Manager manager = new Manager ();
		    
		    string rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE);
		    
		    Directory.CreateDirectory (rootDir);
		    string cvsDir =
		        Path.Combine (rootDir, manager.CVS);
		    
		    Directory.CreateDirectory (cvsDir);
		    
		    try
		    {
		        manager.Fetch (rootDir, Entry.FILE_NAME);
		    } catch (FileNotFoundException) {
		        Assertion.Assert ("Should not be here, this should be trapped.", true);
		    }
		}
		
		/// <summary>
		///     Remove the local path directory that we were testing with.
		/// </summary>
		[TearDown]
		public void TearDown () {
//		    if (Directory.Exists (TestConstants.LOCAL_PATH)) {
//    		    Directory.Delete (TestConstants.LOCAL_PATH, true);
//		    }
		}
		
		/// <summary>
		///     Create an entries file and test the date to make sure that 
		///         it equals the value that we created it with.
		/// </summary>
		[Test]
		public void CreateEntriesDateTest () {
		    Manager manager = new Manager ();

		    string rootDir = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE);

            Entry entry = new Entry (rootDir, EntryTest.CHECKOUT_ENTRY_2);
		    
		    manager.Add (entry);
		    String filenameAndPath = Path.Combine (rootDir, entry.Name);
		    System.IO.StreamWriter writer = File.CreateText (filenameAndPath);
		    
		    writer.WriteLine ("This is a test file.");
		    
		    writer.Close ();
		    
		    DateTime fileTime = entry.TimeStamp;
		    
		    manager.SetFileTimeStamp (filenameAndPath, entry.TimeStamp);
		    
		    System.Console.WriteLine ("entry timestamp=[" + entry.TimeStamp + "]");
		    System.Console.WriteLine ("filestamp time=[" + 
                                      File.GetLastWriteTime (filenameAndPath) + "]");
		    
		    System.Console.WriteLine ("utc offset=[" +
		                              System.TimeZone.CurrentTimeZone.GetUtcOffset 
		                                  (File.GetLastWriteTime (filenameAndPath)) + "]");
		    System.Console.WriteLine ("utc offset=[" +
		                              System.TimeZone.CurrentTimeZone.GetUtcOffset 
		                                  (DateTime.Now) + "]");
		    System.Console.WriteLine ("daylight savings=[" +
		                              System.TimeZone.CurrentTimeZone.IsDaylightSavingTime 
		                                  (DateTime.Now) + "]");


            if (System.TimeZone.CurrentTimeZone.IsDaylightSavingTime (DateTime.Now)) {
    		    Assertion.AssertEquals (entry.TimeStamp, 
    		                            File.GetLastWriteTime(filenameAndPath).AddHours (1));
            }
            else {
    		    Assertion.AssertEquals (entry.TimeStamp, 
    		                            File.GetLastWriteTime(filenameAndPath));
            }
		    
		}


    }
}
