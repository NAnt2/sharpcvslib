#region "Copyright"
// CVSServerConnection.cs 
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.
//
//    Author:     Mike Krueger, 
//                Clayton Harbour
#endregion

using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Responses;
using ICSharpCode.SharpCvsLib.FileHandler;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

[assembly: log4net.Config.DOMConfigurator(
	ConfigFileExtension="config", Watch=true)]

namespace ICSharpCode.SharpCvsLib.Client {
	
    /// <summary>
    /// Message handling event.
    /// </summary>
	public delegate void MessageEventHandler(string message);    
	
    /// <summary>
    /// Cvs server connection, handles connections to the cvs server.
    /// </summary>
	public class CVSServerConnection : IConnection, IResponseServices
	{    
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (CVSServerConnection));
	    
		private const int DEFAULT_PORT = 2401;
		
		private string nextFileDate;
		
		private TcpClient tcpclient = null;
		
		private CvsStream inputstream  = null;
		private CvsStream outputstream = null;
		
		private WorkingDirectory repository;
		
		private IFileHandler uncompressedFileHandler = 
		    new UncompressedFileHandler();
	    
	    private const String PSERVER_AUTH_SUCCESS = "I LOVE YOU";
	    private const String PSERVER_AUTH_FAIL = "I HATE YOU";
	    
		
        /// <summary>
        /// Gets a file handler for files that are not zipped.
        /// </summary>
		public IFileHandler UncompressedFileHandler {
			get {
				return uncompressedFileHandler;
			}
		}
		
        /// <summary>
        /// Cvs input stream writer
        /// </summary>
		public CvsStream InputStream {
			get {
				return inputstream;
			}
			set {
				inputstream = value;
			}
		}
		
        /// <summary>
        /// Cvs output stream reader
        /// </summary>
		public CvsStream OutputStream {
			get {
				return outputstream;
			}
			set {
				outputstream = value;
			}
		}
		
        /// <summary>
        /// Message event.
        /// </summary>
		public event MessageEventHandler MessageEvent;

        /// <summary>
        /// Send the message to the message event handler.
        /// </summary>
        /// <param name="message"></param>
		public void SendMessage(string message)
		{
			if (MessageEvent != null) {
				MessageEvent(message);
			}
		}
		
        /// <summary>
        /// Module to execute cvs commands on.
        /// </summary>
		private class Module 
		{
			public string localdir = null;
			public ArrayList entries = new ArrayList();
		}
		
		private void HandleResponses()
		{
			SortedList modules = new SortedList();
			
			while (true) {
				string responseStr = inputstream.ReadToFirstWS();
			    if (LOGGER.IsDebugEnabled)
			    {
				    LOGGER.Debug ("Response : " + responseStr);
			    }
				
				if (responseStr.Length == 0) {
					SendMessage("server timed out");
					break;
				}
				
				IResponse response = ResponseFactory.CreateResponse(responseStr.Substring(0, responseStr.Length - 1));
			    if (LOGGER.IsDebugEnabled) {
				    LOGGER.Debug("cvs server: " + response);
			    }
				
				if (response == null) {
					if (responseStr.EndsWith(" ")) {
						inputstream.ReadLine();
					}
					break;
				}
				response.Process(inputstream, this);
				if (response.IsTerminating) {
					break;
				}
			}
			// TODO: Figure out if this is where the cvs file creation should go.
//			repository.CreateCVSFiles();
		}
		
        /// <summary>
        /// Submit a request to the cvs repository.
        /// </summary>
        /// <param name="request"></param>
		public void SubmitRequest(IRequest request)
		{
		    if (LOGGER.IsDebugEnabled)
		    {
			    LOGGER.Debug("cvs client: " + request.RequestString);
		    }
			
			outputstream.SendString(request.RequestString);
			
			if (request.DoesModifyConnection) {
				request.ModifyConnection(this);
			}
			
			if (request.IsResponseExpected) {
				HandleResponses();
			}
		}
		
        /// <summary>
        /// Connect to the repository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="password"></param>
		public void Connect(WorkingDirectory repository, string password)
		{
			this.repository = repository;
			Authentification(password);
		}
		
		private string shell = "ssh";
        /// <summary>
        /// The cvs connection type
        ///     <ol>
        ///         <li>ssh</li>
        ///         <li>pserver</li>
        ///         <li>ext</li>
        ///     </ol>
        /// </summary>
		public string Shell {
			get {
				return shell;
			}
			set {
				shell = value;
			}
		}
		
		private Process p = null; 
		
		private void ExitShellEvent(object sender, EventArgs e)
		{
		    if (LOGGER.IsDebugEnabled)
		    {
			    LOGGER.Debug("Process EXITED");
		    }
		    
			if (p.ExitCode != 0) {
				throw new AuthenticationException();
			}
		}
		
        /// <summary>
        /// Authentication for the repository.
        /// </summary>
        /// <param name="password"></param>
		public void Authentification(string password)
		{
			switch (repository.CvsRoot.Protocol) {
				case "ext":
					try {
						ProcessStartInfo startInfo = 
						    new ProcessStartInfo(shell, "-l " + 
						                         repository.CvsRoot.User + 
						                         " " + 
						                         repository.CvsRoot.Host + 
						                         " \"cvs server\"");
					    if (LOGGER.IsDebugEnabled)
					    {
						    LOGGER.Debug("-l " + repository.CvsRoot.User + 
						                 " " + 
						                 repository.CvsRoot.Host + 
						                 " \"cvs server\"");
					    }
						startInfo.RedirectStandardError  = true;
						startInfo.RedirectStandardInput  = true;
						startInfo.RedirectStandardOutput = true;
						startInfo.UseShellExecute        = false;
						p = new Process();
						p.StartInfo = startInfo;
						p.Exited += new EventHandler(ExitShellEvent);
						p.Start();
					} catch (Exception e) {
					    if (LOGGER.IsDebugEnabled) {
    						LOGGER.Debug(e);
					    }
						throw new ExecuteShellException(shell);
					}
					BufferedStream errstream = new BufferedStream(p.StandardError.BaseStream);
					inputstream  = new CvsStream(new BufferedStream(p.StandardOutput.BaseStream));
					outputstream = new CvsStream(new BufferedStream(p.StandardInput.BaseStream));
					break;
				case "pserver":
					tcpclient = new TcpClient();
					tcpclient.Connect(repository.CvsRoot.Host, DEFAULT_PORT);
					inputstream  = outputstream = new CvsStream(tcpclient.GetStream());
					SubmitRequest(new PServerAuthRequest(repository.CvsRoot.CvsRepository, repository.CvsRoot.User, password));
					inputstream.Flush();
			        
			        string retStr;
			        
			        try {
    					retStr = inputstream.ReadLine();
			        }
			        catch (IOException e) {
			            String msg = "Failed to read line from server.  " +
			                "It is possible that there was a problem connecting " +
			                "to the remote server.";
			            LOGGER.Error (msg, e);
			            throw new Exception (msg);
			        }
			        
					switch (retStr) {
						case PSERVER_AUTH_SUCCESS:
							SendMessage("Connection established");
							break;
						case PSERVER_AUTH_FAIL:
							throw new AuthenticationException();
						default:
							SendMessage("Unknown Server response : >" + retStr + "<");
							throw new ApplicationException("Unknown Server response : >" + retStr + "<"); // TODO : invent a better exception for this case.
					}
					break;
			}
			SubmitRequest(new ValidResponsesRequest());
			SubmitRequest(new ValidRequestsRequest());
			
			SubmitRequest(new RootRequest(repository.CvsRoot.CvsRepository));
			SubmitRequest(new UseUnchangedRequest());
		}
		
        /// <summary>
        /// Convert the specified path to winnt format.
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
	    public string ConvertPath(string localPath, string repository)
		{
			return localPath + repository.Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString());
		}
		
        /// <summary>
        /// Set the entry.
        ///     TODO: Not implemented
        /// </summary>
        /// <param name="localFile"></param>
        /// <param name="entry"></param>
		public void SetEntry(string localFile, Entry entry)
		{
			// TODO : implement set entry
            throw new NotSupportedException ("Implement the setentry method.");
		}
		
        /// <summary>
        /// The repository information.
        /// </summary>
		public WorkingDirectory Repository {
			get {
				return repository;
			}
		}
		
        /// <summary>
        /// Next file date.
        /// </summary>
		public string NextFileDate {
			get {
				return nextFileDate;
			}
			set {
				nextFileDate = value;
			}
		}
		private string nextFile = null;
        /// <summary>
        /// The next file.
        /// </summary>
		public string NextFile {
			get {
				return nextFile;
			}
			set {
				nextFile = value;
			}
		}
		
        /// <summary>
        /// Close the cvs server connection.
        /// </summary>
		public void Close()
		{ 
			if (repository != null && repository.CvsRoot != null) {
				switch (repository.CvsRoot.Protocol) {
					case "ext":
						if (p != null && !p.HasExited) {
							p.Kill();
							p.WaitForExit();
							p = null;
						}
					break;
				}
			}
		}
	}
}
