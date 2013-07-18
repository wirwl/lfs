using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit;
using System.ComponentModel;
using System.IO;

namespace LFStudio
{
     [Serializable()]
   public class adce
    {         
         public class Frame
         {
             public string init_tag;
             public string close_tag;
             public bool content_in_init_tag;
            // public int level;
             public List<List<string>> body=new List<List<string>>();       
             public List<Frame> subframes=new List<Frame>();
             public Frame() { }
             public Frame(string init, string close="",bool ciit=false) 
             {
                 init_tag=init;
                 close_tag=close;
                 content_in_init_tag = ciit;
             } 
         }
         public class FrameDesc
         {
            public string init_tag;
            public string close_tag;
            public FrameDesc(){}
            public FrameDesc(string it, string ct)
            {
                init_tag=it;
                close_tag=ct;
            }          
         }
         public class textEditorOptions
         {
             private string _EncodingForSavedText="defvalue";
             public string EncodingForSavedText
             { get { return _EncodingForSavedText; } set { _EncodingForSavedText = value; } }

             private byte _FlowDirection=0;
             public byte FlowDirection { get { return _FlowDirection; } set { _FlowDirection = value; } }

             private string _FontFamily="Segoe UI";
             public string FontFamily
             { get { return _FontFamily; } set { _FontFamily = value; } }

             private double _FontSize = 12;
             public double FontSize
             { get { return _FontSize; } set { _FontSize = value; } }

             private string _FontStyle = "Normal";  //Italic // Oblique
             public string FontStyle
             { get { return _FontStyle; } set { _FontStyle = value; } }

             private string _FontWeight = "Normal"; 
             public string FontWeight
             { get { return _FontWeight; } set { _FontWeight = value; } }

             private string _FontStretch = "Normal";
             public string FontStretch
             { get { return _FontStretch; } set { _FontStretch = value; } }

             private bool _WordWrap = false;
             public bool WordWrap
             { get { return _WordWrap; } set { _WordWrap = value; } }

             private bool _ShowLineNumbers = false;
             public bool ShowLineNumbers
             { get { return _ShowLineNumbers; } set { _ShowLineNumbers = value; } }

             public textEditorOptions() { }
         }
         /////////For serialization////////////////////////////////////////         
          [XmlArray(ElementName = "For_autocompletion_window")]          
         public List<Frame> Frames = new List<Frame>();
         [XmlArray(ElementName = "For_fold_system")]          
         public List<FrameDesc> foldtags=new List<FrameDesc>();
         public bool CacheByExtensionEnabled;
         public bool CacheByFileEnabled;
         public string StandardPassword;
         public bool isEnabledActiveLineHighlight;
         public bool isEnabledBackgroundHighlight;
         public string oddLineColor;
         public string evenLineColor;
         public string firstActiveLineColor;
         public string secondActiveLineColor;
         public bool isFixedBackgroundLines;
         [XmlElement(ElementName="AdvancedTextEditorOptions")]
         public TextEditorOptions teAdvOptions;
         [XmlElement(ElementName = "TextEditorOptions")]
         public textEditorOptions teOptions;
         public bool isFoldEnable;
         public bool isSaveFilesAfterAppCrush=true;
         public bool isShowUnderline;         
         public List<string> underlineThisWords;
         public bool isComboboxesInTop = false;
         public string ColorForStandardTheme;
         public string AdvancedTheme;
         public string Language;
         public string BodyBorderColor;
         public double[] BodyDashes;
         public string BodyBackgroundColor;
         public string ItrBorderColor;
         public double[] ItrDashes;
         public string ItrBackgroundColor;
         public string FromColorWpoint;
         public string ToColorWpoint;
         public double DurationWpoint;
         public bool isAutoReverseWpoint;
         public string FromColorCenter;
         public string ToColorCenter;
         public double DurationCenter;
         public bool isAutoReverseCenter;
         public string FromColorOpoint;
         public string ToColorOpoint;
         public double DurationOpoint;
         public bool isAutoReverseOpoint;
         public string FromColorCpoint;
         public string ToColorCpoint;
         public double DurationCpoint;
         public bool isAutoReverseCpoint;
         public bool isSGVTransparent;
         public string SGVColorKey;
         public string sgvBkg = "Transparent";
         public string canvasBkg="Transparent";
         public string canvasBorderBkg = "Transparent";
         public int canvasBorderSize = 0;
         public bool isSGVAnimated;
         public string firstSGVColor="0";
         public string secondSGVColor="0";         
         public double firstSGVColorOffset;        
         public double secondSGVColorOffset;
         public string fromSGVColor1;
         public string toSGVColor1;
         public double DecRatioColor1;
         public double AccRatioColor1;
         public string fromSGVColor2;
         public string toSGVColor2;
         public double DecRatioColor2;
         public double AccRatioColor2;
         public double SGVDuration;
         public bool isSGVreverse;
         public double SGVGradientAngle;
         public bool isEnableDashStyle=true;
         public int lastKindItrIndex = 0;
         public int lastKindCpointIndex = 0;
         public int lastKindOpointIndex = 0;
         public int lastKindWpointIndex = 0;
         public int lastCoverWpointIndex = 0;
         public int lastEffectItrIndex = 0;
         public int scaleLimit = 5;
         public int scaleIndex = 0;
         public string scaleBitmapMode = "NearestNeighbor";
         public int startCanvasWidth=350;
         public int startCanvasHeight=150;
         public int sgvFocusTo = 1;
         public bool isShowAllinSGV = false;
         public bool isShowAllBdy = false;
         public bool isShowAllItr = false;
         public bool isAddFileToTreeViewAfterFlip = true;
         public string filtrForWeapon_id = "100-199,217,218";
         public string filstrForWeapon_type = "1,2,4,6";
         public string filtrForOpoint_type;
         public double baseGameFPS = 30.0;
         public bool isRangeEnabled = false;
         public string Range;
         /////////End serialization/////////////////////////////////////////         
         public adce()
         {
      /*       TextEditor te = new TextEditor();
             teOptions = te.Options;
             TextEditorOptions t = te.Options;
             t.AllowScrollBelowDocument = true;
             t.ConvertTabsToSpaces = true;
             t.CutCopyWholeLine = false;
             t.EnableEmailHyperlinks = false;
             t.EnableHyperlinks = false;
             t.EnableRectangularSelection = false;
             t.EnableTextDragDrop = false;
             t.IndentationSize = 60;
             t.InheritWordWrapIndentation = false;
             t.RequireControlModifierForHyperlinkClick = false;
             t.ShowBoxForControlCharacters = false;
             t.ShowEndOfLine = true;
             t.ShowSpaces = true;
             t.ShowTabs = true;
             t.WordWrapIndentation = 4.3;   
        */                  
             //CacheByExtensionEnabled = true;
             /*      Frame bpoint=new Frame("bpoint:","bpoint_end:");
                   bpoint.body.Add(new List<string>(new string[] { "x: ","y:"}));
            
                   Frame wpoint=new Frame("wpoint:","wpoint_end:");
                   wpoint.body.Add(new List<string>(new string[] { "kind: ", "x: ", "y:", "weaponact: ", "attacking: ", "cover: ", "dvx: ", "dvy: ", "dvz: " }));
            
                   Frame bdy=new Frame("bdy:","bdy_end:");
                   bdy.body.Add(new List<string>(new string[] { "kind: ", "x: ", "y:", "w: ", "h: " }));
            
                   //////////////////////////////////////////////////////////////
                   Frame bmpbegin = new Frame("<bmp_begin>", "<bmp_end>",false);
              * 
                       ((DockManager.ActiveDocument as DocumentContent).Content as TextEditor)
                   Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content)
                   bmpbegin.body.Add(new List<string>(new string[] { "name: "}));
                   bmpbegin.body.Add(new List<string>(new string[] { "head: " }));
                   bmpbegin.body.Add(new List<string>(new string[] { "small: " }));             
                   bmpbegin.body.Add(new List<string>(new string[]{"file(-): ","w: ","h: ","row: ","col: "}));             
                   Frames.Add(bmpbegin);
                   Frame frame = new Frame("<frame>", "<frame_end>", true);
                               frame.body.Add(new List<string>(new string[] { "pic: ","state: ","wait: ","next: "}));
                   frame.subframes.Add(bpoint);
                   frame.subframes.Add(wpoint);
                   frame.subframes.Add(bdy);
                   Frames.Add(frame);
              */
             //  foldtags.Add(new FrameDesc("<frame>","<frame_end>"));
          //   foldtags.Add(new FrameDesc("<weapon_strength_list>", "<weapon_strength_list_end>"));
         }
         public static void Save(string p,adce ad)
         {
             if (ad == null) return;
             XmlSerializer mySerializer = new XmlSerializer(typeof(adce), new Type[] { typeof(adce.Frame), typeof(adce.FrameDesc), typeof(adce.textEditorOptions) });
             StreamWriter myWriter = new StreamWriter(p);
             mySerializer.Serialize(myWriter, ad);
             myWriter.Close();
         }
    }//class adce
   
}
