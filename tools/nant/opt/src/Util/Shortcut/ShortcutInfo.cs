using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Sporadicism.NAntExtras.Util.Shortcut {
	/// <summary>
	/// Summary description for ShortcutHelper.
	/// </summary>
	public class ShortcutInfo: IDisposable {
        private const int MAX_PATH = 260;
        private FileInfo _shortCutFileInfo;

        private IShellLinkA _shellLinkA;
        private IShellLinkW _shellLinkW;
        private IPersistFile _persistFile;

        public string FullName {
            get { return this._shortCutFileInfo.FullName; }
        }

        private bool IsWinNt {
            get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
        }

        private bool IsUnicode {
            get { return null != this._shellLinkW; } 
        }

        public FileInfo TargetPath {
            get {
                StringBuilder realPath = new StringBuilder(MAX_PATH, MAX_PATH);

                if (IsUnicode) {
                    WIN32_FIND_DATAW findData = 
                        new WIN32_FIND_DATAW();
                    this._shellLinkW.GetPath(realPath, realPath.Capacity, 
                        out findData, GetPathFlag.SLGP_UNCPRIORITY);
                } else {
                    WIN32_FIND_DATAA findData = 
                        new WIN32_FIND_DATAA();
                    this._shellLinkA.GetPath(realPath, realPath.Capacity, 
                        out findData, GetPathFlag.SLGP_UNCPRIORITY);
                }

                return new FileInfo(realPath.ToString());
            }
            set {
                if (IsUnicode) {
                    this._shellLinkW.SetPath(value.FullName);
                } else {
                    this._shellLinkA.SetPath(value.FullName);
                }
            }
        }

        /// <value>
        /// Working directory of the shortcut.
        /// </value>
        public DirectoryInfo WorkingDirectory {
            get {
                StringBuilder sb = new StringBuilder(MAX_PATH);
                if (IsUnicode) {
                    this._shellLinkW.GetWorkingDirectory(sb, sb.Capacity);
                } else {
                    this._shellLinkA.GetWorkingDirectory(sb, sb.Capacity);
                }
                return new DirectoryInfo(sb.ToString());
            }
            set {
                if (IsUnicode) {
                    this._shellLinkW.SetWorkingDirectory(value.FullName);
                } else {
                    this._shellLinkA.SetWorkingDirectory(value.FullName); 
                }
            }
        }

        public ShortcutInfo(string file) {
            if (IsWinNt) {
                this._shellLinkW = (IShellLinkW)new ShellLink();
            } else {
                this._shellLinkA = (IShellLinkA)new ShellLink();
            }

            if (!Path.IsPathRooted(file)) {
                file = Path.Combine(Directory.GetCurrentDirectory(), 
                    file);
            }

            this._shortCutFileInfo = new FileInfo(file);

            if (this._shortCutFileInfo.Exists) {
                if (IsUnicode) {
                    this._persistFile = (IPersistFile)this._shellLinkW;
                    this._persistFile.Load(this._shortCutFileInfo.FullName, 0 );
                    this._shellLinkW.Resolve(IntPtr.Zero, ResolveFlag.SLR_NO_UI);
                } else {
                    this._persistFile = (IPersistFile)this._shellLinkA;
                    this._persistFile.Load(this._shortCutFileInfo.FullName, 0 );
                    this._shellLinkA.Resolve(IntPtr.Zero, ResolveFlag.SLR_NO_UI);
                }
            }

        }

		public ShortcutInfo(FileInfo file) : this(file.FullName) {
		}

        public void Dispose() {
            if (this._shellLinkA != null) {
                Marshal.ReleaseComObject(this._shellLinkA);
                this._shellLinkA = null;
            }
            if (this._shellLinkW != null) {
                Marshal.ReleaseComObject(this._shellLinkW);
                this._shellLinkW = null;
            }
        }

        public void Save() {
            IPersistFile persistFile;
            if (IsUnicode) {
                persistFile= (IPersistFile) this._shellLinkW;
            } else {
                persistFile = (IPersistFile)this._shellLinkA;
            }
            persistFile.Save(this._shortCutFileInfo.FullName, true);
        }
	}
}
