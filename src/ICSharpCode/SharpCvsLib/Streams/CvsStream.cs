#region "Copyright"
// CvsStream.cs
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
using System.Threading;

using log4net;

namespace ICSharpCode.SharpCvsLib { 
	
    /// <summary>
    /// Class for handling streams to the cvs server.
    /// </summary>
	public class CvsStream : Stream
	{
	    private readonly ILog LOGGER = LogManager.GetLogger (typeof (CvsStream));
		Stream baseStream;
		
        /// <summary>
        /// Base stream object.
        /// </summary>
		public Stream BaseStream {
			get {
				return baseStream;
			}
			set {
				baseStream = value;
			}
		}
		
        /// <summary>
        /// Cvs server stream object.
        /// </summary>
        /// <param name="baseStream"></param>
		public CvsStream(Stream baseStream)
		{
			this.baseStream = baseStream;
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanRead {
			get {
				return baseStream.CanRead;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanSeek {
			get {
				return baseStream.CanSeek;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanWrite {
			get {
				return baseStream.CanWrite;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Length {
			get {
				return baseStream.Length;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Position {
			get {
				return baseStream.Position;
			}
			set {
				baseStream.Position = value;
			}
		}
		
		/// <summary>
		/// Flushes the baseInputStream
		/// </summary>
		public override void Flush()
		{
			baseStream.Flush();
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return baseStream.Seek(offset, origin);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void SetLength(long val)
		{
			baseStream.SetLength(val);
		}
		
        /// <summary>
        /// Write the specified byte array to the stream.
        /// </summary>
        /// <param name="array"></param>
		public void Write(byte[] array)
		{
			baseStream.Write(array, 0, array.Length);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void Write(byte[] array, int offset, int count)
		{
			baseStream.Write(array, offset, count);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void WriteByte(byte val)
		{
			baseStream.WriteByte(val);
		}
		
		/// <summary>
		/// Closes the base stream
		/// </summary>
		public override void Close()
		{
			baseStream.Close();
		}
	
			
		/// <summary>
		/// Reads one byte of decompressed data.
		///
		/// The byte is baseInputStream the lower 8 bits of the int.
		/// </summary>
		public override int ReadByte()
		{
			return baseStream.ReadByte();
		}
		
		/// <summary>
		/// Decompresses data into the byte array
		/// </summary>
		/// <param name ="b">
		/// the array to read and decompress data into
		/// </param>
		/// <param name ="off">
		/// the offset indicating where the data should be placed
		/// </param>
		/// <param name ="len">
		/// the number of bytes to decompress
		/// </param>
		public override int Read(byte[] b, int off, int len)
		{
			return baseStream.Read(b, off, len);
		}

        /// <summary>
        /// Read the stream from the cvs server.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
		public int Read(byte[] b)
		{
			return baseStream.Read(b, 0, b.Length);
		}
 
        /// <summary>
        /// Read from the stream until a line termination
        ///     character is reached.
        /// </summary>
        /// <returns></returns>
		private string ReadLineBlock()
		{
			StringBuilder builder = new StringBuilder(1024);
			while (true) {
				int i = ReadByte();
				if (i == '\n' || i == -1) {
					break;
				}
				builder.Append((char)i);
			}
			return builder.ToString();
		}
		
        /// <summary>
        /// Read from the stream until the end of the line.
        /// </summary>
        /// <returns></returns>
		public string ReadLine()
		{
			string line = "";
			int x = 0;
			while (line.Length == 0 && ++x < 10) {
				line = ReadLineBlock();
				if (line.Length == 0) {
					Thread.Sleep(10);
				}
			}
			return line;			
		}
		
        /// <summary>
        /// Read from the stream until the first whitespace
        ///     character is reached.
        /// </summary>
        /// <returns></returns>
		public string ReadToFirstWS()
		{
			StringBuilder builder = new StringBuilder(1024);
			while (true) {
				int i = ReadByte();
				
				builder.Append((char)i);
				if (i == '\n' || i ==' ' || i == -1) {
					break;
				}
			}
			return builder.ToString();
		}
				
        /// <summary>
        /// Read a block of data from the stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
		public void ReadBlock(byte[] buffer, int size)
		{
			for (int i = 0; i < size;) {
				int back = Read(buffer, i, size - i);
				i += back;
				if (i < size) {
					Thread.Sleep(10);
				}
			}
		}
		
        /// <summary>
        /// Send the specified string message to the cvs server.
        /// </summary>
        /// <param name="dataStr"></param>
		public void SendString(string dataStr)
		{
			byte[] buff = System.Text.Encoding.ASCII.GetBytes(dataStr);
			baseStream.Write(buff, 0, buff.Length);
			Flush();
		}
	}
}
