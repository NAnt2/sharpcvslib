using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Globalization;
using EnvDTE;
using log4net;

namespace SharpCvsAddIn
{

	public class FileStatusCollection : NameObjectCollectionBase 
	{
		public FileStatusCollection()
		{
		}

		public FileStatusCollection( FileStatus[] ar )
		{
			this.Add( ar );
		}

		public void Add( FileStatus fs )
		{
			BaseAdd( fs.UniqueName, fs );
		}

		public void Add( FileStatus[] ar )
		{
			foreach(FileStatus st in ar )
			{
				BaseAdd( st.UniqueName, st );
			}
		}

		public bool FileStatusExists( string fileName )
		{
			return ( BaseGet( fileName ) != null );
		}

	}

	public class ProjectCollection : NameObjectCollectionBase
	{

		public void Add(string projectName, FileStatusCollection coll )
		{
			BaseAdd( projectName, coll );
		}

		public FileStatusCollection Get( string projectName )
		{
			return (FileStatusCollection)BaseGet(projectName);
		}


	}

	public class ProjectFileInfo
	{
		public string fileName_;
		public string directoryName_;
		public ProjectFileInfo( string file, string dir )
		{
			fileName_ = file;
			directoryName_ = dir;
		}
	}

	public class FileStatus
	{
		private static readonly ILog log_ = LogManager.GetLogger(typeof(FileStatus));
		private CvsStatusType	status_ = CvsStatusType.Unknown;
		private string		fileName_ = string.Empty;
		private string		version_ = string.Empty;
		private string		directory_ = string.Empty;

		private void ParseStatus( string statusInfo )
		{
			status_ = CvsStatusType.UpToDate;
			// check for conflict marker
			string[] timestamps = statusInfo.Split('+');
			// get modification date from file, this may seem like a weird way to do this but
			// we are following the method suggested in the Cederqvist book pg 16
			// format should be local C ISO asctime
			DateTime modTime = File.GetLastWriteTime( Path.Combine( directory_, fileName_ )).ToUniversalTime();
			string stringModTime = modTime.ToString("ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture );
			// check for conflict, if conflict marker exists and is same as modification time then there
			// is an unresolved conflict
			if( timestamps.Length > 1 )
			{
				if( timestamps[1] == stringModTime )
				{
					status_ = CvsStatusType.ConflictsOnMerge;
				}
				else
				{
					status_ = CvsStatusType.LocallyModified;
				}

				return;
			}
			// check for local modifications
			if( stringModTime != timestamps[0] )
			{
				status_ = CvsStatusType.LocallyModified;
			}


		}

		public FileStatus( ProjectFileInfo fileInfo )
		{
			fileName_ = fileInfo.fileName_;
			directory_ = fileInfo.directoryName_;
			status_ = CvsStatusType.Unknown;
		}

		public FileStatus()
		{
		}

		/// <summary>
		/// c'tor
		/// </summary>
		/// <param name="directory">Directory where the file named in entry lives</param>
		/// <param name="entry">A line from CVS/Entries file</param>
		public FileStatus(string directory, string entry )
		{
			directory_ = directory;
			log_.Debug(string.Format("Processing entry {0}", entry));
			string[] entryParts = entry.Split('/');
			fileName_ = entryParts[1];
			version_ = entryParts[2];

			log_.Debug(string.Format("Entry file name - {0} ver {1}", fileName_, version_));

			switch( entryParts[2][0])
			{
				case '0' :	// file added needs commit
					status_ = CvsStatusType.LocallyAdded;
					break;
				case '-' :	// file removed but not yet commited
					status_ = CvsStatusType.LocallyRemoved;
					// remove dash from front of version
					version_ = version_.Substring( 1 );
					break;
				default :
					ParseStatus( entryParts[3] );
					break;
			}
		}



		public string FileName
		{
			get{ return fileName_; }
		}

		public string UniqueName
		{
			get{ return Path.Combine( directory_, fileName_ ) ; }
		}

		public CvsStatusType CvsStatus
		{
			get{ return status_; }
		}
	}

	public class FileStatusCache
	{
		private static readonly ILog log_ = LogManager.GetLogger(typeof(FileStatusCache));

		private ProjectCollection projects_ = new ProjectCollection();
		private Solution solution_ = null;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">A solution or project path</param>
		/// <returns>An array of FileStatus objects that reflect the state of files in Entries file</returns>
		private FileStatus[] ExtractCvsEntries( string path )
		{
			FileStatus[] fs = null;
			string cvsPath = Path.Combine( Path.Combine(path, "CVS"), "Entries" );
			log_.Debug( string.Format("Extracting entries from {0}",cvsPath));
			ArrayList entries = new ArrayList();

			using( StreamReader rdr = new StreamReader(cvsPath))
			{
				Regex rgx = new Regex( "^/" );

				string line = string.Empty;
				while( ( line = rdr.ReadLine()) != null )
				{
					if( rgx.IsMatch(line) )
					{
						entries.Add( new FileStatus( path, line ) );
					}

				}
			}

			fs = new FileStatus[entries.Count];
			int i = 0;
			foreach( object o in entries )
			{
				fs[i++] = (FileStatus)o;
			}

			return fs;

		}


		/***********************************************************************
		 * Goes through add caches cvs status for everything in a project
		 * Args:	path - path of the project
		 *			items - collection of project items
		 * **********************************************************************/
		private void GetProjectEntries( string path, EnvDTE.ProjectItems items )
		{
			FileStatus[] fs = ExtractCvsEntries( path );			
			
			FileStatusCollection fsc = projects_.Get( items.ContainingProject.UniqueName );			
			if( fsc == null )
			{
				fsc = new FileStatusCollection();
				projects_.Add( items.ContainingProject.UniqueName, fsc );
			}

			fsc.Add( fs );

			foreach( EnvDTE.ProjectItem item in items )
			{
				// TODO: use a vsProjectItemKind constant to handle this
				if( item.ProjectItems.Count > 0 )
				{
					string subdir = Path.Combine( path, item.Name );
					FileAttributes attr = File.GetAttributes( subdir );
					if((int)attr != -1 && attr == FileAttributes.Directory )
					{
						GetProjectEntries( subdir, item.ProjectItems );
						// we just processed a subdirectory off of a project
						// so we don't really need to track status for it
						continue;
					}
				}

				// this code exists to determine if a file is in the project, but not in CVS
				// if this is true, we'll add the file with status of unknown
				if(!fsc.FileStatusExists( Path.Combine( path, item.Name ) ) )
				{
					fsc.Add( new FileStatus(path, item.Name));
				}				
			}
		}

		public FileStatusCache( _DTE app )
		{
			solution_ = app.Solution;

			foreach( EnvDTE.Project prj in solution_.Projects )
			{
				if( prj.Kind == EnvDTE.Constants.vsProjectKindUnmodeled )
				{
					log_.Info(string.Format( "Project {0} is unmodeled so it wont be handled be addin", prj.Name ));
					continue;
				}

				string path = string.Empty;

				if( prj.Kind == Constants.vsProjectKindMisc ||
					prj.Kind == Constants.vsProjectKindSolutionItems )
				{
					path = Path.GetDirectoryName( solution_.FileName );
				}
				else
				{
					path = Path.GetDirectoryName( prj.FileName );
				}
				
				GetProjectEntries( path, prj.ProjectItems );					
			}
		}

	}
}