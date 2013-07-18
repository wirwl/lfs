using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Borgstrup.EditableTextBlock;
using myTreeView;

namespace LFStudio.Utils
{
   public static class tv
    {
       public static void RefreshEditableTextBlock(lfTreeViewItem tvi)
       {
           EditableTextBlock tv = tvi.textBlock;
           tv.IsEditable = true;
           tv.IsInEditMode = true;
           tv.IsInEditMode = false;
           tv.IsEditable = false;
           for (int i = 0; i < tvi.Items.Count; i++)
           { 
               EditableTextBlock etb = (tvi.Items[i] as lfTreeViewItem).textBlock;
               etb.IsInEditMode = true;
               etb.IsInEditMode = false;
           }
       }
    }
}
