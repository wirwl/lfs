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
    /// Interaction logic for wSaveOneFile.xaml
    /// </summary>
    public partial class wSaveOneFile : Window
    {
        public int Result;       
        public wSaveOneFile()
        {
            InitializeComponent();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = 3;
            Hide();
        }

        private void bSC_Click(object sender, RoutedEventArgs e)
        {
            Result = 1;
            Hide();
        }

        private void bDSC_Click(object sender, RoutedEventArgs e)
        {
            Result = 2;
            Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Result = 3;
        }
    }
}
