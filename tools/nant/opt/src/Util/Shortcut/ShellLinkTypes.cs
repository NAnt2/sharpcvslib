using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sporadicism.NAntExtras.Util.Shortcut{
    /// <summary>
    /// See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/shellcc/platform/shell/reference/ifaces/ishelllink/getpath.asp
    /// </summary>
    [Flags()]
        public enum GetPathFlag {
        SLGP_SHORTPATH = 0x1,        // SLGP_SHORTPATH
        SLGP_UNCPRIORITY = 0x2,      // SLGP_UNCPRIORITY
        SLGP_RAWPATH = 0x4           // SLGP_RAWPATH
    }

    /// <summary>
    /// See: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/shellcc/platform/shell/reference/ifaces/ishelllink/resolve.asp
    /// </summary>
    [Flags()]
        public enum ResolveFlag {
        SLR_NO_UI = 0x1,  // SLR_NO_UI
        SLR_ANY_MATCH = 0x2,         // SLR_ANY_MATCH
        SLR_UPDATE = 0x4,           // SLR_UPDATE
        SLR_NOUPDATE = 0x8,         // SLR_NOUPDATE
        SLR_NOSEARCH = 0x10,        // SLR_NOSEARCH
        SLR_NOTRACK = 0x20,         // SLR_NOTRACK
        SLR_NOLINKINFO = 0x40,      // SLR_NOLINKINFO
        SLR_INVOKE_MSI = 0x80        // SLR_INVOKE_MSI
    }
}
