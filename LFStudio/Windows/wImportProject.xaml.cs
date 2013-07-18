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
using System.IO;
using myTreeView;
using System.Collections;
using System.Security.Permissions;
using System.Diagnostics;

namespace LFStudio
{


    public partial class wImportProject : Window
    {
        public cProject project;
        public wImportProject(cProject _project)
        {
            InitializeComponent();
            Owner = G.mainWindow;
            tbSource.Text = _project.path_to_folder;
            project = _project;

        }
        private bool requiredExt(string f, string[] ext)
        {
            f = f.ToLower();
            bool result = false;
            for (int i = 0; i < ext.Length; i++)
            {
                
                    if (System.IO.Path.GetExtension(f) ==ext[i]) { result = true; break; }
            }
            return result;
        }
        private void bImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Stopwatch sw = new Stopwatch(); sw.Start();
                (G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).IsExpanded = false;
                //(G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).Items.Clear();
                int curProject = G.mainWindow.WhatProject(G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem);
                cProject cp = G.mainWindow.lProjects[curProject];
                FilesAndFolderCount ffc = GetFilesRecursive(tbSource.Text, ref cp);
                (G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).Tag = G.mainWindow.lProjects[curProject].files;
                sw.Stop();
                G.mainWindow.teOutput.AppendText("Imported "+ffc.files.ToString()+" files and "+ffc.folders.ToString()+" folders. Elapsed: " + sw.Elapsed.TotalSeconds.ToString() + " sec." + Environment.NewLine);
                G.mainWindow.teOutput.ScrollToEnd();
                if ((G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).Items.Count == 0)
                    (G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).Items.Add(null);
                (G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem).isLoaded = false;
                cProject.SaveProject(project.currentpath, G.mainWindow.lProjects[curProject]);
                this.Close();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #region old Code
        /*
        private void bImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int curProject = G.mainWindow.WhatProject(G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem);
                System.Diagnostics.Stopwatch myStopwatch = new System.Diagnostics.Stopwatch();
                //C:\Documents and Settings\
                myStopwatch.Start();
                int kol = 0;

                lfTreeViewItem root = G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem;
                string[] ext = tbSearchPattern.Text.Split(' ');
                for (int i = 0; i < ext.Length; i++) ext[i] = '.' + ext[i];
                DirectoryInfo dir = new DirectoryInfo(tbSource.Text);

                //FileInfo[] fi = dir.GetFiles("*.*", SearchOption.AllDirectories);                
                List<string> ls = GetFilesRecursive(tbSource.Text);
                //foreach (FileInfo file in fi)
                foreach (string file in ls)
                {
                    //Console.WriteLine(file.FullName);
                    if (requiredExt(file, ext))
                    {
                        //string str = file.FullName.Remove(0, tbSource.Text.Length);
                        string str = file.Remove(0, tbSource.Text.Length);
                        string[] names = str.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < names.Length - 1; i++)
                        {
                            kol++;
                            object tdr = root.Tag;
                            ArrayList al = tdr as ArrayList;
                            // if (!isExistInProject(ref al, names[i]))
                            int pos = isFolderExistInProject(ref al, names[i]);
                            if (pos == -1)
                            {
                                root = G.mainWindow.CreatelfTreeViewItem(lfTreeViewItem.isFolder, root, names[i], false);
                            }
                            else
                            {
                                root = root.Items[pos] as lfTreeViewItem;
                            }
                        }
                        object tdr2 = root.Tag;
                        ArrayList al2 = tdr2 as ArrayList;

                        if (isFileExistInProject(ref al2, names[names.Length - 1]) == -1)
                            G.mainWindow.CreatelfTreeViewItem(G.mainWindow.DetectType(str, curProject), root, str, G.mainWindow.isNeedDecrypt(str));
                        root = G.mainWindow.tvExplorer.SelectedItem as lfTreeViewItem;
                        //  if (kol == 2) return;

                    }
                }
                myStopwatch.Stop();
                cProject.SaveProject(project.currentpath, G.mainWindow.lProjects[curProject]);
                MessageBox.Show("Elapsed time: " + myStopwatch.Elapsed.ToString());
                Close();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
         */
        #endregion
        public static int isFolderExistInProject(ref ArrayList parent, string foldername)
        {
            int pos = -1;
            //  TagDesc tdp = parent.Tag as TagDesc;
            //   ArrayList al = tdp.ptofiles as ArrayList;
            for (int i = 0; i < parent.Count; i++)
            {
                if (parent[i] is ArrayList)
                {
                    if (foldername == ((parent[i] as ArrayList)[0] as string))
                    {
                        parent = (parent[i] as ArrayList); pos = i - 1; break;
                    }
                }


            }
            return pos;
        }
        public static int isFileExistInProject(ArrayList parent, string filename)
        {
            int pos = -1;
            //  TagDesc tdp = parent.Tag as TagDesc;
            //   ArrayList al = tdp.ptofiles as ArrayList;
            for (int i = 1; i < parent.Count; i++)
            {
                if (parent[i] is string)
                {
                    //if (foldername == (parent[i] as cProject.EntityDesc).path)
                    if ((parent[i] as string).Contains(filename))
                    {
                        pos = i - 1; break;
                    }
                }


            }
            return pos;
        }
        public ArrayList CreateFolder(string path,ref int fcount)
        {
            int kol = 0;
            ArrayList alRoot = project.files[0] as ArrayList;
            string[] names = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length - 1; i++)
            {
                kol++;
                int pos = isFolderExistInProject(ref alRoot, names[i]);
                if (pos == -1)
                {
                    ArrayList newFolder=new ArrayList();
                    newFolder.Add(names[i]);
                    alRoot.Add(newFolder);
                }
                else
                {
                    //alRoot = alRoot as ArrayList;
                }
            }
            int pos2=isFolderExistInProject(ref alRoot,names[names.Length-1]);
            ArrayList lastFolder = null;
            if (pos2 == -1)
            {
                fcount++;
                lastFolder = new ArrayList();
                lastFolder.Add(names[names.Length - 1]);
                alRoot.Add(lastFolder);
            }
            else lastFolder = alRoot;      
            return lastFolder;
        }
        public FilesAndFolderCount GetFilesRecursive(string b, ref cProject cp)
        {
            string[] ext = tbSearchPattern.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ext.Length; i++) ext[i] = '.' + ext[i].ToLower();
            int filecount = 0;
            int foldercount = 0;
            List<string> result = new List<string>();
            Stack<string> stack = new Stack<string>();
            ArrayList alCurrent = cp.files[0] as ArrayList;
            stack.Push(b);
            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                string path = dir.Remove(0, tbSource.Text.Length);
                if (path.Length > 0)
                    alCurrent = CreateFolder(path,ref foldercount);
                try
                {
                    result.AddRange(Directory.EnumerateFiles(dir,"*.*"));
                    foreach (string fpath in result)
                    {
                        if (requiredExt(fpath, ext))
                        {
                            string fname = fpath.Remove(0, tbSource.Text.Length);
                            if (isFileExistInProject(alCurrent, fname) == -1)
                            {
                                filecount++;
                                alCurrent.Add(fname);
                            }
                        }
                    }
                    result.Clear();
                    foreach (string dn in Directory.EnumerateDirectories(dir))
                    {
                        stack.Push(dn);
                    }
                }
                catch {}
            }
            return new FilesAndFolderCount(filecount, foldercount);
        }
        public List<string> GetFilesRecursive(string b)
        {
            // 1.
            // Store results in the file results list.
            List<string> result = new List<string>();

            // 2.
            // Store a stack of our directories.
            Stack<string> stack = new Stack<string>();

            // 3.
            // Add initial directory.
            stack.Push(b);

            // 4.
            // Continue while there are directories to process
            while (stack.Count > 0)
            {
                // A.
                // Get top directory
                string dir = stack.Pop();

                try
                {
                    // B
                    // Add all files at this directory to the result List.
                    result.AddRange(Directory.GetFiles(dir, "*.*"));

                    // C
                    // Add all directories at this directory.
                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                    }
                }
                catch
                {
                    // D
                    // Could not open the directory
                }
            }
            return result;
        }
        private void bChancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
   public  class FilesAndFolderCount
    {
        public int files;
        public int folders;
       public  FilesAndFolderCount(int files, int folders)
        {
            this.files = files; this.folders = folders;
        }
    }
}
