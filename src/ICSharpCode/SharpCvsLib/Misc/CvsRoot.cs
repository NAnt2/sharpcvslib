#region "Copyright"
// CvsRoot.cs 
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
#endregion

using System;

namespace ICSharpCode.SharpCvsLib.Misc { 
	
    /// <summary>
    /// Class to encapsulate the properties of the cvsroot for the
    ///     repository you are communicating with.
    /// </summary>
	public class CvsRoot
	{
		private string protocol         = String.Empty;
		private string user             = String.Empty;
		private string host             = String.Empty;
		private string cvsrepository    = String.Empty;
		
        /// <summary>
        /// The protocol to use when communicating with the server.
        ///     <ol>
        ///         <li>pserver</li>
        ///         <li>ssh</li>
        ///         <li>ext</li>
        ///     </ol>
        /// </summary>
		public string Protocol {
			get {
				return protocol;
			}
			set {
				protocol = value;
			}
		}
		
        /// <summary>
        /// User name used to access the repository.
        /// </summary>
		public string User {
			get {
				return user;
			}
			set {
				user = value;
			}
		}
		
        /// <summary>
        /// Host running the repository.
        /// </summary>
		public string Host {
			get {
				return host;
			}
			set {
				host = value;
			}
		}
		
        /// <summary>
        /// Module to use in command.
        /// </summary>
		public string CvsRepository {
			get {
				return cvsrepository;
			}
			set {
				cvsrepository = value;
			}
		}
		
        /// <summary>
        /// Constructor.  Parses a cvsroot variable passed in as
        ///     a string into the different properties that make it
        ///     up.
        /// </summary>
        /// <param name="cvsroot"></param>
		public CvsRoot(string cvsroot)
		{
			int s1 = cvsroot.IndexOf(':', 1)  + 1;
			if (s1 == 0 || cvsroot[0] != ':')
				throw new ArgumentException("cvsroot doesn't start with :");
			
			int s2 = cvsroot.IndexOf('@', s1) + 1;
			if (s2 == 0)
				throw new ArgumentException("no username given");
			
			int s3 = cvsroot.IndexOf(':', s2) + 1;
			if (s3 == 0)
				throw new ArgumentException("no host given");
				
			protocol      = cvsroot.Substring(1, s1 - 2);
			user          = cvsroot.Substring(s1, s2 - s1 - 1);
			host          = cvsroot.Substring(s2, s3 - s2 - 1);
			cvsrepository = cvsroot.Substring(s3);
			
			if (protocol.Length == 0 || user.Length == 0 || host.Length == 0 || cvsrepository.Length == 0)
				throw new ArgumentException("invalid cvsroot given");
		}
		
        /// <summary>
        /// Convert CvsRoot object to a human readable format.
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
			return ':' + protocol + ':' + user + '@' + host + ':' + cvsrepository;
		}
	}
}
