using System;
using System.Reflection;
using System.Resources;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Text;


using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Messages;


namespace SharpCvsAddIn
{
	[Serializable]
	public class ConnectionString
	{
		private string workingDir_ = string.Empty;
		private string userName_ = string.Empty;
		private string hostName_ = string.Empty;
		private int port_ = CvsRoots.PORT_UNASSIGNED;
		private string protocol_ = string.Empty;
		private string repository_ = string.Empty;
		private string password_ = string.Empty;

		#region methods

		public ConnectionString()
		{
		}

		public ConnectionString( string workingDir,
			string userName,
			string hostName,
			int port,
			string protocol,
			string repository,
			string password )
		{
			workingDir_ = workingDir;
			userName_ = userName;
			hostName_ = hostName;
			port_ = port;
			protocol_ = protocol;
			repository_ = repository;
			password_ = password;
		}

		public override int GetHashCode()
		{
			return string.Format("{0}{1}{2}{3}{4}{5}", workingDir_, userName_, hostName_, port_, protocol_, repository_ ).GetHashCode();
		}

		public override string ToString()
		{
			//:method:[[user][:password]@]hostname[:[port]][:]/path/to/repository

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

				if( port_ > CvsRoots.PORT_UNASSIGNED )
				{
					sb.AppendFormat( "{0}:", port_ );
				}						
			}

			sb.Append( string.Format("/{0}", repository_) );

			Debug.WriteLine( string.Format("Connection string = {0}", sb.ToString() ) );

			return sb.ToString();
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
		public string WorkingDirectory
		{
			get{ return workingDir_; }
			set{ workingDir_ = value; }
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

	}

	public class ModuleComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			Module left = (Module)x;
			Module right = (Module)y;
			return string.Compare(left.module_, right.module_, false, CultureInfo.CurrentCulture);
		}

		#endregion

	}

	[Serializable]
	public class Module
	{
		public string module_ = string.Empty;
		public ArrayList tags_ = new ArrayList();
		public Module()
		{
		}
		public Module( string name )
		{
			module_ = name;
		}

		public void AddTag( string name )
		{
			int pos = tags_.BinarySearch( name, new Comparer( CultureInfo.CurrentCulture) );
			if( pos < 0 )
			{
				tags_.Insert( ~pos, name );
			}
		}			
	}

	[Serializable]
	public class CvsHistory
	{


		#region members
		[XmlElement( typeof(Module) )]
		public ArrayList modules_ = new ArrayList();        

		#endregion members

		#region methods

		public CvsHistory()
		{
		}

		internal static void AddSorted( ref ArrayList list, string itemToAdd )
		{
			int pos = list.BinarySearch( itemToAdd, new Comparer( CultureInfo.CurrentCulture));
			if( pos < 0 )
			{
				list.Insert( ~pos, itemToAdd );
			}

		}

		public Module AddModule( string name )
		{
			Module newMod = new Module( name );
			int pos = modules_.BinarySearch( newMod, new ModuleComparer() );
			if( pos < 0 )
			{
				modules_.Insert( ~pos, newMod );
				return newMod;
			}

			return (Module)modules_[pos];
		}

		public Module GetModule( string name )
		{
			Module mod = new Module( name );
			int pos = modules_.BinarySearch( mod, new ModuleComparer());
			if( pos < 0 ) throw new ArgumentOutOfRangeException( "moduleName", name,
							  "No such module exists" );
			return (Module)modules_[pos];
		}

		[XmlIgnore]
		public string[] ModuleNames
		{
			get
			{
				if( modules_.Count == 0 ) return null;
				string[] mods = new string[ modules_.Count ];
				int i = 0;
				foreach( Module m in modules_ )
				{
					mods[i++] = m.module_;
				}
				return mods;
			}
		}

		#endregion


	}

	[Serializable]
	public class CvsRoots
	{
		[Serializable]
		public class SerializablePair
		{
			public ConnectionString conn_ = null;
			public CvsHistory hist_ = null;
			public SerializablePair()
			{
			}
			public SerializablePair(ConnectionString conn, CvsHistory hist)
			{
				conn_ = conn;
				hist_ = hist;
			}

		}


		private Hashtable roots_ = new Hashtable();
		private ConnectionString current_ = null;
		public const int PORT_UNASSIGNED = 0;
		public const int PORT_DEFAULT = 2401;


		public CvsRoots()
		{
		}

		public SerializablePair[] Connections
		{
			get
			{
				Debug.WriteLine( "getting roots" );
				if( roots_.Count == 0 ) return null;

				SerializablePair[] pairs = new SerializablePair[ roots_.Count ];

				int i = 0;
				foreach( ConnectionString conn in roots_.Keys )
				{
					pairs[i++] = new SerializablePair( conn, (CvsHistory)roots_[conn] );
				}

				return pairs;
			}
			set
			{
				Debug.WriteLine( "setting roots" );
				Debug.Assert( value != null );

				foreach( SerializablePair pair in value )
				{
					roots_[pair.conn_] = pair.hist_;
				}

			}
		}



		public CvsHistory CurrentRoot
		{
			get
			{
				if(current_ != null )
				{		
					return (CvsHistory)roots_[current_];
				}
				else
				{
					return null;
				}
			}
		}



		public ConnectionString CurrentConnection
		{
			get
			{
				return current_;
			}

			set 
			{
				if( roots_[value] == null )
				{
					roots_[value] = new CvsHistory();
				}
				
				current_ = value;

			}
		}

	}
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Model
	{
		internal static readonly string LanguageAssemblyName =  "language-assembly.dll";
		internal static readonly string LanguageBaseResourceName = "app-strings";
		internal const string STORAGE_FILE_NAME = "sharp-cvs-addin.history.xml";
		public readonly ResourceManager ResourceManager;
		private string storageFileName_ = string.Empty;
		private string assemblyPath_ = string.Empty;

		private CvsRoots roots_;

		public CvsRoots Roots
		{
			get
			{
				return roots_;
			}
			set
			{
				roots_ = value;
			}

		}

		public Model()
		{
			string codebase = this.GetType().Assembly.CodeBase;
			Debug.WriteLine( codebase );
			Uri uriPath = new Uri( codebase );
			Debug.WriteLine( Path.GetDirectoryName(uriPath.LocalPath) );
			assemblyPath_ = Path.GetDirectoryName(uriPath.LocalPath);
			string langAssemblyPath = Path.Combine( assemblyPath_, Model.LanguageAssemblyName ); 
			AssemblyName name = AssemblyName.GetAssemblyName( langAssemblyPath );
			this.ResourceManager = new ResourceManager( Model.LanguageBaseResourceName, 	Assembly.Load( name ) );


			using(IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly())
			{
				string storageFile = Path.GetFileName(STORAGE_FILE_NAME);
				foreach(string isoFile in isf.GetFileNames(storageFile))
				{
					if( Path.GetFileName(isoFile) == storageFile )
					{
						using(IsolatedStorageFileStream ifm = new IsolatedStorageFileStream( storageFile,
								  FileMode.Open, FileAccess.Read, FileShare.Read, isf ))
						{
							XmlSerializer srl = new XmlSerializer(typeof(CvsRoots));
							roots_ = (CvsRoots)srl.Deserialize(ifm);
							return;
						}
					}
				}				
			}

			roots_ = new CvsRoots();
			

		}

		public ConnectionString CurrentConnection
		{
			get
			{
				return roots_.CurrentConnection;
			}
		}

		public CvsHistory CurrentHistory
		{
			get
			{
				return roots_.CurrentRoot;
			}
		}

		public void Save()
		{
			try
			{

				using(IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly()) 
				{
					using(IsolatedStorageFileStream isfs = new IsolatedStorageFileStream( 
							  Path.GetFileName(STORAGE_FILE_NAME), FileMode.OpenOrCreate, FileAccess.Write,
							  FileShare.Read, isf ))
					{
						XmlSerializer srl = new XmlSerializer(typeof(CvsRoots));
						srl.Serialize( isfs, roots_ );
					}

				}

			}
			catch( Exception e)
			{
				Debug.WriteLine( e.Message );
			}
		}
	}
}
