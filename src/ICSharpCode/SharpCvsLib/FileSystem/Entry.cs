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
//
//    Author:     Mike Krueger, 
//                Clayton Harbour 
#endregion

using System;
using System.Collections;
using System.IO;
using System.Globalization;

using log4net;

namespace ICSharpCode.SharpCvsLib.FileSystem { 
	
    /// <summary>
    /// Rcs entry.
    /// </summary>
	public class Entry : ICvsFile
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
	        "ddd MMM dd HH':'mm':'ss yyyy";
//	        "ddd MMM dd HH:mm:ss yyyy";	    
		
		
		public const String FILE_NAME = "Entries";
	    private String path;
	    
		/// <summary>
		///     The name of the file to write to.
		/// </summary>
		public String Filename {
		    get {return Entry.FILE_NAME;}
		}
		
		/// <summary>
		///     The path to the folder above the cvs folder.
		/// </summary>
		public String Path {
		    get {return this.path;}
		}
				
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
        /// Outputs the formatted cvs entry.
        /// </summary>
        /// <returns>The formatted cvs entry.</returns>
		public String FileContents
		{
		    get {
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
    				    String dateString;
					    dateString = this.TimeStamp.ToString(FORMAT_1, 
					                                         DateTimeFormatInfo.InvariantInfo);
    					str += dateString;
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
		public Entry(String path, String line)
		{
		    this.cvsEntry = line;
			Parse(line);
			NewEntry = false;
		    this.path = path;
		}
		
        /// <summary>
        /// Set the file timestamp.
        /// </summary>
		public void SetTimeStamp()
		{
		    DateTime timestamp = DateTime.Now;
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
				    if (LOGGER.IsDebugEnabled) {
				        String msg = "Converted using pattern=[" + RFC1123 + "]" +
				            "timestamp=[" + timestamp + "]";
				        LOGGER.Debug (msg);
				    }
				} catch (Exception) {
					try {
						timestamp = DateTime.ParseExact("0" + date, 
						                                RFC1123, 
						                                DateTimeFormatInfo.InvariantInfo);
				    if (LOGGER.IsDebugEnabled) {
				        String msg = "Converted using pattern=[0 + " + RFC1123 + "]" +
				            "timestamp=[" + timestamp + "]";
				        LOGGER.Debug (msg);
				    }

					} catch (Exception) {
   						timestamp = DateTime.ParseExact(date, 
   						                                FORMAT_1, 
   						                                DateTimeFormatInfo.InvariantInfo);
    				    if (LOGGER.IsDebugEnabled) {
    				        String msg = "Converted using pattern=[" + FORMAT_1 + "]" +
    				            "timestamp=[" + timestamp + "]";
    				        LOGGER.Debug (msg);
    				    }
					}
				}				
			}
			if (LOGGER.IsDebugEnabled) {
			    String msg = "timestamp=[" + timestamp + "]";
			    LOGGER.Debug (msg);
			}
			this.timestamp = timestamp;
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
			    this.name = "";
			}
			string[] tokens = line.Split( new char[] { '/' });
			if (tokens.Length < 6 && !this.isDir) {
				throw new ArgumentException("not enough tokens in entry line (#" + 
				                            tokens.Length + ")\n" + line);
			}
			else if (tokens.Length > 6) {
			    throw new ArgumentException ("Too many tokens in entry line." +
			                                 "tokens.Length=[" + tokens.Length + "]" +
			                                 "line=[" + line + "]");
			}
									
    		name      = tokens[1];
			if (!this.isDir) {
    			revision  = tokens[2];
    			date      = tokens[3];			    

    			int conflictIndex = date.IndexOf('+');
    			
    			if (conflictIndex > 0) {
    				Conflict = date.Substring(conflictIndex + 1);
    				date = date.Substring(0, conflictIndex);
	        	}
    			SetTimeStamp();
    			options   = tokens[4];
    			tag       = tokens[5];
			    
			}
		}		
		
		/// <summary>
		///     Determine if the two objects are equal.
		/// </summary>
		public override bool Equals (object obj) {
		    if (obj is Entry) {
		        Entry that = (Entry)obj;
		        if (that.GetHashCode ().Equals (this.GetHashCode ())) {
		            return true;
		        }
		    }
		    return false;
		}
		
		/// <summary>
		///     Override the hashcode.  This is a combination of the entry
		///         name and the path to the entry file.
		/// </summary>
		public override int GetHashCode () {
		    return this.Name.GetHashCode ();
		}
		
		/// <summary>
		///     Return a human readable string that represents the entry object.
		/// </summary>
		/// <returns>A human readable string that represents the entry object.</returns>
		public override String ToString () {
		    return this.FileContents;
		}
		
		/// <summary>The type of file that this is.</summary>
		public Factory.FileType Type {get {return Factory.FileType.Entries;}}
	}
}
