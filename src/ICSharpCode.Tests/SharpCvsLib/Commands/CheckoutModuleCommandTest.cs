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
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Commands {
    /// <summary>
    ///     Test that checkout module command fetches files from the 
    ///         remote repository.  All files are not verified, only 
    ///         a select few are checked.  After the files are checked
    ///         the entries are verified in the <code>CVS/Entries</code>
    ///         folder.
    /// </summary>
	[TestFixture]
	public class CheckoutModuleCommandTest	{
		private ILog LOGGER = 
			LogManager.GetLogger (typeof(CheckoutModuleCommandTest));
	    
	    private CvsFileManager manager;	    
		/// <summary>
		/// Constructor for customer db test.
		/// </summary>
		public CheckoutModuleCommandTest () {
		}
		
		/// <summary>
		///     
		/// </summary>
		[SetUp]
		public void SetUp () {
		    this.manager = new CvsFileManager ();    
		}
		
		/// <summary>
		///     Test that a checkout with all parameters is successful.
		/// </summary>
		[Test]
		public void CheckoutTest () {
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
		    
		    string buildFile = 
		        Path.Combine (TestConstants.LOCAL_PATH, TestConstants.BUILD_FILE);
		    Assertion.Assert ("Should have found the build file.  file=[" + 
		                      buildFile + "]", File.Exists (buildFile));
		    
		    ICollection entries = manager.ReadEntries (TestConstants.LOCAL_PATH);
		    IEnumerator entriesEnum = entries.GetEnumerator ();
            bool found = false;	
		    
		    while (entriesEnum.MoveNext ()) {
		        Entry entry = (Entry)entriesEnum.Current;
		        
		        System.Console.WriteLine ("entry=[" + entry + "]");
		        if (entry.Name.Equals (TestConstants.BUILD_FILE)) {
		            found = true;
		        }
		    }
		    
		    Assertion.Assert ("Build file should have a cvs entry.", found);
		}
	}
}
