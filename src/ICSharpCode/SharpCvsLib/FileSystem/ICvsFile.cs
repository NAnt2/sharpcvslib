#region Copyright
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
//    Author: Clayton Harbour
//     claytonharbour@sporadicism.com
#endregion

using System;
using System.Collections;
using System.IO;
using System.Globalization;

using log4net;

namespace ICSharpCode.SharpCvsLib.FileSystem { 

    /// <summary>
    ///     Interface for all cvs files.  Allows the file system manager to store
    ///         files that contain cvs information such as:
    ///             <code>Entries</code>
    ///             <code>Repository</code>
    ///             <code>Root</code>
    /// </summary>
    public interface ICvsFile {
        /// <summary>
        ///     The name of the file.  This will be a constant for each
        ///         type of file (i.e. Repository, Entry, etc.).
        /// </summary>
        String Filename {get;}
        /// <summary>
        ///     The path to the folder above the CVS directory.  The
        ///         CVS directory will be appended to this path by
        ///         the manager.
        /// </summary>
        String Path {get;}
        
        /// <summary>
        ///     The contents that are going to be written to the file.
        /// </summary>        
        String FileContents {get;}
                
        /// <summary>
        ///     The type of file that this is.
        /// </summary>
        Factory.FileType Type {get;}
                
    }
}
