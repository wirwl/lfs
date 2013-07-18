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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace LFStudio
{
    /// <summary>
    /// Interaction logic for wReplace.xaml
    /// </summary>
    public partial class wReplace : Window
    {
        public wReplace()
        {
            InitializeComponent();
        }
      

        private void cbFindNext_Click(object sender, RoutedEventArgs e)
        {
            if (G.gte == null) return;
            FindTextInTextEditor(G.gte);
        }

        private void cbChancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbFind_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        public void FindTextInTextEditor(TextEditor te)
        {
            if (tbFind.Text.Length == 0) return;
            //         if (cbFindUp.IsChecked == false) G.CurOffset = te.SelectionStart;
            //          else G.CurOffset = te.SelectionStart + te.SelectionLength;
            string stext = tbFind.Text;
            if (cbMatchCase.IsChecked == false) stext = stext.ToLower();
            //    if (cbMatchWholeWord.IsChecked == true) stext = ' ' + stext + ' ';
            if (cbFindUp.IsChecked == false) //Find down
            {
                while (true)
                {
                    DocumentLine dl = te.Document.GetLineByOffset(G.CurOffset);
                    string str = te.Document.GetText(G.CurOffset, dl.Length - (G.CurOffset - dl.Offset));
                    if (cbMatchCase.IsChecked == false) str = str.ToLower();
                    int pos;

                    pos = str.IndexOf(stext);
                    if (pos != -1)
                    {
                    }
                    if (cbMatchWholeWord.IsChecked == true)
                        if (!G.LineConsist(str.Split(' '), stext)) pos = -1;
                    if (pos != -1)
                    {
                        te.ScrollToLine(dl.LineNumber);
                        te.Select(dl.Offset + pos, stext.Length);
                        G.CurOffset = dl.Offset + dl.TotalLength; ;
                        break;
                    }


                    G.CurOffset = dl.Offset + dl.TotalLength;
                    if (G.CurOffset >= te.Document.TextLength) { MessageBox.Show("Reached end of document."); G.CurOffset = 0; break; }
                }//for                             
            }
            else // Find up
            {
                while (true)
                {
                    DocumentLine dl = te.Document.GetLineByOffset(G.CurOffset);
                    string str = te.Document.GetText(dl.Offset, G.CurOffset - dl.Offset);
                    if (cbMatchCase.IsChecked == false) str = str.ToLower();
                    int pos = str.IndexOf(stext);
                    if (pos != -1)
                    {
                        te.ScrollToLine(dl.LineNumber);
                        te.Select(dl.Offset + pos, stext.Length);
                        G.CurOffset = dl.Offset + pos;
                        break;
                    }//if 
                    if (dl.PreviousLine != null)
                        G.CurOffset = dl.PreviousLine.Offset + dl.PreviousLine.Length;
                    else G.CurOffset = 0;
                    if (G.CurOffset <= 0) { MessageBox.Show("Reached begin of document."); G.CurOffset = 0; break; }
                }//for                             

            }
        }//func
        private void cbFindUp_Click(object sender, RoutedEventArgs e)
        {
            if (G.gte.SelectionLength > 0)
                if (cbFindUp.IsChecked == true)
                    G.CurOffset = G.gte.SelectionStart;
                else G.CurOffset = G.gte.SelectionStart + G.gte.SelectionLength;
        }//func
    }
}
