// project created on 18/07/2003 at 6:10 PM
using System;

class MainClass
{
    private String cvsroot;
    private String command;
    private String options;
    
    public String Command {
        get {return this.command;}
        set {this.command = value;}
    }
    
    public String Cvsroot {
        get {return this.cvsroot;}
        set {this.cvsroot = value;}
    }
    
    public String Options {
        get {return this.options;}
        set {this.options = value;}
    }
    
    public static void Main(String[] args)
    {
        if (args.Length < 1) {
            System.Console.WriteLine (Usage);
        }
        
        MainClass main = new MainClass ();
    
        for (int i = 0; i < args.Length; i++) {
            switch (args[i].Substring (0, 2)) {
                case "checkout":
                case "co": 
                    main.Command = args[i];
                    break;
                case "update":
                    main.command = args[i];
                    break;
                case "--help":
                    main.command = args[i];
                    break;
                case "-d":
                    main.cvsroot = args[i];
                    break;
                default:
                    throw new System.Exception ("not knowon");
            }
        }
        System.Console.WriteLine ("Thanks for using the command line tool.");
        
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
