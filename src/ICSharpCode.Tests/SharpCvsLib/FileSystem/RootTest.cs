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

using ICSharpCode.SharpCvsLib.Tests.Config;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    ///     Test the root file parses the input string correctly
    ///         and assigns the correct values to the properties.
    /// </summary>
    [TestFixture]
    public class RootTest {
        private SharpCvsLibTestsConfig settings = 
            SharpCvsLibTestsConfig.GetInstance();

        private readonly String ROOT_ENTRY1 =
            ":pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        private readonly String ROOT_ENTRY2 =
            ":pserver:user@cvs.sourceforge.net:/cvsroot/sharpcvslib";
        private readonly String ROOT_FILE_NAME = "Root";
        /// <summary>
        ///     Constructor for test case.
        /// </summary>
        public RootTest () {

        }

        /// <summary>
        ///     Ensure that the values the root is initialized with
        ///         can be determined.
        /// </summary>
        [Test]
        public void CreateRootTest () {
            String fullPath = this.settings.Config.LocalPath;
            Root root = new Root (fullPath, this.ROOT_ENTRY1);

            String cvsPath = Path.Combine (fullPath, "CVS");
            Assertion.Assert ("Path not set/returned", root.Path.Equals (fullPath));
            Assertion.Assert ("FileContents not set/returned", root.FileContents.Equals (this.ROOT_ENTRY1));
            Assertion.Assert ("Filename not correct", root.Filename.Equals (this.ROOT_FILE_NAME));
            Assertion.Assert ("Type not correct", root.Type == Factory.FileType.Root);
            Assertion.Assert ("IsMultiLined not correct", root.IsMultiLined == false);
        }

        /// <summary>
        ///     Test that the equals method correctly identifies two root objects
        ///         as equal.
        /// </summary>
        [Test]
        public void EqualsTest () {
            String cvsPath = Path.Combine (this.settings.Config.LocalPath,
                                        this.settings.Config.Module);
            Root RootSame1 = new Root (cvsPath, this.ROOT_ENTRY1);
            Root RootSame2 = new Root (cvsPath, this.ROOT_ENTRY1);
            Root RootDiff1 = new Root (cvsPath, this.ROOT_ENTRY2);

            Assertion.Assert (RootSame1.Equals (RootSame1));
            Assertion.Assert (RootSame1.Equals (RootSame2));
            Assertion.Assert (RootSame2.Equals (RootSame1));

            Assertion.Assert (!RootDiff1.Equals (RootSame1));
            Assertion.Assert (!RootDiff1.Equals (RootSame2));
            Assertion.Assert (!RootSame1.Equals (RootDiff1));
            Assertion.Assert (!RootSame2.Equals (RootDiff1));
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
    }
}
