/*******************************************************************
 * $Author: murphy_bytes $
 * $Date: 2005/02/07 04:54:09 $
 * $Revision: 1.1 $
 * 
 * This file contains classes that store status of solution files
 * *******************************************************************/

using System;
using EnvDTE;
using log4net;

namespace SharpCvsAddIn
{
	public enum CvsStatus
	{
		/// <summary>
		/// The file is identical with the latest revision in the repository for the branch in use. 
		/// </summary>
		UpToDate,
		/// <summary>
		/// You have edited the file, and not yet committed your changes. 
		/// </summary>
		LocallyModified,
		/// <summary>
		///	You have added the file with add, and not yet committed your changes. 
		/// </summary>
		LocallyAdded,
		/// <summary>
		///	You have removed the file with remove, and not yet committed your changes. 
		/// </summary>
		LocallyRemoved,
		/// <summary>
		/// Someone else has committed a newer revision to the repository. The name is slightly misleading; you will ordinarily use update rather than checkout to get that newer revision. 
		/// </summary>
		NeedsCheckout,
		/// <summary>
		/// Like Needs Checkout, but the cvsnt server will send a patch rather than the entire file. Sending a patch or sending an entire file accomplishes the same thing. 
		/// </summary>
		NeedsPatch,
		/// <summary>
		/// Someone else has committed a newer revision to the repository, and you have also made modifications to the file. 
		/// </summary>
		NeedsMerge,	
		/// <summary>
		/// This is like Locally Modified, except that a previous update command gave a conflict. If you have not already done so, you need to resolve the conflict as described in the section called “Conflicts example ”. 
		/// </summary>
		ConflictsOnMerge,
		/// <summary>
		/// cvsnt doesn't know anything about this file. For example, you have created a new file and have not run add. 
		/// </summary>
		Unknown,
	}

	public class FileStatus
	{
		CvsStatus	status_ = CvsStatus.Unknown;
		string		fileName_ = string.Empty;

		public FileStatus()
		{
		}

		public CvsStatus CvsStatus
		{
			get{ return status_; }
		}
	}

	public class FileStatusCache
	{

		private string[] ExtractEntries( string path )
		{
			return null;
		}

		public FileStatusCache( _DTE app )
		{
			Solution sln = app.Solution;


		}

	}
}