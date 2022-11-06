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
//    Author: Gerald Evans
#endregion

using System;
using System.Collections;
using System.IO;

using log4net;
using NUnit.Framework;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Tests.Config;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Tests the PathTranslator.
    ///
    /// Note: This test is also dependent on the correct functioning
    /// of WorkingDirectory and CvsRoot.
    /// </summary>
    [TestFixture]
    public class PathTranslatorTest	{
        private ILog LOGGER =
            LogManager.GetLogger (typeof(EntryTest));
        private String moduleDir;

        private SharpCvsLibTestsConfig settings = 
            SharpCvsLibTestsConfig.GetInstance();

        private const String ROOT_ENTRY1 =
            ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        // TODO: need *nix style dir when testing on *nix
        private String LOCAL_ROOT_DIR1
        {
            get {
                if (IsWindows) {
                return "Z:\\dev-src";
            } else if (IsUnix) {
                return "/tmp/dev-src";
            } else {
                String msg = "Path seperator unknown.";
                throw new Exception (msg);
                }
            }
        }
        private String LOCAL_DIR1 {
            get {return Path.Combine (LOCAL_ROOT_DIR1, moduleDir);}
        }

        private bool IsUnix {
            get {return Path.DirectorySeparatorChar.Equals ('/');}
        }

        private bool IsWindows {
            get {return Path.DirectorySeparatorChar.Equals ('\\');}
        }

        private String ModuleDir {
            get {return this.moduleDir;}
            set {this.moduleDir = value;}
        }
        private const String REPOS_NAME1 = "sharpcvslib";
        private const String REPOS_FILE_PATH1 = "/cvsroot/sharpcvslib/sharpcvslib/src/ICSharpCode/SharpCvsLib/FileSystem/PathTranslator.cs";
        private const String REPOS_DIR_PATH1 = "/cvsroot/sharpcvslib/sharpcvslib/src/ICSharpCode/SharpCvsLib/FileSystem/";
        private const String REPOS_FILE_PATH2 = "/cvsroot/sharpcvslib/sharpcvslib/src/ICSharpCode/SharpCvsLib/FileSystem/Sharp";
        private const String REPOS_DIR_PATH2 = "/home/cvs/src/./";

        /// <summary>
        /// Constructor for customer db test.
        /// </summary>
        public PathTranslatorTest () {
        }

        /// <summary>
        ///     Perform setup operations for the test.  Create a new
        ///         file manager object.
        /// </summary>
        [SetUp]
        public void SetUp () {
            this.moduleDir = REPOS_NAME1;
        }

        /// <summary>
        ///     Clean up any test directories, etc.
        /// </summary>
        [TearDown]
        public void TearDown () {
            if (Directory.Exists (this.settings.Config.LocalPath)) {
                Directory.Delete (this.settings.Config.LocalPath, true);
            }
        }

        /// <summary>
        /// Test translation of a file path
        /// </summary>
        [Test]
        public void TestFilePathTranslation () {
            CvsRoot cvsRoot;
            WorkingDirectory workingDirectory;
            PathTranslator pathTranslator;

            cvsRoot = new CvsRoot (ROOT_ENTRY1);
            workingDirectory = new WorkingDirectory (cvsRoot, LOCAL_ROOT_DIR1, REPOS_NAME1);
            pathTranslator = new PathTranslator (workingDirectory, REPOS_FILE_PATH1);

            LOGGER.Debug ("pathTranslator=[" + pathTranslator.ToString () + "]");
            Assertion.Assert (pathTranslator.CvsRoot.CvsRepository,
                            pathTranslator.CvsRoot.CvsRepository.Equals ("/cvsroot/sharpcvslib"));
            Assertion.AssertEquals ("src/ICSharpCode/SharpCvsLib/FileSystem/", pathTranslator.RelativePath);
            Assertion.Assert (pathTranslator.Filename, pathTranslator.Filename.Equals ("PathTranslator.cs"));
            String expectedLocalPath =
                PathTranslator.ConvertToOSSpecificPath (Path.Combine (LOCAL_DIR1, "src/ICSharpCode/SharpCvsLib/FileSystem/"));
            Assertion.AssertEquals (pathTranslator.LocalPath, expectedLocalPath, pathTranslator.LocalPath);
            Assertion.Assert (pathTranslator.LocalPathAndFilename, pathTranslator.LocalPathAndFilename.Equals (Path.Combine (expectedLocalPath, "PathTranslator.cs")));
            Assertion.Assert (pathTranslator.IsDirectory == false);
        }

        /// <summary>
        /// Test translation of a directory path
        /// </summary>
        [Test]
        public void TestDirPathTranslation () {
            CvsRoot cvsRoot;
            WorkingDirectory workingDirectory;
            PathTranslator pathTranslator;

            cvsRoot = new CvsRoot (ROOT_ENTRY1);
            workingDirectory = new WorkingDirectory (cvsRoot, LOCAL_ROOT_DIR1, REPOS_NAME1);
            pathTranslator = new PathTranslator (workingDirectory, REPOS_DIR_PATH1);

            Assertion.Assert (pathTranslator.CvsRoot.CvsRepository,
                            pathTranslator.CvsRoot.CvsRepository.Equals ("/cvsroot/sharpcvslib"));
            Assertion.AssertEquals (pathTranslator.RelativePath, "src/ICSharpCode/SharpCvsLib/FileSystem/", pathTranslator.RelativePath);
            Assertion.AssertEquals (Path.DirectorySeparatorChar.ToString(), pathTranslator.Filename);
            String expectedLocalPath =
                PathTranslator.ConvertToOSSpecificPath (Path.Combine (LOCAL_DIR1, "src/ICSharpCode/SharpCvsLib/FileSystem/"));
            Assertion.AssertEquals (pathTranslator.LocalPath, expectedLocalPath, pathTranslator.LocalPath);
            Assertion.AssertEquals (expectedLocalPath, pathTranslator.LocalPathAndFilename);
            Assertion.Assert (pathTranslator.IsDirectory == true);
        }

        /// <summary>
        /// Test translation of a file path where the filename is repeated somewhere in the path.
        /// This exploits a know problem in release 1.3 of PathTranslator.
        /// </summary>
        [Test]
        public void TestRepeatedFileComponent () {
            CvsRoot cvsRoot;
            WorkingDirectory workingDirectory;
            PathTranslator pathTranslator;

            cvsRoot = new CvsRoot (ROOT_ENTRY1);
            workingDirectory = new WorkingDirectory (cvsRoot, LOCAL_ROOT_DIR1, REPOS_NAME1);
            pathTranslator = new PathTranslator (workingDirectory, REPOS_FILE_PATH2);

            Assertion.Assert (pathTranslator.CvsRoot.CvsRepository, pathTranslator.CvsRoot.CvsRepository.Equals ("/cvsroot/sharpcvslib"));
            Assertion.AssertEquals ("src/ICSharpCode/SharpCvsLib/FileSystem/", pathTranslator.RelativePath);
            Assertion.Assert (pathTranslator.Filename, pathTranslator.Filename.Equals ("Sharp"));
            String expectedLocalPath =
                PathTranslator.ConvertToOSSpecificPath (Path.Combine (LOCAL_DIR1, "src/ICSharpCode/SharpCvsLib/FileSystem/"));
            Assertion.Assert (pathTranslator.LocalPath, pathTranslator.LocalPath.Equals (expectedLocalPath));
            Assertion.Assert (pathTranslator.LocalPathAndFilename, pathTranslator.LocalPathAndFilename.Equals (Path.Combine (expectedLocalPath, "Sharp")));
            Assertion.Assert (pathTranslator.IsDirectory == false);
        }

        /// <summary>
        /// Test that the contains cvs directory correctly identifies a cvs directory
        ///     by looking at the path seperator surrounding the name, as well as
        ///     any other surrounding/ embedded characters.  
        /// </summary>
        [Test]
        public void TestContainsCvsDirectory () {
            Assertion.Assert(PathTranslator.ContainsCVS ("c:\\temp\\CVS"));
            Assertion.Assert(PathTranslator.ContainsCVS ("c:\\temp\\cvs"));
            Assertion.Assert(PathTranslator.ContainsCVS ("c:\\temp\\cVs"));
            Assertion.Assert(PathTranslator.ContainsCVS ("c:\\temp\\cVs\\Crap"));
            Assertion.Assert(!PathTranslator.ContainsCVS ("c:\\temp\\testCvs\\"));
            Assertion.Assert(!PathTranslator.ContainsCVS ("c:\\temp\\CVSFile"));
            Assertion.Assert(PathTranslator.ContainsCVS(
                "C:\\Documents and Settings\\Administrator\\Local Settings\\Temp\\sharpcvslib-tests\\sharpcvslib-test-repository\\conf\\CVS\\conf"));
            Assertion.Assert(PathTranslator.ContainsCVS(
                "C:\\DOCUME~1\\ADMINI~1\\LOCALS~1\\Temp\\sharpcvslib-tests\\sharpcvslib-test-repository\\test\\CVS"));
        }

        /// <summary>
        /// Test that the slash/ or backslash is maintained on the end of a directory
        ///     entry that is passed down from the repository.  This is the only way
        ///     to determine if an entry is a directory or a file.
        /// </summary>
        [Test]
        public void TestSlashMaintained () {
            CvsRoot cvsRoot = new CvsRoot (ROOT_ENTRY1);
            WorkingDirectory workingDirectory = 
                new WorkingDirectory (cvsRoot, LOCAL_ROOT_DIR1, REPOS_NAME1);
            PathTranslator pathTranslator = 
                new PathTranslator (workingDirectory, REPOS_DIR_PATH1);

            Assertion.AssertEquals("src/ICSharpCode/SharpCvsLib/FileSystem/", pathTranslator.RelativePath);
            Assertion.AssertEquals (Path.DirectorySeparatorChar.ToString(), pathTranslator.Filename);
            String expectedLocalPath =
                PathTranslator.ConvertToOSSpecificPath (Path.Combine (LOCAL_DIR1, "src/ICSharpCode/SharpCvsLib/FileSystem/"));
            Assertion.AssertEquals(expectedLocalPath, pathTranslator.LocalPath);
            Assertion.AssertEquals (expectedLocalPath, pathTranslator.LocalPathAndFilename);
            Assertion.Assert (pathTranslator.IsDirectory == true);

        }

    }
}

