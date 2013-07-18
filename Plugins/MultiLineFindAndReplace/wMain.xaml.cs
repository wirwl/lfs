using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using LFStudio;
using AvalonDock;

namespace MultiLineFindAndReplace
{

    public partial class wMain : Window
    {
        public Host AppObjects;
        public int CurOffset;
       
        public int numSearch = 0;
        public int numSearchForReplace = 0;
        public wMain(Host _AppObjects)
        {
            InitializeComponent();
            AppObjects = _AppObjects;
            this.Owner = _AppObjects.wMain;
            CurOffset = 0;                        
        }
        public int DeleteSpaces(ref string st)
        {
            int num = 0;
            for (int i = 0; i < st.Length; i++)
            {
                if (st[i] == ' ')
                { st = st.Remove(i, 1); i--; num++; }

            }
            return num;
        }
        #region For Find
        private void cbChancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void tbFind_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void cbFindNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int cor = 0;
                DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");               
                TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);
                if (numSearch == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                CurOffset = FindTextInTextEditor(te, CurOffset, teFind.Text, ref cor);          
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
                        te.Select(CurOffset, cor);
                        te.ScrollToLine(te.Document.GetLineByOffset(CurOffset).LineNumber);
                        if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                        numSearch++;
                    }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public int DeleteEscapeSymbols(ref string st)
        {
            int num = 0;
            for (int i = 0; i < st.Length; i++)
            {
                if (/*st[i] == '\n' ||*/ st[i] == '\r')
                { st = st.Remove(i, 1); i--; num++; }

            }
            return num;
        }
        public int RestorePosition(TextEditor te,string str, int offset)
        {
            //string str = te.Document.GetText(CurOffset, dl.Length - (CurOffset - dl.Offset));
            return 0;
        }
        public int FindTextInTextEditor(TextEditor te, int ofst, string whatfind, ref int cor)
        {
            int num_ds=0;
            if (ofst < 0) return -1;
            string searchtext = whatfind;
            if (cbMatchCase.IsChecked == false) searchtext = searchtext.ToLower();
            if (cbDiscardspaces.IsChecked == true)
            { num_ds=DeleteSpaces(ref searchtext); }
            int num_st = DeleteEscapeSymbols(ref searchtext);
            int lenst = searchtext.Length;
            string tetext = "";
            {
                while (true)
                {
                    if (ofst < 0) return -1;
                    if (cbDiscardspaces.IsChecked == true && ofst < te.Document.TextLength)
                    {
                        string oneletter = te.Document.GetText(ofst, 1);
                        if (oneletter[0] == ' ')
                        {
                            if (cbFindUp.IsChecked == false) ofst++; else ofst--;
                            continue;
                        }
                    }
                    if (cbFindUp.IsChecked == false)
                    { if (ofst + lenst > te.Document.TextLength) { return -2; } }
                    else
                    {
                        if (ofst < 0) { return -1; }
                        if (ofst > te.Document.TextLength - lenst) ofst = ofst - lenst;
                    }
                    tetext = te.Document.GetText(ofst, lenst);
                    int num_tt = DeleteEscapeSymbols(ref tetext);
                    StringBuilder peace = new StringBuilder(lenst - tetext.Length);
                    int next = ofst + lenst;
                    while (peace.Length < lenst - tetext.Length)
                    {
                        if (next >= te.Document.TextLength) break;
                        string letter = te.Document.GetText(next, 1);
                        if (letter[0] != '\r') peace.Append(letter);
                        next++;
                    }
                    tetext += peace.ToString();
                    if (cbMatchCase.IsChecked == false) tetext = tetext.ToLower();
                    int num_space = 0;
                    int num_peace = 0;
                    StringBuilder peace2 = new StringBuilder("");
                    if (cbDiscardspaces.IsChecked == true)
                    {
                        num_space = DeleteSpaces(ref tetext);
                        peace2 = new StringBuilder(lenst - tetext.Length);
                        int next2 = ofst + lenst + peace.Length;
                        while (peace2.Length < lenst - tetext.Length)
                        {
                            if (next2 >= te.Document.TextLength) break;
                            string letter = te.Document.GetText(next2, 1);
                            if (letter[0] != ' ') peace2.Append(letter); else num_peace++;
                            next2++;
                        }
                        tetext += peace2.ToString();
                    }
                    StringBuilder newtetext = new StringBuilder(tetext.Length);//tetext.Length
                    if (searchtext.Length==tetext.Length)
                    for (int i = 0; i < searchtext.Length; i++)
                    {                       
                        if (searchtext[i] == '?') newtetext.Append('?'); else newtetext.Append(tetext[i]);//error!
                    }
                    if (searchtext == newtetext.ToString())
                    {
                        cor = tetext.Length + num_tt + num_space + num_peace+num_ds;
                        return ofst;
                    }
                    if (cbFindUp.IsChecked == false) ofst++; else ofst--;
                    
                }
            }
        }//func        
        
        private void cbFindUp_Click(object sender, RoutedEventArgs e)
        {        
            DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");
            if (dm.ActiveDocument == null) return;
            if ((dm.ActiveDocument as DocumentContent).Content==null) return;
            TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);
            te.Select(te.SelectionStart,0);
        }        
        #endregion
        #region For Replace
    
        #endregion

        private void bFindForReplace_Click(object sender, RoutedEventArgs e)//Find for replace
        {
            try
            {
                int cor = 0;
                DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");
                TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);
                if (numSearchForReplace == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                CurOffset = FindTextInTextEditor(te, CurOffset, teFindForReplace.Text, ref cor);
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
                        te.ScrollToLine(te.Document.GetLineByOffset(CurOffset).LineNumber);
                        if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                        numSearchForReplace++;
                    }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void bReplaceAll_Click(object sender, RoutedEventArgs e) //Replace all
        {
            try
            {
                int iter = 0;
                DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");
                TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);
                for (; ; )
                {
                    if (iter > te.Text.Length) { MessageBox.Show("Error!", "Break from infinite loop"); break; }
                    string oldtext = te.SelectedText;
                    DeleteEscapeSymbols(ref oldtext);
                    string newtext = teFindForReplace.Text;
                    DeleteEscapeSymbols(ref newtext);
                    if (cbDiscardspaces.IsChecked == true)
                    {
                        DeleteSpaces(ref oldtext);
                        DeleteSpaces(ref newtext);
                    }
                    if (oldtext == newtext)
                    {
                        te.Document.Replace(te.SelectionStart, te.SelectionLength, teNewText.Text, OffsetChangeMappingType.RemoveAndInsert);
                    }
                    CurOffset = te.SelectionStart + teNewText.Text.Length;
                    //////////////////
                    int cor = 0;
                    if (numSearchForReplace == 0) if (cbFindUp.IsChecked == false) CurOffset = 0; else CurOffset = te.Document.TextLength;
                    CurOffset = FindTextInTextEditor(te, CurOffset, teFindForReplace.Text, ref cor);
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
                            te.Select(CurOffset, cor);
                            te.ScrollToLine(te.Document.GetLineByOffset(CurOffset).LineNumber);
                            if (cbFindUp.IsChecked == false) CurOffset++; else CurOffset--;
                            numSearchForReplace++;
                        }
                    iter++;
                }//for
                //////////////////////
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
       
        private void bFindNextr_Click(object sender, RoutedEventArgs e)//Replace 
        {
          try
            {
                
                DockingManager dm = (DockingManager)AppObjects.AppGrid.FindName("DockManager");
                TextEditor te = LFStudio.Utils.AvalonEdit.GetTextEditorFromContent((dm.ActiveDocument as DocumentContent).Content);        
                string oldtext = te.SelectedText;
                DeleteEscapeSymbols(ref oldtext);
                string newtext = teFindForReplace.Text;
                DeleteEscapeSymbols(ref newtext);                
                if (cbDiscardspaces.IsChecked == true)
                {
                    DeleteSpaces(ref oldtext);
                    DeleteSpaces(ref newtext);
                }
                if (oldtext == newtext)
                {
                    te.Document.Replace(te.SelectionStart, te.SelectionLength, teNewText.Text, OffsetChangeMappingType.RemoveAndInsert);
                }
                CurOffset = te.SelectionStart + teNewText.Text.Length;
                bFindForReplace_Click(sender, e);
            }
          catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
   
      
    }
}