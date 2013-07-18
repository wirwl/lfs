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
    /// Interaction logic for wSaveManyFiles.xaml
    /// </summary>
    public partial class wSaveManyFiles : Window
    {
        
        public int Result;
        public wSaveManyFiles(List<string> ls)
        {
            
            InitializeComponent();
            for (int i=0;i<ls.Count;i++)
            {
                lbList.Items.Add(new ListBoxItem() { Content = ls[i] });
            }
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

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = 3;
            Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Result = 3;
        }
    }
}
