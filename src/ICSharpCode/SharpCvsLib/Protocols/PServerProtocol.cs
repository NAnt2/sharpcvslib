#region "Copyright"
// Copyright (C) 2004 Mike Krueger, Clayton Harbour
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
//  <author>Mike Krueger</author>
//  <author>Clayton Harbour</author>
//
#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Protocols
{
	/// <summary>
	/// Handle connect and authentication for the pserver protocol.
	/// </summary>
	public class PServerProtocol : AbstractProtocol {
        private readonly ILog LOGGER =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const String PSERVER_AUTH_SUCCESS = "I LOVE YOU";
        private const String PSERVER_AUTH_FAIL = "I HATE YOU";

        private TcpClient tcpClient = null;

        /// <summary>
        /// The server timeout value.
        /// </summary>
        private int Timeout {
            get {return this.Config.Timeout;}
        }

        /// <summary>
        /// Tcp socket that the connect method should initialize.
        /// </summary>
        public TcpClient TcpClient {
            get {return this.tcpClient;}
        }

        /// <summary>
        /// Create a new instance of the pserver protocol.
        /// </summary>
		public PServerProtocol() {
		}

        /// <summary>
        /// Connect to the cvs server.
        /// </summary>
        public override void Connect () {
            this.HandlePserverAuthentication(this.Password);
        }

        /// <summary>
        /// Disconnect from the cvs server.
        /// </summary>
        public override void Disconnect() {
            this.TcpClient.Close();
        }

        ///<summary>Either accept the pserver authentication response from the server or if the user
        /// is invalid then throw an authentication exception.</summary>
        ///<param name="password">The password to send.</param>
        ///<exception cref="AuthenticationException">If the user is not valid.</exception>
        private void HandlePserverAuthentication(String password) {
            String retStr = this.SendPserverAuthentication(password);
            if (retStr.Equals(PSERVER_AUTH_SUCCESS)) {
                this.SendConnectedMessageEvent(this, new MessageEventArgs("Connection established"));
            } else if (retStr.Equals(PSERVER_AUTH_FAIL)) {
                try {
                    tcpClient.Close();
                } finally {
                    throw new AuthenticationException();
                }
            } else {
                StringBuilder msg = new StringBuilder ();
                msg.Append("Unknown Server response : >").Append(retStr).Append("<");
                this.SendDisconnectedMessageEvent(this, new MessageEventArgs(msg.ToString()));
                try {
                    tcpClient.Close();
                } finally {
                    throw new AuthenticationException(msg.ToString());
                }
            }   
        }

        private String SendPserverAuthentication (String password) {
            tcpClient = new TcpClient ();
            tcpClient.SendTimeout = this.Timeout;

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder();
                msg.Append("Before submitting pserver connect request.  ");
                msg.Append("this.Repository.CvsRoot.CvsRepository=[").Append(this.Repository.CvsRoot.CvsRepository).Append("]");
                msg.Append("this.Repository.CvsRoot.User=[").Append(this.Repository.CvsRoot.User).Append("]");
                msg.Append("has password=[").Append(null != password).Append("]");
                msg.Append("port=[").Append(this.Repository.CvsRoot.Port).Append("]");
                LOGGER.Debug (msg);
            }

            tcpClient.Connect(this.Repository.CvsRoot.Host, this.Repository.CvsRoot.Port);
            SetInputStream(new CvsStream(tcpClient.GetStream()));
            SetOutputStream(this.InputStream);
//            inputStream  = outputStream = new CvsStream(tcpClient.GetStream());

            int MAX_RETRY = 1;
            for (int i=0; i < MAX_RETRY; i++) {
                try {
                    SubmitRequest(new PServerAuthRequest(this.Repository.CvsRoot.CvsRepository,
                        this.Repository.CvsRoot.User,
                        password));
                    break;
                } catch (Exception e) {
                    LOGGER.Error (e);
                }
            }

            InputStream.Flush();

            string retStr;
            try {
                retStr = InputStream.ReadLine();
            } catch (IOException e) {
                String msg = "Failed to read line from server.  " +
                    "It is possible that the remote server was down.";
                LOGGER.Error (msg, e);
                throw new AuthenticationException (msg);
            }

            return retStr;
        }

        /// <summary>
        /// Submit a request to the cvs repository.  
        /// 
        /// NOTE: I copied this from the CvsServerConnection because I did not want to create an
        /// cyclic dependancy with the class for this one method.
        /// </summary>
        /// <param name="request"></param>
        private void SubmitRequest(IRequest request) {
            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("\nSubmit Request");
                msg.Append ("\n\trequest=[").Append (request).Append ("]");
                LOGGER.Debug (msg);
            }

            OutputStream.SendString(request.RequestString);

// PServer request does not modify the connection.
//            if (request.DoesModifyConnection) {
//                request.ModifyConnection(this);
//            }

// Response is not expected from the pserver authentication...well it is handled seperately from
// this mechanism in any case.
//            if (request.IsResponseExpected) {
//                HandleResponses();
//            }
        }
	}
}
