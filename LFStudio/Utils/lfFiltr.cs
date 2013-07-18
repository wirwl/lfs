using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LFStudio.Utils
{
   public class lfFiltr
    {
       public List<Range> ranges;
       public List<int> values;
       public List<int> allvalues;
       public lfFiltr(string filtr)  // 100-199, 217, 218
       {
           ranges = new List<Range>();
           values = new List<int>();
           allvalues = new List<int>();
           string[] items = filtr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
           for (int i = 0; i < items.Length; i++)
           {
               if (items[i].LastIndexOf('-') == -1)
               {
                   int v;
                   if (int.TryParse(items[i], out v))
                   { values.Add(v); allvalues.Add(v); }
               }
               else
               {
                   string[] d = items[i].Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                   if (d.Length > 1)
                   {
                       int f;
                       int l;
                       if (int.TryParse(d[0], out f) && int.TryParse(d[1], out l))
                       {
                           Range r = new Range(f, l);
                           ranges.Add(r);
                           allvalues.AddRange(RangeToValues(r));
                       }
                   }
               }
           }
       }

       public bool isRelevant(int value)
       {
           for (int i = 0; i < ranges.Count; i++)
           {
               if (value >= ranges[i].first && value <= ranges[i].last) return true;
           }
           for (int i = 0; i < values.Count; i++)
           {
               if (value == values[i]) return true;
           }
           return false;
       }
       public List<int> RangeToValues(Range r)
       {
           List<int> result = new List<int>();
           if (r.first>r.last) return result;
           for (int i = r.first; i <= r.last; i++) result.Add(i);
           return result;
       }
    }
    public class Range
    {
        public int first;
        public int last;
        public Range(int f, int l)
        {
            first = f; last = l;
        }
    }
}
