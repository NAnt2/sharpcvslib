#region "Copyright"
// LogCommand.cs 
// Copyright (C) 2002 Mike Krueger
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

using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Misc;

namespace ICSharpCode.SharpCvsLib.Commands { 
	
    /// <summary>
    /// Log command.
    ///     TODO: Figure out what this is for.
    /// </summary>
	public class LogCommand : ICommand
	{
		private WorkingDirectory workingdirectory;
		private string directory;
		private Entry entry;
		
		private bool defaultBranch     = false;
		private bool headerAndDescOnly = false;
		private bool headerOnly        = false;
		private bool noTags            = false;
		
        /// <summary>
        /// The default branch to use for the module.
        /// </summary>
		public bool DefaultBranch {
			get {
				return defaultBranch;
			}
			set {
				defaultBranch = value;
			}
		}
		
        /// <summary>
        /// TODO: Figure out what this is used for.
        /// </summary>
		public bool HeaderAndDescOnly {
			get {
				return headerAndDescOnly;
			}
			set {
				headerAndDescOnly = value;
			}
		}
		
        /// <summary>
        /// TODO: Figure out what this is used for.
        /// </summary>
		public bool HeaderOnly {
			get {
				return headerOnly;
			}
			set {
				headerOnly = value;
			}
		}
		
        /// <summary>
        /// TODO: Figure out what this is used for.
        /// </summary>
		public bool NoTags {
			get {
				return noTags;
			}
			set {
				noTags = value;
			}
		}
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workingdirectory"></param>
        /// <param name="directory"></param>
        /// <param name="entry"></param>
		public LogCommand(WorkingDirectory workingdirectory, string directory, Entry entry)
		{
			this.workingdirectory    = workingdirectory;
			this.directory = directory;
			this.entry = entry;
		}

        /// <summary>
        /// Do the dirty work.
        /// </summary>
        /// <param name="connection"></param>
		public void Execute(CVSServerConnection connection)
		{
			connection.SubmitRequest(new DirectoryRequest(".", workingdirectory.CvsRoot.CvsRepository + directory));
            
            if (defaultBranch) {
				connection.SubmitRequest(new ArgumentRequest("-b"));
            }
            if (headerAndDescOnly) {
				connection.SubmitRequest(new ArgumentRequest("-t"));
            }
            if (headerOnly) {
				connection.SubmitRequest(new ArgumentRequest("-h"));
            }
            if (noTags) {
				connection.SubmitRequest(new ArgumentRequest("-N"));
            }
			
			connection.SubmitRequest(new ArgumentRequest(entry.Name));
			connection.SubmitRequest(new LogRequest());
		}
	}
}
