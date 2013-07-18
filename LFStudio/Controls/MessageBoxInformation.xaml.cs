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
using Tomers.WPF.Localization;

namespace LFStudio.Controls
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBoxInformation : Window
    {
        public enum WPFMessageBoxResult
        {
            Yes,
            No
        }
        public WPFMessageBoxResult Result;
        public MessageBoxInformation(string caption, string question, string buttonyestext)
        {
            InitializeComponent();
            this.Owner = G.mainWindow;
            this.Title = caption;
            this.tbQuestion.Text = question;
            this.bYes.Content = buttonyestext;         
        }
        public static WPFMessageBoxResult Show(string t,string q, string yes, string no)
        {
            MessageBoxInformation mbq = new MessageBoxInformation(t, q, yes);
            mbq.ShowDialog();
            return mbq.Result;
        }

        public static WPFMessageBoxResult Show(string question)
        {
            MessageBoxInformation mbq = new MessageBoxInformation(LanguageDictionary.Current.Translate<string>("wtInf", "Text", "Information"),
                                                            question,
                                                            LanguageDictionary.Current.Translate<string>("bqYes", "Text", "Yes"));
            mbq.ShowDialog();
            return mbq.Result;
        }

        private void bYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WPFMessageBoxResult.Yes;
            Close();
        }
     
    }
}
