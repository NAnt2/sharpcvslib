#region "Copyright"
// Copyright (C) 2003 Gerald Evans
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
//    <author>Gerald Evans</author>
//
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.Misc;

namespace ICSharpCode.SharpCvsLib.Extension.ChangeLogReport {
    /// <summary>
    /// Produces an XML change log of a cvs project.
    /// Typical usage is as follows:
    /// 
    ///    string password = "password";
	///    string xmlFilename = "C:\\tmp\\output.xml";
    ///	    
    ///    CvsChangeLog cvsChangeLog = new CvsChangeLog("sharpcvslib", "C:\\sharpcvslib");
    ///
    ///    cvsChangeLog.AddNameMapping("gne", "Gerald Evans"); 
    ///    cvsChangeLog.AddNameMapping("drakmar", "Clayton Harbour");
    ///    cvsChangeLog.AddNameMapping("skyward", "Steve Kenzell");
    /// 
    ///    cvsChangeLog.SetLastNDays(7);
    ///    // or cvsChangeLog.StartDate = new DateTime(...);
    ///    // and/or cvsChangeLog.EndDate = new DateTime(...);
    ///	    
    ///    cvsChangeLog.Run(xmlFilename, password);
    ///
    /// </summary>
    class CvsChangeLog
    {
    	// data set by ctor
        private string module;
        private string localDirectory;
        
    	// date information set by caller
    	private DateTime startDate;
    	private bool hasStartDate;
    	private DateTime endDate;
    	private bool hasEndDate;
    
        // Name mapping set by caller    
        private StringDictionary nameMap = new StringDictionary();   
     
        // Represents what we want next from the messages output by the log command
        private enum LogState { 
            WANT_FILE,
            WANT_REVISION,
            WANT_DATE,
            WANT_COMMENT,
            WANT_PREV_REVISION
        }
        private LogState logState = LogState.WANT_FILE;
    
        // data currently extracted from the log command output
        private string date;
        private string author;
        private string comment;
        private string filename;
        private string revision;
        private string prevRevision;    
        
        // This is where we accumulate information on all the entries from the log command
        private SortedList entries = new SortedList();
        
        /// <summary>
        /// ctor
        /// </summary>
        public CvsChangeLog(string module, string localDirectory)
        {
            this.module = module;
            this.localDirectory = localDirectory;
        }
    
        /// <summary>
        /// If set, only report changes on or after this date
        /// </summary>
        public DateTime StartDate 
        {
        	set { 
        		startDate = value;
        		hasStartDate = true;
        	}
        }
            
        /// <summary>
        /// If set, only report changes on or before this date
        /// </summary>
        public DateTime EndDate 
        {
        	set { 
        		endDate = value;
        		hasEndDate = true;
        	}
        }
        
        /// <summary>
        /// Only report changes during the last given number of days.
        /// </summary>
        public void SetLastNDays(int days)
        {
        	//endDate = DateTime.Now;
        	startDate = DateTime.Now.AddDays(- days);
        	hasStartDate = true;
        	//hasEndDate = true;
        }
        
        /// <summary>
        /// Adds a single mapping between a user name as used within cvs and
        /// a full users name.
        /// Can be called multiple times, once for each user name to add.
        /// </summary>
        public void AddNameMapping(string id, string fullName)
        {
            nameMap.Add(id, fullName); 
        }
    
        /// <summary>
        /// Produce the report
        /// </summary>
        public void Run(string xmlFilename, string password)
        {
            // read Root and Repository from local directory
            Manager manager = new Manager(localDirectory);
            Repository repository = (Repository)manager.FetchSingle (localDirectory,
                                Factory.FileType.Repository);
            Root root = (Root)manager.FetchSingle (localDirectory,
                                Factory.FileType.Root);
        
            CvsRoot cvsRoot = new CvsRoot(root.FileContents);
            
            WorkingDirectory workingDirectory = new WorkingDirectory(cvsRoot,
                                                                     localDirectory,
                                                                     module);
        
            // Recursively add all cvs folders/files under the localDirectory
            workingDirectory.FoldersToUpdate = FetchFiles(localDirectory);
            
            LogCommand command = new LogCommand(workingDirectory, repository.FileContents, null);
    
            // add any date restrictions        
            if (hasStartDate && hasEndDate) {
            	command.AddInclusiveDateRange(startDate, endDate);
            } else if (hasStartDate) {
            	command.AddInclusiveDateStart(startDate);
            } else if (hasEndDate) {
            	command.AddInclusiveDateEnd(endDate);
            }
            
            // Get a connection
            CVSServerConnection connection = new CVSServerConnection();
     
            // Initialse state machine
            ResetState();
            
            try {
                connection.MessageEvent.MessageEvent += new EncodedMessage.MessageHandler(OnMessage);
                connection.Connect(workingDirectory, password);
                command.Execute(connection);
            } catch (AuthenticationException e) {
                System.Console.WriteLine("Authentication error: {0}", e.Message);
            } catch (Exception e) {
                System.Console.WriteLine("Connection error: {0}", e.Message);
            }
            
            // clean up
            connection.Close();
             
            // now output the XML
            try {
                XmlTextWriter textWriter = new XmlTextWriter(xmlFilename, new UTF8Encoding());
                textWriter.Formatting = Formatting.Indented;
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("changelog");
                
                // add the entries ...
                foreach (DictionaryEntry de in entries) {
                    LogEntry logEntry = (LogEntry)de.Value;
                    logEntry.ExportToXml(textWriter, nameMap);
                }
                
                // finish off
                textWriter.WriteEndElement();    // changelog
                textWriter.WriteEndDocument();
                textWriter.Close();
            } catch (Exception e) {
                System.Console.WriteLine("XML create/write error: {0}", e.Message);
            }
        }
        
        /// <summary>
        /// Returns a list of all the folders (and the files in each of those folders)
        /// that we need to get the change log for.
        /// </summary>
        private Folder[] FetchFiles(string localDirectory)
        {
            ArrayList folders = new ArrayList ();
            FetchFilesRecursive(folders, localDirectory);
            return (Folder[])folders.ToArray (typeof (Folder));
        }
         
        private void FetchFilesRecursive(ArrayList folders, string localDirectory)
        {
            Folder folder = new Folder ();
            Manager manager = new Manager(localDirectory);
            
            folder.Repository = (Repository)manager.FetchSingle (localDirectory,
                                Factory.FileType.Repository);
            ArrayList colEntries = new ArrayList (manager.Fetch (localDirectory,
                                            Factory.FileType.Entries));
            foreach (Entry entry in colEntries) {
                if (!entry.IsDirectory) {
    //Console.WriteLine("Found file {0}", entry.FullPath);
                    folder.Entries.Add (entry.FullPath, entry);
                }
            }
            folders.Add (folder);
            
            foreach (Entry entry in colEntries) {
                if (entry.IsDirectory) {
                    string childDir = Path.Combine(localDirectory, entry.Name);
    //Console.WriteLine("Found directory {0}", childDir);
                    FetchFilesRecursive(folders, childDir);
                }
            }
        }
       
    //    /// <summary>
    //    /// </summary>
    //    public void OnResponse(string message)
    //    {
    //        Console.Write(message);
    //    }
        
        /// <summary>
        /// This is called for each Message response we receive from the cvs server.
        /// </summary>
        public void OnMessage(string message)
        {
            const string filePrefix = "Working file: ";
            const string datePrefix = "date: ";
            const string revisionPrefix = "revision ";
            const string fileEndPrefix = "==========";
            const string revisionEndPrefix = "----------";
     
            //System.Console.WriteLine(message);
    
            // only process the lines starting with "M "        
            if (message.StartsWith("M ")) {
                // Strip of the leading "M "
                message = message.Substring(2);
            
                switch (logState) {
                case LogState.WANT_FILE:
                    // file line is of form 'Working file: <filename>'
                    if (message.StartsWith(filePrefix)) {
                        filename = message.Substring(filePrefix.Length);
                        logState = LogState.WANT_REVISION;
                    }
                    break;
                                
                case LogState.WANT_REVISION:
                    // revision line is of form 'revision: <revision>'
                    if (message.StartsWith(fileEndPrefix)) {
                        // End of file - looks like there were no revisions for thie file
                        // start loking for the next file
                        ResetState();
                    } else if (message.StartsWith(revisionPrefix)) {
                        revision = message.Substring(revisionPrefix.Length);
                        logState = LogState.WANT_DATE;
                    }
                    break;
        
                case LogState.WANT_DATE:
                    // date line is of form 'date: yyyy/mm/dd hh:mm:ss; author: <author>;  <other stuff>
                    if (message.StartsWith(datePrefix)) {
                        ExtractDateAndAuthor(message);
                        logState = LogState.WANT_COMMENT;
                    }
                    break;
                    
                case LogState.WANT_COMMENT:
                    if (message.StartsWith(fileEndPrefix)) {
                        AddNewEntry();
                        ResetState();    // logState = WANT_FILE
                    } else if (message.StartsWith(revisionEndPrefix)) {
                        logState = LogState.WANT_PREV_REVISION;
                    } else {
                        // append comment line to the comment
                        if (comment.Length > 0) {
                            comment += Environment.NewLine;
                        }
                        comment += message;
                    }
                    break;
                    
                case LogState.WANT_PREV_REVISION:
                    // revision line is of form 'revision: <revision>'
                    if (message.StartsWith(revisionPrefix)) {
                        prevRevision = message.Substring(revisionPrefix.Length);
                        AddNewEntry();
                        revision = prevRevision;
                        prevRevision = "";
                        date = "";
                        author = "";
                        comment = "";
                        logState = LogState.WANT_DATE;
                    }
                    break;
                }
            }
        }
        
        // TODO: make this function more resiliant to minor changes in the input
        private void ExtractDateAndAuthor(string message)
        {
            // date line is of form 'date: yyyy/mm/dd hh:mm:ss; author: <author>;  <other stuff>
            const string datePrefix = "date: ";       
            const string dateFormat = "yyyy/mm/dd hh:mm:ss";  
            const string authorPrefix = ";  author: ";  
            int authorIndex = datePrefix.Length + dateFormat.Length + authorPrefix.Length;
            
            date = message.Substring(datePrefix.Length, dateFormat.Length);
            author = message.Substring(authorIndex, message.IndexOf(';', authorIndex) - authorIndex);
        }
        
        private void ResetState()
        {
            logState = LogState.WANT_FILE;
            date = "";
            author = "";
            comment = "";
            filename = "";
            revision = "";
            prevRevision = "";
        }
        
        // Add the current values of file, revision, date & comment 
        private void AddNewEntry()
        {
            // first do any necessary tidying up
    
                         
            // TODO: there is some duplication of effort in the following code
            
            LogEntry entry = new LogEntry(date, author, comment);
            // determine if this entry already exists
            if (entries.ContainsKey(entry.Key)) {
                // need to update an existing entry
                entry = (LogEntry)entries[entry.Key];
            } else {
                // add new entry
                entries.Add(entry.Key, entry);
            }
            // finally add details about the file/revision
            entry.AddFileRevision(filename, revision, prevRevision);
        }
    }
}

