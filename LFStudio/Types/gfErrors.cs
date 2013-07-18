using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LFStudio
{
    public class gfErrors
    {
        public string fullName;
      //  public int nproject;
        public int N { get; set; }
        public string Description { get; set; }
        private string _File;
        public string File { get { return _File; } set { _File =Path.GetFileName(value); fullName = value; } }
        public int Line { get; set; }
        public string Project { get; set; }
        public gfErrors() { }
        public gfErrors(string d,int l, string f, string p)
        { 
            Description=d;
            Line=l;
            File=f;
            Project=p;        
        }
        public List<gfErrors> lErrors()
        {
            List<gfErrors> gfe = new List<gfErrors>();


            return gfe;
        }
    }
}
