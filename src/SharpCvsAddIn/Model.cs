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

using SharpCvsAddIn.Persistance;


namespace SharpCvsAddIn
{

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class Model
	{
		internal static readonly string LanguageAssemblyName =  "language-assembly.dll";
		internal static readonly string LanguageBaseResourceName = "app-strings";
		internal const string STORAGE_FILE_NAME = "sharp-cvs-addin4.history.xml";
		public readonly ResourceManager ResourceManager;
		private string storageFileName_ = string.Empty;
		private string assemblyPath_ = string.Empty;


		private Storage storage_ = new Storage();
		/// <summary>
		/// Returns object that handles isolated storage data for application
		/// </summary>
		public Storage Storage
		{
			get{ return storage_ ; }
		}

		public Model(string satelliteDllPath )
		{
			AssemblyName name = AssemblyName.GetAssemblyName( satelliteDllPath );
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
							XmlSerializer srl = new XmlSerializer(typeof(Persistance.Storage));
							storage_ = (Persistance.Storage)srl.Deserialize(ifm);
							return;
						}
					}
				}				
			}		

		}

		public Connection[] Connections
		{
			get{ return storage_.Connections; }
			set{ storage_.Connections = value; }
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
						XmlSerializer srl = new XmlSerializer(typeof(Persistance.Storage));
						srl.Serialize( isfs, storage_ );
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
