using System;
using System.IO;

using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Misc {
	/// <summary>
	/// Summary description for CustomDBTest.
	/// </summary>
	[TestFixture]
	public class CvsFileManagerTest	{
		private ILog LOGGER = 
			LogManager.GetLogger (typeof(CvsFileManagerTest));
	    
	    private CvsFileManager manager;
	    private readonly String TEST_PATH = "c:/test/sharpdevelop-tests/";
		
		/// <summary>
		/// Constructor for customer db test.
		/// </summary>
		public CvsFileManagerTest () {
		}

        /// <summary>
        ///     Perform setup operations for the test.  Create a new
        ///         file manager object.
        /// </summary>
        [SetUp]
        public void SetUp () {
            this.manager = new CvsFileManager ();
        }
		/// <summary>
		/// Test the login event.
		/// </summary>
		[Test]
		public void AddEntryTest ()	{
		    string entryString = 
		        "/CvsFileManager.cs/1.1/Sun May 11 09:07:28 2003//";
		    Entry entry = new Entry (entryString);
		    this.manager.AddEntry (this.TEST_PATH, entry);
		    Assertion.Assert (File.Exists (this.TEST_PATH + this.manager.ENTRIES));
		}
	}
}
