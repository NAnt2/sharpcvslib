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
//
//    Author:     Mike Krueger,
//                Clayton Harbour  {claytonharbour@sporadicism.com}
#endregion

using System;
using System.Collections;

using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.FileSystem;

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
    
    private ArrayList dateArgs = new ArrayList();

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

    // TODO: see if there is a better way to handle optional DateTime arguments
    // Note: you can't use null, as DateTime is a value type.
    
    /// <summary>
    /// Adds a date range using exclusive dates.
    /// This is equivalent to the command line option "-d startDate&lt;endDate"
    /// 
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// </summary>
    public void AddExclusiveDateRange(DateTime startDate, DateTime endDate) {
        AddDateRange(true, startDate, true, endDate, "<");
    }
    
    /// <summary>
    /// Adds a open ended date range with an exclusive start date.
    /// This is equivalent to the command line option "-d startDate&lt;"
    /// 
    /// <param name="startDate"></param>
    /// </summary>
    public void AddExclusiveDateStart(DateTime startDate) {
        DateTime dummyDate = new DateTime();
        AddDateRange(true, startDate, false, dummyDate, "<");
    }
    
    /// <summary>
    /// Adds a open ended date range with an exclusive start date.
    /// This is equivalent to the command line option "-d &lt;endDate"
    /// 
    /// <param name="endDate"></param>
    /// </summary>
    public void AddExclusiveDateEnd(DateTime endDate) {
        DateTime dummyDate = new DateTime();
        AddDateRange(false, dummyDate, true, endDate, "<");
    }

    /// <summary>
    /// Adds a date range using inclusive dates.
    /// This is equivalent to the command line option "-d startDate&lt;=endDate"
    /// 
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// </summary>
    public void AddInclusiveDateRange(DateTime startDate, DateTime endDate) {
        AddDateRange(true, startDate, true, endDate, "<=");
    }
    
    /// <summary>
    /// Adds a open ended date range with an inclusive start date.
    /// This is equivalent to the command line option "-d startDate&lt;="
    /// 
    /// <param name="startDate"></param>
    /// </summary>
    public void AddInclusiveDateStart(DateTime startDate) {
        DateTime dummyDate = new DateTime();
        AddDateRange(true, startDate, false, dummyDate, "<");
    }
    
    /// <summary>
    /// Adds a open ended date range with an inclusive start date.
    /// This is equivalent to the command line option "-d &lt;=endDate"
    /// 
    /// <param name="endDate"></param>
    /// </summary>
    public void AddInclusiveDateEnd(DateTime endDate) {
        DateTime dummyDate = new DateTime();
        AddDateRange(false, dummyDate, true, endDate, "<");
    }
    
    /// <summary>
    /// Adds a single date to specify the most recent revision at or prior to this date.
    /// This is equivalent to the command line option "-d date"
    /// 
    /// <param name="date"></param>
    /// </summary>
    public void AddDate(DateTime date) {
        // re-use the code for adding ranges.
        DateTime dummyDate = new DateTime();
        AddDateRange(true, date, false, dummyDate, "");
    }
    
    private void AddDateRange(bool hasStartDate, DateTime startDate, 
                              bool hasEndDate, DateTime endDate, 
                              string separator) {
        string dateArg = "";
	    string dateFormat = "dd MMM yyyy";

        if (hasStartDate || hasEndDate) {
            if (hasStartDate) {
                dateArg += startDate.ToString(dateFormat);
            }
            dateArg += separator;
            if (hasEndDate) {
                dateArg += endDate.ToString(dateFormat);
            }
            dateArgs.Add(dateArg);
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
    public void Execute(ICommandConnection connection)
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
        
        // add any date arguments
        foreach (object o in dateArgs) {
            string dateArg = (string)o;
            connection.SubmitRequest(new ArgumentRequest("-d"));
            connection.SubmitRequest(new ArgumentRequest(dateArg));
        }

        connection.SubmitRequest(new ArgumentRequest(entry.Name));
        connection.SubmitRequest(new LogRequest());
    }
}
}
