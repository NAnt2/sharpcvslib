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
//    <author>Mike Krueger</author>
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Threading;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using ICSharpCode.SharpCvsLib.Config;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Responses;
using ICSharpCode.SharpCvsLib.FileHandler;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Streams;
using ICSharpCode.SharpCvsLib.Logs;

using log4net;

namespace ICSharpCode.SharpCvsLib.Client {

    /// <summary>
    /// Cvs server connection, handles connections to the cvs server.
    /// </summary>
    public class CVSServerConnection : 
        IConnection, IResponseServices, ICommandConnection {

        private readonly ILog LOGGER =
            LogManager.GetLogger (typeof (CVSServerConnection));

        private string nextFileDate;

        private TcpClient tcpclient = null;

        private CvsStream inputStream;
        private CvsStream outputStream;

        private WorkingDirectory repository;

        private IFileHandler uncompressedFileHandler =
            new UncompressedFileHandler();

        private const String PSERVER_AUTH_SUCCESS = "I LOVE YOU";
        private const String PSERVER_AUTH_FAIL = "I HATE YOU";

        private RequestLog requestLog;
        private ResponseLog responseLog;

        private SharpCvsLibConfig config;

        private Process p = null;

        /// <summary>
        ///     Initialize the cvs server connection.
        /// </summary>
        public CVSServerConnection () {
            inputStream  = new CvsStream (new MemoryStream());
            outputStream = new CvsStream (new MemoryStream());

            this.config = SharpCvsLibConfig.GetInstance();
            try {
                if (config.Log.DebugLog.Enabled) {
                    requestLog = new RequestLog ();
                    responseLog = new ResponseLog ();

                    this.InputStream.RequestMessage.MessageEvent +=
                        new EncodedMessage.MessageHandler (requestLog.Log);
                    this.OutputStream.ResponseMessage.MessageEvent +=
                        new EncodedMessage.MessageHandler (responseLog.Log);
                }
            } catch (Exception e) {
                LOGGER.Error (e);
            }

            if (null == config) {
                config = new SharpCvsLibConfig ();
            }
            LOGGER.Debug("Config=["  + config.ToString() + "]");
        }

        /// <summary>
        /// Gets a file handler for files that are not zipped.
        /// </summary>
        public IFileHandler UncompressedFileHandler {
            get {return uncompressedFileHandler;}
        }

        /// <summary>
        ///     Set the server timeout value.
        /// </summary>
        public int Timeout {
            get {return this.config.Timeout;}
        }

        /// <summary>
        ///     Set the time to sleep between sending the authentication request
        ///         and receiving the authentication response.  Accounts for
        ///         slow responses on some servers.
        /// </summary>
        public int AuthSleep {
            get {return this.config.AuthSleep;}
        }

        /// <summary>
        /// The port that should be used for cvs connections.
        /// </summary>
        public int Port {
            get {
                return this.repository.CvsRoot.Port;
            }
        }

            /// <summary>
            /// Cvs input stream writer
            /// </summary>
            public CvsStream InputStream {
            get {return inputStream;}
            set {inputStream = value;}
        }

        /// <summary>
        /// Wrapper for the request message delegate on the input stream.
        /// </summary>
        public EncodedMessage RequestMessage {
            get {return InputStream.RequestMessage;}
        }

        /// <summary>
        /// Wrapper for the response message delegate on the output
        ///     stream.
        /// </summary>
        public EncodedMessage ResponseMessage {
            get {return OutputStream.ResponseMessage;}
        }

        /// <summary>
        /// Cvs output stream reader
        /// </summary>
        public CvsStream OutputStream {
            get {return outputStream;}
            set {outputStream = value;}
        }

        /// <summary>
        /// Message event.
        /// </summary>
        public EncodedMessage MessageEvent = new EncodedMessage ();

        /// <summary>
        /// Send the message to the message event handler.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message) {
            //System.Console.WriteLine (message);
            LOGGER.Info (message);
            MessageEvent.SendMessage(message);
        }

        /// <summary>
        /// Send the message to the message event handler.
        /// </summary>
        /// <param name="module">The cvs module.</param>
        /// <param name="repositoryPath">The path to the cvs repository.</param>
        /// <param name="filename">The name of the file being manipulated.</param>
        public void SendMessage (String module,
                                String repositoryPath,
                                String filename) {
            StringBuilder sb = new StringBuilder ();
            sb.Append (module);
            sb.Append (repositoryPath);
            sb.Append (filename);

            this.SendMessage (sb.ToString ());
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
                string responseStr = inputStream.ReadToFirstWS();
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
                        inputStream.ReadLine();
                    }
                    break;
                }
                response.Process(inputStream, this);
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
        public void SubmitRequest(IRequest request) {
            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nSubmit Request");
                msg.Append ("\n\trequest=[").Append (request).Append ("]");
                LOGGER.Debug (msg);
            }

            outputStream.SendString(request.RequestString);

            if (request.DoesModifyConnection) {
                request.ModifyConnection(this);
            }

            if (request.IsResponseExpected) {
                HandleResponses();
            }
        }

        /// <summary>
        /// Send a file to the cvs repository.
        /// </summary>
        /// <param name="fullPath">The full path to the file being sent to the server.</param>
        /// <param name="isBinary"><code>true</code> if the file is binary; 
        ///     otherwise <code>false</code>.</param>
        public void SendFile(string fullPath, bool isBinary)
        {
            if (isBinary) {
                UncompressedFileHandler.SendBinaryFile(OutputStream, fullPath);
            } else {
                UncompressedFileHandler.SendTextFile(OutputStream, fullPath);
            }
        }

        /// <summary>
        /// Connect to the repository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="password"></param>
        public void Connect(WorkingDirectory repository, string password) {
            this.repository = repository;
            Authentication(password);
        }

        private void ExitShellEvent(object sender, EventArgs e) {
            if (LOGGER.IsDebugEnabled) {
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
        public void Authentication(string password) {
            switch (repository.CvsRoot.Protocol) {
                case "ext": {
                    StringBuilder processArgs = new StringBuilder ();
                    processArgs.Append ("-l ").Append (repository.CvsRoot.User);
                    processArgs.Append (" -q ");  // quiet
                    processArgs.Append (" ").Append (repository.CvsRoot.Host);
                    processArgs.Append (" \"cvs server\"");

                    try {

                        ProcessStartInfo startInfo =
                            new ProcessStartInfo(this.config.Shell, processArgs.ToString ());
                        if (LOGGER.IsDebugEnabled) {
                            StringBuilder msg = new StringBuilder ();
                            msg.Append("Process=[").Append(this.config.Shell).Append("]");
                            msg.Append("Process Arguments=[").Append(processArgs).Append("]");
                            LOGGER.Debug(msg);
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
                        throw new ExecuteShellException(this.config.Shell + processArgs.ToString ());
                    }
                    BufferedStream errstream = new BufferedStream(p.StandardError.BaseStream);
                    //inputstream  = new CvsStream(new BufferedStream(p.StandardOutput.BaseStream));
                    //outputstream = new CvsStream(new BufferedStream(p.StandardInput.BaseStream));
                    StreamWriter streamWriter  = p.StandardInput;
                    StreamReader streamReader = p.StandardOutput;

                    //streamWriter.AutoFlush = true;
                    //streamReader.ReadToEnd ();
                    //streamWriter.WriteLine(password);

                    inputStream = new CvsStream (streamReader.BaseStream);
                    outputStream = new CvsStream (streamWriter.BaseStream);
                    break;
                }
                case "pserver": {
                    tcpclient = new TcpClient ();
                    tcpclient.SendTimeout = this.Timeout;

                    if (LOGGER.IsDebugEnabled) {
                        StringBuilder msg = new StringBuilder();
                        msg.Append("Before submitting pserver connect request.  ");
                        msg.Append("repository.CvsRoot.CvsRepository=[").Append(repository.CvsRoot.CvsRepository).Append("]");
                        msg.Append("repository.CvsRoot.User=[").Append(repository.CvsRoot.User).Append("]");
                        msg.Append("has password=[").Append(null != password).Append("]");
                        msg.Append("port=[").Append(repository.CvsRoot.Port).Append("]");
                        LOGGER.Debug (msg);
                    }

                    tcpclient.Connect(repository.CvsRoot.Host, repository.CvsRoot.Port);
                    inputStream  = outputStream = new CvsStream(tcpclient.GetStream());

                    for (int i=0; i < 5; i++) {
                        try {
                            SubmitRequest(new PServerAuthRequest(repository.CvsRoot.CvsRepository,
                                repository.CvsRoot.User,
                                password));
                            break;
                        } catch (Exception e) {
                            LOGGER.Error (e);
                        }
                    }

                    inputStream.Flush();

                    string retStr;
                    try {
                        retStr = inputStream.ReadLine();
                    }
                    catch (IOException e) {
                        String msg = "Failed to read line from server.  " +
                            "It is possible that the remote server was down.";
                        LOGGER.Error (msg, e);
                        throw new AuthenticationException (msg);
                    }

                    switch (retStr) {
                        case PSERVER_AUTH_SUCCESS: {
                            SendMessage("Connection established");
                            break;
                        }
                        case PSERVER_AUTH_FAIL: {
                            try {
                                tcpclient.Close();
                            } finally {
                                throw new AuthenticationException();
                            }
                        }
                        default: {
                            StringBuilder msg = new StringBuilder ();
                            msg.Append("Unknown Server response : >").Append(retStr).Append("<");
                            SendMessage(msg.ToString());
                            try {
                                tcpclient.Close();
                            } finally {
                                throw new AuthenticationException(msg.ToString());
                            }
                        }
                    }
                    break;
                }
                default: {
                    StringBuilder notSupportedMsg = new StringBuilder ();
                    notSupportedMsg.Append("Unknown protocol=[").Append(repository.CvsRoot.Protocol).Append("]");
                    throw new UnsupportedProtocolException (notSupportedMsg.ToString());
                }
            }

            // TODO: Move these into an abstract request class
            SubmitRequest(new ValidResponsesRequest());
            SubmitRequest(new ValidRequestsRequest());

            SubmitRequest(new UseUnchangedRequest());
            SubmitRequest(new RootRequest(repository.CvsRoot.CvsRepository));

            SubmitRequest(new GlobalOptionRequest(GlobalOptionRequest.Options.QUIET));
            SubmitRequest(new CaseRequest());

        }

        /// <summary>
        /// The repository information.
        /// </summary>
        public WorkingDirectory Repository {
            get {return repository;}
        }

        /// <summary>
        /// Next file date.
        /// </summary>
        public string NextFileDate {
            get {return nextFileDate;}
            set {nextFileDate = value;}
        }
        private string nextFile = null;
        /// <summary>
        /// The next file.
        /// </summary>
        public string NextFile {
            get {return nextFile;}
            set {nextFile = value;}
        }

        /// <summary>
        /// Close the cvs server connection.
        /// </summary>
        public void Close() {
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
