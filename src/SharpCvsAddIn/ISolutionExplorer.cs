/********************************************************************************************
 *
 * *******************************************************************************************/
using EnvDTE;
using System.Collections;
using SharpCvsAddIn.UI;

namespace SharpCvsAddIn
{
    public interface ISolutionExplorer //: ISelectionContainer
    {
		TreeView TreeView { get; }
        /// <summary>
        /// Refreshes all subnodes of a specific project.
        /// </summary>
        /// <param name="project"></param>
        //void Refresh( Project project );

		void Refresh();
		void Initialize();
		void Cleanup();

		IStatusNode Root { get; }

        /// <summary>
        /// Updates the status of the given item.
        /// </summary>
        /// <param name="item"></param>
       // void Refresh( ProjectItem item );

        /// <summary>
        /// Retrieves the resources associated with a project item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        //IList GetItemResources( ProjectItem item, bool recursive );

        /// <summary>	 	
        /// Visits all the selected nodes.	 	
        /// </summary>	 	
        /// <param name="visitor"></param>	 	
       // void VisitSelectedNodes( INodeVisitor visitor );

        /// <summary>
        /// Returns the selected ProjectItem
        /// </summary>
        /// <returns></returns>
        //ProjectItem GetSelectedProjectItem();

        /// <summary>
        /// Notify the Solution Explorer to unload.
        /// </summary>
        //void Unload();
    }
}
