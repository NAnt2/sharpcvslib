#region "Copyright"
// Copyright (C) 2003 Gerald Evans
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
//    <author>Gerald Evans</author>
#endregion

using System;
using System.Collections;
using System.IO;
using System.Threading;

using ICSharpCode.SharpCvsLib;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Streams {

/// <summary>
///     Test the CvsStream class.
///
///     To perform these tests, we provide ourselve as the base stream.
///     That way we can check that CvsStream is passing the correct
///     arguments to the BaseStream and is returning the correct replies.
/// </summary>
[TestFixture]
public class CvsStreamTest : Stream {
    private static readonly ILog LOGGER =
        LogManager.GetLogger (typeof (CvsStreamTest));

    CvsStream cvsStream;

    private bool replyForCanRead = false;
    private bool replyForCanSeek = false;
    private bool replyForCanWrite = false;
    private int replyForLength = 0;

    // Following used for Position
    private long replyForPosition = 0;
    private long valueParamFromPosition = 0;

    private bool flushCalled = false;

    // Following used to test Seek
    private long offsetParamFromSeek = 0;
    private SeekOrigin originParamFromSeek = SeekOrigin.Begin;
    private long replyForSeek = 0;

    private long lenParamFromSetLength = 0;

    // Following used to test Write
    private byte[] dataParamFromWrite;
    private int offsetParamFromWrite = 0;
    private int countParamFromWrite = 0;

    private byte byteParamFromWriteByte = 0;
    private bool closeCalled = false;
    private byte replyForReadByte = 0;

    // Following used to test Read
    private byte[] dataParamFromRead;
    private int offsetParamFromRead = 0;
    private int countParamFromRead = 0;
    private int replyFromRead = 0;

    private const String TEST_DATA = "Hello World!\nGoodbye\n";


    /// <summary>
    ///     Creates the test object.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        cvsStream = new CvsStream(this);
        cvsStream.BaseStream = this;
    }

    /// <summary>
    ///     Tidies up.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to get the CanRead value.
    /// </summary>
    [Test]
    public void CanReadTest ()
    {
        replyForCanRead = true;
        Assertion.Assert(cvsStream.CanRead == replyForCanRead);
        replyForCanRead = false;
        Assertion.Assert(cvsStream.CanRead == replyForCanRead);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.CanRead.
    /// </summary>
    public override bool CanRead
    {
        get {
            return replyForCanRead;
        }
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to get the CanSeek value.
    /// </summary>
    [Test]
    public void CanSeekTest ()
    {
        replyForCanSeek = true;
        Assertion.Assert(cvsStream.CanSeek == replyForCanSeek);
        replyForCanSeek = false;
        Assertion.Assert(cvsStream.CanSeek == replyForCanSeek);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.CanSeek.
    /// </summary>
    public override bool CanSeek
    {
        get {
            return replyForCanSeek;
        }
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to get the CanWrite value.
    /// </summary>
    [Test]
    public void CanWriteTest ()
    {
        replyForCanWrite = true;
        Assertion.Assert(cvsStream.CanWrite == replyForCanWrite);
        replyForCanWrite = false;
        Assertion.Assert(cvsStream.CanWrite == replyForCanWrite);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.CanWrite.
    /// </summary>
    public override bool CanWrite
    {
        get {
            return replyForCanWrite;
        }
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to get the Length value.
    /// </summary>
    [Test]
    public void LengthTest ()
    {
        replyForLength = 0;
        Assertion.Assert(cvsStream.Length == replyForLength);
        replyForLength = 999;
        Assertion.Assert(cvsStream.Length == replyForLength);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.Length.
    /// </summary>
    public override long Length
    {
        get {
            return replyForLength;
        }
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to get the Position value.
    /// </summary>
    [Test]
    public void PositionTest ()
    {
        // First test get
        replyForPosition = 0;
        Assertion.Assert(cvsStream.Position == replyForPosition);
        replyForPosition = 999;
        Assertion.Assert(cvsStream.Position == replyForPosition);

        // now test set
        cvsStream.Position = 111;
        Assertion.Assert(valueParamFromPosition == 111);
        cvsStream.Position = 222;
        Assertion.Assert(valueParamFromPosition == 222);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.Position.
    /// </summary>
    public override long Position
    {
        get {
            return replyForPosition;
        }
        set {
            valueParamFromPosition = value;
        }
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a Flush.
    /// </summary>
    [Test]
    public void FlushTest ()
    {
        flushCalled = false;
        cvsStream.Flush();
        Assertion.Assert(flushCalled);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.Flush.
    /// </summary>
    public override void Flush()
    {
        flushCalled = true;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a Seek.
    /// </summary>
    [Test]
    public void SeekTest ()
    {
        replyForSeek = 666;
        Assertion.Assert(cvsStream.Seek(999, SeekOrigin.Begin) == replyForSeek);
        Assertion.Assert(offsetParamFromSeek == 999 && originParamFromSeek == SeekOrigin.Begin);
        replyForSeek = 777;
        Assertion.Assert(cvsStream.Seek(0, SeekOrigin.End) == replyForSeek);
        Assertion.Assert(offsetParamFromSeek == 0 && originParamFromSeek == SeekOrigin.End);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.Seek.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin)
    {
        offsetParamFromSeek = offset;
        originParamFromSeek = origin;

        return replyForSeek;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a SetLength.
    /// </summary>
    [Test]
    public void SetLengthTest ()
    {
        cvsStream.SetLength(111);
        Assertion.Assert(lenParamFromSetLength == 111);
        cvsStream.SetLength(222);
        Assertion.Assert(lenParamFromSetLength == 222);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.SetLength.
    /// </summary>
    public override void SetLength(long len)
    {
        lenParamFromSetLength = len;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a Write.
    ///     This tests both of the overloaded versions of Write.
    /// </summary>
    [Test]
    public void WriteTest()
    {
        byte[] array = { 1, 2, 3, 4, 5 };

        // First test Write(byte[], int, int)
        cvsStream.Write(array, 1, 3);
        Assertion.Assert(dataParamFromWrite == array &&
                         offsetParamFromWrite == 1 &&
                         countParamFromWrite == 3);

        // Now test Write(byte[])
        dataParamFromWrite = null;
        cvsStream.Write(array);
        Assertion.Assert(dataParamFromWrite == array &&
                         offsetParamFromWrite == 0 &&
                         countParamFromWrite == array.Length);
    }

    /// <summary>
    ///     Tests SendString().
    /// </summary>
    [Test]
    public void SendStringTest()
    {
        String str = "Hello World!";
        cvsStream.SendString(str);

        Assertion.Assert(dataParamFromWrite.Length == str.Length);
        Assertion.Assert(countParamFromWrite == str.Length);
        Assertion.Assert(offsetParamFromWrite == 0);
        for (int n = 0; n < str.Length; n++) {
            Assertion.Assert(str[n] == (char)dataParamFromWrite[n]);
        }
    }

    /// <summary>
    ///     Our 'Base' implementation for Stream.Write.
    /// </summary>
    public override void Write(byte[] data, int offset, int count)
    {
        dataParamFromWrite = data;
        offsetParamFromWrite = offset;
        countParamFromWrite = count;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a WriteByte.
    /// </summary>
    [Test]
    public void WriteByteTest ()
    {
        cvsStream.WriteByte(42);
        Assertion.Assert(byteParamFromWriteByte == 42);
        cvsStream.WriteByte(222);
        Assertion.Assert(byteParamFromWriteByte == 222);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.WriteByte.
    /// </summary>
    public override void WriteByte(Byte val)
    {
        byteParamFromWriteByte = val;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a Close.
    /// </summary>
    [Test]
    public void CloseTest ()
    {
        closeCalled = false;
        cvsStream.Close();
        Assertion.Assert(closeCalled);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.Close.
    /// </summary>
    public override void Close()
    {
        closeCalled = true;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a ReadByte.
    /// </summary>
    [Test]
    public void ReadByteTest ()
    {
        replyForReadByte = 42;
        Assertion.Assert(cvsStream.ReadByte() == 42);
        replyForReadByte = 222;
        Assertion.Assert(cvsStream.ReadByte() == 222);
    }
    /// <summary>
    ///     Our 'Base' implementation for Stream.ReadByte.
    /// </summary>
    public override int ReadByte()
    {
        return replyForReadByte;
    }

    /// <summary>
    ///     Ensure that cvsStream calls the embedded stream to perform a Read.
    ///     This tests both of the overloaded versions of Read.
    /// </summary>
    [Test]
    public void ReadTest ()
    {
        byte[] array = { 1, 2, 3, 4, 5 };

        // First test Read(byte[], int, int)
        replyFromRead = 2;
        Assertion.Assert(cvsStream.Read(array, 1, 3) == replyFromRead);
        Assertion.Assert(dataParamFromRead == array &&
                         offsetParamFromRead == 1 &&
                         countParamFromRead == 3);

        // Now test Read(byte[])
        dataParamFromRead = null;
        replyFromRead = 4;
        Assertion.Assert(cvsStream.Read(array) == replyFromRead);
        Assertion.Assert(dataParamFromRead == array &&
                         offsetParamFromRead == 0 &&
                         countParamFromRead == array.Length);
    }

    /// <summary>
    ///     Our 'Base' implementation for Stream.Read.
    /// </summary>
    public override int Read(byte[] data, int offset, int count)
    {
        dataParamFromRead = data;
        offsetParamFromRead = offset;
        countParamFromRead = count;

        return replyFromRead;
    }

    /// <summary>
    ///     Tests ReadBlock.
    /// </summary>
    [Test]
    public void ReadBlockTest ()
    {
        // Use a MemoryStream as the underlying stream
        MemoryStream memoryStream = new MemoryStream();
        cvsStream.BaseStream = memoryStream;

        byte[] buff = System.Text.Encoding.ASCII.GetBytes(TEST_DATA);
        cvsStream.BaseStream.Write(buff, 0, buff.Length);
        cvsStream.BaseStream.Flush();
        cvsStream.BaseStream.Position = 0;

        // Now read the stream.
        byte[] buffer = new Byte[TEST_DATA.Length];
        cvsStream.ReadBlock(buffer, buffer.Length);

        // and check we have received what we expected.
        for (int n = 0; n < TEST_DATA.Length; n++)
        {
            Assertion.Assert(TEST_DATA[n] == (char)buffer[n]);
        }

        cvsStream.BaseStream.Close();
    }

    /// <summary>
    ///     Tests ReadLine.
    /// </summary>
    [Test]
    public void ReadLineTest ()
    {
        string line;
        // Use a MemoryStream as the underlying stream
        MemoryStream memoryStream = new MemoryStream();
        cvsStream.BaseStream = memoryStream;

        byte[] buff = System.Text.Encoding.ASCII.GetBytes(TEST_DATA);
        cvsStream.BaseStream.Write(buff, 0, buff.Length);
        cvsStream.BaseStream.Flush();
        cvsStream.BaseStream.Position = 0;

        // Now read the stream.
        line = cvsStream.ReadLine();
        Assertion.Assert(line.Equals("Hello World!"));
        line = cvsStream.ReadLine();
        Assertion.Assert(line.Equals("Goodbye"));

        cvsStream.BaseStream.Close();
    }

    /// <summary>
    ///     Tests ReadToFirstWS.
    /// </summary>
    [Test]
    public void ReadToFirstWSTest ()
    {
        string word;
        // Use a MemoryStream as the underlying stream
        MemoryStream memoryStream = new MemoryStream();
        cvsStream.BaseStream = memoryStream;

        byte[] buff = System.Text.Encoding.ASCII.GetBytes(TEST_DATA);
        cvsStream.BaseStream.Write(buff, 0, buff.Length);
        cvsStream.BaseStream.Flush();
        cvsStream.BaseStream.Position = 0;

        // Now read the stream.
        word = cvsStream.ReadToFirstWS();
        Assertion.Assert(word.Equals("Hello "));
        word = cvsStream.ReadToFirstWS();
        Assertion.Assert(word.Equals("World!\n"));
        word = cvsStream.ReadToFirstWS();
        Assertion.Assert(word.Equals("Goodbye\n"));

        cvsStream.BaseStream.Close();
    }
}
}
