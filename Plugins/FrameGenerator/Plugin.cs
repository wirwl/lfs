using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using LFStudio;
using System.ComponentModel.Composition;

namespace FrameGenerator
{
    [Export(typeof(IPlugin))]
    class Plugin : IPlugin
    {
        wMain w;
        public Host AppObjects;
        #region Preferences
        public string Name { get { return "Frame generator"; } }
        public int Build { get { return 102; } } 
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
                w.Activate();
                return;
            }
            w = new wMain(AppObjects);
            w.Closed += new EventHandler(w_Closed);
            w.Owner = AppObjects.wMain;
            //AppObjects.wMain.IsEnabled = false;
            w.Show();
            //w.Activate();
        }
        private void w_Closed(object sender, EventArgs e)
        {
            w = null;
        }
    }
}
