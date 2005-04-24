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
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

using log4net;

using ICSharpCode.SharpCvsLib.Assertions;
using ICSharpCode.SharpCvsLib.Attributes;
using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Rcs entry.
    /// </summary>
    [Author("Mike Krueger", "mike@icsharpcode.net", "2001")]
    [Author("Clayton Harbour", "claytonharbour@sporadicism.com", "2003-2005")]
    public class Entry : AbstractCvsFile, ICvsFile, IComparable {
        private static ILog LOGGER = LogManager.GetLogger (typeof (Entry));

        /// <summary>
        /// Indicator specifying if this is a new entry or not.
        ///     The default value is <code>true</code>.
        /// </summary>
        public bool   NewEntry  = true;

        private bool   isDir       = false;
        private string name        = null;
        private string revision    = "0";
        private DateTime timestamp = DateTime.Now;

        private string conflict    = null;
        private string options     = null;
        private string tag         = null;
        private string date        = null;

        private Tag tagFile        = null;

        private bool isUtcTimeStamp= true;

        private string key;
        /// <summary>
        /// Get a key that represents this cvs entry.
        /// </summary>
        public override string Key {
            get {
                if (null == key) {
                    this.key = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(this.FullPath), this.Name);
                } 
                return this.key;
            }
        }

        /// <summary>
        ///     The name of the entries file.
        /// </summary>
        public const String FILE_NAME = "Entries";

        /// <summary>
        ///     The name of the file to write to.
        /// </summary>
        public override String Filename {
            get {return Entry.FILE_NAME;}
        }

        /// <summary>
        /// Timestamp for the file.
        /// </summary>
        public DateTime TimeStamp {
        get {return timestamp;}
        set {timestamp = value;}
        }

        /// <summary>
        /// Indicates whether the UTC timestamp should be used for files, or if the
        ///     timestamp from the current timezone should be used.
        /// </summary>
        public bool IsUtcTimeStamp {
            get {return this.isUtcTimeStamp;}
        }

        /// <summary>
        /// String indicating a conflict with the server and
        ///     client files (if any).
        /// </summary>
        public string  Conflict {
            get {return conflict;}
            set {conflict = value;}
        }

        /// <summary>
        /// Date of the revision.
        /// </summary>
        public string Date {
            get {return date;}
            set {
                date = value;
                SetTimeStamp();
            }
        }

        /// <summary>
        /// Sticky tag for the file (if any).
        /// </summary>
        public string Tag {
            get {return tag;}
            set {tag = value;}
        }

        /// <summary>
        /// TODO: figure out what this is for.
        /// </summary>
        public string Options {
            get {return options;}
            set {options = value;}
        }

        /// <summary>
        /// The revision number for the file.
        /// </summary>
        public string Revision {
            get {return revision;}
            set {revision = value;}
        }

        /// <summary>
        /// The name of the file or directory.
        /// </summary>
        public string Name {
            get {return name;}
            set {name = value;}

        }

        /// <summary>
        /// <code>true</code> if the item is a directory, <code>false</code>
        ///     otherwise.
        /// </summary>
        public bool IsDirectory {
            get {return isDir;}
            set {isDir = value;}
        }

        /// <summary>
        /// Indicate if the file contents string is for a directory entry.  This
        ///     is identified by an entry line string that contains a "D" 
        ///     as the first character in the string.
        /// </summary>
        /// <param name="fileContents">The contents of the entry file.</param>
        /// <returns><code>true</code> if the file contents string indicates a 
        ///     directory, otherwise return <code>false</code>.</returns>
        public static bool IsDirectoryEntry (String fileContents) {
            return fileContents[0].ToString().ToUpper().Equals("D");
        }

        /// <summary>
        /// <code>true</code> if the options tag specifies the file
        ///     is binary (i.e. has the option <code>-kb</code> specified).
        /// </summary>
        public bool IsBinaryFile {
            get {return options == "-kb";}
            set {options = value ? "-kb" : null;}
        }

        /// <summary>
        /// Outputs the formatted cvs entry.
        /// </summary>
        /// <returns>The formatted cvs entry.</returns>
        public override String FileContents {
            get {
                string str = "";
                if (isDir) {
                    str += "D";
                }
                str += "/";
                if (name != null) {
                    str += name + "/";
                    if (revision != null && !this.isDir) {
                        str += revision;
                    }
                    str += "/";

                    if (date != null &&
                        date.Length != 0 &&
                        !this.IsDirectory) {
                        string dateString;
                        dateString =
                            DateParser.GetCvsDateString (this.TimeStamp);
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

                LOGGER.Debug("str=[" + str + "]");
                return str;
            }
        }

        /// <summary>
        /// Create a new instance of the cvs object.  The file that is passed in should
        /// be the cvs file that will contain the file contents.
        /// </summary>
        /// <param name="cvsFile">The cvs management file, CVS\Entries.</param>
        /// <param name="fileContents">The line entry used to represent the file in the CVS\Entries file.</param>
        /// <example>
        ///     <list type="table">
        ///         <term>cvsFile</term>
        ///         <description>C:\dev\src\sharpcvslib\sharpcvslib\src\ICSharpCode\SharpCvsLib\FileSystem\Entry.cs</description>
        ///         <term>fileContents</term>
        ///         <description>/Entry.cs/1.28/Sun Jan 23 23:12:09 2005//</description>
        ///     </list>
        /// </example>
        public Entry (FileInfo cvsFile, String fileContents) : base (cvsFile, fileContents) {
            if (this.IsDirectory) {
                // Assert the management file is stored one level up
                // project\CVS\Entries      
                // project\build\
                Assert.Equal(new DirectoryInfo(this.ManagedPath.FullName).Parent.FullName,
                    this.CvsFile.Directory.Parent.FullName);
            } else {
                // Assert the management file is correct.
                // project\CVS\Entries      <-- project directory of management file
                // project\SomeFile.cs      <-- should equal project directory of managed file
                Assert.Equal(new FileInfo(this.ManagedPath.FullName).Directory.FullName, 
                    this.CvsFile.Directory.Parent.FullName);
                // The managed file and the management file should be two different entities.
                Assert.NotEqual(this.ManagedPath.FullName, this.CvsFile.FullName);
            }
        }

        /// <summary>
        /// Create a new instance of the cvs management object.  The file that is passed 
        /// in should be the path <warn>ABOVE</warn> the object being managed.
        /// </summary>
        /// <param name="cvsPath">The full path of the cvs management file.</param>
        /// <param name="fileContents">Contents of the cvs management file or line item.</param>
        /// <example>
        ///     <list type="table">
        ///         <term>cvsPath</term>
        ///         <description>C:\dev\src\sharpcvslib\sharpcvslib\src\ICSharpCode\SharpCvsLib\FileSystem</description>
        ///         <term>fileContents</term>
        ///         <description>/Entry.cs/1.18/Tue Dec 16 07:44:54 2003//</description>
        ///     </list>
        /// </example>
        [Obsolete ("Use 'Entry (FileInfo, string)' instead.")]
        public Entry (string cvsPath, string fileContents) : 
            this (new FileInfo(
            System.IO.Path.Combine(
                System.IO.Path.Combine(cvsPath, "CVS")
            , Entry.FILE_NAME)), fileContents) {
        }

        /// <summary>
        /// Creates an <see cref="Entry"/> object that manages the file being passed in.
        /// </summary>
        /// <param name="managedFile">The file that is under cvs control.</param>
        /// <returns>An entry object that is prepopulated with the information from the
        ///     <code>CVS\Entries</code> file if present or the cvs information needed
        ///     for a new file.</returns>
        /// <exception cref="EntryParseException">If the entry file cannot be
        ///     parsed.</exception>
        /// <example>
        ///     <list type="table">
        ///         <term>managedFile</term>
        ///         <description>A full path to a file being managed such as:
        ///             <br />
        ///             <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\someFile.txt</code>
        ///             would create a Entries line like:
        ///             <code>/someText.txt////</code>
        ///         </description>
        ///     </list>
        ///     <warn>If a directory is being managed use the <see cref="DirectoryInfo"/>
        ///     object.</warn>
        /// </example>
        public static Entry CreateEntry (FileInfo managedPath) {
            // this is a directory, we don't want it
            Assert.NotEndsWith(managedPath.FullName, System.IO.Path.DirectorySeparatorChar.ToString());

            DirectoryInfo cvsDir = new DirectoryInfo(
                System.IO.Path.Combine(managedPath.Directory.FullName, "CVS"));

            FileInfo cvsFile = new FileInfo(
                System.IO.Path.Combine(cvsDir.FullName, Entry.FILE_NAME));

            StringBuilder entryString = new StringBuilder();
            entryString.Append("/").Append(managedPath.Name);
            entryString.Append("/0///");

            Entry entry = new Entry(cvsFile, entryString.ToString());
            return entry;
        }

        /// <summary>
        /// Creates a new entry given the path to the file on the filesystem.
        ///     The entry string is fabricated based on the full path of the file
        ///     that is under or is to be placed under CVS management control.
        /// </summary>
        /// <param name="fullPath">The path to the file to put under cvs control.</param>
        /// <returns>A new cvs entry, using the full path to the file for the
        ///     entry information.</returns>
        /// <exception cref="EntryParseException">If the entry file cannot be
        ///     parsed.  This can occur if the fullPath contains a CVS management
        ///     folder.</exception>
        /// <example>
        ///     <p>A full path such as:
        ///         <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\someFile.txt</code>
        ///         would create a Entries line like:
        ///         <code>/someText.txt////</code>
        ///         Although the storage of the file is not the responsibility of the Entry class, the
        ///         file would be stored in the directory:
        ///         <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\</code>
        ///     </p>
        ///     <p>A directory entry would be indicated by a 
        ///         <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\src\</code>
        ///         which is flagged by adding a <code>Path.DirectorySeperatorChar</code> to the
        ///         end of a normal directory, if it does not already exist.  The directory Entry
        ///         would be placed in the CVS management folder ABOVE the directory itself and
        ///         the resulting Entries line would look like:
        ///         <code>D/src////</code>
        ///         The file would then be placed by the <see cref="ICSharpCode.SharpCvsLib.FileSystem.Manager"/> in 
        ///         the directory:
        ///         <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\</code>
        ///         as well.
        ///     </p>
        /// </example>
        public static Entry CreateEntry (string fullPath) {
            if (fullPath.EndsWith("\\") ||
                fullPath.EndsWith("/")) {
                return CreateEntry(new DirectoryInfo(fullPath));
            } else {
                return CreateEntry(new FileInfo(fullPath));
            }
        }

        /// <summary>
        /// Creates an <see cref="Entry"/> object that manages the file being passed in.
        /// </summary>
        /// <param name="managedPath">The directory that is under cvs control.</param>
        /// <returns>A new cvs entry, using the full path to the file for the
        ///     entry information.</returns>
        /// <exception cref="EntryParseException">If the entry file cannot be
        ///     parsed.</exception>
        /// <example>
        ///     <list type="table">
        ///         <term>managedPath</term>
        ///         <description>The full path to a directory being managed by CVS such as:
        ///             <br />
        ///             <code>C:\DOCUME~1\ADMINI~1\LOCALS~1\Temp\sharpcvslib-tests\sharpcvslib-test-repository\someFile.txt</code>
        ///             would create a Entries line like:
        ///             <code>/someText.txt////</code>
        ///         </description>
        ///     </list>
        ///     <warn>If a directory is being managed use the <see cref="DirectoryInfo"/>
        ///     object.</warn>
        /// </example>
        public static Entry CreateEntry (DirectoryInfo managedPath) {
            // if this is a file we don't want it
            Assert.EndsWith(managedPath.FullName, System.IO.Path.DirectorySeparatorChar.ToString());
            DirectoryInfo cvsDir = new DirectoryInfo(
                System.IO.Path.Combine(managedPath.Parent.FullName, "CVS"));

            FileInfo cvsFile = new FileInfo(
                System.IO.Path.Combine(cvsDir.FullName, Entry.FILE_NAME));

            StringBuilder entryString = new StringBuilder();
            entryString.Append("D");
            entryString.Append("/").Append(managedPath.Name);
            entryString.Append("/0///");

            Entry entry = new Entry(cvsFile, entryString.ToString());
            return entry;
        }

        public static Entry CreateEntry (FileSystemInfo fileSystemInfo) {
            if (fileSystemInfo is DirectoryInfo) {
                return Entry.CreateEntry((DirectoryInfo)fileSystemInfo);
            } else {
                return Entry.CreateEntry((FileInfo)fileSystemInfo);
            }
        }

        public static Entry Load (FileInfo managedFile) {
            Entries entries = 
                Entries.Load(new DirectoryInfo(
                System.IO.Path.Combine(managedFile.Directory.FullName, "CVS")));
            return entries[managedFile.FullName];
        }

        /// <summary>
        /// Set the file timestamp.
        /// </summary>
        public void SetTimeStamp() {
            if (null == date) {
                DateTime now = DateTime.Now.ToUniversalTime();
                date = DateParser.GetCvsDateString(now);
                this.timestamp = now;
            } else {
                this.timestamp = DateParser.ParseCvsDate (date);
            }
        }

        /// <summary>
        /// Parses the cvs entries file.
        /// </summary>
        /// <param name="line"></param>
        public override void Parse(string line){
            if (line.StartsWith("D/")) {
                this.isDir = true;
                line = line.Substring(1);
                this.name = string.Empty;
            }
            string[] tokens = line.Split( new char[] { '/' });
            const int TokensExpected = 6;
            if ((tokens.Length < TokensExpected && !this.isDir) ||
                tokens.Length > TokensExpected) {
                string msg = string.Format("Expected {0} tokens in entry line {0}", TokensExpected, line);
                throw new ICSharpCode.SharpCvsLib.Exceptions.EntryParseException(msg);
            } 

            name      = tokens[1];

            // set the managed file name
            if (this.IsDirectory) {
                this.ManagedPath = new DirectoryInfo(
                    System.IO.Path.Combine(this.CvsFile.Directory.Parent.FullName, this.Name));
            } else {
                this.ManagedPath = new FileInfo(
                    System.IO.Path.Combine(this.CvsFile.Directory.Parent.FullName, this.name));
            }

            if (!this.isDir) {
                revision  = tokens[2];
                LOGGER.Debug("revision=[" + revision + "]");
                LOGGER.Debug("line=[" + line + "]");
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
        /// Parse the name of the file from the cvs file.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>The name of the entry in the cvs file.</returns>
        public static String ParseFileName (String line) {
            Entry entry = new Entry(new FileInfo(System.IO.Path.GetTempPath()), line);
            return entry.Filename;
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
            return (this.isDir.ToString() + this.FullPath + this.Name).GetHashCode ();
        }

        /// <summary>
        ///     Return a human readable string that represents the entry object.
        /// </summary>
        /// <returns>A human readable string that represents the entry object.</returns>
        public override String ToString () {
            return this.FileContents;
        }

        /// <summary>The type of file that this is.</summary>
        public override Factory.FileType Type {get {return Factory.FileType.Entries;}}

        /// <summary>Indicates whether the file can contain multiple
        /// lines.</summary>
        /// <returns><code>true</code> if the file can contain multiple
        /// lines; <code>false</code> otherwise.</returns>
        public override bool IsMultiLined {
            get {return true;}
        }

        /// <summary>
        ///     Holds information on a tag file if there is a
        ///         <code>sticky-tag</code> in the cvs directory.  If there
        ///         is no tag in the cvs directory then this value is null.
        /// </summary>
        public Tag TagFile {
            get {return this.tagFile;}
            set {this.tagFile = value;}
        }

        /// <summary>
        ///     <code>true</code> if the cvs entry contains a
        ///         <code>sticky-tag</code>; otherwise <code>false</code>.
        /// </summary>
        public bool HasTag {
            get {return null == this.Tag;}
        }

        #region IComparable Members

        public int CompareTo(object obj) {
            if (!(obj is Entry)) {
                throw new ArgumentException(string.Format("Unable to compare types Entry and {0}", 
                    obj.GetType().FullName), "obj");
            }

            Entry entry1 = this;
            Entry entry2 = (Entry)obj;

            if (entry1.IsDirectory && !entry2.IsDirectory) {
                return 1;
            } else if (!entry1.IsDirectory && entry2.IsDirectory) {
                return -1;
            } else if (entry1.IsDirectory && entry2.IsDirectory) {
                return entry1.Name.CompareTo(entry2.name);
            } else {
                return entry1.Name.CompareTo(entry2.Name);
            }
        }

        #endregion
    }
}
