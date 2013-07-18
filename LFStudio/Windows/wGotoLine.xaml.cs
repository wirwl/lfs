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
using ICSharpCode.AvalonEdit;
using AvalonDock;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wGotoLine.xaml
    /// </summary>
    public partial class wGotoLine : Window
    {
        public wGotoLine()
        {
            InitializeComponent();
            tbLine.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          //  TextEditor te = ((DockManager.ActiveDocument as DocumentContent).Content as TextEditor);           

            try
            {
                lnum.Content = "Line number (1-" + G.gte.LineCount.ToString() + ")";
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int line = Convert.ToInt32(tbLine.Text);
                    G.gte.ScrollTo(line, 0);
                    G.gte.Select(G.gte.Document.GetLineByNumber(line).Offset, 0);
                    this.Close();
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
    }
}
