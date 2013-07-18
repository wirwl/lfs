using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

namespace MemoryReader
{
    /// <summary>
    /// Interaction logic for wSelectProcess.xaml
    /// </summary>
    public partial class wSelectProcess : Window
    {
        public static IntPtr AppHandle;
        public wSelectProcess(Window owner)
        {
            InitializeComponent();
            Owner = owner;                
            UpdateListView();
        }
        public void UpdateListView()
        {                        
            Process[] Processes = Process.GetProcesses();
          //  Array.Sort(Processes, new CompareProcessStartTime());
            var sorted = from p in Processes orderby StartTimeNoException(p) descending, p.Id select p;            
            foreach (Process p in sorted)          
            {
                
                ProcessModule pm;
                string path;
                try { pm = p.MainModule; }
                catch (Exception) { continue; }
                try { path = pm.FileName; }
                catch (Exception) { continue; }
                if (p.ProcessName == "svchost") continue;
                ListViewItem lvi = new ListViewItem() {Tag=p };
                StackPanel sp = new StackPanel() { Width = 64, Height = 64 };
                Image i = new Image() { Source = GetAssociatedIcon(path), Width = 32, Height = 32 };
                TextBlock tb = new TextBlock()
                {
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    MaxWidth = sp.Width,
                    Text = p.ProcessName,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center
                };
                sp.Children.Add(i);
                sp.Children.Add(tb);
                lvi.Content = sp;
                lv.Items.Add(lvi);           
            }
           
        }
        public BitmapSource GetAssociatedIcon(string filePath)
        {

            try
            {          
                BitmapSource bs;          
                using (var assocIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                       assocIcon.Handle,
                       System.Windows.Int32Rect.Empty,
                       System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(32, 32));                    
                    return bs;
                }
            }
            catch (Exception)
            {

                return null;
            }
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bRefresh_Click(object sender, RoutedEventArgs e)
        {
            lv.Items.Clear();
            UpdateListView();
        }
        private static DateTime StartTimeNoException(Process p)
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
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,UInt32 dwProcessId);
        private void bSelect_Click(object sender, RoutedEventArgs e)
        {
       
            try
            {
           //     if (lv.SelectedItem == null) return;
                Process p = (Process)(lv.SelectedItem as ListViewItem).Tag;
                AppHandle = OpenProcess(ProcessAccessFlags.All, true, (uint)p.Id);
                (this.Owner as Window).Title = (string)(this.Owner as Window).Tag + " - " + p.ProcessName;               
                this.
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Error!"); }
        }

        private void lv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            bSelect_Click(sender, e);
        }
       /* public static void LoadData(string filename)
        {
            try
            {
                if (!File.Exists(fn)) return;
                cProject myObject;
                XmlSerializer mySerializer = new XmlSerializer(typeof(cProject), new Type[] { typeof(cProject.EntityDesc) });
                FileStream myFileStream = new FileStream(fn, FileMode.Open);
                myObject = (cProject)mySerializer.Deserialize(myFileStream);
                myObject.currentpath = fn;
                lProjects.Add(myObject);
                curProject = lProjects.Count - 1;
                tviProjects.Tag = new TagDesc(curProject, null, lProjects[curProject].files);
                LoadProjectToTreeView(myObject.files[0] as ArrayList, tviProjects);
                cProject.EntityDesc ed = (cProject.EntityDesc)((lProjects[curProject].files[0] as ArrayList)[0]);
                (tviProjects.Items[curProject] as lfTreeViewItem).Tag = new TagDesc(curProject, ed.path, lProjects[curProject].files[0]);
                myFileStream.Close();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        */

    }
 

}
