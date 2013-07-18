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
    public partial class MessageBoxQuestion : Window
    {
        public enum WPFMessageBoxResult
        {
            Yes,
            No
        }
        public WPFMessageBoxResult Result;
        public MessageBoxQuestion(string caption, string question, string buttonyestext, string buttonnotext)
        {
            InitializeComponent();
            this.Owner = G.mainWindow;
            this.Title = caption;
            this.tbQuestion.Text = question;
            this.bYes.Content = buttonyestext;
            this.bNo.Content = buttonnotext;
        }
        public static WPFMessageBoxResult Show(string t,string q, string yes, string no)
        {
            MessageBoxQuestion mbq = new MessageBoxQuestion(t, q, yes, no);
            mbq.ShowDialog();
            return mbq.Result;
        }

        public static WPFMessageBoxResult Show(string question)
        {
            MessageBoxQuestion mbq = new MessageBoxQuestion(LanguageDictionary.Current.Translate<string>("wtQ", "Text","Question"), 
                                                            question,
                                                            LanguageDictionary.Current.Translate<string>("bqYes", "Text","Yes"),
                                                            LanguageDictionary.Current.Translate<string>("bqNo", "Text","No"));
            mbq.ShowDialog();
            return mbq.Result;
        }

        private void bYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WPFMessageBoxResult.Yes;
            Close();
        }

        private void bNo_Click(object sender, RoutedEventArgs e)
        {
            this.Result = WPFMessageBoxResult.No;
            Close();
        }
    }
}
