using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace MemoryReader
{
    public class MemoryField : INotifyPropertyChanged
    {        
        private string _Value;
        private string Modifier;
        public string Output{get;set;}
        public string Value
        {
            set
            {
                _Value = value;
                string exp;
                if (Modifier == null || Modifier.Length == 0) exp = value.ToString();
                else
                    exp = Modifier.Replace("value", value.ToString());
                double e=Evaluate(exp);
                switch (OutputType)
                {
                    case "string": Output = e.ToString();break;
                    case "int": Output = ((int)e).ToString();break;
                    case "double": Output = e.ToString(); break;
                    case "datetime": Output = new TimeSpan(0, 0, (int)Math.Round(e)).ToString(); break;
                }
            }
            get
            {
                return _Value;
            }
        }
        public string Address { get; set; }
        public string Type { get; set; }
        public string OutputType { get; set; }
        public string Comment { get; set; }
        public static double Evaluate(string expression)
        {
            //if (expression.Length == 0) return 0;
            return (double)new System.Xml.XPath.XPathDocument
            (
             new System.IO.StringReader("<r/>")).CreateNavigator().Evaluate
              (
                string.Format("number({0})",
new System.Text.RegularExpressions.Regex(@"/\*[^*]*\*+(?:[^/*][^*]*\*+)*/")
                    .Replace(expression, " ${1} ")
                    .Replace("/", " div ")
                    .Replace("%", " mod ")
              )
            );
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public string ToBattleTime(int? n)
        {
            if (n == null) return "error";
            double f = (double)n;
            f = (int)n / 30.0;
            n = (int)Math.Round(f);
            return new TimeSpan(0, 0, (int)n).ToString();
        }
        public MemoryField()
        {

        }
        public static List<MemoryField> LoadData(string fname)
        {
            List<MemoryField> result = new List<MemoryField>();
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + fname;
            if (!File.Exists(path)) return null;
            StreamReader fs = File.OpenText(path);
            List<string> lines = new List<string>();            
            while (true)
            {
                string st = fs.ReadLine();
                if (st == null) break;
                st=st.Trim();
                if (st.Length == 0) continue;
                if (st[0] != '$')
                    lines.Add(st);                
            }
            fs.Close();                        
            for (int i = 0; i < lines.Count; i++)
            {            
                string[] astr = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                MemoryField mf = new MemoryField();
                for (int j = 0; j < astr.Length; j++)
                {
                    int pos = astr[j].IndexOf('=');
                    if (pos == 0) continue;
                    string LeftPart = astr[j].Substring(0, pos);
                    string RightPart = astr[j].Substring(pos+1, astr[j].Length - pos-1);
                    switch (LeftPart)
                    {
                        case "Address": mf.Address = RightPart; break;
                        case "Type": mf.Type = RightPart; break;
                        case "Comment": mf.Comment = RightPart; break;
                        case "Modifier": mf.Modifier = RightPart; break;
                        case "OutputType": mf.OutputType = RightPart; break;
                    }
                }
                result.Add(mf);
            }
            return result ;
        }

    }
}

