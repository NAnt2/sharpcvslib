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

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.FileSystem {

    /// <summary>
    ///     Test the FileSystem Factory.
    /// </summary>
    [TestFixture]
    public class FactoryTest {
        
        private const String ENTRY_FILE_NAME = "Entries";
        private const String REPOSITORY_FILE_NAME = "Repository";
        private const String ROOT_FILE_NAME = "Root";
        private const String TAG_FILE_NAME = "Tag";
        
        private const String ENTRY_LINE = 
    	        "/CvsFileManagerTest.cs/1.1/Tue May 13 05:10:17 2003//"; 
        private const String REPOSITORY_LINE = "sharpcvslib/src";
        private const String ROOT_LINE = 
            ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        private const String TAG_LINE = 
            "TVer1.1";

        // TODO: Find out if there is any reason why the Factory class
        // does not have static methods
        private Factory factory = new Factory();
        
        /// <summary>
        ///     Constructor for test case.
        /// </summary>
        public FactoryTest () {
            
        }
        
        /// <summary>
        ///     Check factory creation of an Entry.
        /// </summary>
        [Test]
        public void CreateEntryTest () {
            String fullPath = TestConstants.LOCAL_PATH;
            
            ICvsFile cvsFile = factory.CreateCvsObject (fullPath, Factory.FileType.Entries, ENTRY_LINE);
            Assertion.Assert (cvsFile is Entry);
            Assertion.Assert (cvsFile.Path.Equals (fullPath));
            Assertion.Assert (cvsFile.FileContents.Equals (ENTRY_LINE));
        }
        
        /// <summary>
        ///     Check factory creation of a Repository.
        /// </summary>
        [Test]
        public void CreateRepositoryTest () {
            String fullPath = TestConstants.LOCAL_PATH;
            
            ICvsFile cvsFile = factory.CreateCvsObject (fullPath, Factory.FileType.Repository, REPOSITORY_LINE);
            Assertion.Assert (cvsFile is Repository);
            Assertion.AssertEquals (fullPath, cvsFile.Path);
            Assertion.AssertEquals (REPOSITORY_LINE, cvsFile.FileContents);
        }
        
        /// <summary>
        ///     Check factory creation of a Root.
        /// </summary>
        [Test]
        public void CreateRootTest () {
            String fullPath = TestConstants.LOCAL_PATH;
            
            ICvsFile cvsFile = factory.CreateCvsObject (fullPath, Factory.FileType.Root, ROOT_LINE);
            Assertion.Assert (cvsFile is Root);
            Assertion.AssertEquals (fullPath, cvsFile.Path);
            Assertion.AssertEquals (ROOT_LINE, cvsFile.FileContents);
        }
        
        /// <summary>
        ///     Check factory creation of a Tag.
        /// </summary>
        [Test]
        public void CreateTagTest () {
            String fullPath = TestConstants.LOCAL_PATH;
            
            ICvsFile cvsFile = factory.CreateCvsObject (fullPath, Factory.FileType.Tag, TAG_LINE);
            Assertion.Assert (cvsFile is Tag);
            Assertion.AssertEquals (fullPath, cvsFile.Path);
            Assertion.AssertEquals ("N" + TAG_LINE.Substring (1), 
                                    cvsFile.FileContents);
        }
        
        /// <summary>
        ///     Check file type to filename mapping.
        /// </summary>
        [Test]
        public void CheckFilenamesTest () {
            Assertion.Assert (factory.GetFilename (Factory.FileType.Entries).Equals (ENTRY_FILE_NAME));
            Assertion.Assert (factory.GetFilename (Factory.FileType.Repository).Equals (REPOSITORY_FILE_NAME));
            Assertion.Assert (factory.GetFilename (Factory.FileType.Root).Equals (ROOT_FILE_NAME));
            Assertion.Assert (factory.GetFilename (Factory.FileType.Tag).Equals (TAG_FILE_NAME));
        }
        
        /// <summary>
        ///     Clean up any test directories, etc.
        /// </summary>
        [TearDown]
        public void TearDown () {
		    if (Directory.Exists (TestConstants.LOCAL_PATH)) {
    		    Directory.Delete (TestConstants.LOCAL_PATH, true);
		    }            
        }        
    }
}
