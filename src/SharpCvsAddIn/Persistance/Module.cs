using System;
using System.Xml.Serialization;
using System.Collections;
using System.Diagnostics;

namespace SharpCvsAddIn.Persistance
{
	[Serializable]
	public class Module : IComparable
	{
		private string[] tags_ = new string[0];
		private string name_ = string.Empty;

		public Module()
		{
		}

		public Module( string name )
		{
			name_ = name;
		}

		public Module( string name, string[] tags )
		{
			tags_ = tags;
			name_ = name;
		}

		public string Name
		{
			get{ return name_; }
			set{ name_ = value; }
		}

		public string[] Tags
		{
			get{ return tags_; }
			set{ tags_ = value; }
		}

		public bool IsTagPresent( string test )
		{
			foreach( string tag in tags_ )
			{
				if( tag == test )
				{
					return true;
				}
			}

			return false;
		}

		public void AddTag( string newTag )
		{
			// don't add duplicates
			Debug.Assert( !IsTagPresent( newTag ) );
			ArrayList list = new ArrayList( this.tags_ );
			list.Add( newTag );
			list.Sort();
			tags_ = new string[ list.Count ];
			list.CopyTo( tags_ );
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return this.Name.CompareTo( ((Module)obj).Name );
		}

		#endregion
	}
}


