using System;
using System.Xml.Serialization;
using System.Collections;
using System.Text;
using log4net;
using System.Diagnostics;



namespace SharpCvsAddIn.Persistance
{

	[Serializable]
	public class Connection : IComparable
	{
		public const int PORT_UNASSIGNED = 0;
		public const int PORT_DEFAULT = 2401;

		private string workingDir_ = string.Empty;
		private Module[] modules_ = new Module[0];
		private string userName_ = string.Empty;
		private string hostName_ = string.Empty;
		private int port_ = Connection.PORT_UNASSIGNED;
		private string protocol_ = string.Empty;
		private string repository_ = string.Empty;
		private string password_ = string.Empty;
		
		private static readonly ILog log_ = LogManager.GetLogger( typeof(Connection) );
		
		#region methods

		public Connection()
		{
		}

		public Connection( 	string userName,
							string hostName,
							int port,
							string protocol,
							string repository,
							string password,
							string workingDir
						)
		{
			userName_ = userName;
			hostName_ = hostName;
			port_ = port;
			protocol_ = protocol;
			repository_ = repository;
			password_ = password;
			workingDir_ = workingDir;
		}


		public override int GetHashCode()
		{
			return string.Format("{0}{1}{2}{3}{4}{5}", userName_, hostName_, port_, protocol_, repository_, workingDir_ ).GetHashCode();
		}

		/*

		public static bool Equals(object o1, object o2 )
		{
			if( o1 != null )
			{
				return o1.Equals( o2 );
			}		

			// o1 must be null so test o2 for that
			return o2 == null;
		}
		
		*/

		public override bool Equals(object obj)
		{
			log_.Debug( string.Format( "Called equals on {0}", this.ConnectionString ));
			return this.ConnectionString.Equals( ((Connection)obj).ConnectionString );
		}




		public static bool operator ==( Connection c1, Connection c2 )
		{
			
			return Connection.Equals( c1, c2 );
		}

		public static bool operator != (Connection c1, Connection c2 )
		{
			return !Connection.Equals( c1, c2 );
		}

		[XmlIgnore]
		public string ConnectionString
		{
			//:method:[[user][:password]@]hostname[:[port]][:]/path/to/repository
			get
			{

				StringBuilder sb = new StringBuilder();
				if( protocol_ != string.Empty )
				{
					sb.AppendFormat( ":{0}:", protocol_ );
				}

				if( userName_ != string.Empty )
				{
					sb.AppendFormat( "{0}@", userName_ );
				}

				if( hostName_ != string.Empty )
				{
					sb.AppendFormat( "{0}:", hostName_ );

					if( port_ > Connection.PORT_UNASSIGNED )
					{
						sb.AppendFormat( "{0}:", port_ );
					}						
				}

				sb.Append( string.Format("/{0}", repository_) );

				log_.Debug( string.Format("Connection string = {0}", sb.ToString() ) );

				return sb.ToString();
			}
		}

		#endregion

		public string Host
		{
			get{ return hostName_; }
			set{ hostName_ = value; }
		}
		public string User
		{
			get{ return userName_; }
			set{ userName_ = value; }
		}

		public int Port
		{
			get{ return port_; }
			set
			{
				port_ = value;
			}
		}

		public string Protocol
		{
			get{ return protocol_; }
			set{ protocol_ = value; }
		}

		public string Repository
		{
			get{ return repository_; }
			set{ repository_ = value; }
		}

		/// <summary>
		/// Note that the password is not written to disk where it could be read by any text editor. 
		/// </summary>
		[XmlIgnore]
		public string Password
		{
			get
			{
				return password_;
			}
			set
			{
				password_ = value;
			}
		}

		public string WorkingDirectory
		{
			get
			{
				return workingDir_;
			}
			set
			{
				workingDir_ = value;
			}
		}

		public Module[] Modules
		{
			get{ return modules_; }
			set{ modules_ = value; }
		}

		[XmlIgnore]
		public string[] ModuleNames
		{
			get
			{
				string[] s = new string[ modules_.Length ];
				int i = 0;
				foreach( Module m in modules_ )
				{
					s[i++] = m.Name;
				}

				return s;
			}
		}

		/// <summary>
		/// Checks if a module is present in list of modules maintained by the connection
		/// </summary>
		/// <param name="test">Module to check for existance</param>
		/// <returns>true if present, else false</returns>
		internal Module IsModulePresent( Module test )
		{
			foreach( Module m in modules_ )
			{
				if( m.Name == test.Name )
				{
					return m;
				}
			}

			return null;
		}

		/// <summary>
		/// Adds a new module to the list of modules maintained by connection. 
		/// </summary>
		/// <param name="newModule"></param>
		internal void AddModule( Module newModule )
		{
			// don't add dups
			Debug.Assert( this.IsModulePresent( newModule ) == null );
			// trim all whitespace from module name to account for the possibility that
			// the user may have inadvertantly typed spaces in drop down.
			newModule.Name.Trim();
			if( newModule.Name != string.Empty )
			{
				ArrayList list = new ArrayList( modules_ );
				list.Add( newModule );
				list.Sort();
				modules_ = new Module[list.Count];
				list.CopyTo( modules_ );
			}
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return this.ConnectionString.CompareTo( ((Connection)obj).ConnectionString );
		}

		#endregion
	}
}