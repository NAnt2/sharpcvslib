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
using System.Text;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;

using ICSharpCode.SharpCvsLib.Tests;
using ICSharpCode.SharpCvsLib.Tests.Config;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    ///     Test the functions of the file system manager.
    /// </summary>
    [TestFixture]
    public class ManagerTest : AbstractTest {
        private SharpCvsLibTestsConfig settings = 
            SharpCvsLibTestsConfig.GetInstance();

        private String[] cvsEntries =
            {
                "/CvsFileManager.cs/1.1/Sun May 11 08:02:05 2003//",
                "/SharpCvsLib.build/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.cmbx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.prjx/1.1/Sun May 11 18:02:05 2003//",
                "/SharpCvsLib.Tests.prjx/1.1/Sun May 11 18:02:05 2003//",
                "D/conf////",
                "D/test////"
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
            String path = Path.Combine (this.settings.Config.LocalPath,
                                        this.settings.Config.Module);
            this.AddCvsFileTest (path + Path.DirectorySeparatorChar, 
                this.ROOT_ENTRY, Factory.FileType.Root);
        }

        /// <summary>
        ///     The cvs root entry was appearing twice (and more) times in the
        ///         cvs Root file.
        /// </summary>
        [Test]
        public void AddRootTwiceTest () {
            String path = this.settings.Config.LocalPath + Path.DirectorySeparatorChar;
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Factory.FileType.Root);
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Factory.FileType.Root);

            this.verifyEntryCount (path, Factory.FileType.Root, 1);
        }

        /// <summary>
        ///     You should not be able to add a different root once you have
        ///         added a root.
        /// </summary>
        [Test]
        public void AddDiffRootTest () {
            String path = this.settings.Config.LocalPath + Path.DirectorySeparatorChar;
            this.AddCvsFileTest (path, this.ROOT_ENTRY, Factory.FileType.Root);
            this.AddCvsFileTest (path, this.ROOT_ENTRY + "/changed",
                                Factory.FileType.Root);

            this.verifyEntryCount (path, Factory.FileType.Root, 1);
        }

        /// <summary>
        ///     Test that a repository file can be created successfully.
        /// </summary>
        [Test]
        public void AddRepositoryTest () {
            String path = Path.Combine (this.settings.Config.LocalPath,
                                        this.settings.Config.Module);
            this.AddCvsFileTest (path + Path.DirectorySeparatorChar,
                                this.REPOSITORY_ENTRY,
                                Factory.FileType.Repository);
        }

        private void AddCvsFileTest (String path,
                                    String line,
                                    Factory.FileType fileType) {
            Factory factory = new Factory ();

            ICvsFile cvsEntry = factory.CreateCvsObject (path,
                                fileType,
                                line);

            Manager manager = 
                new Manager (Path.Combine(this.settings.LocalPath, this.settings.Module));
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
            Manager manager = 
                new Manager (Path.Combine(this.settings.LocalPath, this.settings.Module));

            String modulePath = Path.Combine (this.settings.Config.LocalPath,
                                        this.settings.Config.Module);

            LOGGER.Debug ("Enter write many");

            this.WriteTestEntries (modulePath);
            this.verifyEntryCount (modulePath,
                                Factory.FileType.Entries,
                                this.cvsEntries.Length);

            string newEntry =
                "/MyNewFile.cs/1.1/Sun May 11 09:07:28 2003//";
            manager.Add (new Entry (modulePath, newEntry));
            this.verifyEntryCount (modulePath,
                                Factory.FileType.Entries,
                                this.cvsEntries.Length + 1);

            Assertion.Assert(!Directory.Exists(Path.Combine(this.GetTempPath(), "CVS")));

        }

        /// <summary>
        /// TODO: CHANGE THIS TO USE THE ENTRIES OBJECT, INSTEAD OF AN ARRAY LIST.
        /// </summary>
        /// <param name="path"></param>
        private void WriteTestEntries (String path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            ArrayList entries = new ArrayList();
            Manager manager = new Manager (path);
            foreach (String cvsEntry in this.cvsEntries) {
                LOGGER.Debug ("cvsEntry=[" + cvsEntry + "]");
                LOGGER.Debug("path=[" + path + "]");
                if (!PathTranslator.ContainsCVS(path)) {
                    Entry entry = new Entry (path, cvsEntry);
                    entries.Add (entry);
                    String newFile = entry.FullPath;
                    StreamWriter writer;
                    if (!entry.IsDirectory) {
                        LOGGER.Debug("Can't write to a directory=[" + entry.FullPath + "]");
                        writer = File.CreateText(newFile);
                        writer.WriteLine("Test file");
                        writer.Close();
                    }
                    LOGGER.Debug("\n\n\tCreate test file=[" + newFile + "]");
                    LOGGER.Debug("\n\n\tCreate test file entry=[" + entry + "]");
                }
            }

            manager.Add ((ICvsFile[])entries.ToArray (typeof (ICvsFile)));

            Entries entriesTest = manager.FetchEntries(path);

            Assertion.Assert(File.Exists(Path.Combine(Path.Combine(path, "CVS"), "Entries")));
        }

        private void WriteTestDirectoryEntries (String path) {
            Manager manager = new Manager (path);
            LOGGER.Debug ("path=[" + path + "]");

            Directory.CreateDirectory (Path.Combine (path, manager.CVS));
            this.CreateDirAndCvsEntry (path, directories[0]);
            this.CreateDirAndCvsEntry (path, directories[1]);
            this.CreateDirAndCvsEntry (path, directories[2]);
            this.CreateDirAndCvsEntry (path, directories[3]);
        }

        private void CreateDirAndCvsEntry (String path, String dirEntry) {
            Manager manager = new Manager (path);
            String entryDir = Path.Combine (path, dirEntry);
            String entryCvsDir = Path.Combine (entryDir, manager.CVS);

            Directory.CreateDirectory (entryCvsDir);
            this.AddCvsFileTest (entryDir,
                                this.REPOSITORY_ENTRY,
                                Factory.FileType.Repository);
            this.AddCvsFileTest (entryDir,
                                this.cvsEntries[0],
                                Factory.FileType.Entries);

            Entry entry = manager.CreateDirectoryEntry (entryDir);
            manager.Add (entry);
        }

        /// <summary>
        ///     Verify that the count of entries that is expected is found in
        ///         the entries file.  If not then throw an assertion exception.
        /// </summary>
        /// <param name="path">The path to check for the entries in.</param>
        /// <param name="fileType">The type of file that is being checked.</param>
        /// <param name="entriesExpected">The number of entries expected.</param>
        private void verifyEntryCount (String path,
                                    Factory.FileType fileType,
                                    int entriesExpected) {
            Manager manager = new Manager (path);
            ICvsFile[] currentEntries =
                manager.Fetch (path, fileType);
            LOGGER.Error("path=[" + path + "]");
            int entriesFound = currentEntries.Length;
            Assertion.AssertEquals (entriesExpected, entriesFound);
        }

        /// <summary>
        /// Test that only the directories that contain a CVS directory with an
        ///     Entries file are added to the cvs Entries management file in the 
        ///     folder above.
        ///     
        ///     The WriteTestEntries method creates two directories that match this
        ///         criterea, the doc and the src directory.
        /// </summary>
        [Test]
        public void WriteDirectoriesWithCvs () {
            String modulePath = Path.Combine(this.settings.LocalPath, 
                this.settings.Module);
            Manager manager = new Manager (modulePath);

            foreach (String directory in directories) {
                String newDir = Path.Combine(modulePath, directory);
                String newCvsDir = Path.Combine(newDir, manager.CVS);
                String entriesFile = Path.Combine(newCvsDir, Entry.FILE_NAME);
                Directory.CreateDirectory(newDir);
                Directory.CreateDirectory(newCvsDir);
                FileStream stream = File.Create(entriesFile);
                stream.Close();
            }
            manager.AddDirectories(modulePath);

            Entries entries = manager.FetchEntries(modulePath + Path.DirectorySeparatorChar);
            Assertion.Assert(entries.Contains(Path.Combine(modulePath, directories[1])));

            Assertion.Assert(entries.Contains(Path.Combine(modulePath, directories[3])));

            foreach (DictionaryEntry entryDic in entries) {
                LOGGER.Debug("entry=[" + entryDic.Value + "]");
            }
            Assertion.AssertEquals 
                ("Did not find all directory names in entries file.", 4, entries.Count);
        }

        /// <summary>
        ///     Test that all directories are added to an entries file if the
        ///         path is specified that contains those directories.
        /// </summary>
        [Test]
        public void AddDirectoryEntriesFromPath () {
            const String NEW_DIRECTORY = "test";
            String path = Path.Combine (this.settings.Config.LocalPath,
                                        this.settings.Config.Module);
            Manager manager = new Manager (path);

            String newDirectory = Path.Combine (path, NEW_DIRECTORY);
            Directory.CreateDirectory (newDirectory);

            if (!PathTranslator.ContainsCVS(path)) {
                manager.AddDirectories (path);
            }

// Not sure why this is here.
//            try {
//                manager.Find (path, NEW_DIRECTORY);
//                Assertion.Assert ("If program got here a directory was found.  " +
//                                "Because there should not be a cvs entry under " +
//                                "the new directory this should not happen.",
//                                !true);
//            } catch (EntryNotFoundException) {}
            this.WriteTestEntries (path);
            Directory.CreateDirectory (Path.Combine (newDirectory, manager.CVS));

            manager.AddDirectories (path);
            Assertion.Assert ("Should contain the directory entry.",
                            null != manager.Find (path, NEW_DIRECTORY));
            Assertion.Assert ("There should be no cvs entry above the root directory.",
                            !Directory.Exists (Path.Combine (this.settings.Config.LocalPath,
                                                            manager.CVS)));
            String modulePath = 
                Path.Combine(this.settings.Config.LocalPath, this.settings.Module);
            Assertion.Assert("Should not create cvs entry for module path.",
                !Directory.Exists(Path.Combine(modulePath, this.settings.Module)));

            Entries entries = manager.FetchEntries(modulePath);
            entries.Contains(Path.Combine(modulePath, "CvsFileManager"));
            entries.Contains(Path.Combine(modulePath, "SharpCvsLib.build"));
            entries.Contains(Path.Combine(modulePath, "SharpCvsLib.cmbx"));
            entries.Contains(Path.Combine(modulePath, "SharpCvsLib.prjx"));
            entries.Contains(Path.Combine(modulePath, "SharpCvsLib.Tests.prjx"));
            entries.Contains(Path.Combine(modulePath, "conf"));
            entries.Contains(Path.Combine(modulePath, "test"));
        }

        /// <summary>
        ///     Find all of the working folders in the cvs entries files and
        ///         add them to the folders to update collection on the working
        ///         directory object.
        /// </summary>
        [Test]
        public void FindAllWorkingFolders () {
            Manager manager = 
                new Manager (Path.Combine(this.settings.LocalPath, this.settings.Module));
            string rootDir =
                Path.Combine (this.settings.Config.LocalPath, this.settings.Config.Module);

            CvsRoot root = new CvsRoot (this.settings.Config.Cvsroot);
            WorkingDirectory working =
                new WorkingDirectory (root,
                                    rootDir,
                                    this.settings.Config.Module);

            this.WriteTestDirectoryEntries (rootDir);
            this.AddCvsFileTest (rootDir,
                                this.REPOSITORY_ENTRY,
                                Factory.FileType.Repository);
            working.FoldersToUpdate = manager.FetchFilesToUpdate (rootDir);

            Assertion.Assert ("Working folders count should be greater than 1.",
                            working.FoldersToUpdate.Length > 1);
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
            CvsRoot root = new CvsRoot (this.settings.Config.Cvsroot);
            WorkingDirectory working =
                new WorkingDirectory (root,
                                    this.settings.Config.LocalPath,
                                    this.settings.Config.Module);

            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);

            ICommand command = new CheckoutModuleCommand (working);
            Assertion.AssertNotNull ("Should have a command object.", command);

            connection.Connect (working, this.settings.Config.ValidPassword);

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
            string rootDir =
                Path.Combine (this.settings.Config.LocalPath, this.settings.Config.Module);
            Manager manager = new Manager (rootDir);

            Directory.CreateDirectory (rootDir);
            string cvsDir =
                Path.Combine (rootDir, manager.CVS);

            Directory.CreateDirectory (cvsDir);

            try
            {
                manager.Fetch (rootDir, Factory.FileType.Entries);
            } catch (FileNotFoundException) {
                Assertion.Assert ("Should not be here, this should be trapped.", true);
            }
        }

        /// <summary>
        ///     Create an entries file and test the date to make sure that
        ///         it equals the value that we created it with.
        /// </summary>
        [Test]
        public void CreateEntriesDateTest () {
            string rootDir =
                Path.Combine (this.settings.Config.LocalPath, this.settings.Config.Module);
            Manager manager = new Manager (rootDir);

            Entry entry = new Entry (rootDir, EntryTest.CHECKOUT_ENTRY_2);

            manager.Add (entry);
            String filenameAndPath = Path.Combine (rootDir, entry.Name);
            System.IO.StreamWriter writer = File.CreateText (filenameAndPath);

            writer.WriteLine ("This is a test file.");

            writer.Close ();

            DateTime fileTime = entry.TimeStamp;

            manager.SetFileTimeStamp (filenameAndPath, entry.TimeStamp, entry.IsUtcTimeStamp);

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

            // TODO: This is a bad test, however the DateTime.ToUniversalTime () method
            //    is broken in .net 1.0
            Assertion.AssertEquals (entry.TimeStamp,
                                    File.GetLastWriteTime(filenameAndPath).Subtract (System.TimeZone.CurrentTimeZone.GetUtcOffset (entry.TimeStamp)));

        }

        /// <summary>
        /// Test that all the folders for the given path are fetched and populated.
        /// </summary>
        [Test]
        public void GetFoldersTest () {
            this.Checkout();

            String modulePath = Path.Combine (this.settings.Config.LocalPath,
                this.settings.Config.Module);

            Manager manager = new Manager (modulePath);
            Folders folders = manager.GetFolders (modulePath);

            LOGGER.Debug("Folders=[" + folders + "]");

            Assertion.AssertEquals(2, folders.Count);
            Assertion.Assert(folders.Contains(modulePath));
            Assertion.Assert(folders.Contains(Path.Combine(modulePath, "src")));
        }

        /// <summary>
        /// Test that the get cvs directory class is performing correctly and
        ///     returning the correct cvs directory.
        /// </summary>
        [Test]
        public void GetCvsDirTest () {
            String[] cvsDirIn = {
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\someFile.txt",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\someFile",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src"
            };
            String[] cvsDirOut = {
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\CVS",
                @"C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\CVS"
            };

            // NOTE: The tests have to be executed in order because the act of getting a 
            //  CVS directory creates a sub folder, therefore getting the src directory
            //  will create the module directory, etc.
            String workingPath = Path.Combine(this.settings.LocalPath, this.settings.Module);
            Manager manager = new Manager(workingPath);
            for (int i = 0; i < cvsDirIn.Length; i++) {
                Assertion.AssertEquals("cvsDir number=[" + i + "]", 
                    cvsDirOut[i], manager.GetCvsDir(Entry.CreateEntry(cvsDirIn[i])));
            }
        }
    }
}
