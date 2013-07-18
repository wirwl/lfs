using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using LFStudio;

namespace MultiLineFindAndReplace
{
    [Export(typeof(IPlugin))]
    class Plugin : IPlugin
    {
        wMain w;
        public Host AppObjects;
        #region Preferences
        public string Name { get { return "Multiline Find & Replace"; } }
        public int Build { get { return 103; } } 
        public string Author { get { return "wirwl"; } }
        public string Email { get { return "wirwl@hotmail.com"; } }
        public string Url { get { return "http://lf2.codeplex.com"; } }
        public string Comment { get { return ""; } }
        public bool MultipleInstanceAllow { get { return false; } }
        public bool ShowInPluginMenu { get { return true; } }
        #endregion
        public void SilentExecute(Host _AppObjects)
        {
            AppObjects = _AppObjects;
        }
        public void NormalExecute()
        {
            if (MultipleInstanceAllow == false && w != null)
            {
                MessageBox.Show("Multiple instance not allowed.");
                return;
            }
            w = new wMain(AppObjects);
            w.Owner = AppObjects.wMain;
            w.Closed += new EventHandler(w_Closed);
           // w.Topmost = true;            
            w.Show();
         //   w.Activate();
            
            //var test=AppObjects.AppGrid.FindName("dkp");
            //MessageBox.Show(test.ToString());
        }
        private void w_Closed(object sender, EventArgs e)
        {
            w = null;
        }








    }
}