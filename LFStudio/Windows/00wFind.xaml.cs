using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;
using System.Windows.Input;
using System;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wFind.xaml
    /// </summary>
    public partial class wFind : Window
    {        
        public int CurOffset;
        public bool needClose = false;
        public int numSearch = 0;
        public int numSearchForReplace = 0;
        public wFind()
        {
            InitializeComponent();
          
        }

        private void cbFindNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cor = 0;              
                TextEditor te;
                te = G.gte;
                if (numSearch == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                CurOffset = FindTextInTextEditor(te, CurOffset, tbFind.Text, ref cor);
                if (CurOffset == -1)
                {
                    MessageBox.Show("Reached begin of document.");
                    CurOffset = te.Document.TextLength;
                    numSearch = 0;
                }
                else
                    if (CurOffset == -2)
                    {
                        MessageBox.Show("Reached end of document.");
                        CurOffset = 0;
                        numSearch = 0;
                    }
                    else
                    {
                        te.ScrollToLine(te.Document.GetLineByOffset(CurOffset).LineNumber);
                        te.Select(CurOffset, cor);
                        if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                        numSearch++;
                    }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public int FindTextInTextEditor(TextEditor te, int ofst, string whatfind, ref int cor)
        {

            if (ofst < 0) return -1;
            string searchtext = whatfind;
            if (cbMatchCase.IsChecked == false) searchtext = searchtext.ToLower();                        
            int lenst = searchtext.Length;
            string tetext = "";
            {
                while (true)
                {
                    if (ofst < 0) return -1;
                    
                    if (cbFindUp.IsChecked == false)
                    { if (ofst + lenst > te.Document.TextLength) { return -2; } }
                    else
                    {
                        if (ofst < 0) { return -1; }
                        if (ofst > te.Document.TextLength - lenst) ofst = ofst - lenst;
                    }
                    tetext = te.Document.GetText(ofst, lenst);                                        
                    if (cbMatchCase.IsChecked == false) tetext = tetext.ToLower();                    
                    if (searchtext == tetext)
                    {
                        cor = tetext.Length;
                        return ofst;
                    }
                    if (cbFindUp.IsChecked == false) ofst++; else ofst--;
                }
            }
        }//func        
    
        private void cbChancel_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!needClose)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
        private void tbFind_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                cbFindNext_Click(sender, e);
            }
        }
        private void cbFindUp_Click(object sender, RoutedEventArgs e)
        {
            G.gte.Select(G.gte.SelectionStart, 0);
        }
        private void Window_Activated(object sender, System.EventArgs e)
        {
            tbFind.Focus();
        }

        private void tbFindr_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void cbFindNextr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = G.gte;                
                string oldtext = te.SelectedText;             
                string newtext = tbFindForReplace.Text;                             
                if (oldtext == newtext)
                {
                    te.Document.Replace(te.SelectionStart, te.SelectionLength, tbNewText.Text, OffsetChangeMappingType.RemoveAndInsert);
                }
                bFindNextForReplace_Click(sender, e);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void bReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                TextEditor te = G.gte;
                for (; ; )
                {
                    string oldtext = te.SelectedText;
                    string newtext = tbFindForReplace.Text;
                    if (oldtext == newtext)
                    {
                        te.Document.Replace(te.SelectionStart, te.SelectionLength, tbNewText.Text, OffsetChangeMappingType.RemoveAndInsert);
                    }
                    //////////////////
                    int cor = 0;
                    if (numSearchForReplace == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                    CurOffset = FindTextInTextEditor(te, CurOffset, tbFindForReplace.Text, ref cor);
                    if (CurOffset == -1)
                    {
                        MessageBox.Show(numSearchForReplace.ToString() + " occurrence(s) replaced.");
                        CurOffset = te.Document.TextLength;
                        numSearchForReplace = 0;
                        break;
                    }
                    else
                        if (CurOffset == -2)
                        {
                            MessageBox.Show(numSearchForReplace.ToString() + " occurrence(s) replaced.");
                            CurOffset = 0;
                            numSearchForReplace = 0;
                            break;
                        }
                        else
                        {
                            te.ScrollToLine(te.Document.GetLineByOffset(CurOffset).LineNumber);
                            te.Select(CurOffset, cor);
                            if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                            numSearchForReplace++;
                        }
                }
                //////////////////////
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void tbFindForReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) tbNewText.Focus();
        }

        private void tbReplace_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) cbFindNextr_Click(sender,e);
        }

        private void bFindNextForReplace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cor = 0;
                TextEditor te = G.gte;
                if (numSearchForReplace == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                CurOffset = FindTextInTextEditor(te, CurOffset, tbFindForReplace.Text, ref cor);
                if (CurOffset == -1)
                {
                    MessageBox.Show("Reached begin of document.");
                    CurOffset = te.Document.TextLength;
                    numSearchForReplace = 0;
                }
                else
                    if (CurOffset == -2)
                    {
                        MessageBox.Show("Reached end of document.");
                        CurOffset = 0;
                        numSearchForReplace = 0;
                    }
                    else
                    {
                        te.Select(CurOffset, cor);
                        if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                        numSearchForReplace++;
                    }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        
    }
}
