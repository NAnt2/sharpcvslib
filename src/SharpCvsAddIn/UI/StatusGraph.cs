using System;
using EnvDTE;
using VSLangProj;
using SharpCvsAddIn.Utilities;
using log4net;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SharpCvsAddIn.UI
{
	public interface IStatusNode : ITreeNode
	{
		void InitializeNode(IStatusNode parent);
		CvsStatusType Status { get; set; }

		void OnChangeEvent();

		
	}


	public class StatusGraph
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(StatusGraph));

		public class CvsEntry
		{
			private string fileName_ = string.Empty;
			private string version_ = string.Empty;
			private CvsStatusType status_ = CvsStatusType.NullStatus;
			private string directory_ = string.Empty;

			public string FileName
			{
				get{ return fileName_; }
			}

			public string DirectoryName
			{
				get{ return directory_; }
			}

			public CvsStatusType Status
			{
				get{ return status_; }
			}

			public string Version
			{
				get{ return version_ ; }
			}

			public bool IsDirectory
			{
				get { return fileName_ == string.Empty ; }
			}

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

			/// <summary>
			/// c'tor
			/// </summary>
			/// <param name="directory">Directory where the file named in entry lives</param>
			/// <param name="entry">A line from CVS/Entries file</param>
			public CvsEntry(string directory, string entry )
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
		}

		public abstract class StatusNode : TreeNode, IStatusNode	
		{
			protected IntPtr hwnd_ = IntPtr.Zero;
			protected IController controller_;
			protected UIHierarchyItem item_;
			protected TreeView treeview_;
			protected CvsStatusType status_ = CvsStatusType.Unknown;
			// use this to keep track of of modified children
			protected uint modifications_ = 0;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="con"></param>
			/// <param name="it"></param>
			/// <param name="itemName">Item name will be the file name or directory name of the
			/// item</param>
			protected StatusNode( IController con, UIHierarchyItem it, string itemName )
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
				StatusNode pred = null;
				// exception for VSFileProjectItem since it does not have a hwnd of it's own 
				if( this.PrevSibling != null && !typeof(VSFileProjectItem).IsInstanceOfType(this.PrevSibling) )
				{
					pred = (StatusNode)this.PrevSibling;
					hwnd_ = pred.TreeView.GetNextSibling( pred.Hwnd );
					
				}
				else
				{
					pred = (StatusNode)this.Parent;
					hwnd_ = pred.TreeView.GetChild(pred.Hwnd);
				}

				Debug.Assert( hwnd_ != IntPtr.Zero );

				return hwnd_;
			}
			#region IStatusNode Members

			public void InitializeNode(IStatusNode parent)
			{
				Debug.Assert( false, "this should never happen" );
			}

			virtual public SharpCvsAddIn.CvsStatusType Status
			{
				get
				{
					return status_;
				}
				set
				{
					
					status_ = value; 

					// update user interface with new status
					Debug.Assert( hwnd_ != IntPtr.Zero, "hwnd not assigned" );
					this.TreeView.SetStatusImage(hwnd_, CvsStatus.Get( status_ ) );					
				}
			}

			// this doesn't do anything for the most part
			public virtual void OnChangeEvent()
			{
			}


			/// <summary>
			/// If something is added, removed, or changed, it's dirty
			/// </summary>
			public bool Dirty
			{
				get
				{
					return !(status_ == CvsStatusType.NullStatus || 
							status_ == CvsStatusType.Unknown ||
							status_ == CvsStatusType.UpToDate );
				}
			}


			protected virtual string ItemPath
			{
				get
				{
					// this should never happen since solution node is the only one without a parent
					// and it overrides ItemPath
					Debug.Assert( this.Parent != null );
					// ain't recursion great!
					return Path.Combine( ((StatusNode)this.Parent).ItemPath, this.Name );
				}
			}

			private string CVSEntriesFile
			{
				get
				{
					string result = Path.Combine( Path.Combine( this.ItemPath, "CVS" ), "Entries" );
					log_.Debug(string.Format("GetCVSPath returns {0}", result ));
					return result;
				}
			}

			protected virtual CvsEntry[] GetCvsEntries()
			{
				Debug.Assert( this.HasChildren, "Called GetCvsEntries on a terminal node" );
				CvsEntry[] entries = null;
				log_.Debug( string.Format("Extracting entries from {0}",this.CVSEntriesFile));
				ArrayList entryList = new ArrayList();

				using( StreamReader rdr = new StreamReader(this.CVSEntriesFile))
				{
					Regex rgx = new Regex( "^/" );

					string line = string.Empty;
					while( ( line = rdr.ReadLine()) != null )
					{
						if( rgx.IsMatch(line) )
						{
							entryList.Add( new CvsEntry( this.ItemPath, line ) );
						}

					}
				}
				entries = new CvsEntry[ entryList.Count ];
				entryList.CopyTo( entries );
				return entries;


			}
			/// <summary>
			/// Synchronizes local information that cvs stores with the project
			/// items in the solution explorer
			/// </summary>
			public virtual void SynchWithCvsEntries( )
			{
				ITreeNode[] children = this.Children;

				if( children.Length > 0 )
				{
					// process children that have children or their own first
					foreach( ITreeNode node in children )
					{
						if(node.HasChildren )
						{
							StatusNode sn = (StatusNode)node;
							sn.SynchWithCvsEntries();
						}
					}

					// now get cvs entries for terminal nodes that are 
					// children of current node
					CvsEntry[] entries = this.GetCvsEntries();

					foreach( CvsEntry entry in entries )
					{
						StatusNode child = (StatusNode)this.Get( entry.FileName );

						if( child != null )
						{
							log_.Debug( string.Format("Synching terminal {0} with cvs entry", child.Name ));
							// we are processing a terminal node. First we need to see
							// if the status of the node is changed. If it is, we need 
							// to update the modification counts in parent nodes so that
							// they can continue to reflect the status of their children.
							// If the cvs entry status is the same as the child nodes status
							// we do nothing, saving a few cycles
							if( child.Status != entry.Status )
							{
								bool wasDirty = child.Dirty;
								child.Status = entry.Status;

								if( wasDirty )
								{
									// check to see if we're up to date, if we are
									// decrement the modified count in parent nodes
									if( !child.Dirty )
									{
										this.DecrementModified();
									}
								}
								else
								{
									if( child.Dirty )
									{
										this.IncrementModified();
									}
								}
							}
						}
					}		// end for

					// synch this item with it's childrem
					if( entries.Length > 0 )
					{
						if( this.modifications_ > 0 )
						{
							this.Status = CvsStatusType.LocallyModified;
						}
						else
						{
							this.Status = CvsStatusType.UpToDate;
						}
					}



				}				
				
			}


			public void IncrementModified()
			{
				if( this.HasChildren && modifications_++ == 0)
				{
					this.Status = CvsStatusType.LocallyModified;
				}

				if( this.Parent != null )
				{
					((StatusNode)this.Parent).IncrementModified();
				}
			}

			public void DecrementModified()
			{
				Debug.Assert( modifications_ > 0 );
				if( this.HasChildren && --modifications_ == 0 )
				{
					this.Status = CvsStatusType.UpToDate;
				}

				StatusNode parent = (StatusNode)this.Parent;
				if(parent != null)
				{
					parent.DecrementModified();
				}
			}

			#endregion


		}
		/// <summary>
		/// A solutions item project is a virtual directory that doesn't actually live on the file system that
		/// contains a group of solution level files
		/// </summary>
		private class SolutionItemsProject : StatusNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger( typeof(SolutionItemsProject));
			internal SolutionItemsProject( IController cont, UIHierarchyItem item )
				:base(cont, item, item.Name)
			{
				log_.Debug("created solution items project" );
			}

			new public void InitializeNode( IStatusNode parent )
			{
				parent.AddChild(this);
				log_.Debug("added solution items project");

				IntPtr hwnd = InitializeHwnd();
				//this.TreeView.SetStatusImage(hwnd, 1);
				this.Status = CvsStatusType.NullStatus;

				if( item_.UIHierarchyItems.Count > 0 )
				{
					this.Status = CvsStatusType.Unknown;

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

			new public void SynchWithCvsEntries()
			{
				log_.Debug("calling synch");
			}

		}
		/// <summary>
		/// An abstraction of a solution item, these objects
		/// store status of the object and a handle to the items
		/// representation in the tree view. This is so we can easily
		/// update user interface to reflect changes in the underlying
		/// items controlled by CVS
		/// </summary>
		private class DotNetProject : StatusNode, IStatusNode
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


			public new void InitializeNode(IStatusNode parent )
			{	
				parent.AddChild( this );
				log_.Debug(string.Format("Added {0}", item_.Name ));

				IntPtr hwnd = InitializeHwnd();
				//this.TreeView.SetStatusImage(hwnd, 1);
				this.Status = CvsStatusType.Unknown;


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
		private class ReferenceProjectItem : StatusNode , IStatusNode 
		{
			internal ReferenceProjectItem( IController cont, UIHierarchyItem it )
				:base(cont, it, it.Name )
			{}

			public new void InitializeNode( IStatusNode parent)
			{
				parent.AddChild( this );
				IntPtr hwnd = InitializeHwnd();
				//this.TreeView.SetStatusImage(hwnd, 0);
				this.Status = CvsStatusType.NullStatus;
			}
			
			// we don't want base functionality here because 
			// this doesn't correspond to any physical item on disk
			public new void SynchWithCvsEntries()
			{
				log_.Debug("calling synch with cvs entries in reference project item" );
			}
		}
		/// <summary>
		/// This represents a physical file in the project
		/// </summary>
		private class PhysicalFileProjectItem : StatusNode, IStatusNode
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
			public new void InitializeNode(IStatusNode parent)
			{
				parent.AddChild( this );
				log_.Debug(string.Format("Added {0}", item_.Name ));
				IntPtr hwnd = InitializeHwnd();
				//this.TreeView.SetStatusImage(hwnd, 1);				
				this.Status = CvsStatusType.Unknown;
			}

			public new void OnChangeEvent()
			{
				switch( this.Status )
				{
					case CvsStatusType.UpToDate :
					case CvsStatusType.ConflictsOnMerge :
						this.Status = CvsStatusType.LocallyModified ;
						this.IncrementModified();
						break;
					case CvsStatusType.NeedsPatch :
					case CvsStatusType.NeedsCheckout :
						this.Status = CvsStatusType.NeedsMerge;
						this.IncrementModified();
						break;
				}
				
			}

			#endregion			

		}

		/// <summary>
		/// This class represents project files and solution files, they don't have a visual representation but
		/// instead are represented by the project or solution 'node' that is their parent
		/// </summary>
		private class VSFileProjectItem : StatusNode, IStatusNode
		{
			internal VSFileProjectItem( IController cont, string fileName )
				:base(cont,null, fileName)
			{
			}

			#region IStatusNode Members

			public new void InitializeNode(IStatusNode parent)
			{
				// this gets its parents windows handle since it doesn't have one of it's own (it's
				// not visible). It may seem a little wierd to do it this way but it will allow us to preserve
				// the order of the visual elements and keep them synched up with this internal
				// representation of the solution explorer
				hwnd_ = ((StatusNode)parent).Hwnd;
				parent.AddChild( this );				
			}

			public new void OnChangeEvent()
			{
				switch( this.Status )
				{
					case CvsStatusType.UpToDate :
					case CvsStatusType.ConflictsOnMerge :
						this.Status = CvsStatusType.LocallyModified ;
						this.IncrementModified();
						break;
					case CvsStatusType.NeedsPatch :
					case CvsStatusType.NeedsCheckout :
						this.Status = CvsStatusType.NeedsMerge;
						this.IncrementModified();
						break;
				}
				
			}


			#endregion

			

		}


		/// <summary>
		/// this represents a physical folder in the project that we have to traverse to get more project items
		/// </summary>
		private class PhysicalDirectoryProjectItem : StatusNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger(typeof(PhysicalDirectoryProjectItem));

			internal PhysicalDirectoryProjectItem(IController cont, UIHierarchyItem it )
				:base(cont,it, it.Name)
			{
				log_.Debug(string.Format("Creating new {0} named {1}", this.GetType().Name, it.Name));
			}
			#region IStatusNode Members

			public new void InitializeNode(IStatusNode parent)
			{
				parent.AddChild(this);
				log_.Debug(string.Format("Added {0}", item_.Name ));
				IntPtr hwnd = InitializeHwnd();
				this.Status = CvsStatusType.Unknown;
				//this.TreeView.SetStatusImage(hwnd, 1);

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
		public class SolutionNode : StatusNode, IStatusNode
		{
			private static readonly ILog log_ = LogManager.GetLogger( typeof(SolutionNode) );


			internal SolutionNode( IController controller, UIHierarchyItem item )
				:base( controller, item, item.Name )
			{
				log_.Debug( string.Format("Create SolutionNode {0}", item_.Name ));
			}

			override public SharpCvsAddIn.CvsStatusType Status
			{
				get
				{
					return status_;
				}
				set
				{
					// decide which status to use
					status_ = CvsStatus.GetPrecedentStatus( status_, value );
					Debug.Assert( hwnd_ != IntPtr.Zero, "hwnd is not assigned" );
					// update user interface with new status
					controller_.SolutionExplorer.TreeView.SetStatusImage(hwnd_, CvsStatus.Get(status_));
				}
			}

			/// <summary>
			/// No special luvvin needed to get path for solution since we already have it
			/// </summary>
			protected override string ItemPath
			{
				get
				{
					return Path.GetDirectoryName(this.controller_.DTE.Solution.FileName);
				}
			}

			public new void InitializeNode(IStatusNode node )
			{
				hwnd_ = controller_.SolutionExplorer.TreeView.GetRoot();
				this.Status = CvsStatusType.Unknown;

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


		/// <summary>
		/// GetRoot fetches a data structure that represents to contents of the solution. Updates
		/// to the root are reflected in the solution explorer
		/// </summary>
		/// <param name="controller">The object that controls everything, that's why it's called controller</param>
		/// <returns>SolutionNode</returns>
		public static StatusGraph.SolutionNode GetRoot( IController controller )
		{
			UIHierarchy item = 
				(UIHierarchy)controller.DTE.Windows.Item( Constants.vsWindowKindSolutionExplorer ).Object;

			SolutionNode root =  new SolutionNode(controller, item.UIHierarchyItems.Item(1) );
			root.InitializeNode(null);
			root.SynchWithCvsEntries();
			return root;

		}
	}

}