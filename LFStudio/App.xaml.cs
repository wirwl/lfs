using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using Tomers.WPF.Localization;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Markup;
using System.Windows.Documents;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            wException we;
            if (e.IsTerminating)
            {
                we = new wException(e.ExceptionObject.ToString()) { Title = "Detected Unhandled Exception! Application will be terminate!", };
                we.bCon.IsEnabled = false;
            }
            else
                we = new wException(e.ExceptionObject.ToString()) { Title = "Detected Unhandled Exception! You can try continue working!" };
            we.Owner = G.mainWindow;
            //we.Show();

            // if (MessageBox.Show("Application crush. Do you want save modefied files?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //    G.mainWindow.sall_Click(null, null);
            //we.Hide();
            we.ShowDialog();
        }

        private static System.Threading.Mutex _syncObject;
        // эту строковую константу можно изменять на своё усмотрение
        private const string _syncObjectName = "{LFStudio - E663F-AE0D-480e-9FCA-4BE9B8CDB4E9}";
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            G.dtNow = DateTime.Now;
            Stopwatch sw = new Stopwatch();
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            TextElement.LanguageProperty.OverrideMetadata(typeof(TextElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            sw.Start();
            // WpfSingleInstance.Make();
            // base.OnStartup(e);
            bool createdNew;
            _syncObject = new System.Threading.Mutex(true, _syncObjectName, out createdNew);
            if (createdNew)
            {
                G.isAppServer = true;
                ProgramPipeTest Server = new ProgramPipeTest();
                G.ServerThread = new Thread(Server.ThreadStartServer);
                // G.ServerThread.SetApartmentState(ApartmentState.STA);                
                G.ServerThread.IsBackground = true;
                G.ServerThread.Start();              
            }
            else
            {
                G.isAppServer = false;
                if (Environment.GetCommandLineArgs().Length > 1)
                {
                    ProgramPipeTest Client = new ProgramPipeTest();
                    G.ClientThread = new Thread(Client.ThreadStartClient);
                    G.ClientThread.Start();
                    //Client.ThreadStartClient(null);
                }
                if (Environment.GetCommandLineArgs().Length <= 1)
                    MessageBox.Show("Программа уже запущена.");
                Application.Current.Shutdown();

            }
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            ////////////////////////////////////////////////////////////
            //  LanguageDictionary.RegisterDictionary(CultureInfo.GetCultureInfo("en-US"), new XmlLanguageDictionary("Lang/English.xml"));
            //    LanguageDictionary.RegisterDictionary(CultureInfo.GetCultureInfo("ru-RU"), new XmlLanguageDictionary("Lang/Russian.xml"));

            //LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
            sw.Stop();
            G.startup_app = sw.Elapsed.TotalSeconds;
        }

    }
}
