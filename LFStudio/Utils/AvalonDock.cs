using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AvalonDock;
using ICSharpCode.AvalonEdit.Document;
using System.Text;

namespace LFStudio.Utils
{
    public static class AvalonDock
    {
        public static void MoveFirstDocumentToEnd(DockingManager dm)
        {
            var buf = dm.MainDocumentPane.Items[0];
            dm.MainDocumentPane.Items.Remove(buf);
            dm.MainDocumentPane.Items.Add(buf);
        }
        public static string ReturnValidName(string st)
        {
            StringBuilder sb = new StringBuilder("n_");
            if (char.IsLetter(st[0])) sb.Append(st[0]);
            for (int i=1;i<st.Length;i++)            
               if (char.IsLetter(st[i])) sb.Append(st[i]);            
            return sb.ToString();            
        }
        public static ComboBox GetComboBoxByName(object myVisual, string name)
        {
            //if (myVisual is ComboBox)
            //if ((myVisual as ComboBox).Name == name) return myVisual as ComboBox;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(myVisual as Visual); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(myVisual as Visual, i);
                if (childVisual is ComboBox)
                {
                    if ((childVisual as ComboBox).Name == name)
                        return childVisual as ComboBox;
                }
                var cb = GetComboBoxByName(childVisual,name);
                if (cb is ComboBox) return cb;
            }
            return null;
        }
        static public DocumentContent GetActiveDocument(UIElement myVisual)
        {
            if (myVisual is DocumentContent) return myVisual as DocumentContent;
            var container = myVisual;
            while ((container != null))
            {
                container = VisualTreeHelper.GetParent(container) as UIElement;
                if (container is DocumentPane)
                {
                    var item = (container as DocumentPane).SelectedItem;
                    return item as DocumentContent;
                }
            }
            return null;
        }
        static public ItemCollection GetDocuments(UIElement myVisual)
        {            
            var container = myVisual;
            while ((container != null))
            {
                container = VisualTreeHelper.GetParent(container) as UIElement;
                if (container is DocumentPane)
                {
                    return (container as DocumentPane).Items;                    
                }
            }
            return null;
        }     
        public static DocumentContent GetDocumentContentByName(DockingManager dm, string fullname)
        {
            foreach (DocumentContent dc in dm.Documents)
            {
                if (dc.Tag.ToString().Contains(fullname))
                    return dc;
            }
            return null;
        }
    }
}
