#region "Copyright"
//
// Copyright (C) 2003 Steve Kenzell
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
//    <author>Steve Kenzell</author>
//    <author>Clayton Harbour</author>
#endregion
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

using ICSharpCode.SharpCvsLib;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;

using ICSharpCode.SharpCvsLib.Config.Tests;

using log4net;
using NUnit.Framework;

namespace ICSharpCode.SharpCvsLib.Console {

/// <summary>
///     Test the command line args parameters for valid ones
///         and test invalid ones.
/// </summary>
[TestFixture]
public class MainTest {
    private ILog LOGGER = LogManager.GetLogger (typeof (MainTest));
    private TestSettings settings = new TestSettings ();

    private String buildDir;
    /// <summary>
    ///     Constructory for test case.
    /// </summary>
    public MainTest ()
    {
        buildDir = System.AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    ///     Ensure valid help parameter processing.
    ///
    /// </summary>
    [Test]
    public void GenericHelpTest ()
    {
        // Test generic help parameter
        string genericOutput = @"   Usage: cvs [cvs-options] command [command-options-and-arguments]
                               where cvs-options are -q, -n, etc.
                               (specify --help-options for a list of options)
                               where command is add, admin, etc.
                               (specify --help-commands for a list of commands
                               or --help-synonyms for a list of command synonyms)
                               where command-options-and-arguments depend on the specific command
                               (specify -H followed by a command name for command-specific help)
                               Specify --help to receive this message

                               The Concurrent Versions System (CVS) is a tool for version control.
                               For CVS updates and additional information, see
                               the #CvsLib home page at http://sharpcvslib.sourceforge.net/ or
                               the CVS home page at http://www.cvshome.org/ or
                               Pascal Molli's CVS site at http://www.loria.fr/~molli/cvs-index.html
                               the CVSNT home page at http://www.cvsnt.org/

                               Thanks for using the command line tool.
                               ";
        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");

        LOGGER.Debug ("buildDir=[" + buildDir + "]");
        LOGGER.Debug ("filename=[" + filename + "]");

        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename, "--help");
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        Assertion.AssertEquals (genericOutput, output);
    }
    /// <summary>
    ///     Ensure valid option help parameter processing.
    ///
    /// </summary>
    [Test]
    public void OptionHelpTest ()
    {
        // Test option help parameter
        string optionOutput = @"CVS global options (specified before the command name) are:
                              -D prefix       Adds a prefix to CVSROOT.
                              -H              Displays usage information for command.
                              -Q              Cause CVS to be really quiet.
                              -q              Cause CVS to be somewhat quiet.
                              -r              Make checked-out files read-only.
                              -w              Make checked-out files read-write (default).
                              -l              Turn history logging off.
                              -n              Do not execute anything that will change the disk.
                              -t              Show trace of program execution (repeat for more verbosity) -- try with -n.
                              -v              CVS version and copyright.
                              -T tmpdir       Use 'tmpdir' for temporary files.
                              -e editor       Use 'editor' for editing log information.
                              -d CVS_root     Overrides $CVSROOT as the root of the CVS tree.
                              -f              Do not use the ~/.cvsrc file.
                              -z #            Use compression level '#' for net traffic.
                              -x              Encrypt all net traffic (fail if not encrypted).
                              -y              Encrypt all net traffic (if supported by protocol).
                              -a              Authenticate all net traffic.
                              -s VAR=VAL      Set CVS user variable.

                              --version       CVS version and copyright.
                              --encrypt       Encrypt all net traffic (if supported by protocol).
                              --authenticate  Authenticate all net traffic (if supported by protocol).
                              (Specify the --help option for a list of other help options)

                              Thanks for using the command line tool.
                              ";
        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");
        // Add Help option parameter to executable name
        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename, "--help-options");
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        Assertion.AssertEquals (optionOutput, output);

    }		/// <summary>
    ///     Ensure valid commands help parameter processing.
    ///
    /// </summary>
    [Test]
    public void CommandsHelpTest ()
    {
        string commandOutput = @"CVS commands are:
                               add          Add a new file/directory to the repository
                               admin        Administration front end for rcs
                               annotate     Show last revision where each line was modified
                               chacl        Change the Access Control List for a directory
                               checkout     Checkout sources for editing
                               chown        Change the owner of a directory
                               commit       Check files into the repository
                               diff         Show differences between revisions
                               edit         Get ready to edit a watched file
                               editors      See who is editing a watched file
                               export       Export sources from CVS, similar to checkout
                               history      Show repository access history
                               import       Import sources into CVS, using vendor branches
                               init         Create a CVS repository if it doesn't exist
                               info         Display information about supported protocols
                               log          Print out history information for files
                               login        Prompt for password for authenticating server
                               logout       Removes entry in .cvspass for remote repository
                               ls           List files in the repository
                               lsacl        List the directories Access Control List
                               passwd       Set the user's password (Admin: Administer users)
                               authserver   Authentication server mode
                               rannotate    Show last revision where each line of module was modified
                               rdiff        Create 'patch' format diffs between releases
                               release      Indicate that a Module is no longer in use
                               remove       Remove an entry from the repository
                               cvs_rename       Rename a file in the repository
                               rlog         Print out history information for a module
                               rtag         Add a symbolic tag to a module
                               server       Server mode
                               status       Display status information on checked out files
                               tag          Add a symbolic tag to checked out version of files
                               unedit       Undo an edit command
                               update       Bring work tree in sync with repository
                               version      Show current CVS version(s)
                               watch        Set watches
                               watchers     See who is watching a file
                               (Specify the --help option for a list of other help options)

                               Thanks for using the command line tool.
                               ";
        // Test Commands help parameter
        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");

        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename, "--help-commands");
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        Assertion.AssertEquals (commandOutput, output);
    }
    /// <summary>
    ///     Ensure valid synonyms help parameter processing.
    ///
    /// </summary>
    [Test]
    public void SynonymsHelpTest ()
    {
        // Test Synonyms help parameter
        string synonymOutput = @"CVS command synonyms are:
                               add          ad new
                               admin        adm rcs
                               annotate     ann
                               authserver   pserver
                               chacl        setacl setperm
                               checkout     co get
                               chown        setowner
                               commit       ci com
                               diff         di dif
                               export       exp ex
                               history      hi his
                               import       im imp
                               info         inf
                               log          lo
                               login        logon lgn
                               ls           dir list
                               lsacl        lsattr listperm
                               passwd       password setpass
                               rannotate    rann ra
                               rdiff        patch pa
                               release      re rel
                               cvs_rename   ren move
                               remove       rm delete
                               rlog         rl
                               rtag         rt rfreeze
                               status       st cvs_stat
                               tag          ta freeze
                               update       up upd
                               version      ve ver
                               (Specify the --help option for a list of other help options)

                               Thanks for using the command line tool.
                               ";

        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");

        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename, "--help-synonyms");
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        LOGGER.Debug ("synonym output expected=[\n" + synonymOutput + "]");
        LOGGER.Debug ("\n\nsynonym output actual=[\n" + output + "]");
        Assertion.AssertEquals (synonymOutput, output);
    }
    /// <summary>
    ///     Ensure invalid help parameter processing.
    ///
    /// </summary>
    [Test]
    public void BadHelpTest ()
    {
        // Test invalid parameters - Show usage
        string badOutput = @"   Usage: cvs [cvs-options] command [command-options-and-arguments]
                           where cvs-options are -q, -n, etc.
                           (specify --help-options for a list of options)
                           where command is add, admin, etc.
                           (specify --help-commands for a list of commands
                           or --help-synonyms for a list of command synonyms)
                           where command-options-and-arguments depend on the specific command
                           (specify -H followed by a command name for command-specific help)
                           Specify --help to receive this message

                           The Concurrent Versions System (CVS) is a tool for version control.
                           For CVS updates and additional information, see
                           the #CvsLib home page at http://sharpcvslib.sourceforge.net/ or
                           the CVS home page at http://www.cvshome.org/ or
                           Pascal Molli's CVS site at http://www.loria.fr/~molli/cvs-index.html
                           the CVSNT home page at http://www.cvsnt.org/

                           ";
        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");

        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename, "--help-bad");
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        Assertion.AssertEquals (badOutput, output);
    }
    /// <summary>
    ///     Ensure no parameters processing.
    ///
    /// </summary>
    [Test]
    public void NoParamTest ()
    {
        // Test no parameters
        string noOutput = @"   Usage: cvs [cvs-options] command [command-options-and-arguments]
                          where cvs-options are -q, -n, etc.
                          (specify --help-options for a list of options)
                          where command is add, admin, etc.
                          (specify --help-commands for a list of commands
                          or --help-synonyms for a list of command synonyms)
                          where command-options-and-arguments depend on the specific command
                          (specify -H followed by a command name for command-specific help)
                          Specify --help to receive this message

                          The Concurrent Versions System (CVS) is a tool for version control.
                          For CVS updates and additional information, see
                          the #CvsLib home page at http://sharpcvslib.sourceforge.net/ or
                          the CVS home page at http://www.cvshome.org/ or
                          Pascal Molli's CVS site at http://www.loria.fr/~molli/cvs-index.html
                          the CVSNT home page at http://www.cvsnt.org/

                          Thanks for using the command line tool.
                          ";
        // Get Executable name
        String filename = Path.Combine (buildDir, "cvs.exe");

        // Create new process
        ProcessStartInfo cvsProcessInfo = new ProcessStartInfo(filename);
        cvsProcessInfo.UseShellExecute = false;
        cvsProcessInfo.RedirectStandardOutput = true;
        cvsProcessInfo.CreateNoWindow = true;

        // Run the process
        Process cvsProcess = new Process();
        cvsProcess.StartInfo = cvsProcessInfo;
        cvsProcess.Start();
        string output = cvsProcess.StandardOutput.ReadToEnd();
        cvsProcess.WaitForExit();

        // Check the results of process output
        Assertion.AssertEquals (noOutput, output);
    }
}
}
