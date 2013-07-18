using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using lf2dat;
using ICSharpCode.AvalonEdit;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;

namespace LFStudio.Utils
{
    public static class DatFiles
    {
        public static int firstPic = -1;
        public static int lastPic = -1;
        public static DatatxtDesc ParseDatatxt(FileInfo path)
        {
            if (!path.Exists) return null;
            DatatxtDesc result = new DatatxtDesc();
            StreamReader sr = new StreamReader(path.FullName);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0) continue;
                if (line == "<object>")
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0) continue;
                        if (line == "<object_end>") break;
                        ObjectInfo of = new ObjectInfo(GetTagAndIntValue(line, "id:"),
                                                         GetTagAndIntValue(line, "type:"),
                                                         GetTagAndStrValue(line, "file:"));
                        result.lObject.Add(of);

                    }
                if (line == "<background>")
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0) continue;
                        if (line == "<background_end>") break;
                        BackgroundInfo bf = new BackgroundInfo(GetTagAndIntValue(line, "id:"), GetTagAndStrValue(line, "file:"));
                        result.lBackground.Add(bf);
                    }
            }
            sr.Close();
            return result;
        }
        public static DatatxtDesc ParseDatatxt(string st)
        {
            DatatxtDesc result = new DatatxtDesc();
            StringReader sr = new StringReader(st);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim().Length == 0) continue;
                if (line.Trim() == "<object>")
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() == "<object_end>") break;
                        ObjectInfo of = new ObjectInfo(GetTagAndIntValue(line, "id:"),
                                                         GetTagAndIntValue(line, "type:"),
                                                         GetTagAndStrValue(line, "file:"));
                        result.lObject.Add(of);

                    }
                if (line.Trim() == "<background>")
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim() == "<background_end>") break;
                        BackgroundInfo bf = new BackgroundInfo(GetTagAndIntValue(line, "id:"), GetTagAndStrValue(line, "file:"));
                        result.lBackground.Add(bf);
                    }

            }
            sr.Close();
            return result;
        }
        public static DatFileDesc ParseDatFileWithErrorsReturn(FileInfo path, string pass, ref List<gfErrors> errors, cProject cp = null)
        {
            if (!path.Exists) return null;
            errors = new List<gfErrors>();
            string text = functions.DatFileToPlainText(path.FullName, pass);
            return ParseTextWithErrorsReturn(text, path.FullName, ref errors, cp);
        }
        public static DatFileDesc ParseTextWithErrorsReturn(string text, string path, ref List<gfErrors> errors, cProject cp = null)
        {
            DatFileDesc result = null;
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (text.Trim().Length == 0) return null;
                result = new DatFileDesc() { header = new Header(), frames = new List<FrameInfo>(), regions = new List<RegionInfo>() };
                errors = new List<gfErrors>();
                StringReader sr = new StringReader(text);
                string line;
                int nline = 0;
                string oTag = "";
                string cTag = "";
                RegionInfo cri = null;
                WeaponStrListInfoEntry wsl = null;
                bool isRegion = false;
                bool isRegionend = false;
                while ((line = sr.ReadLine()) != null)
                {
                    nline++;
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    //---------------------------------------------------------------------------------------------------------------------------------
                    if (line.Contains("#region"))
                    {
                        cri = new RegionInfo();
                        isRegion = true;
                        oTag = "#region";
                        string[] astr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (astr.Length > 1) cri.caption = astr[1];
                        cri.oline = nline;
                        result.regions.Add(cri);
                        // continue;
                    }
                    if (line.Contains("#endregion"))
                    {
                        cTag = "#endregion";
                        if (oTag != "#region")
                            errors.Add(new gfErrors("Closing tag #endregion found, but where opening tag?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = "";
                        cri.cline = nline;
                        continue;
                    }
                    if (line == "<bmp_begin>") { result.header.foldcaption = line; result.header.oline = nline; oTag = "<bmp_begin>"; continue; }
                    if (line == "<weapon_strength_list>")
                    {
                        result.wsl_oline = nline;
                        if (cTag != "<bmp_end>")
                            errors.Add(new gfErrors("Closing tag <bmp_end> expected", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = "<weapon_strength_list>"; continue;
                    }
                    if (line.Contains("<frame>") || line == "<frame_end>")
                    {

                        if (oTag == "<bmp_begin>")
                            errors.Add(new gfErrors("Closing tag <bmp_end> expected", nline, path, Utils.Project.GetNameProject(cp)));

                        if (oTag == "<weapon_strength_list>")
                            errors.Add(new gfErrors("Closing tag <weapon_strength_list_end> expected", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = "frame"; break;
                    }
                    //---------------------------------------------------------------------------------------------------------------------------------
                    if (line == "<bmp_end>")
                    {
                        if (result.header.cline==-1)
                        result.header.cline = nline;
                        cTag = "<bmp_end>";
                        if (oTag != "<bmp_begin>" && oTag != "#region")
                            errors.Add(new gfErrors("Closing tag <bmp_end> found, but where opening tag?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = "";
                        continue;
                    }
                    if (line == "<weapon_strength_list_end>")
                    {
                        result.wsl_cline = nline;
                        cTag = "<weapon_strength_list_end>";
                        if (oTag != "<weapon_strength_list>")
                            errors.Add(new gfErrors("Closing tag <weapon_strength_list_end> found, but where opening tag?", nline, path, Utils.Project.GetNameProject(cp)));
                        if (oTag == "<bmp_begin>")
                            errors.Add(new gfErrors("Closing tag <bmp_end> expected", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = "";
                        break;
                    }
                    //----------------------------------------Parsing-----------------------------------------------------------------------------------------
                    if (oTag == "<bmp_begin>")
                    {
                        #region Parse bmp_begin
                        if (line.Contains("file"))
                        {
                            FileDesc fd = new FileDesc();
                            string st = GetTextBetweenBraces(line);
                            string[] s2 = st.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (s2.Length > 1)
                            {
                                try
                                {
                                    fd.firstFrame = Convert.ToInt32(s2[0]); fd.lastFrame = Convert.ToInt32(s2[1]);
                                }
                                catch 
                                { 
                                }
                            }
                            List<PropDesc> pd = GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag);
                            if (pd.Count > 0) fd.path = pd[0].value;
                            for (int i = 1; i < pd.Count; i++)
                            {
                                try
                                {
                                    switch (pd[i].name)
                                    {
                                        case "w:": fd.width = Convert.ToInt32(pd[i].value); break;
                                        case "h:": fd.height = Convert.ToInt32(pd[i].value); break;
                                        case "row:": fd.col = Convert.ToInt32(pd[i].value); break;
                                        case "col:": fd.row = Convert.ToInt32(pd[i].value); break;
                                    }
                                }
                                catch
                                {
                                }
                            }
                            result.header.files.Add(fd);
                        }
                        else
                        {
                            PropDesc pd = GetProperty(line);
                            result.header.properties.Add(pd);
                            if (pd.name != null && pd.value != null)
                            {
                                if (!pd.name.Contains("sound") && !pd.name.Contains("file") &&
                                     pd.name != "name:" && pd.name != "head:" && pd.name != "small:")
                                {
                                    if (!isStringConsistsDigits(pd.value))
                                        errors.Add(new gfErrors("Number value expected", nline, path, Utils.Project.GetNameProject(cp)));
                                }
                            }
                        }
                        #endregion
                        continue;
                    }
                    if (oTag == "<weapon_strength_list>")
                    {
                        #region Parse weapon_strength_list block
                        if (line.Contains("entry:"))
                        {
                            wsl = new WeaponStrListInfoEntry();
                            string[] st3 = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            wsl.number = Convert.ToInt32(st3[1]);
                        }
                        else
                        {
                            if (wsl != null)
                            {
                                wsl.props.AddRange(GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag));
                                result.wsl.Add(wsl);
                            }
                        }
                        #endregion
                        continue;
                    }
                    //--------------------------------------End parsing-----------------------------------------------------------------------------------------
                    if (line == "<bmp_end>")
                    {
                        if (oTag != "<bmp_begin>")
                            errors.Add(new gfErrors("Closing tag <bmp_end> found, but where opening tag?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    if (line == "<weapon_strength_list_end>")
                    {
                        if (oTag != "<weapon_strength_list>")
                            errors.Add(new gfErrors("Closing tag <weapon_strength_list_end> found, but where opening tag?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                }
                if (line == null) return result;
                //---------------------------------------End parsing frame_begin and weapon list------------------------------------------------------------------------------------------                                
                if (result.header.files.Count > 0)
                {
                    firstPic = result.header.files[0].firstFrame;
                    lastPic = result.header.files[0].lastFrame;
                    for (int i = 1; i < result.header.files.Count; i++)
                    {
                        if (result.header.files[i].firstFrame < firstPic) firstPic = result.header.files[i].firstFrame;
                        if (result.header.files[i].lastFrame > lastPic) lastPic = result.header.files[i].lastFrame;
                    }
                }

                ///////////////////////////////
                bool isFrame = false;
                bool isFrameend = false;

                FrameInfo cfi = null;
                /*      bool isBpoint = false;
                      bool isBpointend = false;
                      bool isCpoint = false;
                      bool isCpointend = false;
                      bool isWpoint = false;
                      bool isWpointend = false;
                      bool isOpoint = false;
                      bool isOpointend = false;
                      bool isBdy = false;
                      bool isBdyend = false;
                      bool isItr = false;
                      bool isItrend = false;*/
                //FrameInfo prev = new FrameInfo();
                nline--;
                do
                {
                    nline++;
                    line = line.Trim();
                    if (line.Length == 0) continue;

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (line.Contains("#region") || line.Contains("<frame>") ||
                        line == "bpoint:" || line == "wpoint:" || line == "cpoint:" || line == "opoint:" || line == "bdy:" || line == "itr:" ||
                        line == "<frame_end>")
                    {
                        if (oTag == "bpoint:")
                            if (cTag != "bpoint_end:")
                            { cfi.lnst.cl_bpoint = nline - 1; errors.Add(new gfErrors("Closing tag \"bpoint_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        if (oTag == "wpoint:")
                            if (cTag != "wpoint_end:")
                            { cfi.lnst.cl_wpoint = nline - 1; errors.Add(new gfErrors("Closing tag \"wpoint_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        if (oTag == "cpoint:")
                            if (cTag != "cpoint_end:")
                            { cfi.lnst.cl_cpoint = nline - 1; errors.Add(new gfErrors("Closing tag \"cpoint_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        if (oTag == "opoint:")
                            if (cTag != "opoint_end:")
                            { cfi.lnst.cl_opoint = nline - 1; errors.Add(new gfErrors("Closing tag \"opoint_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        if (oTag == "bdy:")
                            if (cTag != "bdy_end:")
                            { cfi.lnst.cl_bdy.Add(nline - 1); errors.Add(new gfErrors("Closing tag \"bdy_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        if (oTag == "itr:")
                            if (cTag != "itr_end:")
                            { cfi.lnst.cl_itr.Add(nline - 1); errors.Add(new gfErrors("Closing tag \"itr_end:\" expected", nline, path, Utils.Project.GetNameProject(cp))); }
                    }
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    if (line.Contains("#region"))
                    {
                        if (isRegion && !isRegionend)
                            errors.Add(new gfErrors("Closing tag #endregion expected", nline, path, Utils.Project.GetNameProject(cp)));
                        isRegion = true; isRegionend = false;
                        oTag = "#region";
                        // if (cri != null) 
                        //    result.regions.Add(cri);
                        cri = new RegionInfo();
                        string[] astr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (astr.Length > 1) cri.caption = astr[1];
                        cri.oline = nline;
                        result.regions.Add(cri);
                        continue;
                    }
                    if (line.Contains("<frame>"))
                    {
                        if (line.Contains("dummy"))
                        {
                        }
                        if (isFrame == true && isFrameend == false)
                        { cfi.cline = nline - 1; errors.Add(new gfErrors("Closing tag <frame_end> expected", nline, path, Utils.Project.GetNameProject(cp))); }
                        isFrame = true; isFrameend = false;
                        oTag = "<frame>";
                        if (cfi != null)
                        {
                            //prev = cfi;
                            result.frames.Add(cfi);
                        }
                        cfi = new FrameInfo();
                        string[] astr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (astr.Length > 1) try 
                        { 
                            cfi.number = Convert.ToInt32(astr[1]); 
                        }                            
                        catch 
                        { 
                            cfi.number = -1; 
                        }
                        ////////////////    

                        for (int i = 0; i < result.frames.Count; i++)
                        {
                            if (cfi.number == result.frames[i].number)
                            {
                                errors.Add(new gfErrors("Frame with this number value already exists", nline, path, Utils.Project.GetNameProject(cp)));
                                break;
                            }
                        }
                        //if (cfi.number < prev.number)                        
                        //         errors.Add(new gfErrors("Frame number value must be greater than number value in  previous frames", nline, path, Utils.Project.GetNameProject(cp)));                  
                        ////////////////
                        if (astr.Length > 2) cfi.caption = astr[2];
                        else cfi.number = null;
                        cfi.oline = nline;
                        cfi.foldcaption = line;
                        continue;
                    }                    
                    if (line == "bpoint:") { if (oTag == "<frame>") cfi.lastheaderline = nline - 1; oTag = "bpoint:"; cfi.lnst.ol_bpoint = nline; continue; }
                    if (line == "wpoint:") { if (oTag == "<frame>") cfi.lastheaderline = nline - 1; oTag = "wpoint:"; cfi.lnst.ol_wpoint = nline; continue; }
                    if (line == "bdy:")
                    {
                        if (oTag == "<frame>") cfi.lastheaderline = nline - 1;
                        oTag = "bdy:";
                        cfi.lnst.ol_bdy.Add(nline);
                        cfi.bdy.Add(new List<PropDesc>());
                        continue;
                    }
                    if (line == "itr:")
                    {
                        if (oTag == "<frame>") cfi.lastheaderline = nline - 1;
                        oTag = "itr:";
                        cfi.lnst.ol_itr.Add(nline);
                        cfi.itr.Add(new List<PropDesc>());
                        continue;
                    }
                    if (line == "cpoint:") { if (oTag == "<frame>") cfi.lastheaderline = nline - 1; oTag = "cpoint:"; cfi.lnst.ol_cpoint = nline; continue; }
                    if (line == "opoint:") { if (oTag == "<frame>") cfi.lastheaderline = nline - 1; oTag = "opoint:"; cfi.lnst.ol_opoint = nline; continue; }
                    //////////////////////////////////////////////////////////
                    #region Check errors (if open tag miss)
                    if (line == "#endregion")
                    {
                        if (isRegion == false) errors.Add(new gfErrors("Found #endregion tag but where opening tag #region?", nline, path, Utils.Project.GetNameProject(cp)));
                        isRegionend = true; oTag = "";
                        if (isRegion == true && isRegion == true) isRegion = false;
                        if (cri != null)
                            cri.cline = nline;
                        continue;
                    }
                    if (line == "<frame_end>")
                    {
                        if (cfi.lastheaderline==-1)
                        cfi.lastheaderline = nline - 1;
                        if (isFrame == false) errors.Add(new gfErrors("Found <frame_end> tag but where opening tag <frame>?", nline, path, Utils.Project.GetNameProject(cp)));
                        isFrameend = true; oTag = "";
                        if (isFrame == true && isFrameend == true) isFrame = false;
                        if (cfi != null)
                            cfi.cline = nline;
                        //    continue;
                    }
                    if (line == "bpoint_end:")
                    {
                        cfi.lnst.cl_bpoint = nline;
                        if (oTag != "bpoint:")
                            errors.Add(new gfErrors("Found \"bpoint_end:\" tag but where opening tag \"bpoint:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        //isBpoint = true;
                        //oTag = ""; 
                        cTag = "bpoint_end:";
                        continue;
                    }
                    if (line == "wpoint_end:")
                    {
                        cfi.lnst.cl_wpoint = nline;
                        if (oTag != "wpoint:")
                            errors.Add(new gfErrors("Found \"wpoint_end:\" tag but where opening tag \"wpoint:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    if (line == "bdy_end:")
                    {
                        cfi.lnst.cl_bdy.Add(nline);
                        if (oTag != "bdy:")
                            errors.Add(new gfErrors("Found \"bdy_end:\" tag but where opening tag \"bdy:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    if (line == "itr_end:")
                    {
                        cfi.lnst.cl_itr.Add(nline);
                        if (oTag != "itr:")
                            errors.Add(new gfErrors("Found \"itr_end:\" tag but where opening tag \"itr:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    if (line == "cpoint_end:")
                    {
                        cfi.lnst.cl_cpoint = nline;
                        if (oTag != "cpoint:")
                            errors.Add(new gfErrors("Found \"cpoint_end:\" tag but where opening tag \"cpoint:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    if (line == "opoint_end:")
                    {
                        cfi.lnst.cl_opoint = nline;
                        if (oTag != "opoint:")
                            errors.Add(new gfErrors("Found \"opoint_end:\" tag but where opening tag \"opoint:\"?", nline, path, Utils.Project.GetNameProject(cp)));
                        oTag = ""; continue;
                    }
                    #endregion
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    if (oTag == "<frame>")
                    {
                        if (cfi.firstheaderline == -1) cfi.firstheaderline = nline;
                        cfi.header.AddRange(GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag)); continue;
                    }
                    if (oTag == "bpoint:" && cfi != null) { cfi.bpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag))); continue; }
                    if (oTag == "wpoint:" && cfi != null) { cfi.wpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag))); continue; }
                    if (oTag == "bdy:" && cfi != null)
                    {
                        cfi.bdy[cfi.bdy.Count - 1].AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag)));
                        continue;
                    }
                    if (oTag == "itr:" && cfi != null)
                    {
                        cfi.itr[cfi.itr.Count - 1].AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag)));
                        continue;
                    }
                    if (oTag == "cpoint:" && cfi != null) { cfi.cpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag))); continue; }
                    if (oTag == "opoint:" && cfi != null) { cfi.opoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp), oTag))); continue; }



                }
                while ((line = sr.ReadLine()) != null);
                if (isRegion == true && isRegionend == false)
                    errors.Add(new gfErrors("Closing tag #endregion expected", nline, path, Utils.Project.GetNameProject(cp)));

                if (isFrame == true && isFrameend == false)
                    errors.Add(new gfErrors("Closing tag <frame_end> expected", nline, path, Utils.Project.GetNameProject(cp)));
                sw.Stop();
                result.frames.Add(cfi);
                //     G.mainWindow.teOutput.AppendText
                //     ("ParseDatFileFromString-время прошло: " + sw.ElapsedMilliseconds + " миллисекунд" + Environment.NewLine);
                //     G.mainWindow.teOutput.ScrollToEnd();

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
            return result;
        }
        #region Old code
        /*
        public static DatFileDesc ParseDatFileWithErrorsReturn(string s, string path, cProject cp, ref List<gfErrors> errors)
        {
            #region 1
            DatFileDesc result = new DatFileDesc() { header = new Header(), frames = new List<FrameInfo>() };
            errors = new List<gfErrors>();

            StringReader sr = new StringReader(s);
            string line;
            int nline = 0;
            ////////////////Init Parse bmp_begin/////////////////////////////////////////////////////////
            bool isBmpbeginfound = false;
            bool isBmpclosefound = false;
            while ((line = sr.ReadLine()) != null)
            {
                nline++;
                if (line.Contains("<bmp_begin>")) { isBmpbeginfound = true; break; }
                if (line.Contains("frame") || line.Contains("<weapon_strength_list>") || line.Contains("<bmp_end>")) break;
            }
            if (isBmpbeginfound)
            {
                while ((line = sr.ReadLine()) != null)
                {
                    nline++;
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    if (line == "<bmp_end>") 
                    { isBmpclosefound = true; break; }
                    if (line.Contains("<frame>"))
                        break;
                    if (line == "<weapon_strength_list>" || line == "<weapon_strength_list_end>")
                        break;
                    if (line.Contains("file"))
                    {
                        FileDesc fd = new FileDesc();
                        string st = GetTextBetweenBraces(line);
                        string[] s2 = st.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        if (s2.Length > 1) { fd.firstFrame = Convert.ToInt32(s2[0]); fd.lastFrame = Convert.ToInt32(s2[1]); }
                        List<PropDesc> pd = GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp));
                        if (pd.Count > 0) fd.path = pd[0].value;
                        for (int i = 1; i < pd.Count; i++)
                        {
                            switch (pd[i].name)
                            {
                                case "w:": fd.width = Convert.ToInt32(pd[i].value); break;
                                case "h:": fd.height = Convert.ToInt32(pd[i].value); break;
                                case "row:": fd.col = Convert.ToInt32(pd[i].value); break;
                                case "col:": fd.row = Convert.ToInt32(pd[i].value); break;
                            }

                        }
                        result.header.files.Add(fd);
                    }
                    else
                    {
                        result.header.properties.Add(GetProperty(line));
                    }

                }
                  if (isBmpclosefound==false)
                      errors.Add(new gfErrors() { Description = "Tag <bmp_end> not found", Line = nline, File = path, Project = Utils.Project.GetNameProject(cp) });
            }
            else errors.Add(new gfErrors() { Description = "Tag <bmp_begin> not found", Line = nline, File = path, Project = Utils.Project.GetNameProject(cp) });
            ////////////////End Parse bmp_begin/////////////////////////////////////////////////////////       
            ////////////////Begin Parse weapon strength list////////////////////////////////////////////
            bool isWsl = false;
            bool isWsl_end=false;
            bool isFrame = false; 
            bool isFrameend = false;
            #region
            if (line == "<weapon_strength_list>")
            {
                
                isWsl = true;
                WeaponStrListInfoEntry wsl = null;
                while ((line = sr.ReadLine()) != null)
                {
                    nline++;
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    if (line == "<weapon_strength_list_end>") { isWsl_end = true; break; }
                    if (line.Contains("<frame>")) { isFrame = true; break; }
                    if (line.Contains("entry:"))
                    {
                        if (wsl != null) result.wsl.Add(wsl);
                        wsl = new WeaponStrListInfoEntry();
                        string[] st3 = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (st3.Length > 1) wsl.number = Convert.ToInt32(st3[1]);
                    }
                    else wsl.props.AddRange(GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)));
                }
            }
            #endregion
            if (isWsl==false)
            while ((line = sr.ReadLine()) != null)
            {
                nline++;
                line = line.Trim();
                if (line.Length == 0) continue;
                if (line.Contains("<frame_end>")) 
                { isFrameend = true; break; }
                if (line.Contains("<frame>")) { isFrame = true; break; }                
                if (line == "<weapon_strength_list_end>") 
                { isWsl_end = true; break; }
                if (line == "<weapon_strength_list>")
                {
                    isWsl = true;
                    WeaponStrListInfoEntry wsl = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        nline++;
                        line = line.Trim();
                        if (line.Length == 0) continue;
                        if (line == "<weapon_strength_list_end>") 
                        { isWsl_end = true; break; }
                        if (line.Contains("<frame>")) { isFrame = true; break; }
                        if (line.Contains("<frame_end>")) 
                        { isFrameend = true; break; }
                        if (line.Contains("entry:"))
                        {
                            if (wsl != null) result.wsl.Add(wsl);
                            wsl = new WeaponStrListInfoEntry();
                            string[] st3 = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (st3.Length > 1) wsl.number = Convert.ToInt32(st3[1]);
                        }
                        else wsl.props.AddRange(GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)));
                    }
                }
            }
            if (isWsl == true && isWsl_end == false)
                errors.Add(new gfErrors() { Description = "Closing tag <weapon_strength_list_end> not found", Line = nline, File = path, Project = Utils.Project.GetNameProject(cp) });
            if (isWsl == false && isWsl_end == true)
                errors.Add(new gfErrors() { Description = "Opening tag <weapon_strength_list> not found", Line = nline, File = path, Project = Utils.Project.GetNameProject(cp) });
            ////////////////End Parse weapon strength list////////////////////////////////////////////
            string oTag = "";
            FrameInfo cfi = null;
            if (line == null)
            { }
            if (line.Contains("<frame_end>"))
            {
                if (isFrame == false) errors.Add(new gfErrors("Found <frame_end> tag but where opening tag <frame>?", nline, path, Utils.Project.GetNameProject(cp)));
            }
            if (line.Contains("<frame>"))
            {
                oTag = "<frame>";
                if (cfi != null) result.frames.Add(cfi);
                cfi = new FrameInfo();
                string[] astr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (astr.Length > 1) cfi.number = Convert.ToInt32(astr[1]);
                else cfi.number = null;
            }
            #endregion
            while ((line = sr.ReadLine()) != null)
            {
                nline++;
                line = line.Trim();
                if (line.Length == 0) continue;
                if (line.Contains("<frame>"))
                {
                    if (isFrame == true && isFrameend == false) 
                        errors.Add(new gfErrors("Closing tag <frame_end> not found",nline,path,Utils.Project.GetNameProject(cp)));
                    isFrame = true; isFrameend = false;
                    oTag = "<frame>";
                    if (cfi != null) result.frames.Add(cfi);
                    cfi = new FrameInfo();
                    string[] astr = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (astr.Length > 1) cfi.number = Convert.ToInt32(astr[1]);
                    else cfi.number = null;
                    continue;
                }

                if (line == "bpoint:") { oTag = "bpoint:"; continue; }
                if (line == "wpoint:") { oTag = "wpoint:"; continue; }
                if (line == "bdy:") { oTag = "bdy:"; continue; }
                if (line == "itr:") { oTag = "itr:"; continue; }
                if (line == "cpoint:") { oTag = "cpoint:"; continue; }
                if (line == "opoint:") { oTag = "opoint:"; continue; }

                if (line == "<frame_end>")
                {
                    if (isFrame == false) errors.Add(new gfErrors("Found <frame_end> tag but where opening tag <frame>?", nline, path, Utils.Project.GetNameProject(cp)));
                  isFrameend = true; oTag = "";
                  if (isFrame == true && isFrameend == true) isFrame = false;
                  continue; 
                }
                if (line == "bpoint_end:") { oTag = ""; continue; }
                if (line == "wpoint_end:") { oTag = ""; continue; }
                if (line == "bdy_end:") { oTag = ""; continue; }
                if (line == "itr_end:") { oTag = ""; continue; }
                if (line == "cpoint_end:") { oTag = ""; continue; }
                if (line == "opoint_end:") { oTag = ""; continue; }

                if (oTag == "<frame>") { cfi.header.AddRange(GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp))); continue; }
                if (oTag == "bpoint:") { cfi.bpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
                if (oTag == "wpoint:") { cfi.wpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
                if (oTag == "bdy:") { cfi.bdy.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
                if (oTag == "itr:") { cfi.itr.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
                if (oTag == "cpoint:") { cfi.cpoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
                if (oTag == "opoint:") { cfi.opoint.AddRange((GetPropertiesFromString(line, ref errors, nline, path, Utils.Project.GetNameProject(cp)))); continue; }
            }
            if (isFrame == true && isFrameend == false)
                errors.Add(new gfErrors("Closing tag <frame_end> not found", nline, path, Utils.Project.GetNameProject(cp)));
            return result;
        }
        */
        #endregion
        public static string GetTextBetweenBraces(string line)
        {
            StringBuilder sb = new StringBuilder();
            bool start = false;
            foreach (char s in line)
            {
                if (s == ')') break;
                if (start) sb.Append(s);
                if (s == '(') start = true;
            }
            return sb.ToString();
        }
        public static bool isLineContainsInitTagSecondLevel(string line)
        {
            if (line.Contains("bpoint:") || line.Contains("wpoint:") || line.Contains("bdy:") ||
                            line.Contains("itr:") || line.Contains("cpoint:") || line.Contains("opoint:")) return true;
            else return false;
        }
        public static List<string> SplitPropertiesAndValues(string st)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder("");
            foreach (char c in st)
            {

                sb.Append(c);
                if (c == ' ')
                {
                    if (sb.ToString().Trim().Length > 0)
                        result.Add(sb.ToString()); sb.Clear();
                }

                if (c == ':')
                {
                    if (sb.ToString().Trim().Length > 0)
                        result.Add(sb.ToString()); sb.Clear();
                }
            }
            if (sb.ToString().Trim().Length > 0) result.Add(sb.ToString());
            return result;
        }
        private static List<PropDesc> GetPropertiesFromString(string st, ref List<gfErrors> errors, int nline, string path, string projectname, string oTag)
        {
            List<PropDesc> result = new List<PropDesc>();
            string[] words = st.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //List<string> words=SplitPropertiesAndValues(st);
            PropDesc current = new PropDesc(null, null);
            byte skip = 0;

            foreach (string s in words)
            {
                // if (skip > 0) { skip--; continue; }
                if (s == "catchingact:") { skip = 2; }
                if (s == "caughtact:") { skip = 2; }
                if (s[s.Length - 1] == ':') //if tag name
                {
                    if (s.Length < 2)
                        errors.Add(new gfErrors() { Description = "Whitespace before colon", Line = nline, File = path, Project = projectname });
                    else
                    {
                        if (current.value == null && current.name != null)
                        {
                            result.Add(current);
                            errors.Add(new gfErrors() { Description = "Property name found but where value?", Line = nline, File = path, Project = projectname });
                        }
                        current = new PropDesc(s, null);
                        if (s == words[words.Length - 1])//if last tag hasn't value
                        {
                            errors.Add(new gfErrors() { Description = "Property name found but where value?", Line = nline, File = path, Project = projectname });
                            result.Add(current);
                        }
                    }
                }
                else   //if value
                {
                    if (current.name == null)
                        errors.Add(new gfErrors() { Description = "Value found but where Property name?", Line = nline, File = path, Project = projectname });
                    if (skip > 0)
                    { current.value += ' ' + s; skip--; }
                    else current.value = s;
                    if (skip == 0)
                    {
                        result.Add(current);
                        #region Check pic
                        //                    if (current.name == "pic:")
                        {
                            //                            try
                            {
                                //                                int cv = Convert.ToInt32(current.value);
                                //                                if (cv < firstPic || cv > lastPic)
                                //                                    errors.Add(new gfErrors() { Description = "Wrong value for property 'pic'", Line = nline, File = path, Project = projectname });
                            }
                            //                          catch { }
                        }
                        #endregion
                        #region Check property value
                        if (current.name != null)
                            if (!current.name.Contains("sound") && !current.name.Contains("file"))
                            {
                                if (!isStringConsistsDigits(current.value))
                                    errors.Add(new gfErrors() { Description = "Number value expected", Line = nline, File = path, Project = projectname });
                            }
                        #endregion
                        current = new PropDesc(null, null);
                    }
                }
            }
            if (errors.Count > 0)
            {
            }
            return result;
        }
        public static bool isStringConsistsDigits(string st)
        {
            foreach (char c in st)
                if (!char.IsDigit(c))
                    if (c != ' ' && c != '-' && c != '+' && c != '.')
                        return false;
            return true;
        }
        private static PropDesc GetProperty(string st)
        {
            string[] astr = st.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (astr.Length == 1) return new PropDesc(astr[0], "");
            if (astr.Length > 1)
            {
                return new PropDesc(astr[0], astr[1]);
            }
            return new PropDesc();
        }
        private static int? GetTagAndIntValue(string str, string tagname)
        {
            int pos = str.IndexOf(tagname);
            if (pos == -1) return null;
            StringBuilder sb = new StringBuilder();
            for (int i = pos + tagname.Length; i < str.Length; i++)
            {
                if (str[i] == ' ' || Char.IsDigit(str[i]))
                    sb.Append(str[i]);
                else
                {
                    return Convert.ToInt32(sb.ToString());
                }
            }
            return null;
        }
        private static string GetTagAndStrValue(string str, string tagname)
        {
            int pos = str.IndexOf(tagname);
            StringBuilder sb = new StringBuilder();
            bool SecondWhiteSpace = false;
            for (int i = pos + tagname.Length; i < str.Length; i++)
            {
                if (str[i] == ' ' && SecondWhiteSpace) break;
                sb.Append(str[i]);
                if (str[i] != ' ') SecondWhiteSpace = true;
            }
            return sb.ToString().Trim();
        }
    }
    public static class DatFileTextEditor
    {
        static string space1 = " ";
        static string space2 = "  ";
        static string space3 = "   ";
        static string space6 = "      ";
        public static void ChangePicInCurrentFrame(TextEditor te, int pic)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].header, "pic:", pic.ToString());
            string text = space3 + ListPropDescToString(oi.data.frames[index].header) + Environment.NewLine;
            DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].oline];
            int lcount = oi.data.frames[index].lastheaderline - oi.data.frames[index].firstheaderline;
            te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
            te.Select(dlFirst.Offset, 0);
        }
        public static void ChangeBpointInCurrentFrame(TextEditor te, int x, int y)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            string text =
            space3 + "bpoint:" + Environment.NewLine + space6 + "x: " + x.ToString() + space2 + "y: " + y.ToString() + Environment.NewLine + space3 + "bpoint_end:" + Environment.NewLine;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (oi.data.frames[index].lnst.ol_bpoint != -1)
            {
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_bpoint - 1];
                int lcount = oi.data.frames[index].lnst.cl_bpoint - oi.data.frames[index].lnst.ol_bpoint;
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }
            else
            {
                DocumentLine dl = te.Document.Lines[oi.data.frames[index].lastheaderline];
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);              
            }
        }
        public static void ChangeWpointInCurrentFrame(TextEditor te, int x, int y, int cover)
        {
            ChangeWpointInCurrentFrame(te, int.MaxValue, x, y,cover);
        }
        public static void ChangeWpointInCurrentFrame(TextEditor te, int weaponact)
        {
            ChangeWpointInCurrentFrame(te, int.MaxValue, int.MaxValue,int.MaxValue, int.MaxValue, weaponact);    
        }
        public static void ChangeWpointInCurrentFrame(TextEditor te, int kind, int x, int y, int cover, int weaponact=-1)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (kind!=int.MaxValue)            
                ChangeValuesInTagOrAdd(ref oi.data.frames[index].wpoint, "kind:", kind.ToString());
            if (x != int.MaxValue)            
                ChangeValuesInTagOrAdd(ref oi.data.frames[index].wpoint, "x:", x.ToString());
            if (y != int.MaxValue)            
                ChangeValuesInTagOrAdd(ref oi.data.frames[index].wpoint, "y:", y.ToString());
            if (cover != int.MaxValue)            
                ChangeValuesInTagOrAdd(ref oi.data.frames[index].wpoint, "cover:", cover.ToString());
            if (weaponact>-1)
                ChangeValuesInTagOrAdd(ref oi.data.frames[index].wpoint, "weaponact:", weaponact.ToString());
            string text = space3 + "wpoint:" + Environment.NewLine +
                          space6 + ListPropDescToString(oi.data.frames[index].wpoint) + Environment.NewLine +
                          space3 + "wpoint_end:" + Environment.NewLine;
            if (oi.data.frames[index].lnst.ol_wpoint != -1)
            {
                List<PropDesc> lpd = oi.data.frames[index].wpoint;
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_wpoint - 1];
                int lcount = oi.data.frames[index].lnst.cl_wpoint - oi.data.frames[index].lnst.ol_wpoint;
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }
            else
            {
                DocumentLine dl = te.Document.Lines[oi.data.frames[index].lastheaderline];
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
        }
        public static void ChangeCpointInCurrentFrame(TextEditor te, int x, int y)
        {
            ChangeCpointInCurrentFrame(te,int.MaxValue,x,y);
        }
        public static void ChangeCpointInCurrentFrame(TextEditor te, int kind, int x, int y)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (kind!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].cpoint, "kind:", kind.ToString());
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].cpoint, "x:", x.ToString());
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].cpoint, "y:", y.ToString());
            string text = space3 + "cpoint:" + Environment.NewLine +
                          space6 + ListPropDescToString(oi.data.frames[index].cpoint) + Environment.NewLine +
                          space3 + "cpoint_end:" + Environment.NewLine;
            if (oi.data.frames[index].lnst.ol_cpoint != -1)
            {
                List<PropDesc> lpd = oi.data.frames[index].cpoint;
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_cpoint - 1];
                int lcount = oi.data.frames[index].lnst.cl_cpoint - oi.data.frames[index].lnst.ol_cpoint;
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }
            else
            {
                DocumentLine dl = te.Document.Lines[oi.data.frames[index].lastheaderline];
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
        }

        public static void ChangeOpointInCurrentFrame(TextEditor te, int x, int y)
        {
            ChangeOpointInCurrentFrame(te,int.MaxValue,x,y);
        }
        public static void ChangeOpointInCurrentFrame(TextEditor te, int kind, int x, int y, int oid=int.MaxValue)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (kind!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].opoint, "kind:", kind.ToString());
            if (x!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].opoint, "x:", x.ToString());
            if (y!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].opoint, "y:", y.ToString());
            if (oid!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].opoint, "oid:", oid.ToString());
            string text = space3 + "opoint:" + Environment.NewLine +
                          space6 + ListPropDescToString(oi.data.frames[index].opoint) + Environment.NewLine +
                          space3 + "opoint_end:" + Environment.NewLine;
            if (oi.data.frames[index].lnst.ol_opoint != -1)
            {
                List<PropDesc> lpd = oi.data.frames[index].opoint;
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_opoint - 1];
                int lcount = oi.data.frames[index].lnst.cl_opoint - oi.data.frames[index].lnst.ol_opoint;
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }
            else
            {
                DocumentLine dl = te.Document.Lines[oi.data.frames[index].lastheaderline];
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
        }
        public static void ChangeOpointInCurrentFrame(TextEditor te, int oid)
        {
            ChangeOpointInCurrentFrame(te, int.MaxValue, int.MaxValue, int.MaxValue, oid);  
        }
        public static void ChangeCenterXYInCurrentFrame(TextEditor te, int cx, int cy)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].header, "centerx:", cx.ToString());
            ChangeValuesInTagOrAdd(ref oi.data.frames[index].header, "centery:", cy.ToString());
            string text = ListPropDescToString(oi.data.frames[index].header) + Environment.NewLine;
            DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].oline];
            int lcount = oi.data.frames[index].lastheaderline - oi.data.frames[index].firstheaderline;
            te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
            te.Select(dlFirst.Offset, 0);
        }        
        public static void ChangeCurrentItrInCurrentFrame(TextEditor te, Rect itr, int itrindex)
        {
            ChangeCurrentItrInCurrentFrame(te, int.MaxValue,itr, itrindex);
        }
        public static void ChangeCurrentItrInCurrentFrame(TextEditor te, int kind, Rect itr, int itrindex, int effect=int.MaxValue)
        {
            if (itr == Rect.Empty) return;
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (itrindex < 0 && itrindex >= oi.data.frames[index].itr.Count) return;
            List<PropDesc> lpd = oi.data.frames[index].itr[itrindex];
            if (kind!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref lpd, "kind:", kind.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "x:", itr.X.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "y:", itr.Y.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "w:", itr.Width.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "h:", itr.Height.ToString());
            if (effect!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref lpd, "effect:", effect.ToString());
            oi.data.frames[index].itr[itrindex] = lpd;
            string text = space3 + "itr:" + Environment.NewLine +
                      space6 + ListPropDescToString(lpd) + Environment.NewLine +
                      space3 + "itr_end:" + Environment.NewLine;
            if (oi.data.frames[index].lnst.ol_itr[itrindex] != -1)
            {
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_itr[itrindex] - 1];
                int lcount = oi.data.frames[index].lnst.cl_itr[itrindex] - oi.data.frames[index].lnst.ol_itr[itrindex];
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }        
        }
        public static void ChangeCurrentBdyInCurrentFrame(TextEditor te, int kind, Rect bdy, int bindex)
        {
            if (bdy == Rect.Empty) return;
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (bindex < 0 && bindex >= oi.data.frames[index].bdy.Count) return;
            List<PropDesc> lpd = oi.data.frames[index].bdy[bindex];
            if (kind!=int.MaxValue)
            ChangeValuesInTagOrAdd(ref lpd, "kind:", kind.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "x:", bdy.X.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "y:", bdy.Y.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "w:", bdy.Width.ToString());
            ChangeValuesInTagOrAdd(ref lpd, "h:", bdy.Height.ToString());
            oi.data.frames[index].bdy[bindex] = lpd;
            string text = space3 + "bdy:" + Environment.NewLine +
                      space6 + ListPropDescToString(lpd)+ Environment.NewLine +
                      space3 + "bdy_end:" + Environment.NewLine;
            if (oi.data.frames[index].lnst.ol_bdy[bindex] != -1)
            {                
                DocumentLine dlFirst = te.Document.Lines[oi.data.frames[index].lnst.ol_bdy[bindex] - 1];
                int lcount = oi.data.frames[index].lnst.cl_bdy[bindex] - oi.data.frames[index].lnst.ol_bdy[bindex];
                te.Document.Replace(dlFirst.Offset, LenghtForGivenLinesCount(dlFirst, lcount), text);
                te.Select(dlFirst.Offset, 0);
            }
        }
        public static void ChangeValuesInTagOrAdd(ref List<PropDesc> lpd, string propname, string newvalue, bool isAddIfNew = true)
        {
            for (int i = 0; i < lpd.Count; i++)
            {
                if (lpd[i].name == propname) { lpd[i] = new PropDesc(lpd[i].name, newvalue); return; }
            }
            if (isAddIfNew) lpd.Add(new PropDesc(propname, newvalue));
        }
        public static string ListPropDescToString(List<PropDesc> lpd)
        {
            string rez = lpd[0].name + space1 + lpd[0].value;
            for (int i = 1; i < lpd.Count; i++)
            {
                rez += space2 + lpd[i].name + space1 + lpd[i].value;
            }
            return rez;
        }
        public static int LenghtForGivenLinesCount(DocumentLine dlFirst, int count)
        {
            int sum = dlFirst.TotalLength;
            DocumentLine dl = dlFirst;
            for (int i = 0; i < count; i++)
            {
                dl = dl.NextLine;
                if (dl == null) break;
                sum += dl.TotalLength;
            }
            return sum;
        }
        public static void PasteBdyInCurrentFrame(TextEditor te, int kind, Rect bdy)
        {
            PasteRegionableTagInCurrentFrame(te, kind, bdy);
        }        
        public static void PasteItrInCurrentFrame(TextEditor te, int kind, int effect, Rect itr)
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            string text =
                space3 + "itr:" + Environment.NewLine +
                space6 + "kind: " + kind.ToString() + space2 + "x: " + itr.X.ToString() + space2 + "y: " + itr.Y.ToString() + space2 + "w: " + itr.Width.ToString() + space2 + "h: " + itr.Height.ToString() + space2 + "effect: " + effect.ToString() + Environment.NewLine +
                space3 + "itr_end:" + Environment.NewLine;
            ObjectInfo oi = te.oi as ObjectInfo;
            int count = oi.data.frames[index].itr.Count;
            if (count > 0)
            {
                List<int> cl_tag = oi.data.frames[index].lnst.cl_itr;
                cl_tag = oi.data.frames[index].lnst.cl_itr;
                int i = cl_tag[count - 1];
                DocumentLine dl = te.Document.GetLineByNumber(i + 1);
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
            else
            {
                int i = oi.data.frames[index].cline;
                DocumentLine dl = te.Document.GetLineByNumber(i);
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
        }
        private static void PasteRegionableTagInCurrentFrame(TextEditor te, int kind, Rect reg, string tag = "bdy")
        {
            if (te == null) return;
            DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
            int index = WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
            if (index == -1) return;
            string text = space3 + tag + ":" + Environment.NewLine + 
                          space6 + "kind: " + kind.ToString() + space2 + "x: " + reg.X.ToString() + space2 + "y: " + reg.Y.ToString() + space2 + "w: " + reg.Width.ToString() + space2 + "h: " + reg.Height.ToString() + Environment.NewLine + 
                          space3 + tag + "_end:" + Environment.NewLine;
            ObjectInfo oi = te.oi as ObjectInfo;
            List<List<PropDesc>> tagobject = oi.data.frames[index].bdy;
            if (tag == "itr") tagobject = oi.data.frames[index].itr;
            int count = tagobject.Count;

            if (count > 0)
            {
                List<int> cl_tag = oi.data.frames[index].lnst.cl_bdy;
                if (tag == "itr") cl_tag = oi.data.frames[index].lnst.cl_itr;
                int i = cl_tag[count - 1];
                DocumentLine dl = te.Document.GetLineByNumber(i + 1);
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
            else
            {
                int i = oi.data.frames[index].cline;
                DocumentLine dl = te.Document.GetLineByNumber(i);
                te.Document.Insert(dl.Offset, text);
                te.Select(dl.Offset, 0);
            }
        }
        public static int WhatFrameFromLineNumber(ObjectInfo oi, int ln)
        {
            if (oi == null) return -1;
            for (int i = 0; i < oi.data.frames.Count; i++)
            {
                if (ln >= oi.data.frames[i].oline && ln <= oi.data.frames[i].cline)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}