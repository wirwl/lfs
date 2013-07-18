using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AvalonDock;
using ICSharpCode.AvalonEdit.Document;
using System.Windows.Data;
using FindReplace;
using System.Globalization;

namespace LFStudio.Utils
{
    public static class AvalonEdit
    {
        public static string GetWordInLeftSide(TextEditor te)
        {
            StringBuilder sb = new StringBuilder("");
            int offset = te.SelectionStart-1;            
            int nline = te.Document.GetLineByOffset(offset).LineNumber;
            do
            {
                char c = te.Document.GetCharAt(offset);
                offset -= 1;
                if (c == ' ') break;                
            }
            while (nline == te.Document.GetLineByOffset(offset).LineNumber);
            do
            {
                char c = te.Document.GetCharAt(offset);
                if (c == ' ') break;
                sb.Append(c);                
                offset -= 1;
            }
            while (nline == te.Document.GetLineByOffset(offset).LineNumber);
            return ReverseString(sb.ToString()); 
        }
        public static string GetTextFromRightSide(TextEditor te, int incr=+1)
        {
            StringBuilder sb = new StringBuilder("");
            int offset = te.SelectionStart;
            if (incr == -1) offset--;
            int nline = te.Document.GetLineByOffset(offset).LineNumber;
            do
            {
                char c = te.Document.GetCharAt(offset);
                if (c == ' ' || c=='\n' || c=='\r') break; else sb.Append(c);
                offset += incr;
            }
            while (nline == te.Document.GetLineByOffset(offset).LineNumber);
            if (incr == 1) return sb.ToString(); else return ReverseString(sb.ToString());
            
        }
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
        public static string GetTextFromLeftSide(TextEditor te)
        {
            return GetTextFromRightSide(te, -1);
        }
        public static TextEditor FindTextEditorByDocumentText(DockingManager dm, TextDocument dt)
        {
            foreach (DocumentContent dc in dm.Documents)
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(dc.Content);
                if (te.Document.Equals(dt)) return te;
            }
            return null;
        }
        static public TextEditor GetTextEditorFromContent(Object myVisual)
        {
            if (myVisual is TextEditor) return myVisual as TextEditor;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual as Visual); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual as Visual, i);
                if (childVisual is TextEditor)
                {
                    return childVisual as TextEditor;
                }
                var te= GetTextEditorFromContent(childVisual);
                if (childVisual is TextEditor) return te;
            }
            return null;
        }
        public static void HighlightText(TextEditor te, int nl)
        {
            DocumentLine dl = te.Document.GetLineByNumber(nl);
            te.UpdateLayout();
            te.Select(dl.Offset, dl.Length);
            te.ScrollToLine(nl);
        }
    }
    public class IEditorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object
                   parameter, CultureInfo culture)
        {
            if (value is TextEditor)
                return new TextEditorAdapter(value as TextEditor);
            else if (value is TextBox)
                return new TextBoxAdapter(value as TextBox);
            else if (value is RichTextBox)
                return new RichTextBoxAdapter(value as RichTextBox);
            else return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType,
               object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
