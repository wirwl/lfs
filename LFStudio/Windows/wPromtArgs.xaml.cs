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

namespace LFStudio.Windows
{
    /// <summary>
    /// Interaction logic for wPromtArgs.xaml
    /// </summary>
    public partial class wPromtArgs : Window
    {          
        public string args;
        public bool isChancel = false;
        public wPromtArgs(string _args)
        {
            InitializeComponent();
            args = _args;
            tbArgs.Text = args;
            Owner = G.mainWindow;
        }

        private void bChancel_Click(object sender, RoutedEventArgs e)
        {
            isChancel = true;
            Close();
        }

        private void bRun_Click(object sender, RoutedEventArgs e)
        {
            args = tbArgs.Text;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //isChancel = true;            
        }
    }
}
