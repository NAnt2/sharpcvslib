// project created on 18/07/2003 at 6:10 PM
using System;

class MainClass
{
    public static void Main(string[] args)
    {
        if (args.Length < 1) {
            System.Console.WriteLine (Usage);
        }
    }
    
    private static String Usage {
        get {
            return  
@"   Usage: cvs [cvs-options] command [command-options-and-arguments]
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
        }
    }
}
