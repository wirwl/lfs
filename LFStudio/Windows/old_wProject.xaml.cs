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
    /// Interaction logic for wProject.xaml
    /// </summary>
    public partial class wProject : Window
    {
        public cProject project;
        public wProject(cProject p)
        {
            InitializeComponent();
            project=p;
            tbPathExe.Text = p.path_to_exe;
            //tbFolder.Text = p.path_to_folder;
            tbPass.Text = p.pass;
           
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                tbPathExe.Text = ofd.FileName;                
            }
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            project.path_to_exe = tbPathExe.Text;
            project.path_to_folder = System.IO.Path.GetDirectoryName(project.path_to_exe);
                //tbFolder.Text;
            project.pass = tbPass.Text;
            //System.IO.Path.

            if (File.Exists(tbPathExe.Text) & System.IO.Path.GetExtension(tbPathExe.Text) == ".exe")
            {
                G.sbRungamepathtoexe = tbPathExe.Text;
                Close();
            }
            else
                MessageBox.Show("Please type exist path to game executable.", "Error!");

        }

      

        private void bChancel_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
