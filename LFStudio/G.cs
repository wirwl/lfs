using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using AvalonDock;
using ICSharpCode.AvalonEdit;
namespace LFStudio
{
    public static class G
    {
        public static DateTime dtNow;
        public static DateTime dtEndOfMainWindowConstructor;
        public static int OldActiveProject = -1;
        public static int CurrentActiveProject = -1;
        public static double startup_app;
        public static bool isAppServer = false;
        public static Thread ServerThread;
        public static Thread ClientThread;
        public static adce AppSettings;
        public static TextEditor gte;
        public static int CurOffset;
        public static string sbRungamepathtoexe;
        public static bool needCreateProject = false;
        public static MainWindow mainWindow;
        // BitmapSource bsProjects, bsProject, bsFolderFold, bsUnFolderFold;
        public static string folderfold;
        public static string folderunfold;
        public static string project;
        public static string projects;
        public static string anothertype;
        public static BitmapSource bsProjects;
        public static BitmapSource bsProject;
        public static BitmapSource bsFolderFold;
        public static BitmapSource bsUnFolderFold;
        public static BitmapSource bsAnotherType;
        public static void CreateObjects()
        {
            folderfold = @"pack://application:,,,/LFStudio;component/img/folderfold.png";
            //G.mainWindow.folderpath + "\\img\\folderfold.png";
            folderunfold = @"pack://application:,,,/LFStudio;component/img/folderunfold.png";
            //G.mainWindow.folderpath + "\\img\\folderunfold.png";
            project = @"pack://application:,,,/LFStudio;component/img/project.png";
            //G.mainWindow.folderpath + "\\img\\project.png";
            projects = @"pack://application:,,,/LFStudio;component/img/projects.png";
            //G.mainWindow.folderpath + "\\img\\projects.png";

            anothertype = @"pack://application:,,,/LFStudio;component/img/anothertype.png";
            //G.mainWindow.folderpath + "\\img\\anothertype.png";
            bsProjects = myTreeView.lfTreeViewItem.CreateImage(projects);
            bsProject = myTreeView.lfTreeViewItem.CreateImage(project);
            bsFolderFold = myTreeView.lfTreeViewItem.CreateImage(folderfold);
            bsUnFolderFold = myTreeView.lfTreeViewItem.CreateImage(folderunfold);
            bsAnotherType = myTreeView.lfTreeViewItem.CreateImage(anothertype);
        }
        public static bool LineConsist(string[] ls, string obj)
        {
            obj = obj.TrimEnd();
            bool result = false;
            for (int i = 0; i < ls.Length; i++)
            {
                if (ls[i] != "")
                    if (ls[i] == obj) result = true;
            }
            return result;
        }
        public static string[] DeleteEmptyElements(string[] astr)
        {
            string[] rez = new string[GetLengthArrayWithoutEmpty(astr)];
            int k = 0;
            for (int i = 0; i < astr.Length; i++)
            {
                if (astr[i] != "")
                {
                    rez[k] = astr[i];
                    k++;
                }
            }
            return rez;
        }
        public static int GetLengthArrayWithoutEmpty(string[] astr)
        {
            int len = 0;
            for (int i = 0; i < astr.Length; i++)
            {
                if (astr[i] != "") len++;
            }
            return len;
        }
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        UInt32 dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [In, Out] byte[] buffer,
            UInt32 size,
            out IntPtr lpNumberOfBytesRead
            );
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
        public static uint GetAppId(string ExecutablePath)
        {
            string strTargetProcessName = System.IO.Path.GetFileNameWithoutExtension(ExecutablePath);
            Process[] Processes = Process.GetProcessesByName(strTargetProcessName);
            var sorted = from p in Processes orderby StartTimeNoException(p) descending, p.Id select p;
            foreach (Process p in sorted)
            {
                // foreach (ProcessModule m in p.Modules)
                ProcessModule m = p.MainModule;
                {
                    if (ExecutablePath.ToLower() == m.FileName.ToLower())
                    {
                        return (uint)p.Id;
                    }
                }
            }
            return 0;
        }
        public static DateTime StartTimeNoException(Process p)
        {
            try
            {
                return p.StartTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public static uint? ReadValueFromProcess(IntPtr handle, int address, uint size)
        {
            byte[] buffer = new byte[4];
            IntPtr bytes = new IntPtr();
            //buffer=new byte[size];
            if (ReadProcessMemory(handle, new IntPtr(address), buffer, size, out bytes) <= 0)
            {
                //   throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            if (size == 4) return ArrayToDword(buffer);
            if (size == 2) return ArrayToWord(buffer);
            if (size == 1) return ArrayToByte(buffer);
            return null;
        }
        public static uint? WriteValueToProcess(IntPtr handle, int address, byte[] value)
        {
            // byte[] buffer = BitConverter.GetBytes(value);           
            int bytes = new int();
            WriteProcessMemory(handle, new IntPtr(address), value, (uint)value.Length, out bytes);
            return null;
        }
        public static uint ArrayToDword(byte[] b)
        {
            uint result = 0;
            result = result | b[3];
            result = result << 8;
            result = result | b[2];
            result = result << 8;
            result = result | b[1];
            result = result << 8;
            result = result | b[0];
            return result;
        }
        public static uint ArrayToWord(byte[] b)
        {
            uint result = 0;
            result = result | b[1];
            result = result << 8;
            result = result | b[0];
            return result;
        }
        public static uint ArrayToByte(byte[] b)
        {
            return (uint)b[0];
        }
        public static int? ReadDwordValueFromFile(string fname, int offset)
        {
            FileStream fs;
            try
            {
                fs = new FileStream(fname, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                return null;
            }
            BinaryReader br = new BinaryReader(fs);
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            try
            {
                return br.ReadInt32();
            }
            catch
            {
                return null;
            }
        }
        public static int? ReadDwordValueFromFileWithSearchPattern(string fname, int offset = 0)
        {
            //8B B8 A0 00 00 00 81 FF
            return null;
        }
        public static FontStyle GetFontStyleByName(string st)
        {
            switch (st)
            {
                case "Normal": return FontStyles.Normal;
                case "Italic": return FontStyles.Italic;
                case "Oblique": return FontStyles.Oblique;
                default: return FontStyles.Normal;
            }
        }
        public static string GetObjectFromTag(object tag, int n)
        {
            if (tag == null)
                return "";
            string result = "";
            string st = tag.ToString();
            string[] str = st.Split(';');
            if (str.Length > n)
                result = str[n];
            else
                MessageBox.Show("Error in GetObjectFromTag");
            return result;
        }
        public static string GetObjectFromTag(string tag, int n)
        {
            string result = "";
            string[] str = tag.Split(';');
            if (str.Length > n)
                result = str[n];
            else
                MessageBox.Show("Error in GetObjectFromTag");
            return result;
        }

        public static bool areThereDontsavedfiles(DockingManager dm)
        {
            bool result = false;
            for (int i = 0; i < dm.Documents.Count; i++)
            {
                if (!((dm.Documents[i] as DocumentContent).Content is TextEditor)) continue;
                TextEditor te = ((dm.Documents[i] as DocumentContent).Content as TextEditor);
                if (isModified((te.Parent as DocumentContent).Title))
                    return true;
            }

            return result;
        }
        public static bool isModified(string st)
        {
            if (st[st.Length - 1] == '*') return true; else return false;
        }
        public static TextEditor[] GetListOfTextEditors(DockingManager dm)
        {
            List<TextEditor> ltes = new List<TextEditor>(dm.Documents.Count);
            //TextEditor[] tes = new TextEditor[dm.Documents.Count];
            for (int i = 0; i < dm.Documents.Count; i++)
            {
                var te = Utils.AvalonEdit.GetTextEditorFromContent((dm.Documents[i] as DocumentContent).Content);
                if (te != null) ltes.Add(te);
                //tes[i] = te;
            }
            return ltes.ToArray();
        }
        public enum ShowWindowCommands : int
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(int ZeroOnly, string lpWindowName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);

    }
}
                               