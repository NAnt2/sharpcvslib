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

using NUnit.Framework;

using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Misc;

// TODO: Change to internalize helpers (remove)
[assembly: log4net.Config.DOMConfigurator(
  ConfigFileExtension="config", Watch=true)]

namespace ICSharpCode.SharpCvsLib.Client {

    /// <summary>
    ///     Tests the connection class.  Tests a successful connection and the
    ///         recovery after an unsuccessful connection attempt.
    /// </summary>
    [TestFixture]
    public class CVSServerConnectionTest {
        
        /// <summary>
        ///     Constructor.
        /// </summary>
        public CVSServerConnectionTest () {
        }
        
        /// <summary>
        ///     Makes a connection to a cvs server using parameters that 
        ///         should work.
        /// </summary>
        [Test]
        public void MakeConnection_Good () {
            System.Threading.Thread.Sleep (500);
            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        TestConstants.LOCAL_PATH, 
                                        TestConstants.MODULE);

            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
            
            connection.Connect (working, TestConstants.PASSWORD_VALID);
        }

        /// <summary>
        ///     Try to make a connection to the cvs server using bad parameters.
        ///         These should fail.
        /// </summary>
        [Test]
        public void MakeConnection_Bad () {
            System.Threading.Thread.Sleep (500);
            CvsRoot root = new CvsRoot (TestConstants.CVSROOT);
            root.User = "some_other_user";
            WorkingDirectory working = 
                new WorkingDirectory (root, 
                                        TestConstants.LOCAL_PATH, 
                                        TestConstants.MODULE);

            CVSServerConnection connection = new CVSServerConnection ();
            Assertion.AssertNotNull ("Should have a connection object.", connection);
            
            try {
                connection.Connect (working, TestConstants.PASSWORD_INVALID);
                Assertion.Assert ("Connection should have failed and this code " +
                                  "should not be reached.", true == false);
            } catch (Exception) {
                Assertion.Assert ("Connection failed, this is a good thing.", true == true);
            } 
            
        }        
        
    }
}
