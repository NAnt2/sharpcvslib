using System;
using System.Collections;
using System.Diagnostics;

namespace SharpCvsAddIn.Utilities
{
	public interface ITreeNode
	{
		string Name
		{
			get;
		}
		ITreeNode[] Children
		{
			get;
		}

		bool HasChildren
		{
			get;
		}

		ITreeNode Parent
		{
			get;
		}

		ITreeNode PrevSibling
		{
			get;
		}

		ITreeNode NextSibling
		{
			get;
		}

		void AddChild( ITreeNode n );

		ITreeNode Get( string path );





	}

	public abstract class TreeNode : ITreeNode
	{
		ITreeNode parent_ = null;
		ITreeNode prev_ = null;
		ITreeNode next_ = null;
		ITreeNode child_ = null;
		string name_ = string.Empty;

		protected TreeNode( string name )
		{
			name_ = name;
		}

		#region ITreeNode Members

		public string Name
		{
			get{ return name_; }
		}

		public ITreeNode[] Children
		{
			get
			{
				ArrayList result = new ArrayList();
				ITreeNode node = child_;
				while( node != null )
				{
					result.Add( node );
					node = node.NextSibling;
				}

				ITreeNode[] nodes = new TreeNode[result.Count];
				result.CopyTo( nodes );				
				return nodes;
			}
		}

		/// <summary>
		/// Fast way to find out if a node has children
		/// </summary>
		public bool HasChildren
		{
			get
			{
				return (child_ != null );
			}
		}

		public ITreeNode Parent
		{
			get
			{
				return parent_;
			}
		}

		public ITreeNode PrevSibling
		{
			get
			{
				return prev_;
			}
		}

		public ITreeNode NextSibling
		{
			get
			{
				return next_;
			}
		}


		public void AddChild(ITreeNode n)
		{
			if( child_ == null )
			{
				child_ = n;
				((TreeNode)n).parent_ = this;
			}
			else
			{
				ITreeNode node = child_;
				Debug.Assert( node.Name != n.Name );

				while( node.NextSibling != null )
				{
					node = node.NextSibling;
					Debug.Assert( node.Name != n.Name );
				}


				((TreeNode)node).next_ = n;
				((TreeNode)n).parent_ = this;
				((TreeNode)n).prev_ = node;				
			}			
		}

		/// <summary>
		/// Gets a tree node 
		/// </summary>
		/// <param name="path">a backslash delimited path to the node child/child's child/child's child's child </param>
		/// <returns></returns>
		public ITreeNode Get(string path)
		{
			int pos = path.IndexOf( '\\' );
			string token = pos != -1 ? path.Substring( 0, pos ) : path;

            ITreeNode[] children = this.Children;
			foreach( ITreeNode child in children )
			{
				// do a case insensitive compare
				if( string.Compare( child.Name, token, true) == 0 )
				{
					// if we are at end of path we found the item
					// so return it, otherwise remove head of path
					// and use recursion to traverse the path
					// until we find what we want
					if( pos == -1 )
					{
						return child;
					}

					string newpath = path.Substring( ++pos );
					return child.Get( newpath );
				}
			}
			
					
			return null;
		}

		#endregion
	}

}