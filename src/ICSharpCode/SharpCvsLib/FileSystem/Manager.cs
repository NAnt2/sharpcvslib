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
//
//    Author: Clayton Harbour
//     claytonharbour@sporadicism.com
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using log4net;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.FileSystem {

    /// <summary>
    ///     Manages the addition and creation of cvs files such as
    ///         - Entries
    ///         - Repository
    ///         - Root
    ///         - Tag
    /// </summary>
    // TODO: Change to internalize helpers (accessor)
    public class Manager {
        /// <summary>
        /// The Name of the cvs directory.
        /// </summary>
        public readonly String CVS = PathTranslator.CVS;

        private readonly ILog LOGGER =
            LogManager.GetLogger (typeof (Manager));

        private String workingPath;

        /// <summary>Constructory</summary>
        /// <param name="workingPath">The local directory that is being affected
        ///     during this cvs checkout.  This is used for program control,
        ///     to stop the cvs commands from leaving this sandbox location.</param>
        public Manager (String workingPath) {
            this.workingPath = PathTranslator.ConvertToOSSpecificPath(workingPath);;
        }

        /// <summary>
        ///     Recurse through the directory entries and add a cvs file
        ///         entry for each directory found in the physical path.
        /// </summary>
        /// <param name="path">The path to look in for directory entries.</param>
        public void AddDirectories (String path) {
            LOGGER.Debug("path=[" + path + "]");
            if (!PathTranslator.ContainsCVS(path)) {
                String[] directories = Directory.GetDirectories (path);

                foreach (String directory in directories) {
                    LOGGER.Debug("directory=[" + directory + "]");
                    if (!PathTranslator.ContainsCVS(directory)) {
                        Entry entry = Entry.CreateEntry(directory);
                        LOGGER.Debug("entry=[" + entry + "]");
                        LOGGER.Debug("entry.FullPath=[" + entry.FullPath + "]");
                        this.Add (entry);
                        this.AddDirectories (directory);
                    }
                }
            }
        }

        /// <summary>
        ///     Fetch the cvs file information to update.
        /// </summary>
        /// <param name="directory">The directory to fetch the files information
        ///        from.</param>
        /// <returns>A collection of folders that contain the cvs entries for
        ///       each directory.</returns>
        public Folder[] FetchFilesToUpdate (String directory) {
            ArrayList folders = new ArrayList ();
            Folder folder = new Folder ();
            folder.Repository = (Repository)this.FetchSingle (directory,
                                Factory.FileType.Repository);
            folder.Entries = new Entries ();
            ArrayList colEntries = new ArrayList (this.Fetch (directory,
                                            Factory.FileType.Entries));
            foreach (Entry entry in colEntries) {
                folder.Entries.Add (entry.FullPath, entry);
            }
            folders.Add (folder);
            this.FetchFilesToUpdateRecursive (folders, directory);

            return (Folder[])folders.ToArray (typeof (Folder));
        }

        private void FetchFilesToUpdateRecursive (ArrayList folders,
                String directory) {
            foreach (String subDir in Directory.GetDirectories (directory)) {
                Folder folder = new Folder ();
                folder.Repository = (Repository)this.FetchSingle (directory,
                                    Factory.FileType.Repository);
                ArrayList colEntries = new ArrayList (this.Fetch (directory,
                                                Factory.FileType.Entries));
                foreach (Entry entry in colEntries) {
                    folder.Entries.Add (entry.FullPath, entry);
                }
                folders.Add (folder);
                this.FetchFilesToUpdateRecursive (folders, subDir);
            }
        }

        /// <summary>
        ///     Create a directory entry based on the local directory path.
        /// </summary>
        /// <param name="localPath">The local path to create the directory
        ///     entry for.</param>
        public Entry CreateDirectoryEntry (String localPath) {
            return Entry.CreateEntry(localPath);
        }

        /// <summary>
        ///     Create a directory entry from the specified path translator.
        /// </summary>
        /// <param name="path">The information about the path to create the
        ///     directory entry for.</param>
        /// <returns>An entry object that contains information about the directory
        ///       entry.</returns>
        public Entry CreateDirectoryEntry (PathTranslator path) {
            return this.CreateDirectoryEntry(path.LocalPath);
        }

        /// <summary>
        ///     Add an entry to the filesystem.
        /// </summary>
        /// <param name="cvsEntries">The collection of cvs entries to add
        ///     to the filesystem.  This collection can contain 1 entry,
        ///     as in the case of a Repository entry, or many entries, as
        ///     in the case of an Entries file).</param>
        public void Add (ICvsFile[] cvsEntries) {
            Hashtable newCvsEntries = new Hashtable();

            try {
                ArrayList currentCvsFiles = 
                    new ArrayList(this.Fetch (cvsEntries[0].FullPath, cvsEntries[0].Type));

                int originalCount = currentCvsFiles.Count;
                if (currentCvsFiles.Count >= 1 && !cvsEntries[0].IsMultiLined) {
                    LOGGER.Debug ("The file already has an entry and cannot be modified.");
                    return;
                }
                foreach (ICvsFile currentCvsFile in currentCvsFiles) {
                    if (newCvsEntries.Contains(currentCvsFile.FullPath)) {
                        throw new DuplicateEntryException("Should not have a duplicate.");
                    }
                    newCvsEntries.Add(currentCvsFile.FullPath, currentCvsFile);
                }
            } catch (FileNotFoundException e) {
                // If we can't find the file, chances are this is the first
                //    entry that we are adding.
                LOGGER.Debug (e);
            }

            foreach (ICvsFile cvsFile in cvsEntries) {
                // replace old entry or create new
                if (newCvsEntries.Contains(cvsFile.FullPath)) {
                    LOGGER.Debug("current entry=[" + newCvsEntries[cvsFile.FullPath] + "]");
                    LOGGER.Debug("new entry=[" + cvsFile + "]");
                    newCvsEntries[cvsFile.FullPath] = cvsFile;
                } else {
                    LOGGER.Debug("Adding new entry to the entries file=[" + cvsFile + "]");
                    newCvsEntries.Add(cvsFile.FullPath, cvsFile);
                }
            }

            this.WriteToFile (
                (ICvsFile[])(new ArrayList(newCvsEntries.Values)).ToArray
                (typeof (ICvsFile)));
        }
        /// <summary>
        /// Add the contents of the cvs file object to the respective file.
        /// </summary>
        public void Add (ICvsFile newCvsEntry) {
            String cvsPath = this.GetCvsDir (newCvsEntry);
            LOGGER.Debug("Add ICvsFile cvsPath=[" + cvsPath + "]");

            Hashtable newCvsEntries = new Hashtable();
            try {
                LOGGER.Error("newCvsEntry=[" + newCvsEntry + "]");
                LOGGER.Error("newCvsEntry.Path=[" + newCvsEntry.Path + "]");
                LOGGER.Error("newCvsEntry.FullPath=[" + newCvsEntry.FullPath + "]");
                ArrayList currentCvsFiles = 
                    new ArrayList(this.Fetch (newCvsEntry.Path, newCvsEntry.Type));

                int originalCount = currentCvsFiles.Count;

                if (currentCvsFiles.Count >= 1 && !newCvsEntry.IsMultiLined) {
                    LOGGER.Debug ("The file already has an entry and cannot be modified.");
                    return;
                }
                foreach (ICvsFile currentCvsFile in currentCvsFiles) {
                    if (newCvsEntries.Contains(currentCvsFile.FullPath)) {
                        throw new DuplicateEntryException("Should not have a duplicate.");
                    }
                    newCvsEntries.Add(currentCvsFile.FullPath, currentCvsFile);
                }
                // replace old entry or create new
                if (newCvsEntries.Contains(newCvsEntry.FullPath)) {
                    LOGGER.Error("replacing entry");
                    LOGGER.Error("current entry=[" + newCvsEntries[newCvsEntry.FullPath] + "]");
                    LOGGER.Error("new entry=[" + newCvsEntry + "]");
                    newCvsEntries[newCvsEntry.FullPath] = newCvsEntry;
                } else {
                    LOGGER.Error("Adding new entry to the entries file=[" + newCvsEntry + "]");
                    newCvsEntries.Add(newCvsEntry.FullPath, newCvsEntry);
                }
            } catch (FileNotFoundException e) {
                // If we can't find the file, chances are this is the first
                //    entry that we are adding.
                LOGGER.Error(e);
                newCvsEntries.Add(newCvsEntry.FullPath, newCvsEntry);
            }

            ArrayList modifiedEntries = new ArrayList(newCvsEntries.Values);
            LOGGER.Error("modifiedEntries.Count=[" + modifiedEntries.Count + "]");

            this.WriteToFile (
                (ICvsFile[])modifiedEntries.ToArray
                (typeof (ICvsFile)));
        }

        /// <summary>
        ///     Find an entry given the name of the entry and a starting
        ///         search path.
        /// </summary>
        /// <param name="path">The path to the entry.</param>
        /// <param name="name">The name of the entry to search for.</param>
        /// <returns>The entry object found in the directory path specified if
        ///     found.  If no entry is found then an exception is thrown.</returns>
        /// <exception cref="ICSharpCode.SharpCvsLib.Exceptions.EntryNotFoundException">
        ///     If no directory entry is found.</exception>
        public Entry Find (String path, String name) {
            String errorMsg = "Entry not found.  " +
                            "path=[" + path + "]" +
                            "name=[" + name + "]";
            ICvsFile[] cvsEntries;
            try {
                cvsEntries = this.Fetch (path, Factory.FileType.Entries);
            } catch (IOException e) {
                LOGGER.Error (e);
                throw new EntryNotFoundException (errorMsg);
            }

            foreach (ICvsFile cvsEntry in cvsEntries) {
                Entry entry = (Entry)cvsEntry;
                if (entry.Name.Equals (name)) {
                    return entry;
                }
            }
            throw new EntryNotFoundException (errorMsg);
        }

        /// <summary>
        /// Remove the contents from the cvs control file.
        /// </summary>
        public void Remove (ICvsFile file) {
            String cvsPath = this.GetCvsDir(file);
            this.RemoveFromFile (cvsPath, file.Filename, file.FileContents);
        }

        /// <summary>
        /// Remove the specified entry from the cvs file.
        /// </summary>
        /// <param name="path">The path to the cvs file.</param>
        /// <param name="file">The file that is to be modified, can be any CVS 
        ///     management file (i.e. Entries, Root, etc.).</param>
        /// <param name="line">The line that is to be removed.</param>
        private void RemoveFromFile (String path, String file, String line) {
            Hashtable cvsFiles =
                this.ReadFromFile (path, file);

            Factory factory = new Factory();
            ICvsFile cvsFile = 
                factory.CreateCvsObject(path, factory.GetFileType(file), line);

            cvsFiles.Remove(cvsFile.FullPath);
            this.WriteToFile ((ICvsFile[])(new ArrayList(cvsFiles.Values)).ToArray(typeof(ICvsFile)));
        }

        /// <summary>
        ///     Adds a collection of lines to the cvs file.  The first
        ///         entry overwrites any file currently in the directory
        ///         and all other following entries are appended to the
        ///         file.
        /// </summary>
        /// <param name="entries">The collection of cvs entries to add to the
        ///     file system.</param>
        private void WriteToFile (ICvsFile[] entries) {
            LOGGER.Debug("entries count=[" + entries.Length + "]");
            Hashtable testEntries = 
                this.ReadFromFile(entries[0].Path, entries[0].Filename);
            LOGGER.Debug("test entries count=[" + testEntries.Count + "]");
            if (entries.Length == 1) {
                LOGGER.Debug("stack trace=[" + Environment.StackTrace + "]");
            }

            bool append = false;
            foreach (ICvsFile entry in entries) {
                LOGGER.Debug("fullPath=[" + entry.FullPath + "]");
                String cvsPath = this.GetCvsDir(entry);
                String cvsFullPath = Path.Combine(cvsPath, entry.Filename);
                this.WriteToFile (cvsFullPath,
                                entry.FileContents,
                                append);
                if (!append) {
                    append = true;
                }
            }
        }

        /// <summary>
        ///     Write to the cvs file.
        /// </summary>
        /// <param name="cvsFullPath">The current working directory.</param>
        /// <param name="line">The line to enter into the file.</param>
        /// <param name="append">Whether or not to append to the file.</param>
        private void WriteToFile (String cvsFullPath,
                                    String line,
                                    bool append) {
            this.ValidateInSandbox(cvsFullPath);
            line = line.Replace ("\\", "/");

            if (cvsFullPath.IndexOf(Repository.FILE_NAME) >= 0) {
                LOGGER.Debug("cvsFullPath=[" + cvsFullPath + "]");
                LOGGER.Debug("repository sniffing, stack trace=[" + 
                    Environment.StackTrace + "]");
            }

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Writing to a cvs file.  ");
                msg.Append("cvsFullPath=[").Append(cvsFullPath).Append("]");
                msg.Append("line=[").Append(line).Append("]");
                msg.Append("append=[").Append(append).Append("]");
                LOGGER.Debug (msg);
            }

            StreamWriter sw = null;
            try {
                sw =
                    new StreamWriter(cvsFullPath, append, EncodingUtil.DEFAULT_ENCODING);
                sw.WriteLine (line);
            } finally {
                try {
                    sw.Close();
                } catch (Exception e) {
                    LOGGER.Debug (e);
                    throw e;
                }
            }
        }

        /// <summary>
        /// Indicates whether the path that is being written to is inside or outside
        ///     of the sandbox on the local system.
        /// </summary>
        /// <param name="path">The path that is being written to.</param>
        /// <returns>Returns <code>true</code> if the path being written to is
        ///     inside the working path, otherwise returns <code>false</code>.</returns>
        private bool IsInSandbox (String path) {
            return path.IndexOf(this.workingPath) >= 0;
        }

        /// <summary>
        ///     Checks if a cvs directory exists in the specified path,
        ///         if it does not then it is created.  If the CVS directory
        ///         already exists at the end of the path specified, then it
        ///         is returned untouched.
        /// </summary>
        /// <param name="cvsFile">The full path to the file or directory.</param>
        /// <returns>The path to the cvs directory.</returns>
        internal String GetCvsDir (ICvsFile cvsFile) {
            String path = cvsFile.FullPath;
            if ((path.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                path.EndsWith("/")) && !this.HasCvsDir(path)) {
                // If the full path was passed in then get the directory above.
                StringBuilder msg = new StringBuilder();
                msg.Append("The Path is a directory, management is controlled by cvs folder above.");
                msg.Append("path=[").Append(path).Append("]");
                // because the path ended with a slash 
                // the directory is really the last thing before the slash,
                //  and we need the directory above that...so do this twice
                path = Path.GetDirectoryName(path);

                //msg.Append("need the directory name above this=[").Append(path).Append("]");

                msg.Append("this should be the directory the passed in folder is managed by=[").Append(path).Append("]");
                LOGGER.Debug(msg);
            } /*else if (Directory.Exists(path) && !this.HasCvsDir(path)) {
                // if the directory exists then get the directory above
                StringBuilder msg = new StringBuilder();
                msg.Append("The Path is a directory, but does not have a seperator character.");
                msg.Append("Management is controlled by cvs folder above and we can safely use ");
                msg.Append("GetDirectory(String) to get the management directory.");
                msg.Append("path=[").Append(path).Append("]");

                path = Path.GetDirectoryName(path);

                msg.Append("cvs dir goes in path=[").Append(path).Append("]");
                LOGGER.Error(msg);
            }*/ else if (File.Exists(path) || String.Empty != Path.GetExtension(path)) {
                // if the file exists then get the directory that the file is contained in.
                StringBuilder msg = new StringBuilder();
                msg.Append("The Path is a file, management is controlled by the containing directory.");
                msg.Append("path=[").Append(path).Append("]");

                path = Path.GetDirectoryName(path);

                LOGGER.Debug(msg);
            }  else if (cvsFile is Entry) {
                LOGGER.Info("Is entry file=[" + (cvsFile is Entry) + "], it is a " + cvsFile.GetType().FullName.ToString());
                Entry entry = (Entry)cvsFile;
                if (entry.IsDirectory) {
                    path = Path.GetDirectoryName(path);
                }
            } else {
                StringBuilder msg = new StringBuilder();
                msg.Append("Unable to determine whether this is a file or a directory, ");
                msg.Append("however the path does not end with a directory seperator ");
                msg.Append("character so it should be safe in either case to use GetDirectory(String) ");
                msg.Append("to determine the cvs management folder.");
                msg.Append("path=[").Append(path).Append("]");

                path = Path.GetDirectoryName(path);
                LOGGER.Debug(msg);
                if (LOGGER.IsDebugEnabled) {
                    LOGGER.Debug ("stack trace=[" + Environment.StackTrace + "]");
                }
            }

            String cvsDir = path;
            if (!this.HasCvsDir(cvsDir)) {
                cvsDir = Path.Combine(cvsDir, CVS);
            }

            // verify that there is only one cvs directory
            if (this.HasCvsDir(cvsDir.Replace(CVS, ""))) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Should only have 1 cvs directory.");
                msg.Append("cvsDir=[").Append(cvsDir).Append("]");
                LOGGER.Debug(msg);
                throw new Exception(msg.ToString());
            }
            
            this.ValidateInSandbox(cvsDir);
            if (!Directory.Exists(cvsDir)) {
                Directory.CreateDirectory(cvsDir);
            }
            LOGGER.Debug("path=[" + path + "]");
            LOGGER.Error("GetCvsDir(String)=[" + cvsDir + "]");
            return cvsDir;
        }

        private String RemoveCvsDir (String dir) {
            if (this.HasCvsDir(dir)) {
                dir = Path.GetDirectoryName(dir);
            }

            // verify that there is only one cvs directory
            if (this.HasCvsDir(dir)) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Should only have 1 cvs directory.");
                msg.Append("cvsDir=[").Append(dir).Append("]");
                LOGGER.Debug(msg);
                throw new Exception(msg.ToString());
            }            
            return dir;
        }

        /// <summary>
        ///     Determines if the path ends with the <code>CVS</code> constant.
        /// </summary>
        /// <returns><code>true</code> if the path ends with the <code>CVS</code>
        ///     constant, <code>false</code> otherwise.</returns>
        private bool HasCvsDir (String path) {
            bool hasCvsDir = true;;
            if (path.IndexOf(Path.DirectorySeparatorChar + CVS) < 0) {
                hasCvsDir = false;
            }
            return hasCvsDir;
        }

        /// <summary>
        ///     Fetch a single entry.  If more than one entry is found then an
        ///         exception is thrown.
        /// </summary>
        /// <param name="path">The path to the current working directory
        ///    or to the cvs directory.</param>
        /// <param name="fileType">The type of cvs file to fetch.</param>
        /// <returns>A single <see cref="ICvsFile">Cvs file</see></returns>
        /// <exception cref="FileNotFoundException">If an entries file cannot
        ///     be found.</exception>
        public ICvsFile FetchSingle (String path, Factory.FileType fileType) {
            ICvsFile [] entries = this.Fetch (path, fileType);

            StringBuilder msg = new StringBuilder ();
            msg.Append ("path=[").Append (path).Append ("]");
            msg.Append ("fileType=[").Append (fileType).Append ("]");

            if (entries.Length == 0) {
                msg.Append ("File not found.  ");
                throw new FileNotFoundException (msg.ToString ());
            }
            if (entries.Length > 1) {
                msg.Append ("Expecting maximum of 1 entry.");
                msg.Append ("found=[").Append (entries.Length).Append ("]");
                throw new Exception (msg.ToString ());
            }

            return entries[0];
        }

        /// <summary>
        ///     Fetch a single entry.  If more than one entry is found then an
        ///         exception is thrown.
        /// </summary>
        /// <param name="path">The path to the current working directory
        ///    or to the cvs directory.</param>
        /// <param name="fileType">The type of cvs file to fetch.</param>
        /// <param name="filename">The name of the specific entry to search for.</param>
        /// <returns>A single <see cref="ICvsFile">Cvs file</see></returns>
        /// <exception cref="FileNotFoundException">If the cvs file cannot be found.</exception>
        public ICvsFile FetchSingle (String path, Factory.FileType fileType, String filename) {
            ICvsFile [] entries = this.Fetch (path, fileType);

            foreach (ICvsFile file in entries) {
                if (file.FileContents.IndexOf (filename) != -1) {
                    return file;
                }
            }
            String fullPath = Path.Combine(path, filename);

            if (Directory.Exists (Path.Combine(path, filename))) {
                return new Entry (path, "D/" + Path.GetFileName(filename) + "////");
            } else {
                return new Entry (path, "/" + Path.GetFileName(filename) + "////");
            }
        }

        /// <summary>
        ///     Fetch all of the entry objects for the specified cvs filename
        ///         in the specified path.
        /// </summary>
        /// <param name="path">The path to the current working directory
        ///    or to the cvs directory.</param>
        /// <param name="fileType">The type of the cvs file to fetch.</param>
        /// <returns>A collection of <see cref="ICvsFile">Cvs files</see></returns>
        public ICvsFile [] Fetch (String path, Factory.FileType fileType) {
            Factory factory = new Factory ();
            String filename = factory.GetFilename (fileType);
            Hashtable cvsFiles = this.ReadFromFile (path,
                                                filename);

            ArrayList entries = new ArrayList (cvsFiles.Values);
            return (ICvsFile[])entries.ToArray (typeof (ICvsFile));
        }

        /// <summary>
        /// Fetch the entries from the cvs file located in the subdirectory of
        ///     the path given.
        /// </summary>
        /// <param name="fullPath">The path of the files that are being managed, 
        ///     the cvs directory is derived from this.</param>
        /// <returns>A collection of entries from the cvs management file.</returns>
        public Entries FetchEntries (String fullPath) {
            Entries entries = new Entries();
            Factory factory = new Factory();
            String filename = factory.GetFilename(Factory.FileType.Entries);
            ICollection cvsFiles = 
                this.ReadFromFile(Path.GetDirectoryName(fullPath), filename);

            foreach (DictionaryEntry entryEntry in cvsFiles) {
                Entry entry = (Entry)entryEntry.Value;
                entries.Add(entry.FullPath, entry);
            }
            return entries;
        }


        /// <summary>
        ///     Read the contents of the specified file line by line.
        ///         The contents are placed in a collection object and
        ///         can be later extracted by the specified value object.
        ///         This is used to keep the file access in one location.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="file">The name of the file to read.</param>
        /// <returns>A collection of strings, one for each line
        ///     in the specified file.</returns>
        private Hashtable ReadFromFile (String path, String file) {
            Hashtable fileContents = new Hashtable ();
            //String cvsPath = this.GetCvsDir(path, file);
            String cvsPath = Path.Combine(path, CVS);
            String filePath = Path.Combine(cvsPath, file);
            Factory factory = new Factory();
            LOGGER.Error("filePath=[" + filePath + "]");
            if (File.Exists(filePath)) {
                StreamReader sr = null;
                try {
                    sr = File.OpenText(filePath);

                    while (true) {
                        string line = sr.ReadLine();
                        if (line == null || line.Length == 1) {
                            break;
                        }
                        ICvsFile cvsFile = factory.CreateCvsObject(path, file, line);
                        if (!fileContents.Contains(cvsFile.FullPath)) {
                            fileContents.Add(cvsFile.FullPath, cvsFile);
                        } else {
                            StringBuilder msg = new StringBuilder();
                            msg.Append("Found a duplicate entry in the cvs management file.");
                            msg.Append("Your repository is corrupt.");
                            msg.Append("cvsFile=[").Append(cvsFile).Append("]");
                            throw new DuplicateEntryException (msg.ToString());
                        }
                    }
                } finally {
                    if (null != sr) {
                        try {
                            sr.Close();
                        } catch (Exception e) {
                            LOGGER.Error(e);
                        }
                    }
                }
            }
            return fileContents;
        }

        /// <summary>
        ///     Sets the timestamp on the file specified.  Sets the create
        ///         timestamp, access timestamp and the last write timestamp.
        /// </summary>
        /// <param name="filenameAndPath">The file name and path.</param>
        /// <param name="timeStamp">The timestamp to set on the file.</param>
        /// <param name="correctTimeStampForUtc">Indicates whether the file time stamp
        ///     should be corrected to the UTC timezone, or if it should adopt the
        ///     time for the local time zone.</param>
        public void SetFileTimeStamp (String filenameAndPath, DateTime timeStamp, 
            bool correctTimeStampForUtc) {
            if (File.Exists (filenameAndPath)) {
                DateTime fileTimeStamp;
                if (correctTimeStampForUtc) {
                    fileTimeStamp = DateParser.GetCorrectedTimeStamp (timeStamp);
                } else {
                    fileTimeStamp = timeStamp.AddHours(1);
                    System.Console.WriteLine("fileTimeStamp=[" + fileTimeStamp + "]");
                }

                File.SetCreationTime(filenameAndPath, fileTimeStamp);
                File.SetLastAccessTime(filenameAndPath, fileTimeStamp);
                File.SetLastWriteTime(filenameAndPath, fileTimeStamp);

                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder ();
                    msg.Append ("creation timestamp=[").Append (File.GetCreationTime (filenameAndPath)).Append ("]");
                    msg.Append ("timeStamp=[").Append (timeStamp).Append ("]");
                    LOGGER.Debug (msg);
                }
            }
        }

        /// <summary>
        ///     Populate the tag file (if any) found in the given directory.
        /// </summary>
        /// <param name="directory">The directory containing the cvs folder,
        ///     CVS will be appended to this directory.</param>
        /// <returns>The tag file object that holds the contents of the tag
        ///     in the given directory (if any).</returns>
        public Tag FetchTag (String directory) {
            try {
                return
                    (Tag)this.FetchSingle (directory, Factory.FileType.Tag);
            } catch (FileNotFoundException e) {
                LOGGER.Debug (e);
                // No tag information found, this is normal.  Return null.
                return null;
            }
        }

        /// <summary>
        ///     Load the cvs entry from the file.  Each cvs entry contains all
        ///         of the information that is needed to update the individual
        ///         file from the cvs repository.
        /// </summary>
        /// <param name="directory">The path to the directory that contains the 
        ///     file.</param>
        /// <param name="fileName">The name of the file that we are looking for.</param>
        /// <exception cref="EntryNotFoundException">If the given entry cannot
        ///     be found in the cvs file.</exception>
        public Entry FetchEntry (String directory, String fileName) {
            return this.FetchEntry(Path.Combine(directory, fileName));
        }

        /// <summary>
        /// Fetch the information for a file that is under cvs control from the 
        ///     <code>CVS\Entries</code> folder.  If no file is found then an
        ///     exception is thrown.
        /// </summary>
        /// <param name="fullPath">The directory and file name of the file under
        ///     cvs control.</param>
        /// <returns>The entry object, containing the information from the Entries
        ///     file.</returns>
        /// <exception cref="EntryNotFoundException">If the given entry cannot
        ///     be found in the cvs file.</exception>
        public Entry FetchEntry (String fullPath) {
            Entries entries = this.FetchEntries(fullPath);

            if (entries.Contains (fullPath)) {
                return entries[fullPath];
            }
            StringBuilder msg = new StringBuilder();
            msg.Append("Unable to find an entry for the file specified.");
            msg.Append("fullPath=[").Append(fullPath).Append("]");
            throw new EntryNotFoundException(msg.ToString());
        }

        /// <summary>
        /// Fetch the repository information from the cvs folder in the given directory.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <returns>The repository folder.</returns>
        public Repository FetchRepository (String directory) {
            Repository repository = 
                (Repository)this.FetchSingle (directory, Factory.FileType.Repository);
            return repository;
        }

        /// <summary>
        /// Fetch the root information from the cvs folder in the given directory.
        /// </summary>
        /// <param name="directory">The directory to search for the information in.</param>
        /// <returns>A root object that represents the cvs root information in
        ///     the given cvs folder.</returns>
        public Root FetchRoot (String directory) {
            Root root =
                (Root)this.FetchSingle(directory, Factory.FileType.Root);
            return root;
        }

        /// <summary>
        ///     Load the entry files in the given directory.
        ///
        /// NOTE: This should be recursive (and will be) just lazy tonight.
        /// </summary>
        /// <param name="directory">The directory to start loading the
        ///     file name from.</param>
        /// <returns>A collection of <see cref="Entry">Entries</see>.</returns>
        public ICollection LoadEntries (String directory) {
            String[] files = Directory.GetFiles (directory);

            ArrayList entries = new ArrayList ();

            foreach (String file in files) {
                Entry entry = this.FetchEntry (Path.GetDirectoryName (directory),
                                            Path.GetFileName (file));
                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder ();
                    msg.Append ("Entry=[").Append (entry).Append ("]");
                    LOGGER.Debug (msg);
                }
                entries.Add (entry);
            }

            return entries;
        }

        /// <summary>
        ///     Create the repository file in the cvs sub directory of the
        ///         current working directory.
        /// </summary>
        /// <param name="workingDirectory">Holds information about the current
        ///     path and cvs root.</param>
        /// <param name="localPath">The local path response sent down from
        ///     the server.</param>
        /// <param name="repositoryPath">The path to the file name on the
        ///     server.</param>
        /// <returns>The object contents of the newly created repository file.</returns>
        public Repository AddRepository (WorkingDirectory workingDirectory,
                                        String localPath,
                                        String repositoryPath) {
            PathTranslator pathTranslator =
                new PathTranslator (workingDirectory,
                                    repositoryPath);
            Factory factory = new Factory ();

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nAdd Repository File.");
                msg.Append ("\n\tworkingDirectory=[").Append (workingDirectory).Append ("]");
                msg.Append ("\n\tlocalPath=[").Append (localPath).Append ("]");
                msg.Append ("\n\trepositoryPath=[").Append (repositoryPath).Append ("]");

                LOGGER.Debug (msg);
            }
            String repositoryContents = workingDirectory.ModuleName + "/" +
                                        pathTranslator.RelativePath;

            String path = pathTranslator.LocalPathAndFilename;
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                path = path + Path.DirectorySeparatorChar.ToString();
            }
            Repository repository =
                (Repository)factory.CreateCvsObject (path,
                                                    Factory.FileType.Repository,
                                                    repositoryContents);
            this.Add (repository);

            return repository;
        }

        /// <summary>
        ///     Create the root file in the local cvs directory.  This file holds
        ///         the details about the cvs root used in this sandbox.
        /// </summary>
        /// <param name="workingDirectory">Holds information about the current
        ///     path and cvs root.</param>
        /// <param name="localPath">The local path response sent down from
        ///     the server.</param>
        /// <param name="repositoryPath">The path to the file name on the
        ///     server.</param>
        /// <returns>The object contents of the newly created root file.</returns>
        public Root AddRoot (WorkingDirectory workingDirectory,
                            String localPath,
                            String repositoryPath) {
            PathTranslator pathTranslator =
                new PathTranslator (workingDirectory,
                                    repositoryPath);
            Factory factory = new Factory ();

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nAdd Root File.");
                msg.Append ("\n\tworkingDirectory=[").Append (workingDirectory).Append ("]");
                msg.Append ("\n\tlocalPath=[").Append (localPath).Append ("]");
                msg.Append ("\n\trepositoryPath=[").Append (repositoryPath).Append ("]");

                LOGGER.Debug (msg);
            }

            Root root =
                (Root)factory.CreateCvsObject (pathTranslator.LocalPath + Path.DirectorySeparatorChar,
                                            Factory.FileType.Root,
                                            pathTranslator.CvsRoot.ToString ());
            this.Add (root);

            return root;
        }

        /// <summary>
        ///     Create the root file in the local cvs directory.  This file holds
        ///         the details about the cvs root used in this sandbox.
        /// </summary>
        /// <param name="workingDirectory">Holds information about the current
        ///     path and cvs root.</param>
        /// <param name="localPath">The local path response sent down from
        ///     the server.</param>
        /// <param name="repositoryPath">The path to the file name on the
        ///     server.</param>
        /// <param name="stickyTag">The sticky tag to add to the tag file.</param>
        /// <returns>The object contents of the newly created root file.</returns>
        public Tag AddTag (WorkingDirectory workingDirectory, String localPath,
            String repositoryPath, String stickyTag) {
            PathTranslator pathTranslator =
                new PathTranslator (workingDirectory,
                repositoryPath);
            Factory factory = new Factory ();

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nAdd Root File.");
                msg.Append ("\n\tworkingDirectory=[").Append (workingDirectory).Append ("]");
                msg.Append ("\n\tlocalPath=[").Append (localPath).Append ("]");
                msg.Append ("\n\trepositoryPath=[").Append (repositoryPath).Append ("]");

                LOGGER.Debug (msg);
            }

            Tag tag =
                (Tag)factory.CreateCvsObject (pathTranslator.LocalPath + Path.DirectorySeparatorChar,
                Factory.FileType.Tag, pathTranslator.CvsRoot.ToString ());
            this.Add (tag);

            return tag;
        }

        /// <summary>
        ///     Create the root file in the local cvs directory.  This file holds
        ///         the details about the cvs root used in this sandbox.
        /// </summary>
        /// <param name="workingDirectory">Holds information about the current
        ///     path and cvs root.</param>
        /// <param name="localPath">The local path response sent down from
        ///     the server.</param>
        /// <param name="repositoryPath">The path to the file name on the
        ///     server.</param>
        /// <param name="entry">The string value that represents the cvs
        ///     entry.</param>
        /// <returns>The contents of the newly created entries file that match
        ///     the given file name created.</returns>
        public Entry AddEntry (WorkingDirectory workingDirectory,
                            String localPath,
                            String repositoryPath,
                            String entry) {
            PathTranslator pathTranslator =
                new PathTranslator (workingDirectory,
                                    repositoryPath);
            Factory factory = new Factory ();

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nAdd Entry.");
                msg.Append ("\n\tlocalPath=[").Append (localPath).Append ("]");
                msg.Append ("\n\trepositoryPath=[").Append (repositoryPath).Append ("]");
                LOGGER.Debug (msg);
            }

            System.Console.WriteLine("pathTranslator.LocalPathAndFilename=[" + 
                pathTranslator.LocalPathAndFilename + "]");
            Entry cvsEntry = Entry.CreateEntry(pathTranslator.LocalPathAndFilename);

//            if (pathTranslator.IsDirectory) {
//                cvsEntry = 
//                    this.CreateDirectoryEntry (pathTranslator.LocalPathAndFilename);
//            } else {
//                (Entry)factory.CreateCvsObject (pathTranslator.LocalPath,
//                    Factory.FileType.Entries,
//                   entry);
//            }

            // make sure the directory is not added outside of the current working
            //  directory
            this.ValidateInSandbox(cvsEntry.FullPath);
            this.Add(cvsEntry);

            return cvsEntry;
        }

        /// <summary>
        /// Gets one folder, or a collection of folder depending on whether the 
        ///     path passed in is a directory (recursive get) or a single file.
        /// </summary>
        /// <param name="path">The file or directory to fetch the folder(s) for.</param>
        /// <returns>A folders object which can contain one or many folder
        ///     objects.</returns>
        public Folders GetFolders (String path) {
            // Get the file system information.
            Probe probe = new Probe ();
            probe.Start = path;
            probe.Execute ();

            ICollection fileList = probe.Files;

            Folders folders = this.GetFolders(fileList);

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("count of files=[").Append(fileList.Count).Append("]");
                LOGGER.Debug(msg);
            }

            return folders;
        }

        /// <summary>
        /// Populates a Folders collection for the files that have been specified
        ///     in the file list.
        /// </summary>
        /// <param name="files">Files on the filesystem that are under cvs
        ///     control.</param>
        /// <returns>A collection of Folders object that encapsulates the cvs 
        ///     repository information for the collection of files specified.</returns>
        public Folders GetFolders(ICollection files) {
            Folders folders = new Folders();
            foreach (String file in files) {
                LOGGER.Debug("file=[" + file + "]");
                Folder folder;
                if (!folders.Contains (Path.GetDirectoryName(file))) {
                    folder = this.CreateFolder(file);
                    folders.Add (Path.GetDirectoryName(file), folder);
                } else {
                    folder = folders[Path.GetDirectoryName(file)];
                }

                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder ();
                    msg.Append("file=[").Append(file).Append("]");
                    msg.Append("foundFolder=[").Append(folder).Append("]");
                    LOGGER.Debug (msg);
                }
                // If the entry file is not already contained in the entries 
                //      collection then add it.
                if (!folder.Entries.Contains (Path.GetFullPath(file))) {
                    Entry entry;
                    try {
                        entry = this.FetchEntry (file);
                        folder.Entries.Add (entry.FullPath, entry);
                    } catch (EntryNotFoundException e) {
                        LOGGER.Debug(@"Entry not found, probably does not exist, 
                            or is new file.  Wait for add to add it.", e);
                    }
                }
            }

            return folders;
        }

        /// <summary>
        /// Creates a new folder that represents the directory name specified in 
        ///     the path string passed in.  The folder is also populated with
        ///     the following objects from the <code>CVS</code> directory:
        ///     <ul>
        ///         <li>Repository</li>
        ///         <li>Root</li>
        ///         <li>Tag</li>
        ///     </ul>
        ///     Entries are then populated for each file in the filesystem.
        /// </summary>
        /// <param name="path">A path that represents the new folder location
        ///     on the filesystem.</param>
        /// <returns>A new folder object that contains the information stored in
        ///     the cvs folder.</returns>
        private Folder CreateFolder (String path) {
            Folder newFolder = new Folder();
            newFolder.Entries = new Entries ();
            newFolder.Repository = this.FetchRepository (Path.GetDirectoryName(path));
            newFolder.Tag = this.FetchTag (Path.GetDirectoryName(path));
            newFolder.Root = this.FetchRoot (Path.GetDirectoryName(path));

            return newFolder;
        }

        private void ValidateInSandbox (String path) {
            if (!IsInSandbox(PathTranslator.ConvertToOSSpecificPath(path))) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Unable to write outside of sandbox.  ");
                msg.Append("Attempting to write to path=[").Append(path).Append("]");
                msg.Append("Sandbox path=[").Append(this.workingPath).Append("]");
                throw new InvalidPathException(msg.ToString());
            }
        }
    }
}
