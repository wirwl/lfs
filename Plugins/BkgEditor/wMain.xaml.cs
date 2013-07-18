using System;
using System.Windows;
using LFStudio;
using Microsoft.Win32;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ObjectInstaller
{//Object installation smart wizard
    public enum Errors
    {
        not_error,
        datatxtnotfound
    }

    public struct Datatxt
    {
        public int id;
        public int type;
        public string file;
        public Datatxt(int i, int t, string f)
        {
            id = i; type = t; file = f;
        }
    }
    public partial class wMain : Window
    {
        public string password;
        public bool isNormalChar;
        public Errors CurrentError = Errors.not_error;
        public Host AppObjects;
        public List<Datatxt> ldDatatxt = null;
        public List<string> lsDatatxt = null;
        public List<int> liCharIDs = null;
        public wMain(Host _AppObjects)
        {
            InitializeComponent();
        }

        private void ParseDatatxt()
        {
            using (FileStream fs=new FileStream(tbDatatxt.Text,FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    bool isObjectTagFound = false;
                    ldDatatxt = new List<Datatxt>();
                    lsDatatxt = new List<string>();
                    int id=-1;
                    int type=-1;
                    string file=null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lsDatatxt.Add(line);
                        if (line.Trim().Length == 0) continue;
                        if (isObjectTagFound == false)
                            if (line.Contains("<object>")) {isObjectTagFound = true;
                                continue;
                            }
                        if (line.Contains("<object_end>") || line.Contains("<file_editing>") || line.Contains("<file_editing_end>") || 
                            line.Contains("<background>") || line.Contains("<background_end>")) break;
                        if (isObjectTagFound == true)
                        {
                            string[] words = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 0; i < words.Length; i++)
                            {
                                if (words[i].Contains("id"))
                                    if (i + 1 < words.Length) { int.TryParse(words[i + 1], out id);
                                    i++; continue;
                                    }
                                if (words[i].Contains("type"))
                                    if (i + 1 < words.Length) { int.TryParse(words[i + 1], out type);
                                    i++; continue;
                                    }
                                if (words[i].Contains("file"))
                                    if (i + 1 < words.Length) { file = words[i + 1];
                                    i++; continue;
                                    }
                            }
                            liCharIDs = new List<int>();
                            if (type == 0)
                                liCharIDs.Add(id);
                            ldDatatxt.Add(new Datatxt(id, type, file));
                        }
                    }
                    while ((line = sr.ReadLine()) != null)
                    {
                        lsDatatxt.Add(line);
                    }
                }

            }
        }

        private void pStart_bView_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select folder";
           // ofd.ReadOnlyChecked = true;
            ofd.CheckFileExists = false;
            ofd.FileName = "Just select folder";
            if (ofd.ShowDialog() == true)
            {
                tbPathToGameFolder.Text = Path.GetDirectoryName(ofd.FileName);                
                    tbDatatxt.Text = tbPathToGameFolder.Text + @"\data\data.txt";
              
            }
        }

        private void pStart_Commit(object sender, AvalonWizard.WizardPageConfirmEventArgs e)
        {
            if (!File.Exists(tbPathToGameFolder.Text + @"\data\data.txt")) CurrentError = Errors.datatxtnotfound;
            if (CurrentError != Errors.datatxtnotfound)
                ParseDatatxt();
            else
            {
                //go to another page;
            }
        }

      
    }
}
