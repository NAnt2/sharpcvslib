#region Copyright
// Entry.cs 
// Copyright (C) 2001 Mike Krueger
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
using System.Globalization;

using log4net;

using ICSharpCode.SharpCvsLib.Misc;

namespace ICSharpCode.SharpCvsLib.Misc { 
	
    /// <summary>
    /// Rcs entry.
    /// </summary>
	public class Entry
	{
	    private ILog LOGGER = LogManager.GetLogger (typeof (Entry));
	    
        /// <summary>
        /// Indicator specifying if this is a new entry or not.
        ///     The default value is <code>true</code>.
        /// </summary>
		public bool   NewEntry  = true;
		
		private bool   isDir       = false;
		private string name        = null;
		private string revision    = "1.1.1.1";
		private DateTime timestamp = DateTime.Now;
		
		private string conflict    = null; 
		private string options     = null;
		private string tag         = null;
		private string date        = null;
	    
	    private string cvsEntry;
	    
	    public const String RFC1123 = 
	        "dd MMM yyyy HH':'mm':'ss '-0000'";
	    public const String FORMAT_1 =
//	        "ddd MMM dd HH':'mm':'ss yyyy";
	        "ddd MMM dd HH:mm:ss yyyy";	    
		
        /// <summary>
        /// Timestamp for the file.
        /// </summary>
		public DateTime TimeStamp {
			get {
				return timestamp;
			}
			set {
				timestamp = value;
			}
		}
		
        /// <summary>
        /// String indicating a conflict with the server and
        ///     client files (if any).
        /// </summary>
		public string  Conflict {
			get {
				return conflict;
			}
			set {
				conflict = value;
			}
		}
		
        /// <summary>
        /// Date of the revision.
        /// </summary>
		public string Date {
			get {
				return date;
			}
			set {
				date = value;
				SetTimeStamp();
			}
		}
		
        /// <summary>
        /// Sticky tag for the file (if any).
        /// </summary>
		public string Tag {
			get {
				return tag;
			}
			set {
				tag = value;
			}
		}

        /// <summary>
        /// TODO: figure out what this is for.
        /// </summary>
		public string Options {
			get {
				return options;
			}
			set {
				options = value;
			}
		}

        /// <summary>
        /// The revision number for the file.
        /// </summary>
		public string Revision {
			get {
				return revision;
			}
			set {
				revision = value;
			}
		}
		
        /// <summary>
        /// The name of the file or directory.
        /// </summary>
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
			 
		}

        /// <summary>
        /// <code>true</code> if the item is a directory, <code>false</code>
        ///     otherwise.
        /// </summary>
		public bool IsDirectory {
			get {
				return isDir;
			}
			set {
				isDir = value;
			}
		}
		
        /// <summary>
        /// <code>true</code> if the options tag specifies the file
        ///     is binary (i.e. has the option <code>-kb</code> specified).
        /// </summary>
		public bool IsBinaryFile {
			get {
				return options == "-kb";
			}
			set {
				options = value ? "-kb" : null;
			}
		}
		
		/// <summary>
		///     The 
		/// </summary>
		public String CvsEntry {
		    get {return this.cvsEntry;}
		    set {this.cvsEntry = value;}
		}
		
        /// <summary>
        /// Constructor.
        /// </summary>
		public Entry()
		{
		}
		
        /// <summary>
        ///     The entry class converts a cvs entry string into an 
        ///         entry object by parsing the string into the various
        ///         components.
        /// </summary>
        /// <param name="line">The cvs entry string.</param>
		public Entry(string line)
		{
		    this.cvsEntry = line;
			Parse(line);
			NewEntry = false;
		}
		
        /// <summary>
        /// Set the file timestamp.
        /// </summary>
		public void SetTimeStamp()
		{
		    if (LOGGER.IsDebugEnabled){
		        String msg = "Converting date string.  " +
		            "date=[" + date + "]" +
		            "RFC1123Pattern=[" + RFC1123 + "]" +
		            "FORMAT_1=[" + FORMAT_1 + "]";
			    LOGGER.Debug(msg);
		    }				
		    
			if (date != null && date.Length > 0)  {
				try {
					timestamp = DateTime.ParseExact(date, 
					                                RFC1123,
					                                DateTimeFormatInfo.InvariantInfo);
				} catch (Exception) {
					try {
						timestamp = DateTime.ParseExact("0" + date, 
						                                RFC1123, 
						                                DateTimeFormatInfo.InvariantInfo);
					} catch (Exception) {
   						timestamp = DateTime.ParseExact(date, 
   						                                FORMAT_1, 
   						                                DateTimeFormatInfo.InvariantInfo);
					}
				}				
			}
			if (LOGGER.IsDebugEnabled) {
			    String msg = "timestamp=[" + timestamp + "]";
			    LOGGER.Debug (msg);
			}
		}
		
        /// <summary>
        /// Parses the cvs entries file.
        /// </summary>
        /// <param name="line"></param>
		public void Parse(string line)
		{
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "cvsEntry=[" + line + "]";
		        LOGGER.Debug (msg);
		    }
		    
			if (line.StartsWith("D/")) {
				this.isDir = true;
				line = line.Substring(1);
			}
			string[] tokens = line.Split( new char[] { '/' });
			if (tokens.Length < 6) {
				throw new ArgumentException("not enough tokens in entry line (#" + 
				                            tokens.Length + ")\n" + line);
			}
			/* TODO: See if I should be checking for > 6 tokens...
			else if (tokens.Length > 6) {
			    throw new ArgumentException ("Too many tokens in entry line." +
			                                 "tokens.Length=[" + tokens.Length + "]" +
			                                 "line=[" + line + "]");
			}*/
			
			name      = tokens[1];
			revision  = tokens[2];
			date      = tokens[3];
			
			int conflictIndex = date.IndexOf('+');
			
			if (conflictIndex > 0) {
				Conflict = date.Substring(conflictIndex + 1);
				date = date.Substring(0, conflictIndex);
			}
			
			if (!this.isDir) {
    			SetTimeStamp();
			}
			options   = tokens[4];
			tag       = tokens[5];
		}
		
		/// <summary>
		/// This class gets all entries in from the path <code>path</code>.
		/// (but not it's subpaths)
		/// </summary>
		/// <remarks>
		/// Note that the path/CVS directory must exists and this directory
		/// must contain an the file Entries for the function to work.
		/// </remarks>
		/// <returns>
		/// null if no entry could be read.
		/// </returns>
/*
		public static Entry[] RetrieveEntries(string path) {
            CvsFileManager manager = new CvsFileManager ();		    
		    string entriesPath = Path.Combine (path, manager.ENTRIES);
		    string entriesLogPath = Path.Combine (path, manager.ENTRIES_LOG);
			if (File.Exists(entriesPath)) {
				ArrayList entries = new ArrayList();
				StreamReader sr = File.OpenText(entriesPath);
				
				while (true) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}
					if (line.Length > 1) {					    
						entries.Add(new Entry(line));
					}
				}
				sr.Close();
			    // FIXME: Eventually move all of this to the file manager.
    		    //entries.Add (manager.ReadEntries (path));
    		    if (entries.Count > 0) {
    				if (File.Exists (entriesLogPath)) {
    					sr = File.OpenText(entriesLogPath);
    					
    					while (true) {
    						string line = sr.ReadLine();
    						if (line == null) {
    							break;
    						}
    						if (line.Length > 1) {
    							switch (line[0]) {
    								case 'A':  // Add file
    									entries.Add(new Entry(line.Substring(2)));
    									break;
    								case 'R':  // Remove entry
    									Entry removeMe = new Entry(line.Substring(2));
    									Entry removeObject = null;
    									
    									foreach (Entry entry in entries) {
    										if (removeMe.ToString() == entry.ToString()) {
    											removeObject = entry;
    											break;
    										}
    									}
    									if (removeObject != null) {
    										entries.Remove(removeObject);
    									}
    									break;
    								default:  // other chars are silently ignored (specified behaviour)
    									break;
    							}
    							
    						}
    					}
    				    sr.Close();					
    				}
    				
    				return (Entry[])entries.ToArray(typeof(Entry));
    			}
			}
			return null;				
		}*/
		
        /// <summary>
        /// Renders a human readable string that represents the 
        ///     <code>Entry</code> object.
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			string str = "";
			if (isDir) {
				str += "D";
			}
			str += "/";
			if (name != null) {
				str += name + "/";
				if (revision != null && !isDir) {
					str += revision;
				}
				str += "/";
				
				if (date != null) {
					str += timestamp.ToString(FORMAT_1, DateTimeFormatInfo.InvariantInfo);
				}
				if (conflict != null) {
					str += "+" + conflict;
				}
				
				str += "/";
				
				if (options != null) {
					str += options;
				}
				
				str += "/";
				if (tag != null) {
					str += tag;
				} else if (date != null) {
					str += date;
				}
			}
			return str;
		}		
	}
}
