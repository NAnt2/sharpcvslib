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
//    <authors>
//        <author>Mike Krueger</author>
//        <author>Clayton Harbour</author>
//    </authors>
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;

using log4net;

using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Rcs entry.
    /// </summary>
    public class Entry : AbstractCvsFile, ICvsFile
    {
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


        /// <summary>
        ///     The name of the entries file.
        /// </summary>
        public const String FILE_NAME = "Entries";

        /// <summary>
        ///     The name of the file to write to.
        /// </summary>
        public String Filename {
            get {return Entry.FILE_NAME;}
        }

        /// <summary>
        /// Get the path to the folder that contains the file being managed.
        /// </summary>
        public override String Path {
            get {
                String tempPath;
                tempPath = System.IO.Path.GetDirectoryName(this.FullPath);   
                return this.GetPathWithDirectorySeperatorChar(tempPath);
            }
        }

        private String GetPathWithDirectorySeperatorChar(String path) {
            if (!path[path.Length - 1].Equals(System.IO.Path.DirectorySeparatorChar)) {
                return path + System.IO.Path.DirectorySeparatorChar;
            } else if (!path[path.Length - 1].Equals('/')) {
                return path + System.IO.Path.DirectorySeparatorChar;
            }
            return path;
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
            get {return isDir;			}
            set {isDir = value;}
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
                            date != String.Empty &&
                            !this.IsDirectory) {
                        String dateString;
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
        /// Create a new instance of the cvs object.
        /// 
        ///     NOTE: Derive full path is assumed.  What this means is it is assumed
        ///     that you are passing in the path to the file and would like to use
        ///     the fileContents or entry to get the full path to the managed file.
        /// </summary>
        /// <param name="path">The path to the directory above the object being 
        ///     managed.  The information in the fileContents parameter is used
        ///     to fill in the "missing" information about the file location.</param>
        /// <param name="fileContents">The contents of the cvs management file.</param>
        public Entry (String path, String fileContents) : this (path, fileContents, true) {
        }

        /// <summary>
        /// Create a new Cvs entry.  If the deriveFullPath is set to false then the
        ///     path information specified in the constructor is taken as the
        ///     entire path to the file.
        /// </summary>
        /// <param name="path">Either the path to the directory above the CVS directory,
        ///     or if the deriveFullPath is set to true then this is the full path
        ///     to the file under cvs control.</param>
        /// <param name="fileContents">A line that represents this particular entry
        ///     under cvs control.</param>
        /// <param name="deriveFullPath"><code>true</code> if the full path should
        ///     be derived from combining the path information and the file name
        ///     parsed from the fileContents or if the path information passed
        ///     in represents the full path to the file.</param>
        public Entry (String path, String fileContents, bool deriveFullPath) : base(path, fileContents) {
            if (PathTranslator.ContainsCVS(path)) {
                // attempt recovery if this file contains a cvs folder.
                path = System.IO.Path.GetDirectoryName(path);
                if (PathTranslator.ContainsCVS(path)) {
                    throw new Exception("Path information should not contain cvs folder.");
                }
            }
            LOGGER.Error("path=[" + path + "]");
            LOGGER.Error("name=[" + this.Name + "]");
            if (deriveFullPath) {
                this.FullPath = System.IO.Path.Combine(path, this.Name);
            } else {
                this.FullPath = path;
            }
            if (!File.Exists(this.FullPath) &&
                !Directory.Exists(this.FullPath)) {
                StringBuilder msg = new StringBuilder();
                msg.Append("File does not exist in path.");
                msg.Append("FullPath=[").Append(FullPath).Append("]");
                msg.Append("Stack trace=[").Append(Environment.StackTrace).Append("]");
                LOGGER.Warn(msg);
            }

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Created new entry=[").Append(this).Append("]");
                msg.Append("path=[").Append(path).Append("]");
                LOGGER.Debug(msg);
                if (this.ToString().ToUpper().IndexOf("C:") > 0) {
                    LOGGER.Debug("Should not have an entry formatted like this, should just contain relative paths.");
                    LOGGER.Debug("Stack trace=[" + Environment.StackTrace + "]");
                }
            }
        }

        /// <summary>
        /// Creates a new entry given the path to the file on the filesystem.
        /// </summary>
        /// <param name="fullPath">The path to the file to put under cvs control.</param>
        /// <returns>A new cvs entry, using the full path to the file for the
        ///     entry information.</returns>
        public static Entry CreateEntry (String fullPath) {
            String path;
            String fileName;
            if (PathTranslator.ContainsCVS(fullPath) ||
                fullPath.EndsWith("/")) {
                throw new Exception("Unable to create an entry for CVS files.");
            }
            StringBuilder entryString = new StringBuilder();
            // Sample directory entry:  D/conf////
            if (Directory.Exists(fullPath) ||
                fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ||
                fullPath.EndsWith("/")) {
                entryString.Append("D");
            } 
            path = System.IO.Path.GetDirectoryName(fullPath);
            // get the directory if the path ends with a slash
            if (fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ||
                fullPath.EndsWith("/")) {
                // strip the slash off so the filename is derived correctly
                fileName = 
                    System.IO.Path.GetFileName(fullPath.Substring(0, fullPath.Length - 1));
            } else {
                fileName = System.IO.Path.GetFileName(fullPath);
            }
            entryString.Append("/").Append(System.IO.Path.GetFileName(fileName));
            entryString.Append("/0///");

            LOGGER.Error("entryString=[" + entryString.ToString() + "]");
            Entry entry = new Entry(path, 
                entryString.ToString());
            LOGGER.Error("Created entry=[" + entry + "]");
            LOGGER.Error("Path=[" + entry.Path + "]");
            LOGGER.Error("FullPath=[" + fullPath + "]");
            return entry;
        }

        /// <summary>
        /// Set the file timestamp.
        /// </summary>
        public void SetTimeStamp() {
            if (null == date) {
                DateTime now = DateTime.Now;
                // File system time is stored without regards to daylight savings time
                //  therefore if the file time is different then we can assume
                //  that daylight savings is in effect.
//                if (now.ToFileTime() != now.Ticks) {
//                    now = now.AddHours(-1);
//                } 
                now = now.AddHours(-1);
                date = DateParser.GetCvsDateString(now);
                this.timestamp = now;
                this.isUtcTimeStamp = false;
            } else {
                this.timestamp = DateParser.ParseCvsDate (date);
                this.isUtcTimeStamp = true;
            }
            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("timestamp=[").Append (timestamp).Append ("]");
                msg.Append ("date=[").Append (date).Append ("]");
                LOGGER.Debug (msg);
            }
        }

        /// <summary>
        /// Parses the cvs entries file.
        /// </summary>
        /// <param name="line"></param>
        public override void Parse(string line)
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
                throw new EntryParseException("not enough tokens in entry line (#" +
                                            tokens.Length + ")\n" + line);
            }
            else if (tokens.Length > 6) {
                throw new EntryParseException("Too many tokens in entry line." +
                                            "tokens.Length=[" + tokens.Length + "]" +
                                            "line=[" + line + "]");
            }

            name      = tokens[1];
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
            Entry entry = new Entry(System.IO.Path.GetTempPath(), line);
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
        public Factory.FileType Type {get {return Factory.FileType.Entries;}}

        /// <summary>Indicates whether the file can contain multiple
        /// lines.</summary>
        /// <returns><code>true</code> if the file can contain multiple
        /// lines; <code>false</code> otherwise.</returns>
        public bool IsMultiLined {
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
    }
}
