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
using System.Text;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Misc {

    /// <summary>
    ///     Test CvsRoot parsing.
    /// </summary>
    [TestFixture]
    public class CvsRootTest {

        /// <summary>
        ///     Tests creation of a valid CvsRoot.
        /// </summary>
        [Test]
        public void ValidCvsRootTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:gne@cvs.sourceforge.net:/cvsroot/sharpcvslib");

            Assertion.AssertEquals("ext", cvsRoot.Protocol);
            Assertion.AssertEquals("gne", cvsRoot.User);
            Assertion.AssertEquals("cvs.sourceforge.net", cvsRoot.Host);
            Assertion.AssertEquals("/cvsroot/sharpcvslib", cvsRoot.CvsRepository);
        }

        /// <summary>
        ///     Tests handling of missing protocol.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingProtocolTest () {
            CvsRoot cvsRoot = new CvsRoot ("::gne@cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing user.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingUserTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:@cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing host.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingHostTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:gne@:/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing repository.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingRepositoryTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:gne@cvs.sourceforge.net:");
        }

        /// <summary>
        ///     Tests handling of not starting with a colon.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingFirstColonTest () {
            CvsRoot cvsRoot = new CvsRoot ("a:ext:gne@cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing second colon.
        ///     This tests a known problem in 1.1 which results
        ///     in an ArgumentOutOfRangeException being thrown instead
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingSecondColonTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext-gne@cvs.sourceforge.net-/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing ampersand.
        ///     This tests a known problem in 1.1 which results
        ///     in an ArgumentOutOfRangeException being thrown instead
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingAmpersandTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:gne-cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of missing third colon.
        ///     This tests a known problem in 1.1 which results
        ///     in an ArgumentOutOfRangeException being thrown instead
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void MissingThirdColonTest () {
            CvsRoot cvsRoot = new CvsRoot (":ext:gne@cvs.sourceforge.net-/cvsroot/sharpcvslib");
        }

        /// <summary>
        ///     Tests handling of no colons.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void NoColonTest () {
            CvsRoot cvsRoot = new CvsRoot ("-ext-gne@cvs.sourceforge.net-/cvsroot/sharpcvslib");
        }

        /// <summary>
        /// Tests handling of no username, should be okay if the protocol is 
        ///     not :ext, :ssh, or :pserver.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CvsRootParseException))]
        public void NoUserTestException () {
            CvsRoot cvsRoot = new CvsRoot (":ext:cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }

        /// <summary>
        /// Tests handling of no username, should be okay if the protocol is 
        ///     not :ext, :ssh, or :pserver.
        /// </summary>
        [Test]
        public void NoUserTestNoException () {
            CvsRoot localProtocol = new CvsRoot (":local:cvs.sourceforge.net:/cvsroot/sharpcvslib");
            CvsRoot sspiProtocol = new CvsRoot (":sspi:cvs.sourceforge.net:/cvsroot/sharpcvslib");
        }
    }
}
