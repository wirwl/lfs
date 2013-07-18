using System;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using LFStudio;
using AvalonDock;
using ICSharpCode.AvalonEdit;

namespace FrameGenerator
{
    /// <summary>
    /// Interaction logic for wMain.xaml
    /// </summary>
    public partial class wMain : Window
    {
        public Host AppObjects;
        public char newline='\n';
        public wMain(Host _AppObjects)
        {
            InitializeComponent();
            AppObjects = _AppObjects;
            teHeader.Text = "state: 0  wait: 5  dvx: 0  dvy: 0  dvz: 0  centerx: 39  centery: 79  hit_a: 0  hit_d: 0  hit_j: 0";
            te_itr.Text = "kind: 3  x: 40  y: 16  w: 25  h: 65\n      catchingact: 120 120  caughtact: 130 130";
            try
            {
                teHeader.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                teResult.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_bpoint.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_wpoint.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_itr.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_bdy.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_cpoint.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
                te_opoint.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(AppObjects.wMain.programfolder + "lf2.xshd"), HighlightingManager.Instance);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }

        }

        private void bGenerate_Click(object sender, RoutedEventArgs e)
        {
            teResult.Clear();
            int count = 0;
            try { count = Convert.ToInt32(tbFrameCount.Text); }
            catch { return; }
            int ffv = 0;
            try { ffv = Convert.ToInt32(tbFirstFrameValue.Text); }
            catch { return; }
            int pic = 0;
            try { pic = Convert.ToInt32(tbPic.Text); }
            catch { return; }
            for (int i = 0; i < count; i++)
            {
                teResult.AppendText("<frame> " + (ffv + i) + " " + tbFrameCaption.Text + newline);
                if (i != count - 1)
                    teResult.AppendText("   pic: " + (pic + i) + "  next: " + (ffv + i + 1) + "  " + teHeader.Text + newline);
                else teResult.AppendText("   pic: " + (pic + i) + "  next: " + tbLastframe.Text + "  " + teHeader.Text + newline);
                if (cbBpoint.IsChecked == true)
                {
                    teResult.AppendText("   bpoint:" + newline);
                    teResult.AppendText("      " + te_bpoint.Text + newline);
                    teResult.AppendText("   bpoint_end:" + newline);
                }
                if (cbWpoint.IsChecked == true)
                {
                    teResult.AppendText("   wpoint:" + newline);
                    teResult.AppendText("      " + te_wpoint.Text + newline);
                    teResult.AppendText("   wpoint_end:" + newline);
                }
                if (cbBdy.IsChecked == true)
                {
                    teResult.AppendText("   bdy:" + newline);
                    teResult.AppendText("      " + te_bdy.Text + newline);
                    teResult.AppendText("   bdy_end:" + newline);
                }
                if (cbItr.IsChecked == true)
                {
                    teResult.AppendText("   itr:" + newline);
                    teResult.AppendText("      " + te_itr.Text + newline);
                    teResult.AppendText("   itr_end:" + newline);
                }
                if (cbCpoint.IsChecked == true)
                {
                    teResult.AppendText("   cpoint:" + newline);
                    teResult.AppendText("      " + te_cpoint.Text + newline);
                    teResult.AppendText("   cpoint_end:" + newline);
                }
                if (cbOpoint.IsChecked == true)
                {
                    teResult.AppendText("   opoint:" + newline);
                    teResult.AppendText("      " + te_opoint.Text + newline);
                    teResult.AppendText("   opoint_end:" + newline);
                }

                teResult.AppendText("<frame_end>" + newline + newline);
            }
        }

        private void bPaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");
             //   TextEditor te = ((dm.ActiveDocument as DocumentContent).Content as TextEditor);                
                TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);
                te.Document.Insert(te.SelectionStart, teResult.Text);
              //  AppObjects.wMain.UpdateFoldings((te.oi as ObjectInfo).data,te);
             //   AppObjects.wMain.UpdateComboBoxs(te);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
