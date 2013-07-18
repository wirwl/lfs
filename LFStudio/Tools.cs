using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace LFStudio
{
    [Serializable]
    public class Tools
    {
        internal Tools() { }
        public List<Tool> tools = new List<Tool>();
        public static void SaveProject(string path, Tools cp)
        {
            try
            {
                Tools myObject = cp;
                XmlSerializer mySerializer = new XmlSerializer(typeof(Tools),new Type[]{typeof(Tool)});
                StreamWriter myWriter = new StreamWriter(path);
                mySerializer.Serialize(myWriter, myObject);
                myWriter.Close();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public static Tools LoadTools(string path)
        {
            {
                try
                {
                    if (!File.Exists(path)) return null;
                    Tools myObject;
                    XmlSerializer mySerializer = new XmlSerializer(typeof(Tools),new Type[]{typeof(Tool)});
                    FileStream myFileStream = new FileStream(path, FileMode.Open);
                    myObject = (Tools)mySerializer.Deserialize(myFileStream);
                    myFileStream.Close();
                    return myObject;
                }
                catch (Exception ex) { new wException(ex).ShowDialog(); return null; }
            }
        }
        public static Tool FindTool(Tools tls, string title)
        {
            foreach(Tool tl in tls.tools)
            {
                if (tl.title == title) return tl;
            }
            return null;
        }
    }
}
