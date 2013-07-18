using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Borgstrup.EditableTextBlock;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using LFStudio;
using System.Collections;

namespace myTreeView
{
    public class lfTreeViewItem : TreeViewItem
    {
        public bool isLoaded;
        ImageSource iconSource;
        public EditableTextBlock textBlock;
        public string Pathtofile;
        Image icon;
        public const string isProjects = "*projects";
        public const string isProject = "*project";
        public const string isFolder = "*folder";
        public const string isFile = "*file";

        public lfTreeViewItem(string type = "")
        {
            StackPanel stack = new StackPanel() { IsHitTestVisible = true };
            stack.Orientation = Orientation.Horizontal;
            Header = stack;
            icon = new Image() { IsHitTestVisible = true, SnapsToDevicePixels=true,UseLayoutRounding=true };
            icon.VerticalAlignment = VerticalAlignment.Center;
            icon.Margin = new Thickness(0, 0, 0, 0);
            icon.Width = 16; icon.Height = 16;
            icon.Source = iconSource;
            stack.Children.Add(icon);
            textBlock = new EditableTextBlock() { IsHitTestVisible = true,IsEnabled=false };
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            stack.Children.Add(textBlock);
            Pathtofile = type;
            switch (type)
            {
                case isProjects: this.Type = isProjects; break;
                case isProject: this.Type = isProject; break;
                case isFolder: this.Type = isFolder; break;
                default: this.Type = isFile; break;
            }
        }
        public static BitmapSource GetAssociatedIcon(string filePath)
        {

            try
            {
                string extension = Path.GetExtension(filePath);
                BitmapSource bs=null;
                if (G.AppSettings.CacheByExtensionEnabled && extension!=".bmp" && extension!=".exe")
                {
                   // bs = GetFromCacheByExtension(filePath);
                   // if (bs != null) return bs;
                }
                if (G.AppSettings.CacheByFileEnabled && (extension==".bmp" || extension==".exe"))
                {
                   // bs = GetFromCacheByFile(filePath);
                   // if (bs != null) return bs;
                }
                using (var assocIcon = System.Drawing.Icon.ExtractAssociatedIcon(filePath))
                {
                    bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                       assocIcon.Handle,
                       System.Windows.Int32Rect.Empty,
                       System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(16, 16));
                    if (G.AppSettings.CacheByExtensionEnabled && extension != ".bmp" && extension != ".exe")
                    //AddToCacheByExtension(bs, filePath);
                    //if (G.AppSettings.CacheByFileEnabled && (extension == ".bmp" || extension == ".exe"))
                    //AddToCacheByFile(Path.GetFileName(G.mainWindow.lProjects[G.mainWindow.curProject].currentpath), filePath,bs);
                    return bs;
                }
                return bs;
            }
            catch (Exception)
            {

                return G.bsAnotherType;
                //CreateImage(G.anothertype);
            }
        }

        public static BitmapImage CreateImage(string path)
        {
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
           // if (path.Contains("projects.png"))
            {
                myBitmapImage.UriSource = 
                    new Uri(path, UriKind.Absolute);
            }     
           // myBitmapImage.UriSource = new Uri(path);
            myBitmapImage.EndInit();            
            return myBitmapImage;
        }
        public BitmapImage GetFromCacheByExtension(string path)
        {

            string ext=Path.GetExtension(path);
            string fpath="Cache\\Extensions\\" + ext;
            if (File.Exists(fpath))
            {
                return new BitmapImage(new Uri(G.mainWindow.programfolder+fpath));
            }
            return null;
        }
        public void AddToCacheByExtension(BitmapSource bi, string path)
        {
            if (G.mainWindow.lProjects.Count == 0) return;
            string ext=Path.GetExtension(path);
          //  if (ext == ".bmp" || ext == ".exe") return;
            if (!Directory.Exists("Cache")) Directory.CreateDirectory("Cache");
            if (!Directory.Exists("Cache\\Extensions")) Directory.CreateDirectory("Cache\\Extensions");            
            PngBitmapEncoder bbe = new PngBitmapEncoder();
            bbe.Frames.Add(BitmapFrame.Create(bi));
            using (var stream = File.Create("Cache\\Extensions\\"+ext))
            {
                bbe.Save(stream);
            }

        }
        public BitmapImage GetFromCacheByFile(string path)
        {            
            string fname="";// = DeleteSlash(G.mainWindow.DeletePath(path));
          //  fname = G.mainWindow.folderpath + "Cache\\File\\" +Path.GetFileName(G.mainWindow.lProjects[G.mainWindow.curProject].currentpath)+'\\'+ fname;
            if (File.Exists(fname))
            {
                return new BitmapImage(new Uri(fname));
            }
            return null;
        }
        public void AddToCacheByFile(string pfname,string path, BitmapSource bs)
        {
            if (G.mainWindow.lProjects.Count == 0) return;
            string ext = Path.GetExtension(path);
            //if (ext != ".bmp" && ext != ".exe") return;
            if (!Directory.Exists("Cache")) Directory.CreateDirectory("Cache");
            if (!Directory.Exists("Cache\\File")) Directory.CreateDirectory("Cache\\File");
            if (!Directory.Exists("Cache\\File\\"+pfname)) Directory.CreateDirectory("Cache\\File\\"+pfname);
            string fname = "";// = DeleteSlash(G.mainWindow.DeletePath(path));            
            PngBitmapEncoder bbe = new PngBitmapEncoder();
            bbe.Frames.Add(BitmapFrame.Create(bs));
            using (var stream = File.Create("Cache\\File\\" + pfname + '\\' + fname))
            {
                bbe.Save(stream);
            }


        }
        public string DeleteSlash(string st)
        {
            string[] str = st.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder(str[0], st.Length);
            for (int i = 1; i < str.Length; i++)
            {
                sb.Append('_' + str[i]);
            }
            return sb.ToString();
        }
        #region Properties

        /// <summary>
        /// Gets/Sets the Selected Image for a TreeViewNode
        /// </summary>       
        public ImageSource Icon
        {
            set
            {
                iconSource = value;
                icon.Source = iconSource;
            }
            get
            {
                return iconSource;
            }
        }
        private string type;
        public string Type
        {
            set
            {
                type = value;
                switch (value)
                {
                    case lfTreeViewItem.isProjects: this.Icon = G.bsProjects; break;
                    case lfTreeViewItem.isProject: this.Icon = G.bsProject; break;
                    case lfTreeViewItem.isFolder: this.Icon = G.bsFolderFold; break;
                    case lfTreeViewItem.isFile: this.Icon = GetAssociatedIcon(Pathtofile); break;
                    default: this.Icon = GetAssociatedIcon(Pathtofile); break;
                }//switch

            }//set      
            get { return type; }
        }//Type property

        #endregion Properties
        #region Event Handlers     
        protected override void OnUnselected(RoutedEventArgs args)
        {
            //  base.OnUnselected(args);
            //  icon.Source = iconSource;
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
      
        }
        protected override void OnSelected(RoutedEventArgs args)
        {
            base.OnSelected(args);
            icon.Source = iconSource;
        }
        /// <summary>
        /// Gets/Sets the HeaderText of TreeViewWithIcons
        /// </summary>
        public string HeaderText
        {
            set
            {
                textBlock.Text = value;
            }
            get
            {
                return textBlock.Text;
            }
        }
        protected override void OnExpanded(RoutedEventArgs e)
        {
            base.OnExpanded(e);
            this.IsSelected = true;
            this.Focus();
            if (this.Type == isFolder) 
                Icon = G.bsUnFolderFold;
        }
        protected override void OnCollapsed(RoutedEventArgs e)
        {
            base.OnCollapsed(e);
            this.IsSelected = true;
            this.Focus();
            if (this.Type == isFolder) 
                Icon = G.bsFolderFold;
        }



        #endregion Event Handlers
    }
}
