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
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;

namespace PassDecrypter
{
    //"\"\r\n %&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
    public partial class wMain : Window
    {
        BackgroundWorker bwDecryptFile;
        string Chars = "\t\"\r\n %&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~";
        string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";        
        int iterations = 256;
        int Shift = 12;
        int min_pass_length = 30;
        int max_pass_length = 40;
        int TextOffset = 123;
        int MBV = 15;
       // static int PassSize = "SiuHungIsAGoodBearBecauseHeIsVeryGood".Length - 1;
        byte[] Pass;
        //= new byte[PassSize];
        List<byte[]> CryptedText = new List<byte[]>();
        MemoryStream msCT;
        Stopwatch swDecrypting=new Stopwatch();
        public wMain(Host _AppObjects)
        {
            InitializeComponent();
        }
        int CreateMatrix(int Iter, int PassLength)
        {
            #region Fill Matrix CryptedText
            CryptedText.Clear();
            //srCT.Read(new byte[TextOffset], 0, TextOffset);
            msCT.Read(new byte[TextOffset], 0, TextOffset);
            int count = PassLength;
            int cc = 0;
            //iterations = 100;
            while (count == PassLength)
            {
                cc++;
                if (iterations > 0)
                    if (cc > iterations) break;
                byte[] c = new byte[PassLength];
                //count = srCT.Read(c, 0, PassSize);
                count = msCT.Read(c, 0, PassLength);
                if (count == PassLength) CryptedText.Add(c);
            }
            #endregion
            return cc;
        }
        byte DetectPassLetter(int p, ref int bv)
        {
            int goodvalue = 0;
            int badvalue = 0;
            int min_bad_value = CryptedText.Count;
            byte pos_bv = 0;
            for (byte i = 0; i < 255; i++)
            {
                if (i == 101)
                {

                }
                //foreach (byte[] part in CryptedText)
                for (int j = 0; j < CryptedText.Count; j++)
                {
                    char dc;
                    //if (i < part[0])
                    //if (true)
                    if (i < CryptedText[j][p])
                    {
                        //dc = (char)(part[0] - i);
                        dc = (char)(CryptedText[j][p] - i);
                        if (Chars.Contains(dc))
                        {
                            goodvalue++;
                        }
                        else
                        {
                            badvalue++;
                        }
                    }
                    else { badvalue++; continue; }
                }
                if (badvalue < min_bad_value) { min_bad_value = badvalue; pos_bv = i; }

                // Console.WriteLine("i=" + i.ToString() + " goodvalue=" + goodvalue.ToString() + " badvalue=" + badvalue.ToString());

                goodvalue = 0;
                badvalue = 0;
            }
            //Console.WriteLine("----------------------------------------------------------------------------------------------");
            bv = min_bad_value;
            return pos_bv;
        }
        private void bCDF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { CheckFileExists = true, CheckPathExists = true, ReadOnlyChecked = true };
            ofd.DefaultExt = ".dat";
            ofd.Filter = "*.dat|*.dat";
            if (ofd.ShowDialog() == true)
            {
                Stopwatch sw = new Stopwatch();
                if (msCT != null)
                    msCT.Close();
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                {
                    byte[] b = new byte[fs.Length];
                    fs.Read(b, 0, (int)fs.Length);
                    msCT = new MemoryStream(b);
                }
                lSF.Content = "Selected file: " + System.IO.Path.GetFileName(ofd.FileName);
            }
        }        
        public void bwDecryptFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            swDecrypting.Stop();
            lLength.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    bSD.Content = "Start decrypting";
                    teLog.AppendText("Elapsed time: "+swDecrypting.Elapsed.TotalSeconds.ToString()+" seconds");
                }));            
        }
        private void LoadParameters()
        {
            string pathtofile = System.IO.Path.GetDirectoryName
                (System.Reflection.Assembly.GetExecutingAssembly().Location)+ "\\config.txt";
            if (!File.Exists(pathtofile))
            {
                using (StreamWriter sw = new StreamWriter(pathtofile))
                {
                    sw.WriteLine("ABC=" + ABC);
                    sw.WriteLine("IC=" + iterations.ToString());
                    sw.WriteLine("Shift=" + Shift.ToString());
                    sw.WriteLine("SPL=" + min_pass_length.ToString());
                    sw.WriteLine("EPL=" + max_pass_length.ToString());
                    sw.WriteLine("TextOffset=" + TextOffset.ToString());
                    sw.WriteLine("MBV=" + MBV.ToString());
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(pathtofile))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim().Length == 0) continue;
                        string[] vars = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (vars.Length > 1)
                        {
                            switch (vars[0])
                            {
                                case "ABC": ABC = vars[1]; break;
                                case "IC": int.TryParse(vars[1], out iterations); break;
                                case "Shift": int.TryParse(vars[1], out Shift); break;
                                case "SPL": int.TryParse(vars[1], out min_pass_length); break;
                                case "EPL": int.TryParse(vars[1], out max_pass_length); break;
                                case "TextOffset": int.TryParse(vars[1], out TextOffset); break;
                                case "MBV": int.TryParse(vars[1], out MBV); break;
                            }
                        }
                    }
                }
            }
        }
        public void bwDecryptFile_DoWork(object sender, DoWorkEventArgs e)
        {
            //System.Diagnostics.Process.GetCurrentProcess().PriorityClass = 
            //System.Diagnostics.ProcessPriorityClass.RealTime;
            swDecrypting.Restart();
            bool? isCAL =(bool?) e.Argument;     
            Chars += ABC;
            LoadParameters();
            int old_sbv = -1;
          
            System.Threading.Tasks.Parallel.For(min_pass_length, max_pass_length + 1, p =>
          //for (int p = min_pass_length; p <= max_pass_length; p++)
          {
              bwDecryptFile.ReportProgress(p);
              int bv = 0;
              int sum_bv = 0;
              msCT.Position = 0;
              Pass = new byte[p];
              int k = CreateMatrix(iterations, p);
              for (int i = 0; i < p; i++)
              {
                  int index = i + Shift;
                  if (index >= p)
                      index = index - p;
                  Pass[index] = DetectPassLetter(i, ref bv);
                  sum_bv += bv;
                  if (bwDecryptFile.CancellationPending == true) { e.Cancel = true; return; }
              }
              if (sum_bv <= MBV) // right pass
              {
                  bwDecryptFile.ReportProgress(0, Pass);
                  //if (isCAL == false)
                      //break;
              }
              old_sbv = sum_bv;
              if (isCAL == true)
              {
                  string sb = Encoding.ASCII.GetString(Pass);
                  teLog.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                  {
                      teLog.AppendText("pl=" + p + " bv=" + sum_bv + " pass: " + sb.ToString() + Environment.NewLine); teLog.ScrollToEnd();
                  }));
              }
              if (bwDecryptFile.CancellationPending == true) { e.Cancel = true; return; }
          });
        }
        private void bwDecryptFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                lPass.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    byte[] ar = (byte[])e.UserState;
                    string sb = Encoding.ASCII.GetString(ar);
                    lPass.Content = "Password: " + sb.ToString();
                }));
            }
            else
            {
                lLength.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    lLength.Content = e.ProgressPercentage.ToString() + "\\" + max_pass_length.ToString();
                }));
            }
        }
        private void bSD_Click(object sender, RoutedEventArgs e)
        {
            if (bwDecryptFile!=null)
            if (bwDecryptFile.IsBusy)
            {
                bwDecryptFile.CancelAsync();
                bSD.Content = "Start decrypting";
                return;
            }
            teLog.Clear();
            lPass.Content = "Password: ";
            bwDecryptFile = new BackgroundWorker();            
            bwDecryptFile.WorkerSupportsCancellation = true;                
            bwDecryptFile.WorkerReportsProgress = true;
            bwDecryptFile.DoWork+=new DoWorkEventHandler(bwDecryptFile_DoWork);
            bwDecryptFile.RunWorkerCompleted+=new RunWorkerCompletedEventHandler(bwDecryptFile_RunWorkerCompleted);
            bwDecryptFile.ProgressChanged+=new ProgressChangedEventHandler(bwDecryptFile_ProgressChanged);            
            bSD.Content = "Stop decrypting";
            bwDecryptFile.RunWorkerAsync(cbCAL.IsChecked);     
        }
    }
}
