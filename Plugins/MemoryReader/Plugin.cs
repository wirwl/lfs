using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using LFStudio;
using System.ComponentModel.Composition;

namespace MemoryReader
{
    [Export(typeof(IPlugin))]
    class Plugin : IPlugin
    {
        wMain w;
        public Host AppObjects;
        #region Preferences
        public string Name { get { return "Memory Reader"; } }
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
            try
            {
                if (MultipleInstanceAllow == false && w != null)
                {
                    MessageBox.Show("Multiple instance not allowed.");
                    //w.Activate();
                    w.Show();
                    return;
                }
                w = new wMain(AppObjects);
                w.Closed += new EventHandler(w_Closed);
                //w.Topmost = true;            
                w.Show();            

                //var test=AppObjects.AppGrid.FindName("dkp");
                //MessageBox.Show(test.ToString());
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); } 
        }
        private void w_Closed(object sender, EventArgs e)
        {
            w = null;
        }
    }
}
