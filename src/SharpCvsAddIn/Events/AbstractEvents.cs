/*************************************************
 * Provides common event handling functionality
 * ***********************************************/
using System;
using EnvDTE;

namespace SharpCvsAddIn.Events
{
	public abstract class AbstractEvents
	{
		protected Controller controller_ = null;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="controller"></param>
		protected AbstractEvents( Controller controller )
		{
			controller_ = controller;
		}
		/// <summary>
		/// Override to add event hooks
		/// </summary>
		public abstract void AddHandlers();
		/// <summary>
		/// Override to remove event hooks
		/// </summary>
		public abstract void RemoveHandlers();
	}
}