#region "Copyright"
// UncompressedFileHandler.cs
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
using System.IO;
using System.Text;

namespace ICSharpCode.SharpCvsLib.FileHandler { 
	
	/// <summary>
	/// Implements the uncompressed file handler
	/// </summary>
	public class UncompressedFileHandler : IFileHandler
	{
        /// <summary>
        /// Send a text file to the cvs server.
        /// </summary>
        /// <param name="outStream"></param>
        /// <param name="fileName"></param>
	    public virtual void SendTextFile(CvsStream outStream, string fileName)
	    {
	    	// convert operating system linefeeds (\r\n) to UNIX style 
	    	// linefeeds (\n)
	    	string tmpFileName = Path.GetTempFileName();
	    	FileStream tmpFile = File.Create(tmpFileName);
			
			StreamReader fs = File.OpenText(fileName);
			while (true) {
				string line = fs.ReadLine();
				if (line == null) {
					break;
				}
				
				byte[] buf = new byte[line.Length];
				Encoding.ASCII.GetBytes(line.ToCharArray(), 0, line.Length, buf, 0);
				tmpFile.Write(buf, 0, buf.Length);
				tmpFile.WriteByte((byte)'\n');
			}
	    	tmpFile.Close();
			fs.Close();
	    	
	    	// send converted file like a binary file
	    	SendBinaryFile(outStream, tmpFileName);
	    	
	    	// delete temp file
	    	File.Delete(tmpFileName);
	    }
	    
        /// <summary>
        /// Receive a text file from the cvs server.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="fileName"></param>
        /// <param name="length"></param>
	    public virtual void ReceiveTextFile(CvsStream inputStream, string fileName, int length)
	    {
			byte[] buffer = new byte[length];
			
			inputStream.ReadBlock(buffer, length);
			
			// write to a temp file
	    	string tmpFileName = Path.GetTempFileName();
	    	FileStream tmpFile = File.Create(tmpFileName);
			tmpFile.Write(buffer, 0, length);
			tmpFile.Close();
	    	
	    	StreamReader tmpTxtFile = File.OpenText(tmpFileName);
			StreamWriter fs = File.CreateText(fileName);
			while (true) {
				string line = tmpTxtFile.ReadLine();
				if (line == null) {
					break;
				}
				fs.WriteLine(line);
			}
			tmpTxtFile.Close();
	    	fs.Close();
	    	
			// delete temp file
	    	File.Delete(tmpFileName);
	    }
	    
		/// <summary>
		/// Send a binary file to the cvs server.
		/// </summary>
		/// <param name="outStream"></param>
		/// <param name="fileName"></param>
	    public virtual void SendBinaryFile(CvsStream outStream, string fileName)
	    {
			FileStream fs = File.OpenRead(fileName);
			byte[] data = new byte[fs.Length];
			fs.Read(data, 0, data.Length);
			fs.Close();
	    	
	    	outStream.SendString(data.Length.ToString() + "\n");
	    	outStream.Write(data);
	    }
	    
        /// <summary>
        /// Receive a binary file from the cvs server.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="fileName"></param>
        /// <param name="length"></param>
	    public virtual void ReceiveBinaryFile(CvsStream inputStream, string fileName, int length)
	    {
			byte[] buffer = new byte[length];
			
			inputStream.ReadBlock(buffer, length);
			
			FileStream fs = System.IO.File.Create(fileName);
			fs.Write(buffer, 0, length);
			fs.Close();
	    }
	}
}

