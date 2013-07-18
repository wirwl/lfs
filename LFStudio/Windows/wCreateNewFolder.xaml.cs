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

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wCreateNewFolder.xaml
    /// </summary>
    public partial class wCreateNewFolder : Window
    {
        public string foldername;
        public wCreateNewFolder()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, RoutedEventArgs e) //Chancel
        {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e) //OK
        {
            foldername = tbFolderName.Text;
            if (tbFolderName.Text.Length != 0) this.Close();
        }
    }
}
