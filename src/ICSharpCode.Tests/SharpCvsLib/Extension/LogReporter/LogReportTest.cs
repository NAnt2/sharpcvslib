#region "Copyright"
// Copyright (C) 2004 Gerald Evans
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
//    <author>Gerald Evans</author>
#endregion

using System;
using System.Text;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Extension.LogReporter {

    /// <summary>
    ///     Test the LogReport class
    /// </summary>
    [TestFixture]
    public class LogReportTest {
    
        /// <summary>
        ///     Tests the default constructor.
        /// </summary>
        [Test]
        public void TestDefaultCtor () {
            LogReport logReport = new LogReport();
            
            Assertion.AssertEquals(0, logReport.Count);
        }
    
        /// <summary>
        ///     Tests accessing [0] when no files.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestInvalidIndexZero () {
            LogReport logReport = new LogReport();
            
            LogFile logFile = logReport[0];
        }
    
        /// <summary>
        ///     Tests adding files.
        /// </summary>
        [Test]
        public void TestFiles () {
            LogReport logReport = new LogReport();
            
            LogFile logFile1 = new LogFile();
            logFile1.RepositoryFnm = "File1";
            logReport.AddFile(logFile1);
            
            LogFile logFile2 = new LogFile();
            logFile2.RepositoryFnm = "File2";
            logReport.AddFile(logFile2);
            
            LogFile logFile3 = new LogFile();
            logFile3.RepositoryFnm = "File3";
            logReport.AddFile(logFile3);

            Assertion.AssertEquals(3, logReport.Count);
            
            // Test indexer
            Assertion.AssertEquals("File1", logReport[0].RepositoryFnm);
            Assertion.AssertEquals("File2", logReport[1].RepositoryFnm);
            Assertion.AssertEquals("File3", logReport[2].RepositoryFnm);
            
            // Test foreach
            int nIndex = 0;
            foreach (LogFile logFile in logReport) {
                Assertion.Assert(nIndex <= 2);
                if (nIndex == 0) {
                    Assertion.AssertEquals("File1", logFile.RepositoryFnm);
                } else if (nIndex == 1) {
                    Assertion.AssertEquals("File2", logFile.RepositoryFnm);
                } else if (nIndex == 2) {
                    Assertion.AssertEquals("File3", logFile.RepositoryFnm);
                }
                
                nIndex++;
            }
        }
    }
}
