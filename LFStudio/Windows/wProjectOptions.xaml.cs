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
using Microsoft.Win32;
using System.IO;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wProjectOptions.xaml
    /// </summary>
    public partial class wProjectOptions : Window
    {
        public cProject project;
        public wProjectOptions(cProject p)
        {
            InitializeComponent();
            this.Owner = G.mainWindow;
            project = p;
            tbPathExe.Text = p.path_to_exe;
            tbGameFolder.Text = p.path_to_folder;
            tbPass.Text = p.pass;
            cbAutorun.IsChecked = p.PressGameStartAfterApplicationRun;
        }

        private void bBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                tbPathExe.Text = ofd.FileName;
                tbGameFolder.Text = System.IO.Path.GetDirectoryName(ofd.FileName);
                int? nint= G.ReadDwordValueFromFile(ofd.FileName, 0x18066);
                //if (nint.HasValue) lMaxframecount.Content = "Max frame count: " + (nint + 1);
                //else lMaxframecount.Content = lMaxframecount.Content.ToString() + "<Read error!>";

                

            }
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(tbPathExe.Text) & System.IO.Path.GetExtension(tbPathExe.Text) == ".exe")
            {
                project.path_to_exe = tbPathExe.Text;
                project.path_to_folder = tbGameFolder.Text;               
                project.pass = tbPass.Text;
                G.sbRungamepathtoexe = tbPathExe.Text;
                G.needCreateProject = false;
                project.PressGameStartAfterApplicationRun = (bool)cbAutorun.IsChecked;
                Close();
            }
            else                
                MessageBox.Show("Please type exist path to game executable.", "Warning!");


        }

        private void bChancel_Click(object sender, RoutedEventArgs e)
        {
            if (G.needCreateProject) 
                MessageBox.Show("Please type exist path to game executable. It's need for correct work with project.\nThis window appears again when you will try: run game, add existing file, create new file and so on.", "Warning!");
            G.needCreateProject = false;
            Close();
        }
    }
}
