using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace LFStudio
{
    
    public class DatFileDesc
    {
        //public string path;
        //public List<string> Errors;
        public DatFileDesc() { wsl = new List<WeaponStrListInfoEntry>(); }
        public DatFileDesc(string path)
        { /*this.path = path*/; wsl = new List<WeaponStrListInfoEntry>(); }

        private Header _header;
        public Header header { get { return _header; } set { _header = value; } }

        private List<FrameInfo> _frames;
        public List<FrameInfo> frames { get { return _frames; } set { _frames = value; } }

        public int wsl_oline = -1;
        public int wsl_cline=-1;
        private List<WeaponStrListInfoEntry> _wsl;
        public List<WeaponStrListInfoEntry> wsl { get { return _wsl; } set { _wsl = value; } }

        public List<RegionInfo> regions;
    }
    public class Header
    {
        public int oline=-1;
        public int cline=-1;
        public string foldcaption;
        public List<FileDesc> files = new List<FileDesc>();
        public List<PropDesc> properties = new List<PropDesc>();
    }
    public  class LineNumbersForSubTags
    {
        public int ol_bpoint = -1;
        public int cl_bpoint = -1;
        public int ol_wpoint = -1;
        public int cl_wpoint = -1;
        public int ol_cpoint = -1;
        public int cl_cpoint = -1;
        public int ol_opoint = -1;
        public int cl_opoint = -1;
        public List<int> ol_bdy = new List<int>();
        public List<int> cl_bdy = new List<int>();
        public List<int> ol_itr = new List<int>();
        public List<int> cl_itr = new List<int>();
    }
    public class FrameInfo
    {
        public int oline=-1;
        public int cline=-1;
        public int firstheaderline = -1;
        public int lastheaderline  = -1;
        ////////////////////
        public LineNumbersForSubTags lnst= new LineNumbersForSubTags();
        ////////////////////
        public string foldcaption;
        public string caption;
        public int? number=-1;
        public List<PropDesc> header = new List<PropDesc>();
        public List<PropDesc> bpoint = new List<PropDesc>();
        public List<PropDesc> wpoint = new List<PropDesc>();
        public List<List<PropDesc>> bdy = new List<List<PropDesc>>();
        public List<List<PropDesc>> itr = new List<List<PropDesc>>();
        public List<PropDesc> cpoint = new List<PropDesc>();
        public List<PropDesc> opoint = new List<PropDesc>();
    }
    public class RegionInfo
    {
        public int oline=-1;
        public int cline=-1;
        public string caption;
        public RegionInfo() { }
        public RegionInfo(int nl, string c) { oline = nl; c = caption; }
    }
    public class WeaponStrListInfoEntry
    {
        public int number;
        public List<PropDesc> props = new List<PropDesc>();
    }
    public struct FileDesc
    {
        public int firstFrame;
        public int lastFrame;
        public string path;
        public int width;
        public int height;
        public int row;
        public int col;
    }
    public struct PropDesc
    {
        public string name;
        public string value;
        public PropDesc(string name, string value) { this.name = name; this.value = value; }
    }
    public class DatatxtDesc
    {
        public DatatxtDesc()
        {
        }
        //public List<gfErrors> errors = new List<gfErrors>();
        public string path;
        public List<ObjectInfo> lObject = new List<ObjectInfo>();
        public List<BackgroundInfo> lBackground = new List<BackgroundInfo>();
    }
    public class BackgroundInfo
    {
        public int? id;
        public string path;
        public DatFileDesc data;
        public string comment;
        public BackgroundInfo(int? id, string pathtofile, DatFileDesc dfd = null)
        {
            this.id = id; this.path = pathtofile; this.data = dfd;
        }
    }
    public class ObjectInfo
    {     
        public int? id;
        public int? type;
        public string path;
        public DatFileDesc data;
        public string comment;
        public List<gfErrors> errors = new List<gfErrors>();
        public List<BitmapImage> lbiBitmaps = null;
        public List<CroppedBitmap> lbiCroppedBitmaps = null;
        public ObjectInfo() { }
        public ObjectInfo(int? id, int? type, string path, DatFileDesc dfd = null)
        {
            this.id = id; this.type = type; this.path = path; this.data = dfd;
        }
        
    }
    public enum WhereStand {none,frametitle,frameheader,bpoint,wpoint,cpoint,opoint,bdy,itr,frameend }

}
