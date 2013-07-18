using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace LFStudio.Types
{
    public class lfItem
    {
        public int id;     
        public List<int> Height=new List<int>();
        public List<int> Width=new List<int>();
        public int Type;      // 1-light weapon, 2-heavy weapon, 4-throw weapon, 6-drinks
        public string Title;
        private string _Path;
        public string Path
        {
            get { return _Path; }
            set { _Path = value; Title = System.IO.Path.GetFileName(value); }
        }

        public List<BitmapImage> lbsImages = new List<BitmapImage>();
        public List<BitmapSource> llbsCroppedImage;
        public lfItem(int id,int type, string path)
        {

            this.id = id;
            this.Type = type;
            Path = path;

        }
    }
}
