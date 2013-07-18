using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using Tomers.WPF.Localization;
using System.Collections.Generic;

namespace LFStudio
{
    public class ContextMenuCollection
    {
        public ContextMenu cmProjects = new ContextMenu();
        public ContextMenu cmProject = new ContextMenu() ;
        public ContextMenu cmFile = new ContextMenu() ;
        public ContextMenu cmGraphicFile = new ContextMenu();
        public ContextMenu cmFolder = new ContextMenu() ;
         //cmFile.Background = new SolidColorBrush(Color.FromArgb(255,0xE9,0xEC,0xFA));
        public ContextMenuCollection()
        {
            MenuItem mi = new MenuItem();                        
            cmProjects.Items.Add(new MenuItem() { Header = "Create New Project" });
            LanguageContext.Binder("cmiCNP", "Header", "Create new project", cmProjects.Items[cmProjects.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmProjects.Items.Add(new MenuItem() { Header = "Add existing project" });
            LanguageContext.Binder("cmiAEP", "Header", "Add existing project", cmProjects.Items[cmProjects.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmProjects.Items.Add(new MenuItem() { Header = "Reparse dat-files for all projects from data.txt" });
            LanguageContext.Binder("cmiRDFA", "Header", "Reparse dat-files for all projects from data.txt", cmProjects.Items[cmProjects.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            //-----------------------------------------------------------------------------            
            MenuItem madd = new System.Windows.Controls.MenuItem();            
            MenuItem rgame = new System.Windows.Controls.MenuItem() { Header="Run game"};
            LanguageContext.Binder("cmiRG", "Header", "Run game", rgame, MenuItem.HeaderProperty);
            MenuItem del = new System.Windows.Controls.MenuItem() { Header = "Remove project from list" };
            LanguageContext.Binder("cmiRPFL", "Header", "Remove project from list", del, MenuItem.HeaderProperty);
            MenuItem cp = new System.Windows.Controls.MenuItem() { Header = "Clear project" , InputGestureText="Ctrl + Del" };
            LanguageContext.Binder("cmiCP", "Header", "Clear project", cp, MenuItem.HeaderProperty);
            MenuItem prop  = new System.Windows.Controls.MenuItem() { Header = "Properties" };
            LanguageContext.Binder("cmiProp", "Header", "Properties", prop, MenuItem.HeaderProperty);
            madd.Header="Add";
            LanguageContext.Binder("cmiAdd", "Header", "Add", madd, MenuItem.HeaderProperty);
            madd.Items.Add(new MenuItem(){Header="Add new file"});
            LanguageContext.Binder("cmiANF", "Header", "Add new file", madd.Items[madd.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            madd.Items.Add(new MenuItem(){Header="Add existing file"});
            LanguageContext.Binder("cmiAEF", "Header", "Add existing file", madd.Items[madd.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            madd.Items.Add(new MenuItem() { Header = "Add new virtual folder" });
            LanguageContext.Binder("cmiANVF", "Header", "Add new virtual folder", madd.Items[madd.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmProject.Items.Add(rgame);
            cmProject.Items.Add(madd);
            cmProject.Items.Add(del);
            cmProject.Items.Add(new MenuItem() { Header = "Rename", InputGestureText="F2" });
            LanguageContext.Binder("cmiRen", "Header", "Rename", cmProject.Items[cmProject.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmProject.Items.Add(new MenuItem() { Header = "Import files in project" });
            LanguageContext.Binder("cmiIPFS", "Header", "Import files in project", cmProject.Items[cmProject.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmProject.Items.Add(cp);            
            cmProject.Items.Add(prop);            
            //-----------------------------------------------------------------------------
            MenuItem mdelete = new MenuItem() { Header = "Remove from list", InputGestureText="Delete" };
            LanguageContext.Binder("cmiRFL", "Header", "Remove from list", mdelete, MenuItem.HeaderProperty);
            MenuItem mopen = new MenuItem() { Header = "Open", InputGestureText="Enter" };
            LanguageContext.Binder("cmiO", "Header", "Open", mopen, MenuItem.HeaderProperty);
            cmFile.Items.Add(mopen);
            cmFile.Items.Add(mdelete);
            cmFile.Items.Add(new MenuItem() { Header = "Rename", InputGestureText = "F2" });
            LanguageContext.Binder("cmiRen2", "Header", "Rename", cmFile.Items[cmFile.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            //-----------------------------------------------------------------------------
            MenuItem madd2 = new System.Windows.Controls.MenuItem(); madd2.Header = "Add";
            LanguageContext.Binder("cmiAdd2", "Header", "Add", madd2, MenuItem.HeaderProperty);
            madd2.Items.Add(new MenuItem() { Header = "Add new file" });
            LanguageContext.Binder("cmiANF2", "Header", "Add new file", madd2.Items[madd2.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            madd2.Items.Add(new MenuItem() { Header = "Add existing dat file" });
            LanguageContext.Binder("cmiAEF2", "Header", "Add existing dat file", madd2.Items[madd2.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            madd2.Items.Add(new MenuItem() { Header = "Add new virtual folder" });
            LanguageContext.Binder("cmiANVF", "Header", "Add new virtual folder", madd2.Items[madd2.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmFolder.Items.Add(madd2);
            MenuItem mdeletefolder = new MenuItem() { Header = "Remove from list" };
            LanguageContext.Binder("cmiRFL", "Header", "Remove from list", mdeletefolder, MenuItem.HeaderProperty);
            cmFolder.Items.Add(mdeletefolder);
            cmFolder.Items.Add(new MenuItem() { Header = "Rename", InputGestureText = "F2" });
            LanguageContext.Binder("cmiRen3", "Header", "Rename", cmFolder.Items[cmFolder.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmFolder.Items.Add(new MenuItem() { Header = "Clear folder", InputGestureText = "Ctrl + Del" });
            LanguageContext.Binder("cmiCF", "Header", "Clear folder", cmFolder.Items[cmFolder.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            //cmGraphicFile=cmFile
            //-----------------------------------------------------------------------------                               
            MenuItem mdelete2 = new MenuItem() { Header = "Remove from list", InputGestureText = "Delete" };
            LanguageContext.Binder("cmiRFL", "Header", "Remove from list", mdelete2, MenuItem.HeaderProperty);
            MenuItem mopen2 = new MenuItem() { Header = "Open", InputGestureText = "Enter" };
            LanguageContext.Binder("cmiO", "Header", "Open", mopen2, MenuItem.HeaderProperty);
            cmGraphicFile.Items.Add(mopen2);
            cmGraphicFile.Items.Add(mdelete2);
            cmGraphicFile.Items.Add(new MenuItem() { Header = "Rename", InputGestureText = "F2" });
            LanguageContext.Binder("cmiRen2", "Header", "Rename", cmGraphicFile.Items[cmGraphicFile.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            cmGraphicFile.Items.Add(new MenuItem() { Header = "Flip image", InputGestureText = "" });
            LanguageContext.Binder("cmiFI", "Header", "Flip image", cmGraphicFile.Items[cmGraphicFile.Items.Count - 1] as MenuItem, MenuItem.HeaderProperty);
            //-----------------------------------------------------------------------------

        }
        public MenuItem findMenuItemByName(ItemCollection ic, string header)
        {
            MenuItem result=null;
            for (int i = 0; i < ic.Count; i++)
            {
                if (ic[i] is MenuItem)
                if ((ic[i] as MenuItem).Header.ToString() == header.ToString()) { result = (MenuItem)ic[i]; break; }
            }
            return result;
        }
        public ContextMenu CloneAndPaste(ContextMenu cm, int pos, List<ContextMenu> lcm)
        {
            ContextMenu result = new ContextMenu();
            for (int i = 0; i < cm.Items.Count; i++)
            { 
                MenuItem mi=new MenuItem();
                mi.Header=(cm.Items[i] as MenuItem).Header;
                
                result.Items.Add(mi);
            }
            return result;
        }
    }
}
