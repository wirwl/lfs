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
using LFStudio;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MemoryReader
{
    /// <summary>
    /// Interaction logic for wMain.xaml
    /// </summary>
    public partial class wMain : Window
    {
        int FontIndex = 0;
        List<string> lsFonts;
        public Host AppObjects;
        DispatcherTimer _timer;
       // IntPtr AppHandle;
        byte[] buffer = new byte[4];
        IntPtr bytes = new IntPtr();       
        ObservableCollection<MemoryField> coll;
        List<MemoryField> lmf = new List<MemoryField>();
        wTextEditor wte;
        public wMain(Host _AppObjects)
        {
            try
            {
                InitializeComponent();
                lmf= MemoryField.LoadData("addresses.txt");
                Tag = this.Title;
                coll = new ObservableCollection<MemoryField>();
               // coll.Add(new MemoryField() { Address = Convert.ToString(0x450BBC, 16), Type = "BattleTime", Comment = "" });
                foreach (MemoryField mf in lmf) coll.Add(mf);
                dgMemory.ItemsSource = coll;
                dgMemory.Items.Refresh();
                dgMemory.CanUserReorderColumns = false;
                AppObjects = _AppObjects;               
                this.Owner = AppObjects.wMain;
                _timer = new DispatcherTimer();
                _timer.Tick += new EventHandler(_timer_Tick);

                lsFonts = new List<string>();
                foreach (FontFamily s in Fonts.SystemFontFamilies)
                lsFonts.Add(s.Source);
                FontFamily fm = new System.Windows.Media.FontFamily(SystemFonts.CaptionFontFamily.Source);
                
                dgMemory.FontFamily = fm;
                dgMemory.UpdateLayout();
                
                Start_Click(null, null);                                
            }
            catch (Exception)
            {
                _timer.Stop();
                throw;
            }
        }
        public uint? ReadValueFromProcess(IntPtr handle, string saddress, uint size)
        {
            if (handle.ToInt32() == 0) return 0;            
            bool isPointer = false;
            if (saddress[0] == '*') { isPointer = true; saddress = saddress.Remove(0, 1); }
            int address = Convert.ToInt32(saddress, 16);
            if (isPointer)
            { 
                ReadProcessMemory(handle, new IntPtr(address), buffer, 4, out bytes);
                address =ArrayToDwordInt(buffer);
            }                       
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
        public int ArrayToDwordInt(byte[] b)
        {
            int result = 0;
            result = result | b[3];
            result = result << 8;
            result = result | b[2];
            result = result << 8;
            result = result | b[1];
            result = result << 8;
            result = result | b[0];
            return result;
        }
        public uint ArrayToDword(byte[] b)
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
        public uint ArrayToWord(byte[] b)
        {
            uint result = 0;
            result = result | b[1];
            result = result << 8;
            result = result | b[0];
            return result;
        }
        public uint ArrayToByte(byte[] b)
        {
            return (uint)b[0];
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

        public IntPtr GetAppHandle(string ExecutablePath)
        {
            string strTargetProcessName = System.IO.Path.GetFileNameWithoutExtension(ExecutablePath);
            Process[] Processes = Process.GetProcessesByName(strTargetProcessName);
            foreach (Process p in Processes)
            {
                foreach (ProcessModule m in p.Modules)
                {
                    if (ExecutablePath.ToLower() == m.FileName.ToLower())
                    {
                        return p.Handle;
                    }
                }
            }
            return IntPtr.Zero;
        }
        public uint GetAppId(string ExecutablePath)
        {
            string strTargetProcessName = System.IO.Path.GetFileNameWithoutExtension(ExecutablePath);
            Process[] Processes = Process.GetProcessesByName(strTargetProcessName);
            foreach (Process p in Processes)
            {
                foreach (ProcessModule m in p.Modules)
                {
                    if (ExecutablePath.ToLower() == m.FileName.ToLower())
                    {
                        return (uint)p.Id;
                    }
                }
            }
            return 0;
        }



        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // Установка интервала
            _timer.Interval = TimeSpan.FromMilliseconds(Convert.ToDouble(time.Text));

            // Запуск таймера
            _timer.Start();

            //status.Text = "Запущен";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // Остановка таймера
            _timer.Stop();

            //status.Text = "Остановлен";
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void time_TextChanged(object sender, TextChangedEventArgs e)
        {

            try
            {
                if (_timer != null)
                    _timer.Interval = TimeSpan.FromMilliseconds(Convert.ToDouble(time.Text));
            }
            catch (Exception)
            {

                _timer.Interval = TimeSpan.FromMilliseconds(500);
                //       time.Text = "500";
            }
        }
        private void _timer_Tick(object s, EventArgs a)
        {
            for (int i = 0; i < coll.Count; i++)
            {
                   coll[i].Value = 
                       ReadValueFromProcess(wSelectProcess.AppHandle, coll[i].Address, 
                                            DetectSize(coll[i].Type)).ToString();
            }
                dgMemory.Columns[5].Width =new DataGridLength(100,DataGridLengthUnitType.Star);
                dgMemory.Items.Refresh();
            
        }
        public string ToBattleTime(int? n)
        { 
            if (n == null) return "error";
            double f = (double)n;            
            f=(int)n/30.0;
            n = (int)Math.Round(f);
            return new TimeSpan(0, 0, (int)n).ToString();                      
        }
        public uint DetectSize(string st)
        {
            switch (st)
            {
                case "dword": return 4;
                case "word": return 2;
                case "byte": return 1;
                default: return 4;
            }
        }
  /*      public string Calculator(string exp)
        { 
            
        }*/
        public static double Evaluate(string expression)
        {
            return (double)new System.Xml.XPath.XPathDocument
            (
             new System.IO.StringReader("<r/>")).CreateNavigator().Evaluate
              (
                string.Format("number({0})",
new System.Text.RegularExpressions.Regex(@"/\*[^*]*\*+(?:[^/*][^*]*\*+)*/")
                    .Replace(expression, " ${1} ")
                    .Replace("/", " div ")
                    .Replace("%", " mod ")
              )
            );
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            wSelectProcess wsp = new wSelectProcess(this);
            wsp.Owner = this;            
            wsp.Show();            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stop_Click(sender, null);
        }

        private void miReload_Click(object sender, RoutedEventArgs e)
        {
            lmf = MemoryField.LoadData("addresses.txt");
            coll.Clear();
            foreach (MemoryField mf in lmf) coll.Add(mf);
            dgMemory.ItemsSource = coll;
            dgMemory.Items.Refresh();
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {
            if (wte == null)
            {
                wte = new wTextEditor();
                wte.Owner = this;
                wte.Show();
            }
            else
            {
                wte.Show();              
            }
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Q)
            {
                FontIndex--;
                if (FontIndex < 0) FontIndex = 0;
                dgMemory.FontFamily = new FontFamily(lsFonts[FontIndex]);
                dgMemory.UpdateLayout();

            }
            if (e.Key == Key.E)
            {
                FontIndex++;
                if (FontIndex > lsFonts.Count - 1) FontIndex = lsFonts.Count - 1;
                dgMemory.FontFamily = new FontFamily(lsFonts[FontIndex]);
                dgMemory.UpdateLayout();
            }
        }
    }

}
