using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LFStudio.Types
{
    //public enum TaskType { None, HighlightText }
    public class TaskHighlight
    {
        //public TaskType Type;
        public string PathToFile;
        public int nline;
        public TaskHighlight(string ptf, int nl)
        {            
            PathToFile = ptf;
            nline = nl;
        }
    }
}
