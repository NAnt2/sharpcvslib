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

        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (Manager));
        /// <summary>The cvs directory information.</summary>
        public string CVS {
            get {return "CVS";}
        }
        
        /// <summary>Constructory</summary>        
        public Manager () {
        }
        
        /// <summary>
        ///     Recurse through the directory entries and add a cvs file 
        ///         entry for each directory found in the physical path.
        /// </summary>
        /// <param name="path">The path to look in for directory entries.</param>
        public void AddDirectories (String path) {
            String [] directories;
            try {
                directories = Directory.GetDirectories (path);
            } catch (IOException e) {
                LOGGER.Error (e);
                return;
            }
            LOGGER.Error ("path=[" + path + "]");
            foreach (String directory in directories) {
                // Only add the directory if the folder already contains a
                //    cvs directory.
                try {
                    Entry dirEntry = 
                        this.CreateDirectoryEntryFromPath (path);
                    if (LOGGER.IsDebugEnabled) {
                        String msg = "Adding cvs entry for directory.  " +
                            "directory=[" + directory + "]" +
                            "dirEntry=[" + dirEntry + "]";
                        LOGGER.Debug (msg);    
                    }
                    this.Add (dirEntry);

                } catch (EntryNotFoundException e) {
                    LOGGER.Debug (e);
                }

                this.AddDirectories (directory);
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
            folder.Entries = new ArrayList (this.Fetch (directory, 
                                                        Factory.FileType.Entries));
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
                folder.Entries = new ArrayList (this.Fetch (directory, 
                                                            Factory.FileType.Entries));
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
            return this.CreateDirectoryEntryFromPath (localPath);            
        }
        
        /// <summary>
        ///     Create a directory entry from the specified path translator.
        /// </summary>
        /// <param name="path">The information about the path to create the
        ///     directory entry for.</param>
        /// <returns>An entry object that contains information about the directory
        ///       entry.</returns>
        public Entry CreateDirectoryEntry (PathTranslator path) {
            return this.CreateDirectoryEntryFromPath (path.LocalPath);
        }
                
        /// <summary>
        ///     Create a directory entry from the given path.
        /// </summary>
        /// <param name="path">The path to use in creating the entry.</param>
        /// <returns>The directory entry.</returns>
        private Entry CreateDirectoryEntryFromPath (String path) {
            bool hasCvsDir = this.HasCvsDir (path);
            if (LOGGER.IsDebugEnabled) {
                String msg = "CreateDirectory from path.  " + 
                    "path=[" + path + "]" +
                    "hasCvsDir=[" + hasCvsDir + "]";
                LOGGER.Debug (msg);                
            }
            if (hasCvsDir) {
                String upPath = Directory.GetParent (path).FullName;
                if (!this.HasCvsDir (upPath)) {
                    String msg = "No cvs directory found in the parent " +
                        "path.  This may be the root folder.  " +
                        "upPath=[" + upPath + "]";
                    throw new EntryNotFoundException (msg);
                }
                path = path.Replace ('\\', '/');
                string[] dirTokens = path.Split ('/');
                
                string dirToken = dirTokens[dirTokens.Length - 1];
                if (LOGGER.IsDebugEnabled) {
                    String msg = "Looking at dir tokens.  ";
                    
                    foreach (String token in dirTokens) {
                        msg = msg + ";  token=[" + token + "]";
                    }
                    LOGGER.Debug (msg);
                }
                string dirEntry = "D/" + dirToken;
                
                // If there is some path information append empty slashes,
                //     otherwise just leave the entry as 'D/'.
                if (dirEntry.Length > 2) {
                    int addSlashes = 6 - dirEntry.Split ('/').Length;
                    for (int slashes = 0; slashes < addSlashes; slashes++) {
                        dirEntry = dirEntry + "/";
                    }
                }
                if (LOGGER.IsDebugEnabled) {
                    String msg = "Create directory entry from path.  " +
                        "dirEntry=[" + dirEntry + "]" +
                        "dirToken=[" + dirToken + "]" +
                        "path=[" + path + "]";
                    LOGGER.Debug (msg);
                }
                Entry entry = new Entry (upPath, dirEntry);
                return entry;
            }
            String errorMsg = "No cvs directory found under this path.  " +
                "This directory may not be part of cvs.  " +
                "path=[" + path + "]";
            throw new EntryNotFoundException (errorMsg);
        }
        
        /// <summary>
        ///     Add an entry to the filesystem.
        /// </summary>
        /// <param name="cvsEntries">The collection of cvs entries to add
        ///     to the filesystem.  This collection can contain 1 entry,
        ///     as in the case of a Repository entry, or many entries, as
        ///     in the case of an Entries file).</param>
        public void Add (ICvsFile[] cvsEntries) {
            foreach (ICvsFile entry in cvsEntries) {
                this.Add (entry);
            }
        }
        /// <summary>
        ///     Add the contents of the cvs file object to the respective
        ///         file.
        /// </summary>
        public void Add (ICvsFile newCvsEntry) {
            String cvsPath = this.CombineCvsDir (newCvsEntry.Path);
            
            ArrayList newCvsEntries = new ArrayList ();
            
            bool newEntry = true;
            
            try {
                ICollection cvsFiles = this.Fetch (cvsPath,
                                                   newCvsEntry.Type);
                
                if (cvsFiles.Count >= 1 && !newCvsEntry.IsMultiLined) {
                    LOGGER.Debug ("The file already has an entry and cannot be changed.");
                    return;
                }
                foreach (ICvsFile currentCvsEntry in cvsFiles) {
                    if (currentCvsEntry.Equals (newCvsEntry)) {
                        newCvsEntries.Add (newCvsEntry);
                        newEntry = false;
                    }
                    else {
                        newCvsEntries.Add (currentCvsEntry);
                    }
                }
            } catch (FileNotFoundException e) {
                // If we can't find the file, chances are this is the first
                //    entry that we are adding.
                LOGGER.Debug (e);
                newEntry = true;
            }
            
            if (newEntry) {
                newCvsEntries.Add (newCvsEntry);
            }
            
            this.WriteToFile (
                              (ICvsFile[])newCvsEntries.ToArray 
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
                LOGGER.Debug (e);
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
        ///     Remove the file contents from the file.
        /// </summary>
        public void Remove (ICvsFile file) {
            String cvsPath = this.CombineCvsDir (file.Path);
            this.RemoveFromFile (cvsPath, file.Filename, file.FileContents);
        }
        
        private void RemoveFromFile (String path, String file, String line) {
            ICollection fileLines = 
                this.ReadFromFile (path, file);
            
            ArrayList newFileLines = new ArrayList ();
            foreach (String fileLine in fileLines) {
                if (!fileLine.Equals (line)) {
                    newFileLines.Add (line);
                }
            }
            this.WriteToFile (path, 
                              file, 
                              (String[])newFileLines.ToArray (typeof (String)));
        }
        
        /// <summary>
        ///     Adds a collection of lines to the cvs file.  The first
        ///         entry overwrites any file currently in the directory
        ///         and all other following entries are appended to the
        ///         file.
        /// </summary>
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="lines">A collection of String value lines to add to the cvs file.</param>
        private void WriteToFile (String path,
                                  String file, 
                                  String[] lines) {
            bool overWriteFile = true;
            foreach (String line in lines) {
                this.WriteToFile (path, file, line, overWriteFile);
                if (overWriteFile) {
                    overWriteFile = false;
                }
            }
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
            bool append = false;
            
            foreach (ICvsFile entry in entries) {
                String cvsPath = this.CombineCvsDir (entry.Path);
                this.WriteToFile (cvsPath, 
                                  entry.Filename, 
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
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="line">The line to enter into the file.</param>
        /// <param name="append">Whether or not to append to the file.</param>
        private void WriteToFile (    String path, 
                                      String file, 
                                      String line, 
                                      bool append) {
            string fileAndPath = Path.Combine (path, file.Replace ("/", "").Replace ("\\", ""));
            this.CreateCvsDir (path);
            line = line.Replace ("\\", "/");
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Writing to a cvs file.  " +
                    "path=[" + path + "]" +
                    "file=[" + file + "]" +
                    "line=[" + line + "]" + 
                    "append=[" + append + "]";
                LOGGER.Debug (msg);
            }
            
            StreamWriter sw = 
                new StreamWriter(fileAndPath, append, Encoding.ASCII);
            sw.WriteLine (line);    
            
            sw.Close();            
        }
        
        /// <summary>
        ///     Checks if a cvs directory exists in the specified path,
        ///         if it does not then it is created.
        /// </summary>
        /// <param name="path">The full directory path of the
        ///     directory.</param>
        private void CreateCvsDir (String path) {
            String cvsDir = this.CombineCvsDir (path);
            bool dirExists = Directory.Exists (cvsDir);
            if (LOGGER.IsDebugEnabled) {
                String msg = "Creating cvs directory if it does not exist.  " +
                    "path=[" + path + "]" +
                    "cvsDir=[" + cvsDir + "]" +
                    "dirExists=[" + dirExists + "]";
                LOGGER.Debug (msg);
            }
            
            if (!dirExists) {
                Directory.CreateDirectory(cvsDir);
            }            
        }
        
        /// <summary>
        ///     Determines if the path ends with the <code>CVS</code> constant.
        /// </summary>
        /// <returns><code>true</code> if the path ends with the <code>CVS</code>
        ///     constant, <code>false</code> otherwise.</returns>
        private bool HasCvsDir (String path) {
            String cvsDir = Path.Combine (path, this.CVS);
            
            return Directory.Exists (cvsDir);
        }
        
        /// <summary>
        ///     Determine if the path has the CVS folder at the end of the path.
        /// </summary>
        /// <returns><code>true</code> if the path contains the cvs directory, 
        ///     <code>false</code> otherwise.</returns>
        private bool HasCvsDirInPath (String path) {
            String cvsDir = Path.Combine (path, this.CVS);
            
            String[] dirs = path.Replace ('\\', '/').Split ('/');
            bool hasCvsDir = (dirs[dirs.Length - 1].Equals (this.CVS));
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Does this path have a cvs directory at end of file?  " +
                    "hasCvsDir=[" + hasCvsDir + "]" +
                    "path=[" + path + "]";
                LOGGER.Debug (msg);
            }
            return hasCvsDir;
        }
        
        /// <summary>
        ///     Add the <code>CVS</code> constant to the directory path if it
        ///         does not exist.
        /// </summary>
        /// <returns>The current path if it ends with the <code>CVS</code> 
        ///     constant, or the path plus the <code>CVS</code> constant if it
        ///     does not already contain this.</returns>
        private String CombineCvsDir (String path) {
            bool hasCvs = this.HasCvsDirInPath (path);            
            String cvsDir;            
            if (hasCvs) {
                cvsDir = path;
            }
            else {
                cvsDir = Path.Combine (path, this.CVS);
            }
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Cvsdir=[" + cvsDir + "]";
                LOGGER.Debug (msg);
            }
            
            return cvsDir;
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

            StringBuilder msg = new StringBuilder ();
            msg.Append ("File not found.  ");
            msg.Append ("path=[").Append (path).Append ("]");
            msg.Append ("fileType=[").Append (fileType).Append ("]");
            throw new FileNotFoundException (msg.ToString ());
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
            String cvsDir = this.CombineCvsDir (path);
            Factory factory = new Factory ();
            String filename = factory.GetFilename (fileType);
            ICollection lines = this.ReadFromFile (cvsDir, 
                                                   filename);
            ArrayList entries = new ArrayList ();
            
            foreach (String line in lines) {
                entries.Add (factory.CreateCvsObject (cvsDir, 
                                                      fileType, 
                                                      line));
            }
            
            return (ICvsFile[])entries.ToArray (typeof (ICvsFile));
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
        private ICollection ReadFromFile (String path, String file) {
            ArrayList fileContents = new ArrayList ();
            String cvsPath = this.CombineCvsDir (path);
            
            String filePath = Path.Combine (cvsPath, file);
            if (File.Exists(filePath)) {
                StreamReader sr = File.OpenText(filePath);
                
                while (true) {
                    string line = sr.ReadLine();
                    if (line == null) {
                        break;
                    }
                    if (line.Length > 1) {      
                        if (LOGGER.IsDebugEnabled) {
                            String msg = "Found cvs file, adding contents.  " +
                                "path=[" + path + "]" +
                                "file=[" + file + "]" +
                                "line=[" + line + "]";
                            LOGGER.Debug (msg);
                        }
                        
                        fileContents.Add(line);
                    }
                }
                sr.Close();
            }
            
            return fileContents;
        }
        
        /// <summary>
        ///     Sets the timestamp on the file specified.  Sets the create 
        ///         timestamp, access timestamp and the last write timestamp.
        /// </summary>
        /// <param name="filenameAndPath">The file name and path.</param>
        /// <param name="timeStamp">The timestamp to set on the file.</param>
        public void SetFileTimeStamp (String filenameAndPath, DateTime timeStamp) {
            if (File.Exists (filenameAndPath)) {
                DateTime fileTimeStamp = 
                    DateParser.GetCorrectedTimeStamp (timeStamp);

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
        /// <exception cref="FileNotFoundException">If the file cannot be found.</exception>
        public Tag FetchTag (String directory) {
            try {
                return
                    (Tag)this.FetchSingle (directory, Factory.FileType.Tag);
            } catch (FileNotFoundException e) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("No tag information found for file.");
                msg.Append ("Path=[").Append (directory).Append ("]");
                LOGGER.Debug (msg, e);
                throw e;
            }
        }
        
        /// <summary>
        ///     Load the cvs entry from the file.  Each cvs entry contains all
        ///         of the information that is needed to update the individual 
        ///         file from the cvs repository.  
        /// </summary>
        public Entry FetchEntry (String directory, String filename) {
            Entry entry = 
                (Entry)this.FetchSingle (directory, Factory.FileType.Entries, filename);
            
            try {
                
            } catch (FileNotFoundException) {
                // No tag information found for this file.
            }
            Repository repository = 
                (Repository)this.FetchSingle (directory, Factory.FileType.Repository);
            Root root = 
                (Root)this.FetchSingle (directory, Factory.FileType.Root);
            
            return entry;
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

            Repository repository = 
                (Repository)factory.CreateCvsObject (pathTranslator.LocalPath,
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
                (Root)factory.CreateCvsObject (pathTranslator.LocalPath,
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
            
            Entry _entry = 
                (Entry)factory.CreateCvsObject (pathTranslator.LocalPath,
                                                Factory.FileType.Entries, 
                                                entry);

            if (pathTranslator.IsDirectory) {
                _entry = this.CreateDirectoryEntry (pathTranslator.LocalPath);
            } else {
                this.Add (_entry);
            }

            return _entry;
        }

    }
    
}
