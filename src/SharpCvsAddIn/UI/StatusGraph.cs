using System;
using EnvDTE;
using VSLangProj;
using SharpCvsAddIn.Utilities;
using log4net;
using System.Diagnostics;
using System.IO;

namespace SharpCvsAddIn.UI
{
	public interface IStatusNode : ITreeNode
	{
		void InitializeNode(IStatusNode parent);
	}


	public class StatusGraph
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(StatusGraph));

		private abstract class BaseNode : TreeNode
		{
			protected IntPtr hwnd_ = IntPtr.Zero;
			protected IController controller_;
			protected UIHierarchyItem item_;
			protected TreeView treeview_;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="con"></param>
			/// <param name="it"></param>
			/// <param name="itemName">Item name will be the file name or directory name of the
			/// item</param>
			protected BaseNode( IController con, UIHierarchyItem it, string itemName )
				:base(itemName)
			{
				controller_ = con;
				item_ = it;
				treeview_ = controller_.SolutionExplorer.TreeView;
			}

			internal IntPtr Hwnd
			{
				get
				{

					return hwnd_;
				}
			}

			internal TreeView TreeView
			{
				get{ return treeview_; }
			}


			protected IntPtr InitializeHwnd( )
			{
				BaseNode pred = null;
				// exception for VSFileProjectItem since it does not have a hwnd of it's own 
				if( this.PrevSibling != null && !typeof(VSFileProjectItem).IsInstanceOfType(this.PrevSibling) )
				{
					pred = (BaseNode)this.PrevSibling;
					hwnd_ = pred.TreeView.GetNextSibling( pred.Hwnd );
					
				}
				else
				{
					pred = (BaseNode)this.Parent;
					hwnd_ = pred.TreeView.GetChild(pred.Hwnd);
				}

				Debug.Assert( hwnd_ != IntPtr.Zero );

				return hwnd_;
			}
		
		}
		/// <summary>
		/// A solutions item project is a virtual directory that doesn't actually live on the file system that
		/// contains a group of solution level files
		/// </summary>
		private class SolutionItemsProject : BaseNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger( typeof(SolutionItemsProject));
			internal SolutionItemsProject( IController cont, UIHierarchyItem item )
				:base(cont, item, item.Name)
			{
				log_.Debug("created solution items project" );
			}

			public void InitializeNode( IStatusNode parent )
			{
				parent.AddChild(this);
				log_.Debug("added solution items project");

				IntPtr hwnd = InitializeHwnd();
				this.TreeView.SetStatusImage(hwnd, 1);

				if( item_.UIHierarchyItems.Count > 0 )
				{
					bool fOriginalState = item_.UIHierarchyItems.Expanded;
					item_.UIHierarchyItems.Expanded = true;
				
					foreach( UIHierarchyItem item in item_.UIHierarchyItems )
					{
						IStatusNode node = StatusGraph.CreateNode( controller_, item );
						node.InitializeNode( this );
					}

					item_.UIHierarchyItems.Expanded = fOriginalState;
				}

				
			}

		}
		/// <summary>
		/// An abstraction of a solution item, these objects
		/// store status of the object and a handle to the items
		/// representation in the tree view. This is so we can easily
		/// update user interface to reflect changes in the underlying
		/// items controlled by CVS
		/// </summary>
		private class DotNetProject : BaseNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger( typeof(DotNetProject) );
			private VSProject vsproj_ = null;
			private EnvDTE.Project project_ = null;

			// TODO - check to see if you can change the project name and make sure it still corresponds to directory name
			internal DotNetProject( IController controller, UIHierarchyItem item )
				:base(controller,item, item.Name)
			{
				log_.Debug( string.Format( "Creating csharp project {0}", item.Name ));
				project_ = (EnvDTE.Project)item_.Object;
                vsproj_ = (VSProject)project_.Object;

			}


			public void InitializeNode(IStatusNode parent )
			{	
				parent.AddChild( this );
				log_.Debug(string.Format("Added {0}", item_.Name ));

				IntPtr hwnd = InitializeHwnd();
				this.TreeView.SetStatusImage(hwnd, 1);

				// add a place holder for the project file
				IStatusNode projectFileNode = new VSFileProjectItem( controller_, Path.GetFileName(project_.FileName) );
				projectFileNode.InitializeNode(this);

				if( item_.UIHierarchyItems.Count > 1 )
				{
					bool fOriginalState = item_.UIHierarchyItems.Expanded;

					// handle this items children if it has any
					item_.UIHierarchyItems.Expanded = true;

					// the first child of a project in solution is for references
					// so we skip it
					bool isFirst = true;

					foreach( UIHierarchyItem childItem in item_.UIHierarchyItems )
					{				
						if( isFirst )
						{
							IStatusNode referenceNode = new ReferenceProjectItem( controller_, childItem );
							referenceNode.InitializeNode(this);
							isFirst = false;
							continue;
						}

						// handle first node
						IStatusNode node = StatusGraph.CreateNode( controller_, childItem );
						node.InitializeNode(this);
					}

					item_.UIHierarchyItems.Expanded = fOriginalState;
				}
			}
		}

		/// <summary>
		/// This is a placeholder for the references folder that is a child of projects, since the references 
		/// aren't under source control
		/// </summary>
		private class ReferenceProjectItem : BaseNode , IStatusNode 
		{
			internal ReferenceProjectItem( IController cont, UIHierarchyItem it )
				:base(cont, it, it.Name )
			{}

			public void InitializeNode( IStatusNode parent)
			{
				parent.AddChild( this );
				IntPtr hwnd = InitializeHwnd();
				this.TreeView.SetStatusImage(hwnd, 0);
			}
		}
		/// <summary>
		/// This represents a physical file in the project
		/// </summary>
		private class PhysicalFileProjectItem : BaseNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger(typeof(PhysicalFileProjectItem));
			private EnvDTE.ProjectItem projectItem_ = null;

			internal PhysicalFileProjectItem( IController cont, UIHierarchyItem it )
				:base(cont,it, it.Name)
			{
				log_.Debug(string.Format("creating new {0} named  {1}", this.GetType().Name, it.Name ));
				projectItem_ = (EnvDTE.ProjectItem)it.Object;

			}
			#region IStatusNode Members

			/// <summary>
			/// this is a terminal node
			/// </summary>
			/// <param name="parent"></param>
			public void InitializeNode(IStatusNode parent)
			{
				parent.AddChild( this );
				log_.Debug(string.Format("Added {0}", item_.Name ));
				IntPtr hwnd = InitializeHwnd();
				this.TreeView.SetStatusImage(hwnd, 1);				
			}

			#endregion			

		}

		/// <summary>
		/// This class represents project files and solution files, they don't have a visual representation but
		/// instead are represented by the project or solution 'node' that is their parent
		/// </summary>
		private class VSFileProjectItem : BaseNode, IStatusNode
		{
			internal VSFileProjectItem( IController cont, string fileName )
				:base(cont,null, fileName)
			{
			}

			#region IStatusNode Members

			public void InitializeNode(IStatusNode parent)
			{
				// this gets its parents windows handle since it doesn't have one of it's own (it's
				// not visible). It may seem a little wierd to do it this way but it will allow us to preserve
				// the order of the visual elements and keep them synched up with this internal
				// representation of the solution explorer
				hwnd_ = ((BaseNode)parent).Hwnd;
				parent.AddChild( this );				
			}

			#endregion

			

		}


		/// <summary>
		/// this represents a physical folder in the project that we have to traverse to get more project items
		/// </summary>
		private class PhysicalDirectoryProjectItem : BaseNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger(typeof(PhysicalDirectoryProjectItem));

			internal PhysicalDirectoryProjectItem(IController cont, UIHierarchyItem it )
				:base(cont,it, it.Name)
			{
				log_.Debug(string.Format("Creating new {0} named {1}", this.GetType().Name, it.Name));
			}
			#region IStatusNode Members

			public void InitializeNode(IStatusNode parent)
			{
				parent.AddChild(this);
				log_.Debug(string.Format("Added {0}", item_.Name ));
				IntPtr hwnd = InitializeHwnd();
				this.TreeView.SetStatusImage(hwnd, 1);

				if( item_.UIHierarchyItems.Count > 0 )
				{
					bool fOriginalState = item_.UIHierarchyItems.Expanded;
					item_.UIHierarchyItems.Expanded = true;
					foreach( UIHierarchyItem item in item_.UIHierarchyItems )
					{
						IStatusNode node = StatusGraph.CreateNode( controller_, item );
						node.InitializeNode( this );
					}

					item_.UIHierarchyItems.Expanded = fOriginalState;
				}

			}

			#endregion



		}


		/// <summary>
		/// Special node for solutions, this is the parent of all other nodes
		/// </summary>
		private class SolutionNode : BaseNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger( typeof(SolutionNode) );


			internal SolutionNode( IController controller, UIHierarchyItem item )
				:base( controller, item, item.Name )
			{
				log_.Debug( string.Format("Create SolutionNode {0}", item_.Name ));
			}

			public void InitializeNode(IStatusNode node )
			{
				hwnd_ = controller_.SolutionExplorer.TreeView.GetRoot();
				controller_.SolutionExplorer.TreeView.SetStatusImage( hwnd_, 1 );

				// create an entry for the solution file, it doesn't show up in the
				// solution explorer, but we need it as a placeholder for the file status
				IStatusNode fileNode = new VSFileProjectItem( controller_, Path.GetFileName(controller_.DTE.Solution.FileName) );
				fileNode.InitializeNode(this);

				if( item_.UIHierarchyItems.Count > 0 )
				{

					bool fOriginalState = item_.UIHierarchyItems.Expanded;

					// handle this items children if it has any
					item_.UIHierarchyItems.Expanded = true;

					foreach( UIHierarchyItem childItem in item_.UIHierarchyItems )
					{
						IStatusNode child = null;
						child = StatusGraph.CreateNode( controller_, childItem );
						child.InitializeNode(this);
					}

					item_.UIHierarchyItems.Expanded = fOriginalState;
				}
			}


		}


		private static IStatusNode CreateNode( IController cont, UIHierarchyItem child )
		{
			try
			{
				if( child.Object != null )
				{
					if( typeof(EnvDTE.Project).IsInstanceOfType( child.Object ) )
					{
						EnvDTE.Project prj = (EnvDTE.Project)child.Object;
                        log_.Debug("found project");

						switch( prj.Kind )
						{
							case EnvDTE.Constants.vsProjectKindMisc :
								Debug.Assert( false, "ProjectKindMisc still needs to be implemented" );
								log_.Debug(string.Format("misc project named {0}", prj.Name ));
								break;
							case EnvDTE.Constants.vsProjectKindUnmodeled :
								Debug.Assert(false, "ProjectKindUnmodeled still needs to be implemented" );
								log_.Debug(string.Format("Unmodeled project named {0}", prj.Name ));
								break;
							case EnvDTE.Constants.vsProjectKindSolutionItems :
								log_.Debug(string.Format("solution items project named {0}", prj.Name ));
								return new SolutionItemsProject( cont, child );
							case VSLangProj.PrjKind.prjKindVBProject :
							case VSLangProj.PrjKind.prjKindCSharpProject : 
							case VSLangProj.PrjKind.prjKindVSAProject :
								log_.Debug(string.Format("vb project {0}", prj.Name ));
								return new DotNetProject( cont, child );
							default :
								// TODO - WARN USER HERE
								Debug.Assert(false, "project type needs to be implemented");
								break;
							
						}
					}
					else if( typeof(EnvDTE.ProjectItem ).IsInstanceOfType( child.Object ) )
					{
						log_.Debug("found project item");
						EnvDTE.ProjectItem prjItem = (EnvDTE.ProjectItem)child.Object;

						switch( prjItem.Kind )
						{
							case EnvDTE.Constants.vsProjectItemKindMisc : // A miscellaneous files project item. 
								log_.Debug( "vsProjectItemKindMisc" );
								break;
							case EnvDTE.Constants.vsProjectItemKindPhysicalFile : //A file in the system. 
								return new PhysicalFileProjectItem( cont, child );
							case EnvDTE.Constants.vsProjectItemKindPhysicalFolder : // A folder in the system. 
								return new PhysicalDirectoryProjectItem( cont, child );
							case EnvDTE.Constants.vsProjectItemKindSolutionItems  : // An item in the solution items project. 
								log_.Debug( "ProjectItemKindSolutionItems" );
								break;
							case EnvDTE.Constants.vsProjectItemKindSubProject : //A sub-project under the project. If returned by ProjectItem.Kind, then ProjectItem.SubProject will return as a Project object. 
								log_.Debug("SubProject" );
								break;
							case EnvDTE.Constants.vsProjectItemKindVirtualFolder : // A virtual folder. In Solutio Explorer, a folder that does not physically exist on the system. 
								log_.Debug("Virtual Folder" );
								break;
							default :
								log_.Debug("Unknown project item");
								Debug.Assert(false, "unknown project item type");
								// TODO - WARN USER HERE
								break;
						}
					}
					else
					{
						log_.Debug("found something else" );
					}

				}

				return null;

			}
			catch(Exception e)
			{
				// TODO add a message box here this could probably happen with a project type
				//      we don't know about
				log_.Error( e.Message );
			}

			return null;
		}


		public static IStatusNode GetRoot( IController controller )
		{
			UIHierarchy item = 
				(UIHierarchy)controller.DTE.Windows.Item( Constants.vsWindowKindSolutionExplorer ).Object;

			IStatusNode root =  new SolutionNode(controller, item.UIHierarchyItems.Item(1) );
			root.InitializeNode(null);
			return root;

		}
	}

}