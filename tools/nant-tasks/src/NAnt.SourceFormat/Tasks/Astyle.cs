using System;
using System.Collections;
using System.IO;

using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Tasks;
using NAnt.Core.Util;

namespace NAnt.SourceFormat.Tasks {
    /// <summary>
    /// Formats source code in a given directory to a specified code
    ///     format.
    /// </summary>
    /// <example>
    ///   <code>
    ///     <![CDATA[
    /// <astyle option-file="/path/to/option/file">
    ///     <sources>
    ///         <includes name="**/**.cs" />
    ///     </sources>
    /// </astyle>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("astyle")]
    public class Astyle : ExecTask {

        #region Private Instance Fields

        /// <summary>The default style seems to be the closest 
        ///     to c# standards.</summary>
        private const String DEFAULT_STYLE = "kr";
        private const String DEFAULT_EXECUTABLE_NAME = "astyle.exe";
        private const String ASTYLE_OPTION_ENV_VAR = "ARTISTIC_STYLE_OPTIONS";

        private String _optionFile;
        private FileSet _sources = new FileSet();
        private Hashtable _fileCopyMap = new Hashtable();

        private OptionCollection _styleOptions = new OptionCollection();

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// The location of the option file.
        /// </summary>
        [TaskAttribute("option-file")]
        public String OptionFile {
            get {return this._optionFile;}
            set {this._optionFile = value;}
        }
        /// <summary>
        /// Used to select the files to copy. To use a <see cref="FileSet" />, 
        /// the <see cref="ToDirectory" /> attribute must be set.
        /// </summary>
        [FileSet("sources")]
        public virtual FileSet Sources {
            get { return _sources; }
            set { _sources = value; }
        }

        /// <summary>
        ///     A collection of options that can be used to override the
        ///         default style options.
        /// </summary>
        [BuildElementCollection("style-options")]
        public OptionCollection StyleOptions {
            get {return _styleOptions;}
            set {this._styleOptions = value;}
        }

        /// <summary>
        /// Gets the command-line arguments for the external program.
        /// </summary>
        /// <value>
        /// The command-line arguments for the external program.
        /// </value>
        public override string ProgramArguments {
            get { return this.CommandLineArguments; }
        }

        #endregion Public Instance Properties

        /// <summary>
        /// Prepare the atyle task to parameters.
        /// </summary>
        /// <param name="process"></param>
        protected override void PrepareProcess(System.Diagnostics.Process process) {
            foreach (String pathname in Sources.FileNames) {
                // input filename:
                CommandLineArguments += pathname + " ";
            }

            base.PrepareProcess(process);
            // If they did not specify a location for the exe assume
            //  it is in the path.
            if (null == this.FileName) {
                this.FileName = DEFAULT_EXECUTABLE_NAME;
            }

            // only set WorkingDirectory if it was explicitly set, otherwise
            // leave default (which is BaseDirectory)
            if (WorkingDirectory != null) {
                process.StartInfo.WorkingDirectory = WorkingDirectory;
            }

            if (this._optionFile != null) {
                process.StartInfo.EnvironmentVariables[ASTYLE_OPTION_ENV_VAR] = this.OptionFile;
            }

            System.Console.WriteLine (CommandLineArguments);
        }
	}
}
