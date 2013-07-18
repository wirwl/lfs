using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;
using System.Xml;
using System.Diagnostics;
using System.Windows;
namespace LFStudio
{
    public class Lf2FoldingStrategy : AbstractFoldingStrategy
    {
        public List<string> framenames = new List<string>();
        public List<string> framenumbers = new List<string>();
        public List<string> regions = new List<string>();
        public DatFileDesc dfd;
        public class fold
        {
            internal fold() { }
            internal fold(string s, string e, int istart = -1, int iend = -1, string buf = "")
            { start = s; end = e; this.istart = istart; this.iend = iend; this.buf = buf; }
            public string start;
            public string end;
            public int istart;
            public int iend;
            public string buf;
            public DocumentLine dl = null;
        }
        public List<fold> lFold = new List<fold>();
        public Lf2FoldingStrategy()
        {
            /*lFold.Add(new fold("<frame>","<frame_end>"));//level 1
            lFold.Add(new fold("<weapon_strength_list>", "<weapon_strength_list_end>"));//level 1
            lFold.Add(new fold("<phase>", "<phase_end>"));          //level 2               
            lFold.Add(new fold("<stage>", "<stage_end>"));        //level 1     
            lFold.Add(new fold("#region", "#endregion"));        //level 1                  */
            for (int i = 0; i < LFStudio.G.AppSettings.foldtags.Count; i++)
                lFold.Add(new fold(LFStudio.G.AppSettings.foldtags[i].init_tag, LFStudio.G.AppSettings.foldtags[i].close_tag));

        }
        private bool LineConsist(string[] ls, string obj)
        {
            bool result = false;
            for (int i = 0; i < ls.Length; i++)
            {
                if (ls[i] == obj) result = true;
            }
            return result;
        }
        /*public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            framenames.Clear();
            framenumbers.Clear();
            regions.Clear();           
            List<NewFolding> foldMarkers = new List<NewFolding>();
            firstErrorOffset = 0;
            #region bmp_begin
            int st = -1;
            int end = -1;
            DocumentLine docline=null;
            for (int i=0;i<document.Lines.Count;i++)
            {
                string str = document.GetText(document.Lines[i].Offset,document.Lines[i].Length);               
                string[] astr=str.Split(' ');
                if (st == -1)
                {
                    //if (str.Contains("<bmp_begin>"))
                    if (LineConsist(astr,"<bmp_begin>"))
                    {
                        st = document.Lines[i].Offset;
                        docline = document.Lines[i];
                    }
                }
                else
                    if (LineConsist(astr, "<bmp_end>"))
                    {
                        end = document.Lines[i].Offset+document.Lines[i].Length;
                        foldMarkers.Add(new NewFolding(st, end) { Name = "<bmp_begin>" });
                        st = -1; end = -1;
                        break;
                    } else
                        if (LineConsist(astr, "<frame>"))
                        { st = -1; end = -1; break; }

            }
            #endregion
            //string buf = "";          
            for (int i = 0; i < document.Lines.Count; i++)
            {
                string str = document.GetText(document.Lines[i].Offset, document.Lines[i].Length);
                string[] astr = str.Split(' ');
                for (int k = 0; k < lFold.Count; k++)
                {
                    if (lFold[k].istart == -1)
                    {
                        if (LineConsist(astr, lFold[k].start))
                        {
                            lFold[k].istart = document.Lines[i].Offset;
                            lFold[k].buf = str;
                            lFold[k].dl = document.Lines[i];
                        }
                    }
                    else
                        if (LineConsist(astr, lFold[k].end))
                        {
                            string w3 = GetWord(lFold[k].buf, 2);
                            if (w3 != null  && !Povtor(w3)) framenames.Add(w3);
                            string n2 = GetWord(lFold[k].buf, 1);
                            if (n2 != null && !Povtor(n2))
                                if (lFold[k].buf.Contains("#region"))
                                    regions.Add(n2);
                                else
                                    framenumbers.Add((n2));
                            lFold[k].iend = document.Lines[i].Offset + document.Lines[i].Length;
                            foldMarkers.Add(new NewFolding(lFold[k].istart, lFold[k].iend) 
                            { DefaultClosed = false, Name = lFold[k].buf });
                            lFold[k].istart = -1; lFold[k].iend = -1;
                        }
                        else if (LineConsist(astr, lFold[k].start))
                        { lFold[k].istart = document.Lines[i].Offset; lFold[k].iend = -1; lFold[k].buf = str; continue; }
                }
            }//for
            
            foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            sw.Stop();
            G.mainWindow.teOutput.AppendText
                ("Fold/unfold - время прошло: "+sw.ElapsedMilliseconds+" миллисекунд"+Environment.NewLine);

            return foldMarkers;
        }*/
        private string GetWord(string str, int n)
        {
            string[] ls = new string[3];
            string[] buf = str.Split(' ');
            int k = 0;
            for (int i = 0; i < buf.Length; i++)
                if (buf[i] != "")
                {
                    ls[k] = buf[i];
                    if (k == 2) break;
                    k++;
                }
            return ls[n];
        }
        private bool Povtor(string w)
        {
            bool result = false;
            if (framenames.Count == 0) return false;
            for (int i = 0; i < framenames.Count; i++)
            {
                if (framenames[i] == w) return true;
            }
            return result;
        }
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            try
            {
             //   Stopwatch sw = new Stopwatch();
            //    sw.Start();
                
                List<NewFolding> foldMarkers = new List<NewFolding>();               
                firstErrorOffset = 0;
                if (dfd == null) return foldMarkers;
                // return foldMarkers;
                if (dfd.header.oline != -1 && dfd.header.cline != -1)
                    foldMarkers.Add(
                        new NewFolding(document.GetOffset(dfd.header.oline, 0),
                                       document.GetOffset(dfd.header.cline, document.Lines[dfd.header.cline - 1].TotalLength)) { Name = dfd.header.foldcaption });
                if (dfd.wsl_oline != -1 && dfd.wsl_cline != -1)
                    foldMarkers.Add(
                      new NewFolding(document.GetOffset(dfd.wsl_oline, 0),
                         document.GetOffset(dfd.wsl_cline, document.Lines[dfd.wsl_cline - 1].TotalLength)) { Name = "<weapon_strength_list>" });
                int i = 0;
              
                for (i = 0; i < dfd.frames.Count - 1; i++)
                {
                    if (i == 221)
                    {  }
                    try
                    {
                      //  continue;
                        if (dfd.frames[i].oline == -1 || dfd.frames[i].cline == -1) continue;
                        int start = document.GetOffset(dfd.frames[i].oline, 0);
                        int end = document.GetOffset(dfd.frames[i].cline, document.Lines[dfd.frames[i].cline - 1].TotalLength);                        
                        foldMarkers.Add(new NewFolding(start, end) { Name = dfd.frames[i].foldcaption });
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(i.ToString());
                    }
                }
                try
                {
                    if (dfd.frames.Count != 0)
                    if (dfd.frames[i].oline != -1 && dfd.frames[i].cline != -1)
                    {
                        int start2 = document.GetOffset(dfd.frames[i].oline, 0);
                        int end2 = document.GetOffset(dfd.frames[i].cline, document.Lines[dfd.frames[i].cline - 1].TotalLength + 1);
                        foldMarkers.Add(new NewFolding(start2, end2) { Name = dfd.frames[i].foldcaption });
                    }

                }
                catch (Exception)
                {
                    
                    throw;
                }
                for (i = 0; i < dfd.regions.Count; i++)
                {
                    if (dfd.regions[i].oline == -1 || dfd.regions[i].cline == -1) continue;
                    int start = document.GetOffset(dfd.regions[i].oline, 0);
                    int end = document.GetOffset(dfd.regions[i].cline, document.Lines[dfd.regions[i].cline - 1].TotalLength);
                    foldMarkers.Add(new NewFolding(start, end) { Name = dfd.regions[i].caption });

                }
                foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            //    sw.Stop();
             //   G.mainWindow.teOutput.AppendText
            //    ("Fold/unfold - время прошло: " + sw.ElapsedMilliseconds + " миллисекунд" + Environment.NewLine);
                return foldMarkers;
            }
            catch (Exception ex) 
            { new wException(ex).ShowDialog(); firstErrorOffset = 0; return null; }
        }
    }
}
