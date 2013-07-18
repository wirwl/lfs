using System.Windows.Controls;

namespace LFStudio.Utils
{
    public static class Menu
    {
        
        public static void AbordIsCheckedMenuItems(MenuItem menu)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                if (menu.Items[i] is MenuItem)
                {
                    (menu.Items[i] as MenuItem).IsChecked = false;
                }
            }
        }
    }
}
