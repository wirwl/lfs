using System;
using System.Windows;
using System.IO;

namespace MemoryReader
{
    /// <summary>
    /// Interaction logic for wTextEditor.xaml
    /// </summary>
    public partial class wTextEditor : Window
    {
        public wTextEditor()
        {
            InitializeComponent();
        }

        private void miSave_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\addresses.txt";
            StreamWriter sw = File.CreateText(path);
            rtbTextEditor.Save(sw.BaseStream);            
            sw.Close();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\addresses.txt";
            StreamReader sr = File.OpenText(path);
            rtbTextEditor.Load(sr.BaseStream);            
            sr.Close();
        }
        

        private void miClose_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            (sender as wTextEditor).Hide();
        }
    }
}
