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
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Requests {

    /// <summary>
    ///     Test any remaining Requests that do not have their own test fixture.
    /// 
    ///     Many of the requests are so trivial, it seems madness to add 
    ///     add a test class for each file. 
    /// 
    ///     cvsclient.info from the 1.11.6 release of cvs (http://ccvs.cvshome.org)
    ///     was used as the basis for these tests.
    /// </summary>
    [TestFixture]
    public class OtherRequestsTest {
        private static readonly ILog LOGGER =
            LogManager.GetLogger (typeof (OtherRequestsTest));
        
        
        /// <summary>
        ///     Tests the 'add' request.
        /// </summary>
        [Test]
        public void AddRequestTest() 
        {
            IRequest request = new AddRequest();
            
            MyAssertStringEquals(request.RequestString, "add");
            Assertion.Assert(request.IsResponseExpected);
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'annotate' request.
        /// </summary>
        [Test]
        public void AnnotateRequestTest() 
        {
            IRequest request = new AnnotateRequest();
            
            MyAssertStringEquals(request.RequestString, "annotate");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Argument' request.
        /// </summary>
        [Test]
        public void ArgumentRequestTest() 
        {
            IRequest request = new ArgumentRequest("arg-data");
            
            MyAssertStringEquals(request.RequestString, "Argument", "arg-data");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
            Assertion.AssertEquals(ArgumentRequest.Options.REVISION, "-r");
            Assertion.AssertEquals(ArgumentRequest.Options.MODULE_NAME, "-N");
            Assertion.AssertEquals(ArgumentRequest.Options.OVERRIDE_DIRECTORY, "-d");
        }
        
        /// <summary>
        ///     Tests the 'Argumentx' request.
        /// </summary>
        [Test]
        public void ArgumentxRequestTest() 
        {
            IRequest request = new ArgumentxRequest("arg-data");
            
            MyAssertStringEquals(request.RequestString, "Argumentx", "arg-data");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'case' request.
        /// </summary>
        [Test]
        public void CaseRequestTest() 
        {
            IRequest request = new CaseRequest();
            
            MyAssertStringEquals(request.RequestString, "Case");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'co' request.
        /// </summary>
        [Test]
        public void CheckoutRequestTest() 
        {
            IRequest request = new CheckoutRequest();
            
            MyAssertStringEquals(request.RequestString, "co");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'ci' request.
        /// </summary>
        [Test]
        public void CommitRequestTest() 
        {
            IRequest request = new CommitRequest();
            
            MyAssertStringEquals(request.RequestString, "ci");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'diff' request.
        /// </summary>
        [Test]
        public void DiffRequestTest() 
        {
            IRequest request = new DiffRequest();
            
            MyAssertStringEquals(request.RequestString, "diff");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Directory' request.
        /// </summary>
        [Test]
        public void DirectoryRequestTest() 
        {
            IRequest request = new DirectoryRequest(".", "/cvsroot/myrepos");
            
            MyAssertStringEquals(request.RequestString, "Directory", ".", "/cvsroot/myrepos");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Entry' request.
        /// </summary>
        [Test]
        public void EntryRequestTest() 
        {
            string entryLine = "/EntryRequest.cs/1.3/Thu Jun 12 06:14:16 2003//";
            Entry entry = new Entry("sharpcvslib-tests/", entryLine);
            IRequest request = new EntryRequest(entry);
            
            MyAssertStringEquals(request.RequestString, "Entry", entryLine);
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'expand-modules' request.
        /// </summary>
        [Test]
        public void ExpandModulesRequestTest() 
        {
            IRequest request = new ExpandModulesRequest();
            
            MyAssertStringEquals(request.RequestString, "expand-modules");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'export' request.
        /// </summary>
        [Test]
        public void ExportRequestTest() 
        {
            IRequest request = new ExportRequest();
            
            MyAssertStringEquals(request.RequestString, "export");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Global_option' request.
        ///     Note: it is '_' and not '-'.
        /// </summary>
        [Test]
        public void GlobalOptionRequestTest() 
        {
            IRequest request = new GlobalOptionRequest("-q");
            
            MyAssertStringEquals(request.RequestString, "Global_option", "-q");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'gzip-file-contents' request.
        /// </summary>
        [Test]
        public void GzipFileContentsRequestTest() 
        {
            IRequest request = new GzipFileContents(3);
            
            MyAssertStringEquals(request.RequestString, "gzip-file-contents", "3");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Gzip-stream' request.
        /// </summary>
        [Test]
        public void GzipStreamRequestTest() 
        {
            IRequest request = new GzipStreamRequest(3);
            
            MyAssertStringEquals(request.RequestString, "Gzip-stream", "3");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(request.DoesModifyConnection);    // Connection should be modified
        }
        
        /// <summary>
        ///     Tests the 'import' request.
        /// </summary>
        [Test]
        public void ImportRequestTest() 
        {
            IRequest request = new ImportRequest();
            
            MyAssertStringEquals(request.RequestString, "import");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'init' request.
        /// </summary>
        [Test]
        public void InitRequestTest() 
        {
            IRequest request = new InitRequest("/sharpcvslib");
            
            MyAssertStringEquals(request.RequestString, "init", "/sharpcvslib");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'log' request.
        /// </summary>
        [Test]
        public void LogRequestTest() 
        {
            IRequest request = new LogRequest();
            
            MyAssertStringEquals(request.RequestString, "log");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Modified' request.
        /// </summary>
        [Test]
        public void ModifiedRequestTest() 
        {
            IRequest request = new ModifiedRequest("test.cs");

            // TODO: ModifiedRequest does not allow you to specify the mode!            
            MyAssertStringEquals(request.RequestString, "Modified", "test.cs", "u=rw,g=rw,o=rw");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        // TODO: PServerAuthRequest to be tested elsewhere

        
        /// <summary>
        ///     Tests the 'Questionable' request.
        /// </summary>
        [Test]
        public void QuestionableRequestTest() 
        {
            IRequest request = new QuestionableRequest("test.cs");
            
            MyAssertStringEquals(request.RequestString, "Questionable", "test.cs");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'rdiff' request.
        /// </summary>
        [Test]
        public void RDiffRequestTest() 
        {
            IRequest request = new RDiffRequest();
            
            MyAssertStringEquals(request.RequestString, "rdiff");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'remove' request.
        /// </summary>
        [Test]
        public void RemoveRequestTest() 
        {
            IRequest request = new RemoveRequest();
            
            MyAssertStringEquals(request.RequestString, "remove");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }

        // TODO: RequestMethodEventArgs to be tested elsewhere
        // TODO: RequestMethodEventHandle to be tested elsewhere
        
        /// <summary>
        ///     Tests the 'Root' request.
        /// </summary>
        [Test]
        public void RootRequestTest() 
        {
            IRequest request = new RootRequest("/cvsroot/sharpcvslib");
            
            MyAssertStringEquals(request.RequestString, "Root", "/cvsroot/sharpcvslib");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'status' request.
        /// </summary>
        [Test]
        public void StatusRequestTest() 
        {
            IRequest request = new StatusRequest();
            
            MyAssertStringEquals(request.RequestString, "status");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Sticky' request.
        /// </summary>
        [Test]
        public void StickyRequestTest() 
        {
            IRequest request = new StickyRequest("TV1.1");
            
            MyAssertStringEquals(request.RequestString, "Sticky", "TV1.1");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'tag' request.
        /// </summary>
        [Test]
        public void TagRequestTest() 
        {
            IRequest request = new TagRequest();
            
            MyAssertStringEquals(request.RequestString, "tag");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'Unchanged' request.
        /// </summary>
        [Test]
        public void UnchangedRequestTest() 
        {
            IRequest request = new UnchangedRequest("test.cs");
            
            MyAssertStringEquals(request.RequestString, "Unchanged", "test.cs");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'update' request.
        /// </summary>
        [Test]
        public void UpdateRequestTest() 
        {
            IRequest request = new UpdateRequest();
            
            MyAssertStringEquals(request.RequestString, "update");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        // TODO: UploadFileRequest to be tested elsewhere
        
        /// <summary>
        ///     Tests the 'UseUnchanged' request.
        /// </summary>
        [Test]
        public void UseUnchangedRequestTest() 
        {
            IRequest request = new UseUnchangedRequest();
            
            MyAssertStringEquals(request.RequestString, "UseUnchanged");
            Assertion.Assert(!request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }
        
        /// <summary>
        ///     Tests the 'valid-requests' request.
        /// </summary>
        [Test]
        public void ValidRequestsRequestTest() 
        {
            IRequest request = new ValidRequestsRequest();
            
            MyAssertStringEquals(request.RequestString, "valid-requests");
            Assertion.Assert(request.IsResponseExpected);
            
            Assertion.Assert(!request.DoesModifyConnection);
        }

        // Valid-responses tested in ValidResponsesRequestTest

        
        /// <summary>
        ///     This asserts that the request string has a single command of the given value.
        ///     
        ///     In this function and the over-ridden versions of it,
        ///     we assume that a single space is used to seperate terms
        ///     and there is no other extraneous white space.
        /// 
        ///     It is possible that we could 'fail' a request string that is
        ///     actually accepted by a cvs server.
        /// </summary>
        private void MyAssertStringEquals(string requestString, string command)
        {
            string expectedRequest = String.Format("{0}\n", command);
            
            Assertion.AssertEquals(requestString, expectedRequest);
        }
        
        /// <summary>
        ///     This asserts that the request string has a command of the given value,
        ///     and a single argument of value
        /// </summary>
        private void MyAssertStringEquals(string requestString, string command, string data)
        {
            string expectedRequest = String.Format("{0} {1}\n", command, data);
            
            Assertion.AssertEquals(requestString, expectedRequest);
        }
        
        /// <summary>
        ///     This asserts that the request string has a command of the given value,
        ///     and a single argument of value
        ///     Trailing whitespace is ignored.
        /// </summary>
        private void MyAssertStringEquals(string requestString, string command, string data, string nextLine)
        {
            string expectedRequest = String.Format("{0} {1}\n{2}\n", command, data, nextLine);
            
            Assertion.AssertEquals(requestString, expectedRequest);
        }
    }
}
