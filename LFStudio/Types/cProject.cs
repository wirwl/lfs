using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Windows.Controls;
using System.IO;
using System.Xml;
using lf2dat;
using System.Windows.Media.Imaging;
using System.Windows;
using LFStudio.Types;
namespace LFStudio
{
    [Serializable()]
    // [XmlRoot(Namespace = "urn:Abracadabra")]
    public class cProject
    {
        //[XmlIgnoreAttribute]
        //public List<lfItem> lWeapons = new List<lfItem>();
        //[XmlIgnoreAttribute]
        //public bool isWeaponBitmapsLoaded = false;
       // [XmlIgnoreAttribute]
      //  public List<List<BitmapSource>> lbsCropWeapons= new List<List<BitmapSource>>(); 
        //[XmlIgnoreAttribute]
        //public List<BitmapSource> lbsWeapons = new List<BitmapSource>();
        [XmlIgnoreAttribute]
        public String currentpath;
        [XmlIgnoreAttribute]
        public DatatxtDesc datatxt;
        //[XmlIgnoreAttribute]
        //public List<string> errors=new List<string>();
        public string path_to_exe;
        public string path_to_folder;
        public string pass;
        public bool PressGameStartAfterApplicationRun;
        //  [XmlArrayAttribute("Items",Namespace)]
        //  [XmlElement(Type = typeof(string)),XmlElement(Type = typeof(ArrayList))]
        //   [XmlElement(Namespace = "urn:Whoohoo")]
        public ArrayList files = new ArrayList();
        internal cProject()
        {
        }
        public static void SaveProject(string p, cProject cp)
        {
            try
            {
                /*   XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                   ns.Add("", "urn:Abracadabra");
                   ////////////////////////////////////
                   XmlAttributeOverrides xOver = new XmlAttributeOverrides();
                   XmlAttributes xAttrs1 = new XmlAttributes();                
                   XmlRootAttribute xRoot = new XmlRootAttribute() { Namespace = "" };
                   xAttrs1.XmlRoot = xRoot;
                   xOver.Add(typeof(cProject), xAttrs1);
                   XmlAttributes xAttrs2 = new XmlAttributes();
                   var xElt = new XmlElementAttribute(typeof(string)) { Namespace = "" };
                   xAttrs2.XmlElements.Add(xElt);
                
                   XmlAttributes xAttrs11 = new XmlAttributes();
                   XmlRootAttribute xRoot1 = new XmlRootAttribute() { Namespace = "" };
                   xAttrs11.XmlRoot = xRoot1;
                   xOver.Add(typeof(cProject), xAttrs11);
                   XmlAttributes xAttrs22 = new XmlAttributes();
                   var xElt1 = new XmlElementAttribute(typeof(ArrayList)) { Namespace = "" };
                   xAttrs22.XmlElements.Add(xElt1);


                   xOver.Add(typeof(cProject), "file", xAttrs2);
                   xOver.Add(typeof(cProject), "folder", xAttrs22);
                  ////////////*/
                cProject myObject = cp;
                XmlSerializer mySerializer = new XmlSerializer(typeof(cProject));
                StreamWriter myWriter = new StreamWriter(p);
                mySerializer.Serialize(myWriter, myObject);
                myWriter.Close();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public static cProject LoadProject(string fn)
        {
            {
                try
                {
                    if (!File.Exists(fn)) return null;
                    cProject myObject;
                    XmlSerializer mySerializer = new XmlSerializer(typeof(cProject));
                    FileStream myFileStream = new FileStream(fn, FileMode.Open);
                    myObject = (cProject)mySerializer.Deserialize(myFileStream);
                    myObject.currentpath = fn;
                    myFileStream.Close();
                    return myObject;
                }
                catch (Exception ex) { new wException(ex).ShowDialog(); return null; }
            }

        }

    }
}