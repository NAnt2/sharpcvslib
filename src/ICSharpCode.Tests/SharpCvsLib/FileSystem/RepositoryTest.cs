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
//    Author: Clayton Harbour
//     claytonharbour@sporadicism.com
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
    ///     Test the repository file parses the input string correctly
    ///         and assigns the correct values to the properties.
    /// </summary>
    [TestFixture]
    public class RepositoryTest {
        
        private readonly String RELATIVE_PATH = "src";
        private readonly String REPOSITORY_ENTRY = 
            "sharpcvslib/src";
        /// <summary>
        ///     Constructory for test case.
        /// </summary>
        public RepositoryTest () {
            
        }
        
        /// <summary>
        ///     Ensure that the values the repository is initialized with 
        ///         can be determined.
        /// </summary>
        [Test]
        public void CreateRepositoryTest () {
            String fullPath = 
                Path.Combine (TestConstants.LOCAL_PATH, RELATIVE_PATH);
            Repository repos = new Repository (fullPath, 
                                               this.REPOSITORY_ENTRY);
            
            String cvsPath = Path.Combine (fullPath, "CVS");
            Assertion.Assert (repos.Path.Equals (fullPath));
            Assertion.Assert (repos.FileContents.Equals (this.REPOSITORY_ENTRY));
        }
        
        /// <summary>
        /// The slashes in a cvs repository file are stripped off.  This
        ///     means that the repository + the entry from the entries file
        ///     equal the relative server path for the file in the repository 
        ///     when the two are concatentated.
        /// </summary>
        [Test]
        public void NoSlashAtEnd () {
            String fullPath = 
                Path.Combine (TestConstants.LOCAL_PATH, TestConstants.MODULE);
            
            String repositoryEntryWithSlash = this.REPOSITORY_ENTRY + "/";
            
            Assertion.Assert ("We just added a slash, there should be a slash.", 
                              repositoryEntryWithSlash.EndsWith ("/"));
            Repository repos = new Repository (fullPath, 
                                               repositoryEntryWithSlash);
            
            Assertion.Assert ("Slash should be stripped off.", 
                              !repos.FileContents.EndsWith ("/"));
            Assertion.Assert ("FileContents should equal module name.  FileContents=[" +
                              repos.FileContents + "]",
                              repos.FileContents.Equals ("sharpcvslib/src"));
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