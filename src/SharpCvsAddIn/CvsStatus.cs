using System;

namespace SharpCvsAddIn
{
	public enum CvsStatusType
	{
		/// <summary>
		/// The file is identical with the latest revision in the repository for the branch in use. 
		/// </summary>
		UpToDate = 0,
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
		/// <summary>
		/// This status is reserved for things that we don't want an icon associated with
		/// </summary>
		NullStatus,
	}
	
	/// <summary>
	/// Maps enum to our bitmap
	/// </summary>
	public class CvsStatus
	{
		private static readonly int[] map_ = 
		{
			1, /* up to date */
			9, /* locally modified */
			2, /* locally added */
			3, /* locally removed */
			10, /* needs checkout */
			10, /* needs patch */
			7, /* needs merge */
			6, /* conflicts on merge */
			8, /* unknown */
			0, /* null status */
		};

		private static readonly int[] rank_map_ =
			{
				10, /* up to date */
				30, /* locally modified */
				70, /* locally added */
				80, /* locally removed */
				20, /* needs checkout */
				40, /* needs patch */
				50, /* needs merge */
				60, /* conflicts on merge */
				0, /* unknown */
				100, /* null status */
			};

		public static int Get(CvsStatusType statustype)
		{
			return map_[(int)statustype];

		}

		/// <summary>
		/// Used to decide which status to use
		/// </summary>
		/// <param name="oldtype"></param>
		/// <param name="newtype"></param>
		/// <returns>The highest ranked status</returns>
 
		public static CvsStatusType GetPrecedentStatus( CvsStatusType oldtype, CvsStatusType newtype )
		{
			return rank_map_[(int)newtype] > rank_map_[(int)oldtype] ? newtype : oldtype;
		}



	}
}