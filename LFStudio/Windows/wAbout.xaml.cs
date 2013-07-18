using System.Reflection;
using System.Windows;

namespace LFStudio.wnd
{
    /// <summary>
    /// Interaction logic for wAbout.xaml
    /// </summary>
    public partial class wAbout : Window
    {
        public wAbout()
        {
            InitializeComponent();
            Owner = G.mainWindow;
            label2.Content += "LFStudio " + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.' +
                             Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + '.' +
                             Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
        }

    }
}
