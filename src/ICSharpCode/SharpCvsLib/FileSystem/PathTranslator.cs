#region Copyright
// Entry.cs
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
//    Author:     Clayton Harbour
#endregion

using System;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Text;

using log4net;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    ///     Used to parse out the important parts of the orgainization path
    ///         response from the cvs server.
    /// </summary>
    // TODO: Change to internalize helpers (accessor)
    public class PathTranslator {
        /// <summary>
        /// The name of the cvs management folder.
        /// </summary>
        public const String CVS = "CVS";

        private static readonly ILog LOGGER =
            LogManager.GetLogger (typeof (PathTranslator));

        private WorkingDirectory workingDirectory;
        private String repositoryPath;

        private CvsRoot cvsRoot;
        private String relativePath;
        private String filename;
        private String localPath;
        private String localPathAndFilename;
        private String localRootPath;
        private String localModuleFolderName;
        private bool isDirectory;

        /// <summary>
        ///     The cvs root that defines the who, how and where of the server.
        /// </summary>
        public CvsRoot CvsRoot {
            get {return this.cvsRoot;}
        }

        /// <summary>
        /// The relative path to the file.  Usually this is the information that
        ///    has been sent down by the server and can be used along with
        ///    the current working directory information to find the file on the
        ///    local file system.
        /// </summary>
        public String RelativePath {
            get {return this.relativePath;}
        }

        /// <summary>
        /// The name of the file, without any path information.
        /// </summary>
        public String Filename {
            get {return this.filename;}
        }

        /// <summary>
        ///  The path on the local file system.  This is constructed using the
        ///     local working directory and the relative path from the server.
        ///     This does not contain information about the filename.
        /// </summary>
        public String LocalPath {
            get {return this.localPath;}
        }

        /// <summary>
        /// Local path and filename.  Combines the local path and the name
        ///     of the file.
        /// </summary>
        public String LocalPathAndFilename {
            get {return this.localPathAndFilename;}
        }

        /// <summary>
        ///     Determines if path contains a reference to a file or a directory.
        /// </summary>
        public bool IsDirectory {
            get {return this.isDirectory;}
        }

        /// <summary>
        ///     Constructor, parses out the orgainizational path response from
        ///         the cvs server.
        /// </summary>
        /// <param name="workingDirectory">Contains information about the working
        ///     repository.</param>
        /// <param name="repositoryPath">The relative path to the file served
        ///     down from the cvs server.</param>
        public PathTranslator (WorkingDirectory workingDirectory,
                            String repositoryPath) {
            this.repositoryPath = repositoryPath;
            this.workingDirectory = workingDirectory;
            this.localRootPath = workingDirectory.LocalDirectory;
            this.cvsRoot = workingDirectory.CvsRoot;
            this.localModuleFolderName = workingDirectory.WorkingDirectoryName;
            this.filename = this.GetFilename (repositoryPath);
            this.relativePath =
                this.GetRelativePath (workingDirectory.CvsRoot.CvsRepository,
                                    repositoryPath,
                                    workingDirectory.ModuleName);
            LOGGER.Debug ("relativePath=[" + relativePath + "]");

            LOGGER.Debug ("workingDirectory=[" + workingDirectory.ToString () + "]");
            this.localPath =
                PathTranslator.ConvertToOSSpecificPath (Path.Combine (workingDirectory.WorkingPath,
                                                        relativePath));
            if (this.isDirectory) {
                this.localPathAndFilename =
                    PathTranslator.ConvertToOSSpecificPath(this.LocalPath);
            } else {
                this.localPathAndFilename =
                    PathTranslator.ConvertToOSSpecificPath (Path.Combine (this.localPath, this.filename));
            }
        }

        /// <summary>
        ///     Gets the relative path to the file by removing the cvs root directory
        ///         and the module name from the reporitoy path.
        /// </summary>
        /// <param name="cvsRootDirectory">The cvs root directory.</param>
        /// <param name="repositoryPath">The path to the file/ directory as specified
        ///       by the cvs server.</param>
        /// <param name="moduleName">The name of the module which will be parsed
        ///     out so the local directory name can be different than the module
        ///     name.</param>
        /// <returns>The relative path to the filename, excluding the file name.</returns>
        private String GetRelativePath (String cvsRootDirectory,
                                        String repositoryPath,
                                        String moduleName) {
            String filename = this.GetFilename (repositoryPath);
            LOGGER.Debug ("Enter relative path");
            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("cvsRootDirectory=[").Append (cvsRootDirectory).Append ("]");
                msg.Append ("repositoryPath=[").Append (repositoryPath).Append ("]");
                msg.Append ("moduleName=[").Append (moduleName).Append ("]");
                msg.Append ("filename=[").Append (filename).Append ("]");
                LOGGER.Debug (msg);
            }

            String tempRelativePath = repositoryPath;

            LOGGER.Debug ("before cvsroot touched=[" + tempRelativePath + "]");
            // Remove the cvsRoot from the path.
            tempRelativePath =
                tempRelativePath.Substring (cvsRootDirectory.Length);
            LOGGER.Debug ("after cvsroot removed=[" + tempRelativePath + "]");

            if (tempRelativePath.StartsWith ("/")) {
                tempRelativePath = tempRelativePath.Substring (1);
            }

            if (tempRelativePath.Length >= filename.Length &&
                filename.Length > 0 && 
                !filename.Equals(Path.DirectorySeparatorChar.ToString())) {
                tempRelativePath = tempRelativePath.Substring(0,
                                tempRelativePath.Length - filename.Length);
            }
            LOGGER.Debug ("after filename removed=[" + tempRelativePath + "]");

            // at this point module name should be at the start of the string
            if (tempRelativePath.IndexOf (moduleName) > 0 ||
                    tempRelativePath.Length >= moduleName.Length) {
                tempRelativePath =
                    tempRelativePath.Substring (moduleName.Length);
            }
            LOGGER.Debug ("after module name removed=[" +
                        tempRelativePath + "]");

            if (tempRelativePath.StartsWith ("/")) {
                tempRelativePath = tempRelativePath.Substring (1);
            }
            LOGGER.Debug ("after / removed from start=[" +
                        tempRelativePath + "]");

            return tempRelativePath;

        }

        /// <summary>
        ///     Remove the repository name from the beginning of the path so that
        ///         the local path to the file can contain an override directory.
        /// </summary>
        /// <param name="repositoryPath">The relative path to the repository directory
        ///     which is returned in the server response.</param>
        /// <returns>The value of the filename.</returns>
        private String GetFilename (String repositoryPath) {
            String filename = Path.GetFileName (repositoryPath);
            if (repositoryPath.EndsWith("/")) {
                filename = Path.DirectorySeparatorChar.ToString();
                this.isDirectory = true;
            }

            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug ("filename=[" + filename + "]");
            }
            return filename;
        }

        /// <summary>
        ///     Convert this object to a human readable string.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override String ToString () {
            ToStringFormatter formatter = new ToStringFormatter ("PathTranslator");
            formatter.AddProperty ("cvsRoot", cvsRoot);
            formatter.AddProperty ("repositoryPath=[", this.repositoryPath);
            formatter.AddProperty ("relativePath", relativePath);
            formatter.AddProperty ("localModuleFolderName", localModuleFolderName);
            formatter.AddProperty ("filename", filename);
            formatter.AddProperty ("localRootPath", localRootPath);
            formatter.AddProperty ("localPath", localPath);
            formatter.AddProperty ("localPathAndFilename", this.localPathAndFilename);

            return formatter.ToString ();
        }

        /// <summary>
        ///     Convert the back slashes and front slashes in the path string to
        ///         the direction preferred by the os.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The os friendly path string.</returns>
        public static String ConvertToOSSpecificPath (String path) {
            // Make the path match the os' preference
            String osPath = path;
            osPath = osPath.Replace ('/', Path.DirectorySeparatorChar);
            osPath = osPath.Replace ('\\', Path.DirectorySeparatorChar);

            return osPath;
        }

        /// <summary>
        /// Determines if a valid path was entered.  A valid path will not contain
        ///     any cvs folder information.  If the full path contains information 
        ///     about a cvs folder then false is returned.
        /// </summary>
        /// <param name="fullPath">The full path to validate.</param>
        /// <returns>Returns <code>true</code> if the path does NOT contain a CVS
        ///     folder; otherwise returns <code>false</code></returns>
        public static bool ContainsCVS(String fullPath) {
            if (fullPath.ToUpper().IndexOf(CVS) >= 0) {
                if (fullPath.ToUpper().IndexOf(Path.DirectorySeparatorChar + CVS + Path.DirectorySeparatorChar) >= 0) {
                    return true;
                }
                if (IsSeperatorBeforeAndAfterCvs(fullPath)) {
                    return true;
                } 
            }
            return false;
        }

        private static bool IsSeperatorBeforeAndAfterCvs (String fullPath) {
            // Check for case where file is called CVSFoo
            try {
                int startOfCvs = fullPath.ToUpper().LastIndexOf(CVS);
                String cvsString = 
                    fullPath.ToUpper().Substring (startOfCvs - 1, CVS.Length + 2);
                
                if (LOGGER.IsDebugEnabled) {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("fullPath=[").Append(fullPath).Append("]");
                    msg.Append("cvsString=[").Append(cvsString).Append("]");
                    LOGGER.Debug(msg);
                }
                if (cvsString.Equals(
                    Path.DirectorySeparatorChar + CVS + Path.DirectorySeparatorChar)) {
                    return true;
                }
            } catch (ArgumentOutOfRangeException e) {
                LOGGER.Debug(e);
                return true;
            }

            return false;
        }

    }
}
