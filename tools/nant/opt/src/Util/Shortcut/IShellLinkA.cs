using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Sporadicism.NAntExtras.Util.Shortcut {
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
    public struct WIN32_FIND_DATAA {
        public int dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public int nFileSizeHigh;
        public int nFileSizeLow;
        public int dwReserved0;
        public int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_PATH)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
        public string cAlternateFileName;
        private const int MAX_PATH = 260;
    }

    /// <summary>
    /// From: http://msdn.microsoft.com/library/default.asp?url=/library/en-us/shellcc/platform/shell/reference/ifaces/ishelllink/resolve.asp
    /// </summary>
    [ComImport()]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214EE-0000-0000-C000-000000000046")]
	public interface IShellLinkA {
        void GetPath([Out(), 
            MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            out WIN32_FIND_DATAA pfd,
            GetPathFlag fFlags);

        void GetIDList(out IntPtr ppidl);

        void SetIDList(IntPtr pidl);

        void GetDescription([Out(), 
            MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName,
            int cchMaxName);

        void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

        void GetWorkingDirectory([Out(), 
            MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir,
            int cchMaxPath);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

        void GetArguments([Out(), 
            MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
            int cchMaxPath);

        void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

        void GetHotkey(out short pwHotkey);

        void SetHotkey(short wHotkey);

        void GetShowCmd(out int piShowCmd);

        void SetShowCmd(int iShowCmd);

        void GetIconLocation([Out(), 
            MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
            int cchIconPath,
            out int piIcon);

        void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
            int iIcon);

        void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
            int dwReserved);

        void Resolve(IntPtr hwnd,
            ResolveFlag fFlags);

        void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
	}
}
