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
using System.Configuration;
using System.Text;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Tests.Config;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Extension.LogReporter {

    /// <summary>
    ///     Test the LogReportCommand class
    /// </summary>
    [TestFixture]
    public class LogReportCommandTest {
        private ILog LOGGER =
            LogManager.GetLogger (typeof(LogReportCommandTest));

//        private SharpCvsLibTestsConfig settings = 
//            SharpCvsLibTestsConfig.GetInstance();

    
        /// <summary>
        ///     Tests against the Sharpcvslib-test-repository.
        /// </summary>
        [Test]
        public void Test () {
            
            SharpCvsLibTestsConfig settings;
            string section = SharpCvsLibTestsConfigHandler.APP_CONFIG_SECTION;
System.Console.WriteLine("section={0}", section);
            settings = (SharpCvsLibTestsConfig)ConfigurationSettings.GetConfig(section);

System.Console.WriteLine("target-directory={0}", settings.TargetDirectory);
System.Console.WriteLine("password={0}", settings.ValidPassword);
            string moduleName = settings.Module;
            string workingDir = settings.TargetDirectory;
            string password = settings.ValidPassword;
            bool foundTestFile1 = false;
            bool foundTestFile2 = false;
            LogRevision logRevision;
            
            LogReportCommand logCommand = new LogReportCommand(moduleName, workingDir);
    // 
    //    logCommand.SetLastNDays(7);
    //    // or logCommand.StartDate = new DateTime(...);
    //    // and/or logCommand.EndDate = new DateTime(...);
    //    
            LogReport logReport = logCommand.Run(password);
            
            Assertion.AssertEquals(16, logReport.Count);
            foreach (LogFile logFile in logReport)
            {
                if (logFile.WorkingFnm.EndsWith("test-file.txt"))
                {
                    Assertion.Assert(!foundTestFile1);
                    foundTestFile1 = true;

                    Assertion.AssertEquals("/cvsroot/sharpcvslib-test/sharpcvslib-test-repository/test-file.txt,v", logFile.RepositoryFnm);
                    Assertion.AssertEquals("test-file.txt", logFile.WorkingFnm);
                    Assertion.AssertEquals("", logFile.Description);
                    
                    Assertion.AssertEquals(3, logFile.Count);
                    // most recent version will be first
                    logRevision = logFile[0];
                    Assertion.AssertEquals("1.3", logRevision.Revision);
                    CheckDate(2003, 9, 14, 1, 8, 21, logRevision.Timestamp);
                    Assertion.AssertEquals("claytonharbour", logRevision.Author);
                    Assertion.AssertEquals("Exp", logRevision.State);
                    Assertion.AssertEquals(3, logRevision.LinesAdded);
                    Assertion.AssertEquals(1, logRevision.LinesDeleted);
                    Assertion.AssertEquals("*** empty log message ***", logRevision.Comment);
                    
                    logRevision = logFile[1];
                    Assertion.AssertEquals("1.2", logRevision.Revision);
                    CheckDate(2003, 9, 14, 1, 7, 15, logRevision.Timestamp);
                    Assertion.AssertEquals("claytonharbour", logRevision.Author);
                    Assertion.AssertEquals("Exp", logRevision.State);
                    Assertion.AssertEquals(3, logRevision.LinesAdded);
                    Assertion.AssertEquals(1, logRevision.LinesDeleted);
                    Assertion.AssertEquals("Added line.", logRevision.Comment);
                   
                    logRevision = logFile[2];
                    Assertion.AssertEquals("1.1", logRevision.Revision);
                    CheckDate(2003, 9, 14, 1, 5, 51, logRevision.Timestamp);
                    Assertion.AssertEquals("claytonharbour", logRevision.Author);
                    Assertion.AssertEquals("Exp", logRevision.State);
                    Assertion.AssertEquals(0, logRevision.LinesAdded);
                    Assertion.AssertEquals(0, logRevision.LinesDeleted);
                    Assertion.AssertEquals("Various changes for sticky tag support.  Looked at implementing a message event handling system for request/ responses to output server messages (similar to tortoise).", logRevision.Comment);
                }
                if (logFile.WorkingFnm.EndsWith("test-file-2.txt"))
                {
                    Assertion.Assert(!foundTestFile2);
                    foundTestFile2 = true;

                    Assertion.AssertEquals("/cvsroot/sharpcvslib-test/sharpcvslib-test-repository/src/test-file-2.txt,v", logFile.RepositoryFnm);
                    Assertion.AssertEquals("src/test-file-2.txt", logFile.WorkingFnm);
                    Assertion.AssertEquals("", logFile.Description);
                    
                    Assertion.AssertEquals(1, logFile.Count);
                    // most recent version will be first
                    logRevision = logFile[0];
                    Assertion.AssertEquals("1.1", logRevision.Revision);
                    CheckDate(2003, 9, 14, 15, 57, 48, logRevision.Timestamp);
                    Assertion.AssertEquals("claytonharbour", logRevision.Author);
                    Assertion.AssertEquals("Exp", logRevision.State);
                    Assertion.AssertEquals(0, logRevision.LinesAdded);
                    Assertion.AssertEquals(0, logRevision.LinesDeleted);
                    Assertion.AssertEquals("*** empty log message ***", logRevision.Comment);
                }
            }
            
            Assertion.Assert(foundTestFile1);
            Assertion.Assert(foundTestFile2);
    //            ...
    //	        foreach (LogRevision logRevision in logFile)
    //	        {
    //    	        ...
    //	        }
    //	   }
        }
        
        private void CheckDate(int year, int month, int day, int hour, int minute, int second, DateTime timestamp)
        {
            Assertion.AssertEquals(year, timestamp.Year);
            Assertion.AssertEquals(month, timestamp.Month);
            Assertion.AssertEquals(day, timestamp.Day);
            Assertion.AssertEquals(hour, timestamp.Hour);
            Assertion.AssertEquals(minute, timestamp.Minute);
            Assertion.AssertEquals(second, timestamp.Second);
        }
    }
}
