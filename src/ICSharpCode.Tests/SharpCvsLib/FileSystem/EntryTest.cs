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

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.FileSystem {
	/// <summary>
    /// A cvs entry can contain the following items:
    ///     / name / version / conflict / options / tag_or_date
    /// 
    /// Sharpcvslib converts each cvs entry into an object so the data
    ///     can be accessed easier.  This class tests the parsing of the 
    ///     cvs string and other behavoir related to this entry.
    /// 
	/// </summary>
	[TestFixture]
	public class EntryTest	{
		private ILog LOGGER = 
			LogManager.GetLogger (typeof(EntryTest));
	    
    	public const String CHECKOUT_ENTRY = 
    	        "/CvsFileManagerTest.cs/1.1/Tue May 13 05:10:17 2003//"; 
        public const String CHECKOUT_ENTRY_2 =
                "/EntryTest.cs/1.1/03 Jan 2003 04:07:36 -0000//";

	    private Manager manager;	    
		/// <summary>
		/// Constructor for customer db test.
		/// </summary>
		public EntryTest () {
		}

        /// <summary>
        ///     Perform setup operations for the test.  Create a new
        ///         file manager object.
        /// </summary>
        [SetUp]
        public void SetUp () {
            this.manager = new Manager ();
        }
        
        /// <summary>
        /// 
        /// The items that should be parsed out of the cvs string are:
        ///         <ol>
        ///             <li>name</li>
        ///             <li>version</li>
        ///             <li>conflict</li>
        ///             <li>options</li>
        ///             <li>tag or date</li>
        ///         </ol>
        /// </summary>
        [Test]
        public void TestParseCheckoutEntry () {
            Entry entry = new Entry (TestConstants.LOCAL_PATH, CHECKOUT_ENTRY);
            
            Assertion.Assert (entry.Name.Equals ("CvsFileManagerTest.cs"));
            Assertion.Assert (entry.Revision.Equals ("1.1"));
            Assertion.Assert (entry.Date.Equals ("Tue May 13 05:10:17 2003"));
            
            Assertion.Assert (entry.TimeStamp.Day == 13);
            Assertion.Assert (entry.TimeStamp.Month == 5);
            Assertion.Assert (entry.TimeStamp.Year == 2003);
            Assertion.Assert (entry.TimeStamp.Hour == 5);
            Assertion.Assert (entry.TimeStamp.Minute == 10);
            Assertion.Assert (entry.TimeStamp.Second == 17);
            
            Assertion.Assert (entry.IsBinaryFile == false);
            Assertion.Assert (entry.IsDirectory == false);
            
            Assertion.Assert (entry.FileContents.Equals (CHECKOUT_ENTRY));
        }
        
	}
}

