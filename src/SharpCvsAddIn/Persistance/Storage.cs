using System;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections;

namespace SharpCvsAddIn.Persistance
{
	[Serializable]
	public class Storage
	{
		private Connection[] connections_ = new Connection[0];
		private Connection currentConnection_ = null;
		private Module currentModule_ = null;
		private string currentTag_ = string.Empty;
		// TODO - add user settings and other stuff to persist

		public Storage()
		{
		}
		
		public Connection[] Connections
		{
			get{ return connections_; }
			set{ connections_ = value; }
		}

		public Connection CurrentConnection
		{
			get{ return currentConnection_; }
			set
			{
				if( value != null )
				{
					Connection conn = IsConnectionPresent( value );
					if( conn != null )
					{
						conn.WorkingDirectory = value.WorkingDirectory;
						conn.Password = value.Password;
						currentConnection_ = conn;
					}
					else
					{
						currentConnection_ = value;
						AddConnection( value );
					}
				}
			}
		}

		public Module CurrentModule
		{
			get{ return currentModule_; }
			set
			{
				Module m = null;
				if( this.CurrentConnection != null && value != null )
				{
					m = this.CurrentConnection.IsModulePresent( value );
					if( m == null )
					{
						m = value;
						this.CurrentConnection.AddModule( m );
					}
				}

				currentModule_ = m;
			}
		}

		public string CurrentTag
		{
			get{ return currentTag_; }
			set
			{
				if( this.CurrentModule != null )
				{
					if( !this.CurrentModule.IsTagPresent( value ) )
					{
						this.CurrentModule.AddTag( value );
					}
				}

				currentTag_ = value;
			}
		}
		/// <summary>
		/// Test to see if connection is already in storage.
		/// </summary>
		/// <param name="test">Connection to test.</param>
		/// <returns>The connection in storage if it exists, else null</returns>
		private Connection IsConnectionPresent( Connection test )
		{
			foreach( Connection conn in connections_ )
			{
				if( conn.ConnectionString == test.ConnectionString )
				{
					return conn;
				}
			}

			return null;
		}
		/// <summary>
		/// Adds a connection to storage. You should test to see if connection is
		/// already present before adding it.
		/// </summary>
		/// <param name="newConnection"></param>
		private void AddConnection( Connection newConnection )
		{
			// did you test for duplicates before you added??
			Debug.Assert( this.IsConnectionPresent( newConnection ) == null );

			ArrayList list = new ArrayList( this.connections_ );
			list.Add( newConnection );
			list.Sort();
			connections_ = new Connection[list.Count];
			list.CopyTo(connections_);
		}

	}
}

