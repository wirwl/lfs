using System;
using System.Windows;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wException.xaml
    /// </summary>
    public partial class wException : Window
    {
        public wException(Exception ex)
        {
            try { InitializeComponent(); }               catch { }
            try
            {
                tbShowError.Text = ex.ToString();
                {

                    if (G.mainWindow != null)
                        if (G.mainWindow.DockManager == null)
                            if (G.areThereDontsavedfiles(G.mainWindow.DockManager))
                                bSendEmail.IsEnabled = true;
                            else bSendEmail.IsEnabled = false;
                }
            }
            catch { }
        }
        public wException(string ex)
        {
            try{InitializeComponent();}catch{}
            try{tbShowError.Text = ex;}catch{}
        }
        private void bCC_Click(object sender, RoutedEventArgs e)
        {
            //Clipboard.Clear();
            //Clipboard.SetText(tbShowError.Text);      
            //Clipboard
            tbShowError.SelectAll();
            tbShowError.Copy();
        }

        private void bCon_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
           
            //G.mainWindow.Close();
            Close();
        }

        private void bSendEmail_Click(object sender, RoutedEventArgs e)
        {
           
     /*       bSendEmail.IsEnabled = false;
            bool result = false;
            result = QiHe.CodeLib.Net.EmailSender.Send(
                "user@mail.ru",
                "wirwlru@gmail.com",
                "test",
                "errors");
            
            if (result)
            {
                MessageBox.Show("The email is sent.");
            }
            else
            {
                MessageBox.Show("Send email failed.");
            }
            bSendEmail.IsEnabled = true;*/
        }

        private void bSendEmail_Click_1(object sender, RoutedEventArgs e)
        {
            G.mainWindow.sall_Click(sender, e);
        }      

         
    }
}
