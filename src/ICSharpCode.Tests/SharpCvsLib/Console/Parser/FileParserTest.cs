#region "Copyright"
//
// Copyright (C) 2005 Clayton Harbour
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
//    <author>Clayton Harbour</author>
#endregion
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Console.Parser {
    /// <summary>
    ///     Test the command line args parameters for valid ones
    ///         and test invalid ones.
    /// </summary>
    [TestFixture]
    public class FileParserTest {
        /// <summary>
        /// Test that a list of files passed in with full paths is correctly extracted.
        /// </summary>
        [Test]
        public void ParseFullPaths (){
            string[] fileList = new string[]{@"c:\test\file\file.txt", @"c:\test\file\file",
                                              @"d:\test\file\test.txt"
                                          };

            FileParser parser = new FileParser(fileList);

            Assert.AreEqual(fileList.Length, parser.Files.Count);

            for (int i = 0; i < parser.Files.Count; i++) {
                Assert.AreEqual(new FileInfo(fileList[i]), parser.Files[i]);
            }
        }

        [Test]
        public void ParsePartialPaths() {
            string[] fileList = new string[]{@"file\file.txt", @"file\file",
                                                @"test.txt"
                                            };

            FileParser parser = new FileParser(fileList);

            Assert.AreEqual(fileList.Length, parser.Files.Count);

            for (int i = 0; i < parser.Files.Count; i++) {
                Assert.AreEqual(new FileInfo(Path.Combine(Environment.CurrentDirectory, fileList[i])), parser.Files[i]);
            }
        }
    }
}
