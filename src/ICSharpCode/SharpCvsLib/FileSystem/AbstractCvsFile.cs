#region "Copyright"
// Copyright (C) 2003 Clayton Harbour
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
//    <author>Clayton Harbour</author>
#endregion

using System;

using log4net;

namespace ICSharpCode.SharpCvsLib.FileSystem
{
	/// <summary>
	/// The abstract cvs file implements common methods and provides a common 
	///     implementation for all CVS management files.
	/// </summary>
	public abstract class AbstractCvsFile {
        private readonly ILog LOGGER = LogManager.GetLogger(typeof (AbstractCvsFile));
        private String fullPath;
        private String fileContents;
        private String localCvsFullPath;

        /// <summary>
        /// Return the path to the file that this cvs object is controlling.  In
        ///     most cases this is just the full path to the object, however one
        ///     known exception would be the Entry which would have file information
        ///     stripped from the full path.
        /// </summary>
        public String Path {
            get {
                String tempPath = this.fullPath;
                if (this.fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
                    tempPath = this.fullPath.Substring(0, this.fullPath.Length - 1);
                }
                tempPath = System.IO.Path.GetDirectoryName(tempPath);   
                return this.GetPathWithDirectorySeperatorChar(tempPath);
            }
        }

        /// <summary>
        /// The full path to the local cvs management file.
        /// </summary>
        /// <example>
        ///     <code>/src/sharpcvslib/CVS/Entries</code>
        ///     <code>c:/src/sharpcvslib/CVS/Entries</code>
        /// </example>
        public String LocalCvsFileFullPath {
            get {return this.localCvsFullPath;}
            set {this.localCvsFullPath = value;}
        }

        private String GetPathWithDirectorySeperatorChar(String path) {
            if (!path[path.Length - 1].Equals(System.IO.Path.DirectorySeparatorChar)) {
                return path + System.IO.Path.DirectorySeparatorChar;
            } else if (!path[path.Length - 1].Equals('/')) {
                return path + System.IO.Path.DirectorySeparatorChar;
            }
            return path;
        }

        /// <summary>
        /// The full path to the file or directory that this object is managing.
        /// </summary>
        public String FullPath {
            get {return this.fullPath;}
            set {this.fullPath = value;}
        }

        /// <summary>
        /// Contents that are contained in the management file that this object 
        ///     represents.
        /// </summary>
        public virtual String FileContents {
            get {return this.fileContents;}
            set {this.fileContents = value;}
        }

        /// <summary>
        ///     Create a new object that represents the management file that CVS
        ///         uses to hold information about the repository and local file
        ///         system.
        /// </summary>
        /// <param name="fullPath">The full path to the file or directory on the
        ///     filesystem that this object is managing.</param>
        /// <param name="fileContents">A line of comments that represents information
        ///     to be written to the cvs management file, or is written in the
        ///     cvs management file.</param>
		public AbstractCvsFile(String fullPath, String fileContents) {
            if (PathTranslator.ContainsCVS(fullPath)) {
                // attempt recovery if this file contains a cvs folder.
                fullPath = System.IO.Path.GetDirectoryName(fullPath);
                if (PathTranslator.ContainsCVS(fullPath)) {
                    throw new Exception("Path information should not contain cvs folder.");
                }
            }
            this.fileContents = fileContents;
            this.fullPath = fullPath;

            this.Parse(fileContents);
            //this.DeriveCvsFullPath();

            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug("Created new entry=[" + this + "]");
                if (this.ToString().ToUpper().IndexOf("C:") > 0) {
                    LOGGER.Debug("Should not have an entry formatted like this, should just contain relative paths.");
                    LOGGER.Debug("Stack trace=[" + Environment.StackTrace + "]");
                }
            }
		}

        /// <summary>
        /// Parse command that must be overridden for subclasses.
        /// </summary>
        /// <param name="line"></param>
        public abstract void Parse(String line);

        /// <summary>
        /// Derive the cvs filename and path for the storage file.
        /// </summary>
        /// <returns>The cvs filename and path.</returns>
        protected abstract String DeriveCvsFullPath();
	}
}
