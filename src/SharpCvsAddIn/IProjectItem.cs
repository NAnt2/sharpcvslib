/********************************************************************************************
 *	Contains interface to project items
 * 	
 * ******************************************************************************************/
using System;

namespace SharpCvsAddIn
{
	/// <summary>
	/// Abstraction of various items that can be part of a project, files, resouces, et. al.
	/// </summary>
	public interface IProjectItem
	{
		bool IsModified { get; }
		bool IsVersioned { get; }
		bool IsDirectory { get; }
		bool IsFile { get; }

	}
}
