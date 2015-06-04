#region NameSpaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;
using AvalonDock;
using Borgstrup.EditableTextBlock;
using FindReplace;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using lf2dat;
using LFStudio.Controls;
using LFStudio.Utils;
using Microsoft.Win32;
using myTreeView;
using Tomers.WPF.Localization;
using System.Windows.Media.Animation;
using Microsoft.DwayneNeed.Media.Imaging;
using ZoomAndPanSample;
using LFStudio.Controls.SelectRegion;
using System.Windows.Threading;
using LFStudio.Windows;
using LFStudio.Types;
using Gif.Components;
#endregion

namespace LFStudio
{

    //^~(:Wh@//.+)~(:Wh@\{:Wh@)~(:Wh@\}:Wh@)~(:Wh@/#).+
    public partial class MainWindow : Window
    {
        public char newline = '\n';
        public ObservableCollection<dgExtentedSpriteAnimator> ocESA;
        public BackgroundWorker bwSpawnObjectsLoad;
        public lfFiltr fWId;
        public lfFiltr fWType;
        public lfFiltr fOType;
        public int CurrentNLine = -1;
        private string _FileToPlay;
        public string FileToPlay
        {
            set { _FileToPlay = value; tbAP.Text = Path.GetFileName(value); }
            get { return _FileToPlay; }
        }
        public List<TaskHighlight> Tasks = new List<TaskHighlight>();
        public bool isOpenManyFiles;
        public delegate void DelegateWithoutArguments();
        BackgroundWorker bwLoadPlugins;
        BackgroundWorker bwLoadProjects;
        public int CurrentFrameIndex = -1;
        public int Objectindex = 0;
        public WhereStand Otvet; 
        public bool isFirstMMForKFocus = true;
        public double startX;
        public double startY;
        public Point origZoomAndPanControlMouseDownPoint;
        public Point origContentMouseDownPoint;
        public Point origContentMouseDownPointDouble;
        #region Init
        public TextEditor teCurActiveDocument;
        public Brush ibrush;
        public double ihalfPenWidth;
        public Pen ipen;
        public ColorKeyBitmap shadow;
        public int Old_pic = -1;
        public int Old_nfile = -1;
        //public DrawingVisual dvFrame;
        //public DrawingVisual dvNextFrame;
        //   public List<BitmapImage> lbsCharImages = new List<BitmapImage>();    
        //public List<BitmapSource> lbsCharCropImages = new List<BitmapSource>();
        public Host AppLFStudio;
        [ImportMany(typeof(IPlugin))]
        public IPlugin[] Plugins { get; set; }
        public AggregateCatalog Catalog;
        public CompositionContainer Container;
        //////////////////////////////////////////////////////
        // public List<string> inittags = new List<string>();
        public List<string> subtagsinit = new List<string>();
        public List<string> subtagsclose = new List<string>();
        Lf2FoldingStrategy foldingStrategy;
        //public FoldingManager foldingManager;
        public bool isProject = false;
        public ContextMenuCollection cmc;
        //= new ContextMenuCollection();
        public List<cProject> lProjects = new List<cProject>();
        public List<FileStream> lFileStreams = new List<FileStream>();
        public lfTreeViewItem CurrentTreeviewItem;
        // public int curProject = -1;
        public lfTreeViewItem tviProjects;
        Point _lastMouseDown;
        lfTreeViewItem draggedItem, _target;
        public bool NeedTypeNameForCreateFolder = false;
        public string programfolder;
        //  wFind wf;
        FindReplaceMgr fr;
        ObservableCollection<gfErrors> coll;
        Tools tls;
        List<string> RecentFiles = new List<string>(10);
        int rfCount = 10;
        #endregion
        private BitmapImage _spriteBitmapImage;
        private AnimatedSprite _sprite;

        public MainWindow()
        {
            try
            {
                //using (StreamWriter swText = new StreamWriter(new FileStream("c:\\test1.txt", FileMode.Create)))
                //{ swText.WriteLine(G.dtNow.ToString()); }

                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                InitializeComponent();

                this.Title = "LFStudio " + Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + '.' +
                             Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString() + '.' +
                             Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
                sw1.Stop();
                teOutput.AppendText("Application_Startup: " + G.startup_app.ToString() + " sec.");
                teOutput.AppendText(Environment.NewLine);
                teOutput.AppendText("InitializeComponent(): " + sw1.Elapsed.TotalSeconds.ToString() + " sec.");
                teOutput.AppendText(Environment.NewLine);
                sw1.Restart();
                ////////////////////////////////////
                AppLFStudio = new Host();
                G.mainWindow = this;
                programfolder = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]) + "\\";
                G.CreateObjects();
                ////////////////////////////////////////////////////////
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                sbRunGame.Tag = -1;
                tviProjects = new lfTreeViewItem(lfTreeViewItem.isProjects) { HeaderText = "Projects" };
                /////////////////////////////////////////////////////////////////////////////////// 
                XmlSerializer mySerializer = new XmlSerializer(typeof(adce), new Type[] { typeof(adce.Frame), typeof(adce.FrameDesc), typeof(adce.textEditorOptions) });
                FileStream myFileStream = new FileStream(programfolder + "main.xml", FileMode.Open);
                G.AppSettings = (adce)mySerializer.Deserialize(myFileStream);
                myFileStream.Close();
                /////////////////////////////////////////////////////////////////////////
                #region XAML to C# for plugin system
                AppLFStudio.AppGrid = MainGrid;
                AppLFStudio.wMain = this;
                AppLFStudio.sbRunGame = this.sbRunGame;
                AppLFStudio.bgRenderer = new List<IBackgroundRenderer>();
                //AppLFStudio.bgRenderer.Add(new XBackgroundRenderer(DockManager));
                #endregion
                tvExplorer.KeyDown += new KeyEventHandler(tvExplorer_KeyDown);
                tvExplorer.Items.Add(tviProjects);
                cmc = new ContextMenuCollection();
                (cmc.cmProjects.Items[0] as System.Windows.Controls.MenuItem).Click += new RoutedEventHandler(cnp_Click);
                (cmc.cmProjects.Items[1] as System.Windows.Controls.MenuItem).Click += new RoutedEventHandler(aep_Click);
                cmc.findMenuItemByName(cmc.cmProjects.Items, "Reparse dat-files for all projects from data.txt").Click += new RoutedEventHandler(cse_Click);
                //cmc.findMenuItemByName(cmc.findMenuItemByName(cmc.cmFolder.Items, "Add").Items, "Add new file").Click += new RoutedEventHandler(andf_Click);
                tviProjects.ContextMenu = cmc.cmProjects;
                tviProjects.textBlock.IsEditable = false;
                tviProjects.IsExpanded = true;
                /////////////////////////////                
                tvExplorer.MouseRightButtonUp += new MouseButtonEventHandler(this.myHandler);
                tvExplorer.MouseDoubleClick += new MouseButtonEventHandler(tvExplorer_MouseDoubleClick);
                //---------------------------------------------------------------------------------------------------------------------------
                ((cmc.cmProject.Items[1] as MenuItem).Items[0] as MenuItem).Click += new RoutedEventHandler(andf_Click);//Add new file            
                ((cmc.cmProject.Items[1] as MenuItem).Items[1] as MenuItem).Click += new RoutedEventHandler(aedf_Click);
                ((cmc.cmProject.Items[1] as MenuItem).Items[2] as MenuItem).Click += new RoutedEventHandler(anf_Click);//Add new folder
                //---------------------------------------------------------------------------------------------------------------------------                    
                cmc.findMenuItemByName(cmc.findMenuItemByName(cmc.cmFolder.Items, "Add").Items, "Add new file").Click += new RoutedEventHandler(andf_Click);
                cmc.findMenuItemByName(cmc.findMenuItemByName(cmc.cmFolder.Items, "Add").Items, "Add existing dat file").Click += new RoutedEventHandler(aedf_Click);
                cmc.findMenuItemByName(cmc.findMenuItemByName(cmc.cmFolder.Items, "Add").Items, "Add new virtual folder").Click += new RoutedEventHandler(anf_Click);
                cmc.findMenuItemByName(cmc.cmFolder.Items, "Remove from list").Click += new RoutedEventHandler(delfolder_Click);
                cmc.findMenuItemByName(cmc.cmFolder.Items, "Rename").Click += new RoutedEventHandler(renameitem_Click);
                cmc.findMenuItemByName(cmc.cmFolder.Items, "Clear folder").Click += new RoutedEventHandler(cp_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Run game").Click += new RoutedEventHandler(rungame_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Remove project from list").Click += new RoutedEventHandler(removeproject_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Properties").Click += new RoutedEventHandler(pp_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Clear project").Click += new RoutedEventHandler(cp_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Rename").Click += new RoutedEventHandler(renameitem_Click);
                cmc.findMenuItemByName(cmc.cmProject.Items, "Import files in project").Click += new RoutedEventHandler(importproject_Click);
                cmc.findMenuItemByName(cmc.cmFile.Items, "Open").Click += new RoutedEventHandler(openfile_Click);
                cmc.findMenuItemByName(cmc.cmFile.Items, "Remove from list").Click += new RoutedEventHandler(removefile_Click);
                cmc.findMenuItemByName(cmc.cmFile.Items, "Rename").Click += new RoutedEventHandler(renameitem_Click);
                cmc.findMenuItemByName(cmc.cmGraphicFile.Items, "Open").Click += new RoutedEventHandler(openfile_Click);
                cmc.findMenuItemByName(cmc.cmGraphicFile.Items, "Remove from list").Click += new RoutedEventHandler(removefile_Click);
                cmc.findMenuItemByName(cmc.cmGraphicFile.Items, "Rename").Click += new RoutedEventHandler(renameitem_Click);
                cmc.findMenuItemByName(cmc.cmGraphicFile.Items, "Flip image").Click += new RoutedEventHandler(flipimage_Click);
                //////////////////////////////////////////////////////////////////////////////////////////
                for (int i = 0; i < G.AppSettings.Frames.Count; i++)
                    for (int j = 0; j < G.AppSettings.Frames[i].subframes.Count; j++)
                        subtagsinit.Add(G.AppSettings.Frames[i].subframes[j].init_tag);
                for (int i = 0; i < G.AppSettings.Frames.Count; i++)
                    for (int j = 0; j < G.AppSettings.Frames[i].subframes.Count; j++)
                        subtagsclose.Add(G.AppSettings.Frames[i].subframes[j].close_tag);
                ////////////////////////////////////////////////////////////////////////   
                ///////////////////////////////////                          
                fr = new FindReplaceMgr();
                fr.OwnerWindow = this;
                fr.InterfaceConverter = new LFStudio.Utils.IEditorConverter();
                ///////////////////////////////////        
                //////////////////////////////////////////////////////
                coll = new ObservableCollection<gfErrors>();
                dgErrors.ItemsSource = coll;
                //////////////////////////////////////////////////                          
                //////////////////////////////////////////////////////////////////////////////
                string[] langs = Directory.GetFiles(programfolder + @"Lang\");
                foreach (string st in langs)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(st);
                    XmlAttribute CultureAttribute = xmlDocument.DocumentElement.Attributes["Culture"];
                    LanguageDictionary.RegisterDictionary(CultureInfo.GetCultureInfo(CultureAttribute.Value), new XmlLanguageDictionary(st));

                    MenuItem submenu = new MenuItem() { Header = System.IO.Path.GetFileNameWithoutExtension(st), Tag = st };
                    submenu.Click += LangClick;
                    miLang.Items.Add(submenu);
                }//////////////
                if (G.AppSettings.Language == "Autolang") miAuto_Click(miLang.Items[0] as MenuItem, null);
                else
                {
                    string pathl = programfolder + @"Lang\" + G.AppSettings.Language + ".xml";
                    if (System.IO.File.Exists(pathl))
                        LanguageContext.Instance.Culture = GetCultureInfoFromFile(pathl);
                    MenuItem mi = cmc.findMenuItemByName(miLang.Items, G.AppSettings.Language);
                    if (mi != null) mi.IsChecked = true;
                }
                //LanguageContext.Binder("tProjs", "Text", "Projects", tviProjects, lfTreeViewItem.HeaderProperty);
                LanguageContext.Binder("tProjs", "Text", "Projects", tviProjects.textBlock, EditableTextBlock.TextFormatProperty);

                //LoadProjects
                MenuItem rl = new MenuItem() { Header = "Reload list" }; rl.Click += new RoutedEventHandler(rl_Click);
                miPlugins.Items.Add(rl); miPlugins.Items.Add(new Separator());
                if (G.isAppServer)
                {
                    string[] args = Environment.GetCommandLineArgs();
                    if (args.Length > 1)
                    {
                        isOpenManyFiles = true;
                        for (int i = 1; i < args.Length; i++)
                            OpenFile(args[i]);
                    }
                }
                if (File.Exists(programfolder + "Tools.xml"))
                {
                    tls = new Tools();
                    tls = Tools.LoadTools(programfolder + "Tools.xml");
                    foreach (Tool tl in tls.tools)
                    {
                        BitmapSource bs = lfTreeViewItem.GetAssociatedIcon(tl.path);
                        MenuItem mi = new MenuItem()
                        {
                            Header = tl.title,
                            Icon = new Image() { Source = bs, UseLayoutRounding = true, SnapsToDevicePixels = true }
                        };
                        mi.Click += new RoutedEventHandler(Tool_Click);
                        miTools.Items.Add(mi);
                    }
                }
                if (miTools.Items.Count == 0) miTools.Visibility = System.Windows.Visibility.Collapsed;
                ///////////////////////////////////////////////////
                string rf = programfolder + "recent.ini";
                if (!File.Exists(rf)) File.Create(rf);
                StreamReader srRecent = new StreamReader(File.OpenRead(rf));
                for (int i = 0; i < rfCount; i++)
                {
                    string st = srRecent.ReadLine();
                    if (st == null) break;
                    if (st.Trim().Length == 0) continue;
                    RecentFiles.Add(st);
                }
                srRecent.Close();
                RecentFilesToMenu();
                ///////////////////////////////////////////////////
                string[] adv = Directory.GetFiles(programfolder + @"Themes\");
                for (int i = 0; i < adv.Length; i++)
                {
                    MenuItem mi = new MenuItem() { Header = System.IO.Path.GetFileNameWithoutExtension(adv[i]), Tag = adv[i] };
                    mi.Click += new RoutedEventHandler(miTheme_Click);
                    miAdvanced.Items.Add(mi);

                }
                ///////////////////////////////////////////////////
                lsbBrushes.DataContext = new WPFBrushList();

                if (G.AppSettings.ColorForStandardTheme.Trim().Length > 0)
                {
                    ThemeFactory.ChangeColors((Color)ColorConverter.ConvertFromString(G.AppSettings.ColorForStandardTheme));
                }
                else
                {
                    if (G.AppSettings.AdvancedTheme.Trim().Length > 0)
                        ThemeFactory.ChangeTheme(new Uri(programfolder + "Themes\\" + G.AppSettings.AdvancedTheme.Trim() + ".xaml", UriKind.RelativeOrAbsolute));
                    else
                        ThemeFactory.ChangeTheme("aero.normalcolor");
                }
                //MainMenu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9ECFA"));                                               


                /*       ResourceDictionary rd = new ResourceDictionary();
                       rd.Source = new Uri( @"/AvalonDock;component/Resources/DefaultStyles.xaml",UriKind.RelativeOrAbsolute);
                       ThemeFactory.ResetTheme();
                       Application.Current.Resources.MergedDictionaries.Add(rd);                             
                       */
                //LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
                /////////////////////////////////////////////////////////////////////////////////
                // dvFrame = new DrawingVisual();

                // dcFrame.AddVisual(dvFrame);

                //  dvNextFrame = new DrawingVisual();
                ///   dcNextFrame.AddVisual(dvNextFrame);

                GradientStopCollection gsc = new GradientStopCollection(2);
                GradientStop gs1 = new GradientStop((Color)ColorConverter.ConvertFromString(G.AppSettings.firstSGVColor), G.AppSettings.firstSGVColorOffset);
                GradientStop gs2 = new GradientStop((Color)ColorConverter.ConvertFromString(G.AppSettings.secondSGVColor), G.AppSettings.secondSGVColorOffset);
                gsc.Add(gs1); gsc.Add(gs2);
                LinearGradientBrush lgb = new LinearGradientBrush(gsc, G.AppSettings.SGVGradientAngle);
                dcSGV.Background = lgb;
                if (G.AppSettings.isSGVAnimated)
                {
                    ColorAnimation ca1 = new ColorAnimation((Color)ColorConverter.ConvertFromString(G.AppSettings.fromSGVColor1),
                                                    (Color)ColorConverter.ConvertFromString(G.AppSettings.toSGVColor1),
                                                    new Duration(TimeSpan.FromSeconds(G.AppSettings.SGVDuration))) { AutoReverse = G.AppSettings.isSGVreverse, RepeatBehavior = RepeatBehavior.Forever };
                    ColorAnimation ca2 = new ColorAnimation((Color)ColorConverter.ConvertFromString(G.AppSettings.fromSGVColor2),
                                                    (Color)ColorConverter.ConvertFromString(G.AppSettings.toSGVColor2),
                                                    new Duration(TimeSpan.FromSeconds(G.AppSettings.SGVDuration))) { AutoReverse = G.AppSettings.isSGVreverse, RepeatBehavior = RepeatBehavior.Forever };
                    ca1.DecelerationRatio = G.AppSettings.DecRatioColor1;
                    ca2.DecelerationRatio = G.AppSettings.DecRatioColor2;
                    ca1.AccelerationRatio = G.AppSettings.AccRatioColor1;
                    ca2.AccelerationRatio = G.AppSettings.AccRatioColor2;

                    gs1.BeginAnimation(GradientStop.ColorProperty, ca1);
                    gs2.BeginAnimation(GradientStop.ColorProperty, ca2);
                    /////////////////////////////////////

                }

                #region Load shadow bitmap
                BitmapImage s = new BitmapImage() { };
                s.BeginInit();
                s.CacheOption = BitmapCacheOption.OnLoad;
                s.UriSource = new Uri(@"pack://application:,,,/LFStudio;component/Img\s.bmp", UriKind.RelativeOrAbsolute);
                s.EndInit();
                shadow = new ColorKeyBitmap() { TransparentColor = Colors.White };
                shadow.Source = s;
                //    dcFrame.shadow = shadow;
                //     dcFrameNext.shadow = shadow;
                #endregion
                #region Prepare brushes and pens
                ipen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBorderColor)), 1);
                if (G.AppSettings.isEnableDashStyle)
                    ipen.DashStyle = new DashStyle(G.AppSettings.ItrDashes, 0);
                ihalfPenWidth = ipen.Thickness / 2;
                ibrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBackgroundColor));
                //ipen.Freeze();
                #endregion
                /*   bSGVFrame.BorderBrush = new SolidColorBrush(Colors.Black);
                bSGVFrame.BorderThickness = new Thickness(1);
                bSGVFrameNext.BorderBrush = new SolidColorBrush(Colors.Black);
                bSGVFrameNext.BorderThickness = new Thickness(1);*/
                dcCanvas.frame = new lfDrawingVisual() { shadow = shadow };
                dcCanvas.framenext = new lfDrawingVisual() { shadow = shadow };
                dcCanvas.AddVisual(dcCanvas.SelectRegion = new lfSelectRegion() { shadow = shadow });
                //   dcCanvas.frame.Offset = new Vector(dcCanvas.GetPosXForFrame(dcCanvas.frame.frame.PixelWidth),10);               
                dcCanvas.WhatTool = WhatTool.Hand;

                //////////////////////////////////////////
                #region Create ComboBox Items
                //icItr = new List<string>();
                cbItrKind.Items.Add("0 - normal"); cbItrKind.Items.Add("1 - catch"); cbItrKind.Items.Add("2 - picking"); cbItrKind.Items.Add("3 - grab"); cbItrKind.Items.Add("4 - falling");
                cbItrKind.Items.Add("5 - weapon"); cbItrKind.Items.Add("6 - super"); cbItrKind.Items.Add("7 - pick light"); cbItrKind.Items.Add("8 - heal"); cbItrKind.Items.Add("9 - shield");
                cbItrKind.Items.Add("10 - lift"); cbItrKind.Items.Add("11 - sustain"); cbItrKind.Items.Add("14 - obstacle"); cbItrKind.Items.Add("15 - whirlwind"); cbItrKind.Items.Add("16 - icewind");
                //icWpoint = new List<string>();
                cbWKind.Items.Add("1 - hold"); cbWKind.Items.Add("2 - held"); cbWKind.Items.Add("3 - drop");
                //icOpoint = new List<string>();
                cbOKind.Items.Add("1 - normal"); cbOKind.Items.Add("2 - hold");
                //icCpoint = new List<string>();
                cbCKind.Items.Add("1 - catch"); cbCKind.Items.Add("2 - caught");
                cbItrEffect.Items.Add("0 - normal"); cbItrEffect.Items.Add("1 - blood"); cbItrEffect.Items.Add("2 - fire"); cbItrEffect.Items.Add("3 - frozen");
                cbItrEffect.Items.Add("4 - nocollision"); cbItrEffect.Items.Add("5 - 4,nosound"); cbItrEffect.Items.Add("20 - firing");
                cbItrEffect.Items.Add("21 - firing"); cbItrEffect.Items.Add("22 - firing,noFF_st18"); cbItrEffect.Items.Add("23 - 0 with repulsion");
                cbItrEffect.Items.Add("30 - 20,21 ice");
                //lsWpoint = new List<string>();
                cbWCover.Items.Add("0 - after"); cbWCover.Items.Add("1 - before");
                #endregion
                for (int i = 1; i <= G.AppSettings.scaleLimit; i++) cbScale.Items.Add(i.ToString());
                cbScale.SelectedIndex = G.AppSettings.scaleIndex;
                zpSGV.SetValue(RenderOptions.BitmapScalingModeProperty, Enum.Parse(typeof(BitmapScalingMode), G.AppSettings.scaleBitmapMode));
                dcCanvas.Width = (int)G.AppSettings.startCanvasWidth;
                dcCanvas.Height = (int)G.AppSettings.startCanvasHeight;
                switch (G.AppSettings.sgvFocusTo)
                {
                    case 0: bNoFocus_Click(null, null); bNoFocus.IsChecked = true; break;
                    case 1: bFocusToFrame_Click(null, null); bFocusToFrame.IsChecked = true; break;
                    case 2: bFocusBetweenFrames_Click(null, null); bFocusBetweenFrames.IsChecked = true; break;
                    case 3: bFocusToFrameNext_Click(null, null); bFocusToFrameNext.IsChecked = true; break;
                }
                dcCanvas.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.canvasBkg));
                zpSGV.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.sgvBkg));
                bSGV.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.canvasBorderBkg));
                bSGV.BorderThickness = new Thickness(G.AppSettings.canvasBorderSize);
                tbShowAll.IsChecked = G.AppSettings.isShowAllinSGV;
                tbShowAllBdy.IsChecked = G.AppSettings.isShowAllBdy;
                tbShowAllItr.IsChecked = G.AppSettings.isShowAllItr;

                //using (StreamWriter swText = new StreamWriter(new FileStream("c:\\test2.txt", FileMode.Create)))
                //{ swText.WriteLine(DateTime.Now.ToString()); }
                //LoadPluginsToMenu();
                bwLoadPlugins = new BackgroundWorker();
                bwLoadPlugins.DoWork += bwBackgroundWork_DoWork_LoadPlugins;
                bwLoadPlugins.RunWorkerCompleted += bwBackgroundWork_Completed_LoadPlugins;
                bwLoadPlugins.RunWorkerAsync();
                sw1.Stop();
                teOutput.AppendText("public MainWindow(): " + sw1.Elapsed.TotalSeconds.ToString() + " sec." + Environment.NewLine);

                bwLoadProjects = new BackgroundWorker();
                bwLoadProjects.DoWork += bwBackgroundWork_DoWork_LoadProjects;
                bwLoadProjects.RunWorkerCompleted += bwBackgroundWork_Completed_LoadProjects;
                bwLoadProjects.RunWorkerAsync();

                lfiWDS.tbPic = tbPic;
                lfiCSV.tbPic = tbCharPic;
                lfiSO.tbPic = tbSOPic;
                //bwLoadAndCropBitmaps = new BackgroundWorker();
                //bwLoadAndCropBitmaps.DoWork+=new DoWorkEventHandler(bwLoadAndCropBitmaps_DoWork);
                //bwLoadAndCropBitmaps.RunWorkerCompleted+=new RunWorkerCompletedEventHandler(bwLoadAndCropBitmaps_RunWorkerCompleted);
                fWId = new lfFiltr(G.AppSettings.filtrForWeapon_id);
                fWType = new lfFiltr(G.AppSettings.filstrForWeapon_type);
                fOType = new lfFiltr(G.AppSettings.filtrForOpoint_type);
                tbBaseFPS.Text = G.AppSettings.baseGameFPS.ToString();
                cbEnableRange.IsChecked = G.AppSettings.isRangeEnabled;
                cbEnableRange_Click(null, null);
                tbRange.Text = G.AppSettings.Range;
                CompositionTarget.Rendering += OnRender;
                //---------------------------------------------------------------
                ocESA = new ObservableCollection<dgExtentedSpriteAnimator>();
                dgESA.ItemsSource = ocESA;
                //                dgESA.Columns[0].Header = "№ frame";
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        void flipimage_Click(object sender, RoutedEventArgs e) //flip image
        {
            try
            {
                Stopwatch sw = new Stopwatch(); sw.Start();
                lfTreeViewItem tvi = tvExplorer.SelectedItem as lfTreeViewItem;
                if (tvi == null) return;
                if (!File.Exists(tvi.Pathtofile)) return;
                //BitmapImage bi = new BitmapImage(new Uri(tvi.Pathtofile));

                BitmapDecoder bd = BitmapDecoder.Create(new Uri(tvi.Pathtofile), BitmapCreateOptions.None, BitmapCacheOption.Default);
                if (bd.Frames.Count == 0) return;
                TransformedBitmap tb = new TransformedBitmap(bd.Frames[0], new ScaleTransform(-1, 1));

                string filename = Path.GetDirectoryName(tvi.Pathtofile) + '\\' + Path.GetFileNameWithoutExtension(tvi.Pathtofile) + "_mirror" + Path.GetExtension(tvi.Pathtofile);
                bool result = false;
                switch (bd.CodecInfo.FriendlyName)
                {
                    case "BMP Decoder": result = WriteTransformedBitmapToFile<BmpBitmapEncoder>(tb, filename); break;
                    case "GIF Decoder": result = WriteTransformedBitmapToFile<GifBitmapEncoder>(tb, filename); break;
                    case "PNG Decoder": result = WriteTransformedBitmapToFile<PngBitmapEncoder>(tb, filename); break;
                    case "JPEG Decoder": result = WriteTransformedBitmapToFile<JpegBitmapEncoder>(tb, filename); break;
                    case "TIFF Decoder": result = WriteTransformedBitmapToFile<TiffBitmapEncoder>(tb, filename); break;
                    case "WMP Decoder": result = WriteTransformedBitmapToFile<WmpBitmapEncoder>(tb, filename); break;
                    default: result = WriteTransformedBitmapToFile<BmpBitmapEncoder>(tb, filename); break;
                }
                sw.Stop();
                if (result)
                    if (G.AppSettings.isAddFileToTreeViewAfterFlip)
                    {
                        CreatelfTreeViewItem(lfTreeViewItem.isFile, tvi.Parent as lfTreeViewItem, filename, true);
                        SaveCurrentProject();
                    }
                if (result) teOutput.AppendText("File is flipped. Estimated: " + sw.Elapsed.TotalSeconds.ToString() + Environment.NewLine);
                else teOutput.AppendText("Some error in file flipping process.");
                teOutput.ScrollToEnd();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void SaveCurrentProject()
        { cProject.SaveProject(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].currentpath, lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)]); }
        #region Graphic functions
        public bool WriteTransformedBitmapToFile<T>(BitmapSource bitmapSource, string fileName) where T : BitmapEncoder, new()
        {
            if (string.IsNullOrEmpty(fileName) || bitmapSource == null)
                return false;

            //creating frame and putting it to Frames collection of selected encoder
            var frame = BitmapFrame.Create(bitmapSource);
            var encoder = new T();
            encoder.Frames.Add(frame);
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        private BitmapImage GetBitmapImage<T>(BitmapSource bitmapSource) where T : BitmapEncoder, new()
        {
            var frame = BitmapFrame.Create(bitmapSource);
            var encoder = new T();
            encoder.Frames.Add(frame);
            var bitmapImage = new BitmapImage();
            bool isCreated;
            try
            {
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    isCreated = true;
                }
            }
            catch
            {
                isCreated = false;
            }
            return isCreated ? bitmapImage : null;
        }
        #endregion
        public void ExecuteSpawnObjectsLoading()
        {
            //cbSOBitmap.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { cbSOBitmap.Items.Clear(); }));            
            bwSpawnObjectsLoad = new BackgroundWorker();
            bwSpawnObjectsLoad.DoWork += new DoWorkEventHandler(bwSpawnObjectsLoad_DoWork);
            bwSpawnObjectsLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwSpawnObjectsLoad_RunWorkerCompleted);
            bwSpawnObjectsLoad.RunWorkerAsync();
        }
        void bwSpawnObjectsLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            if (G.CurrentActiveProject == -1) return;
            cbSODatFile.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                   { cbSODatFile.IsEnabled = false; }));
            foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
            {
                if (fOType.isRelevant(oi.type.Value))
                //if (oi.type == 3 || oi.type==0)  //filtr by type
                {
                    if (oi.data == null)
                        oi.data = DatFiles.ParseDatFileWithErrorsReturn(new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + '\\' + oi.path),
                                                      lProjects[G.CurrentActiveProject].pass, ref oi.errors, lProjects[G.CurrentActiveProject]);
                    //oi.lbiCroppedBitmaps=CropBitmaps(oi.data.header.files,G.CurrentActiveProject,ref oi.lbiBitmaps);
                    cbSODatFile.Dispatcher.Invoke(DispatcherPriority.Normal, (Action<ObjectInfo>)((toi) =>
                    {
                        cbSODatFile.Items.Add(new ComboBoxItem()
                        {
                            Content = Path.GetFileName(toi.path),
                            Tag = toi
                        }
                        );
                    }
                    ), oi);
                }
            }
        }
        void bwSpawnObjectsLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cbSODatFile.Dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)(() => { cbSODatFile.IsEnabled = true; if (cbSODatFile.Items.Count > 0) cbSODatFile.SelectedIndex = 0; }));
        }
        void bwLoadAndCropBitmaps_DoWork(object sender, DoWorkEventArgs e)
        {
            bSettings.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { bSettings.Opacity = 0.33; }));
            // lbsCharImages = new List<BitmapImage>();
            if (teCurActiveDocument == null) return;
            ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
            if (oi == null) return;
            //    if ((teCurActiveDocument.oi as ObjectInfo).data != null)
            //        lbsCharCropImages = CropBitmaps((teCurActiveDocument.oi as ObjectInfo).data.header.files, teCurActiveDocument.numProject, ref lbsCharImages);
            /*lfiCSV.item = new lfItem(0, 0, "");
            lfiCSV.item.lbsImages = lbsCharImages;
            lfiCSV.item.llbsCroppedImage = lbsCharCropImages;
            #region Fill Width and Height
            foreach (FileDesc fd in oi.data.header.files)
            {
                lfiCSV.item.Width.Add(fd.width);
                lfiCSV.item.Height.Add(fd.height);
            }
            #endregion
             */
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { SwitchCombobox(lfiCSV, cbCSVBitmap, lfiCSV.SelectingIndex); }));                
        }
        void bwLoadAndCropBitmaps_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*
          //  bSettings.Dispatcher.Invoke(DispatcherPriority.Normal, (Action <List<BitmapImage>>)((lbs) => 
                { 
                    bSettings.Opacity = 1;
                    cbCSVBitmap.Items.Clear();                
                    for (int i = 0; i < lbsCharImages.Count; i++)
                    {
                        cbCSVBitmap.Items.Add(new ComboBoxItem() 
                        { 
                            Tag=lbsCharImages[i],
                            Content = lbsCharImages[i].UriSource.Segments[lbsCharImages[i].UriSource.Segments.Length-1]
                        });
                    }
                    if (cbCSVBitmap.SelectedIndex == -1)
                        if (cbCSVBitmap.Items.Count > 0)
                            cbCSVBitmap.SelectedIndex = 0;
                }
            //    ),lbsCharImages);
/*                 teOutput.Dispatcher.BeginInvoke(DispatcherPriority.Normal,(Action)(()=>
                 {
                 teOutput.AppendText("Loading and cropped " +(teCurActiveDocument.oi as ObjectInfo).data.header.files.Count.ToString() + " files in " + DockManager.ActiveDocument.Title+".");
                 teOutput.AppendText(Environment.NewLine);
                 teOutput.ScrollToEnd();
                 }));*/

        }
        void bwBackgroundWork_DoWork_LoadPlugins(object sender, DoWorkEventArgs e)
        {
            e.Result = LoadPluginsToMenu();
        }
        void bwBackgroundWork_Completed_LoadPlugins(object sender, RunWorkerCompletedEventArgs e)
        {
            teOutput.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action<int>(delegate(int Count)
                {
                    teOutput.AppendText("Load " + Count.ToString() + " plugins." + Environment.NewLine);
                    teOutput.ScrollToEnd();
                }), (int)e.Result);

            // bwAppStartup.DoWork -= bwBackgroundWork_DoWork_LoadPlugins;
            // bwAppStartup.RunWorkerCompleted -= bwBackgroundWork_Completed_LoadPlugins;


        }
        public int LoadPluginsToMenu()
        {
            string mainpath = programfolder + "Plugins";
            string[] folders;
            if (!Directory.Exists(mainpath)) Directory.CreateDirectory(mainpath);
            folders = Directory.GetDirectories(mainpath);
            Catalog = new AggregateCatalog();
            Catalog.Catalogs.Add(new DirectoryCatalog(mainpath));
            foreach (string st in folders) Catalog.Catalogs.Add(new DirectoryCatalog(st));
            Container = new CompositionContainer(Catalog);
            Container.ComposeParts(this);
            miPlugins.Dispatcher.Invoke(DispatcherPriority.Normal,
               (Action)(() =>
               {
                   if (miPlugins.Items.Count > 2)
                   {
                       int cnt = miPlugins.Items.Count - 2;
                       for (int t = 0; t < cnt; t++)
                       {
                           miPlugins.Items.Remove(miPlugins.Items[miPlugins.Items.Count - 1]);
                       }
                   }
               }));
            int i;
            for (i = 0; i < Plugins.Length; i++)
            {
                miPlugins.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                   (Action<IPlugin[], int>)((plgs, index) =>
                   {
                       MenuItem mi = new MenuItem() { Header = plgs[index].Name };
                       mi.Tag = plgs[index];
                       Plugins[index].SilentExecute(AppLFStudio);
                       mi.Click += new RoutedEventHandler(mi_Click);
                       miPlugins.Items.Add(mi);
                   }), Plugins, i);
            }
            return i;
        }
        void bwBackgroundWork_DoWork_LoadProjects(object sender, DoWorkEventArgs e)
        {
            //////////////////////////////////////////////////////////////////////////////
            if (!File.Exists(programfolder + "projects.txt")) new FileStream(programfolder + "projects.txt",FileMode.CreateNew).Close();            
            StreamReader sr = new StreamReader(programfolder + "projects.txt", Encoding.UTF8);
            string project = LanguageDictionary.Current.Translate<string>("tProj", "Text", "Project");
            string loading = LanguageDictionary.Current.Translate<string>("tLoad", "Text", "loading");
            string seconds = LanguageDictionary.Current.Translate<string>("tSec", "Text", "seconds");
            string path; int k = 0;
            while (true)
            {
                // Stopwatch swLoadProject = new Stopwatch(); swLoadProject.Start();
                path = sr.ReadLine();
                if (path == null) break;
                cProject cp = LoadProject(path);
                k++;
                // swLoadProject.Stop();
                // teOutput.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)(() => {
                //     teOutput.AppendText(project + " '" + Utils.Project.GetNameProject(cp) + "' " + loading + " " + swLoadProject.Elapsed.TotalSeconds.ToString() + " " + seconds);
                //     teOutput.AppendText(Environment.NewLine);
                //  }));
            }
            sr.Close();
            sbRunGame.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => { FillsbRunGame(); }));
            ///////////////////////////////////////////////////////////////////////////
        }
        void bwBackgroundWork_Completed_LoadProjects(object sender, RunWorkerCompletedEventArgs e)
        {
            if (lProjects.Count > 0) G.CurrentActiveProject = 0;
            cbTypeItem.SelectedIndex = 0;
            ExecuteSpawnObjectsLoading();
            LoadListOfOpenFiles();

        }
        public CultureInfo GetCultureInfoFromFile(string path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            XmlAttribute CultureAttribute = xmlDocument.DocumentElement.Attributes["Culture"];
            return CultureInfo.GetCultureInfo(CultureAttribute.Value);
        }
        public void LangClick(object sender, RoutedEventArgs e)
        {
            Utils.Menu.AbordIsCheckedMenuItems(miLang);
            (sender as MenuItem).IsChecked = true;
            //Console.WriteLine(CultureInfo.InstalledUICulture.IetfLanguageTag);           
            string path = (sender as MenuItem).Tag.ToString();
            /*XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(path);
			XmlAttribute CultureAttribute = xmlDocument.DocumentElement.Attributes["Culture"];*/
            LanguageContext.Instance.Culture = GetCultureInfoFromFile(path);
            //CultureInfo.GetCultureInfo(CultureAttribute.Value);                        
            G.AppSettings.Language = System.IO.Path.GetFileNameWithoutExtension(path);
            adce.Save(programfolder + "main.xml", G.AppSettings);
            Utils.tv.RefreshEditableTextBlock(tviProjects);
        }
        public void miTheme_Click(object sender, RoutedEventArgs e)
        {
            string uri = (sender as MenuItem).Tag as string;
            ThemeFactory.ChangeTheme(new Uri(uri, UriKind.RelativeOrAbsolute));
            G.AppSettings.ColorForStandardTheme = "";
            G.AppSettings.AdvancedTheme = System.IO.Path.GetFileNameWithoutExtension(uri);
            adce.Save(programfolder + "main.xml", G.AppSettings);
        }
        public void RecentFilesToMenu()
        {
            miRecent.Items.Clear();
            foreach (string st in RecentFiles)
            {
                string[] items = st.Split(new char[] { ';' });
                if (items.Length > 0)
                {
                    MenuItem mi = new MenuItem() { Header = items[0], Tag = st };
                    mi.Click += new RoutedEventHandler(miRecent_Click);
                    miRecent.Items.Add(mi);
                }
            }
        }
        public void miRecent_Click(object sender, RoutedEventArgs e)
        {
            isOpenManyFiles = false;
            string st = (sender as MenuItem).Tag as string;
            string[] items = st.Split(new char[] { ';' });
            if (items.Length == 1) OpenFile(items[0], -1);
            if (items.Length > 1)
            {
                int index = Utils.Project.GetProjectIndex(lProjects, items[1]);
                if (index != -1) OpenFile(items[0], index);
                else
                {
                    if (items.Length > 2) OpenFile(items[0], Convert.ToInt32(items[2]));
                    else OpenFile(items[0], -1);
                }
            }

        }
        public void Tool_Click(object sender, EventArgs e)
        {
            try
            {
                Tool tl = Tools.FindTool(tls, (sender as MenuItem).Header as string);
                if (tl.promt_arguments)
                {
                    Windows.wPromtArgs pa = new Windows.wPromtArgs(tl.arguments);
                    pa.ShowDialog();
                    if (!pa.isChancel)
                        MyShellExecute(tl.path, tl.initial_dir, pa.args);
                }
                else
                    MyShellExecute(tl.path, tl.initial_dir, tl.arguments);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void ParseProjectFromDatatxt(int i)
        {
            FileInfo fi = new FileInfo(lProjects[i].path_to_folder + "\\data\\data.txt");
            if (!fi.Exists) { MessageBox.Show("data.txt not found in this path:" + Environment.NewLine + fi.FullName, "Error"); return; }
            lProjects[i].datatxt = Utils.DatFiles.ParseDatatxt(fi);

            foreach (ObjectInfo oi in lProjects[i].datatxt.lObject)

                oi.data = Utils.DatFiles.ParseDatFileWithErrorsReturn(new FileInfo(lProjects[i].path_to_folder + '\\' + oi.path),
                                                      lProjects[i].pass, ref oi.errors, lProjects[i]);
            //foreach (BackgroundInfo bi in lProjects[i].datatxt.lBackground)
            //   bi.data = functions.ParseDatFile(new FileInfo(lProjects[0].path_to_folder + "\\" + bi.path), lProjects[i].pass, ref lProjects[i].errors);
        }
        public void SendErrorsListToGridErrors(ObjectInfo oi2)
        {
            int n = 1;
            coll.Clear();
            if (oi2 != null)
                if (oi2.id == null)
                    foreach (gfErrors fge in oi2.errors) { fge.N = n; coll.Add(fge); n++; }

            for (int i = 0; i < lProjects.Count; i++)
            {
                if (lProjects[i].datatxt == null) continue;
                foreach (ObjectInfo oi in lProjects[i].datatxt.lObject)
                {
                    foreach (gfErrors st in oi.errors)
                    { st.N = n; coll.Add(st); n++; }
                }
            }
        }
        public void cse_Click(object Sender, EventArgs e)   //reparse projects dat files
        {
            for (int i = 0; i < lProjects.Count; i++)
            {
                ParseProjectFromDatatxt(i);
            }
            SendErrorsListToGridErrors(null);

        }
        public void cp_Click(object sender, EventArgs e) //clear project
        {
            //LanguageDictionary.Current.Translate<string>("NotEnoughMemory", "Message");
            //if (MessageBox.Show("Do you want remove all items?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            if (MessageBoxQuestion.Show(LanguageDictionary.Current.Translate<string>("qRAI", "Text", "Do you want remove all items?")) == MessageBoxQuestion.WPFMessageBoxResult.No) return;
            lfTreeViewItem si = tvExplorer.SelectedItem as lfTreeViewItem;
            int curproject = WhatProject(si);
            ArrayList al = si.Tag as ArrayList;
            string buf = (al[0] as ArrayList)[0] as string;
            al.Clear();
            al.Add(new ArrayList());
            (al[0] as ArrayList).Add(buf);
            si.Items.Clear();
            cProject.SaveProject(lProjects[curproject].currentpath,
                                 lProjects[curproject]);
        }
        void renameitem_Click(object sender, RoutedEventArgs e) // rename item
        {
            //  if ((tvExplorer.SelectedItem as lfTreeViewItem).Type==lfTreeViewItem.isFile ||
            //      (tvExplorer.SelectedItem as lfTreeViewItem).Type == lfTreeViewItem.isProject)  //переименовывать только файлы
            SetCurrentItemInEditMode(true);
        }
        void rl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bwLoadPlugins.RunWorkerAsync();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void importproject_Click(object sender, RoutedEventArgs e)
        {
            new wImportProject(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)]).ShowDialog();
        }

        private void mi_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((sender as MenuItem).Tag as IPlugin).NormalExecute();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #region Load/Unload Initial layout
        private void DockManager_Loaded(object sender, RoutedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            FileInfo fi = new FileInfo(programfolder + "layout.xml");
            if (fi.Exists)
                if (fi.Length > 0)
                    DockManager.RestoreLayout(programfolder + "layout.xml");
                else
                    File.Delete(programfolder + "layout.xml");
            if (DockManager.Documents.Count > 0)
            {
                DockManager.ActiveDocument = DockManager.Documents[0];
                DockManager.ActiveContent = DockManager.Documents[0];
            }
            sw.Stop();
            teOutput.AppendText("Layout restore: " + sw.Elapsed.TotalSeconds.ToString() + " sec.");
            teOutput.AppendText(Environment.NewLine);

        }


        #endregion
        public void FillsbRunGame()
        {
            int cnt = sbRunGame.Items.Count;
            for (int i = 0; i < cnt; i++)
            {
                sbRunGame.Items.Remove(sbRunGame.Items[0]);
            }
            for (int i = 0; i < lProjects.Count; i++)
            {
                sbRunGame.Items.Add(new MenuItem()
                {
                    Header = ((lProjects[i].files[0] as ArrayList)[0] as string),
                    Tag = lProjects[i].path_to_exe
                });
                (sbRunGame.Items[sbRunGame.Items.Count - 1] as MenuItem).Click += new RoutedEventHandler(sbRunGameClick);
            }
        }
        private void sbRunGameClick(object sender, RoutedEventArgs e) //Click on button that show  list
        {
            string sRG = LanguageDictionary.Current.Translate<string>("tbRG", "Text", "Run game");
            sbRunGame.Tag = sbRunGame.Items.IndexOf(sender);
            ((sbRunGame.Content as StackPanel).Children[1] as TextBlock).Text = sRG + "(" + (sbRunGame.Items.IndexOf(sender) + 1).ToString() + ")";
        }
        delegate void AsyncMethodHandler();
        public void RunGame(int n)
        {
            if (n == -1) n = 0;
            MyShellExecute(lProjects[n].path_to_exe);
            if (lProjects[n].PressGameStartAfterApplicationRun)
            {
                AsyncMethodHandler caller = default(AsyncMethodHandler);
                caller = new AsyncMethodHandler(new Action(delegate() { this.TryPatchExecutable(n); }));
                caller.BeginInvoke(null, null);
            }
        }
        public void MyShellExecute(string fpath, string initdir = "", string args = "")
        {
            //if (!File.Exists(fpath)) MessageBox.Show("Wrong path","Error");
            //if (initdir.Length!=0)
            //if (!Directory.Exists(initdir))  MessageBox.Show("Wrong Initial directory", "Error");
            if (initdir.Length == 0) initdir = System.IO.Path.GetDirectoryName(fpath);
            ProcessStartInfo startInfo = new ProcessStartInfo(fpath);
            startInfo.WorkingDirectory = initdir;
            if (args.Length != 0) startInfo.Arguments = args;

            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process p = new Process() {StartInfo=startInfo };
                           
            p.EnableRaisingEvents = true;
            p.OutputDataReceived+=new DataReceivedEventHandler(p_OutputDataReceived);
            p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
            // Process.Start(startInfo);
            p.Start();

            
            

           // IntPtr ip = G.FindWindowByCaption(0, p.StartInfo.FileName);
           // G.ShowWindow(ip, G.ShowWindowCommands.Hide);
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
        }

        void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            teOutput.AppendText(e.Data);
        }


        public void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            //if (!String.IsNullOrEmpty(e.Data))
            using (FileStream fs = new FileStream(@"c:\main\test.txt", FileMode.Append))
            using (StreamWriter sw = new StreamWriter(fs))
                sw.Write(e.Data);
        }
        public void TryPatchExecutable(int n)
        {
            uint id = G.GetAppId(lProjects[n].path_to_exe);
            if (id == 0) return;
            IntPtr handle = G.OpenProcess(LFStudio.G.ProcessAccessFlags.All, true, id);
            uint? value = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (value != 0)
            {
                value = G.ReadValueFromProcess(handle, 0x44D064, 4);
            }// when break for cycle we now that main menu is "loading".
            sw.Stop();
            G.WriteValueToProcess(handle, 0x42790F, new byte[] { 0xE9, 0xB6, 0x00, 0x00, 0x00, 0x90 });
            G.WriteValueToProcess(handle, 0x4279D1, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            G.WriteValueToProcess(handle, 0x4279F6, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            G.WriteValueToProcess(handle, 0x427A02, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            G.CloseHandle(handle);
        }

        private void sbRunGame_Click(object sender, RoutedEventArgs e) //Run game from toolbar
        {
            try
            {
                if (lProjects.Count == 0) return;
                if ((int)(sbRunGame.Tag) != -1)
                {
                    RunGame((int)sbRunGame.Tag);
                }
                else { RunGame(0); ((sbRunGame.Content as StackPanel).Children[1] as TextBlock).Text = "Run game(" + ("1").ToString() + ")"; }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private bool LineConsist(string[] ls, string obj)
        {
            obj = obj.TrimEnd();
            bool result = false;
            for (int i = 0; i < ls.Length; i++)
            {
                if (ls[i] != "")
                    if (ls[i] == obj) result = true;
            }
            return result;
        }

        protected void te_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextEditor te = sender as TextEditor;
            te.TextArea.TextView.Redraw(te.CaretOffset, 0);

            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                string propname = Utils.AvalonEdit.GetWordInLeftSide(te);
                string value = Utils.AvalonEdit.GetTextFromLeftSide(te) + Utils.AvalonEdit.GetTextFromRightSide(te);
                // if (propname.Contains("file") || propname=="head:" || propname=="small:" || propname=="sound:")
                if (propname != "next:")
                {
                    if (te.numProject != -1)
                    {
                        if (!File.Exists(value)) value = lProjects[te.numProject].path_to_folder + "\\" + value;
                        if (File.Exists(value)) { isOpenManyFiles = false; OpenFile(value, te.numProject); }
                        else MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("wtFNE", "Text", "File not exists."));
                        //MessageBox.Show("File not exists");
                    }
                    else
                        MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFNS", "Text", "This feature don't support in non-project files."));
                }
                else if (value != "999")
                {
                    #region 999
                    string whatfind = value.Trim();
                    if (whatfind.Length == 0) return;
                    ObjectInfo oi = te.oi as ObjectInfo;
                    int nline = -1;
                    if (e == null)
                    {
                        DocumentLine dl = te.Document.GetLineByOffset(te.SelectionStart);
                        nline = dl.LineNumber;
                    }

                    for (int i = 0; i < oi.data.frames.Count; i++)
                    {
                        if (nline != -1 && oi.data.frames[i].oline <= nline) continue;
                        if (oi.data.frames[i].number.ToString() == whatfind)
                        {
                            te.ScrollToLine(oi.data.frames[i].oline);
                            te.Select(te.Document.Lines[oi.data.frames[i].oline - 1].Offset,
                                      te.Document.Lines[oi.data.frames[i].oline - 1].Length);
                            break;

                        }
                    }
                    #endregion
                }
            }
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)//Autocomplete box
            {
                e.Handled = true;
                ShowCompleteionWindow((sender as TextEditor).TextArea);
            }//ifkey
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                bFind_Click(sender, e);
            }
            if (e.Key == Key.F3)
            {
                fr.FindNext();
            }
            if (e.Key == Key.F3 && Keyboard.Modifiers == ModifierKeys.Shift) fr.FindPrevious();
        }
        #region for autocomplete list
        private List<string> StringsFromComboBox(ComboBox cb)
        {
            List<string> result = new List<string>(cb.Items.Count);
            //StringBuilder result=new StringBuilder("");
            for (int i = 0; i < cb.Items.Count; i++)
                result.Add((string)cb.Items[i]);
            return result;
        }
        private List<string> BodyToString(List<List<string>> lls, string element = "")
        {
            List<string> result = new List<string>();
            for (int i = 0; i < lls.Count; i++)
            {
                for (int j = 0; j < lls[i].Count; j++)
                {
                    result.Add(lls[i][j]);
                }
            }
            if (element != "") result.Add(element);
            return result;

        }
        private List<string> GetTags0(string element = "")
        {
            List<string> result = new List<string>();
            for (int i = 0; i < G.AppSettings.Frames.Count; i++)
            {
                result.Add(G.AppSettings.Frames[i].init_tag);
            }
            if (element != "") result.Add(element);
            return result;
        }
        private List<string> SubFramesToString(string element = "")
        {
            List<string> result = new List<string>();

            for (int i = 0; i < G.AppSettings.Frames.Count; i++)
            {
                for (int j = 0; j < G.AppSettings.Frames[i].subframes.Count; j++)
                {
                    result.Add(G.AppSettings.Frames[i].subframes[j].init_tag);
                }
            }
            if (element != "") result.Add(element);
            return result;
        }
        protected List<string> GetWordsList(TextArea ta)
        {
            List<string> EmptyList = new List<string>();
            DocumentLine dl = ta.Document.GetLineByOffset(ta.Caret.Offset);

            string str = ta.Document.GetText(dl.Offset, dl.Length);
            for (int i = 0; i < G.AppSettings.Frames.Count; i++)
            {
                /*  if (LineConsist(str.Split(' '), G.AppSettings.Frames[i].init_tag))
                      if (G.AppSettings.Frames[i].content_in_init_tag)
                          return StringsFromComboBox(cbGotoframename);
                      else return EmptyList;*/
                if (LineConsist(str.Split(' '), G.AppSettings.Frames[i].close_tag))
                    return EmptyList;
            }
            for (int i = 0; i < subtagsinit.Count; i++)
            {
                if (LineConsist(str.Split(' '), subtagsinit[i]) || LineConsist(str.Split(' '), subtagsclose[i]))
                    return EmptyList;
            }
            bool exitFromWhile = false;
            int iter = 0;
            while (exitFromWhile == false)
            {
                iter++;
                dl = dl.PreviousLine;
                if (dl == null)
                    //break;
                    return GetTags0();
                string txtprevline = ta.Document.GetText(dl.Offset, dl.Length);
                for (int i = 0; i < G.AppSettings.Frames.Count; i++)
                {
                    if (LineConsist(txtprevline.Split(' '), G.AppSettings.Frames[i].init_tag))
                    {
                        if (G.AppSettings.Frames[i].body.Count == 0) return SubFramesToString(G.AppSettings.Frames[i].close_tag);
                        if (G.AppSettings.Frames[i].body.Count == 1)
                            if (iter > 1)
                            {
                                return SubFramesToString(G.AppSettings.Frames[i].close_tag);
                            }
                            else return BodyToString(G.AppSettings.Frames[i].body);
                        else//if body is multilines
                            return BodyToString(G.AppSettings.Frames[i].body, G.AppSettings.Frames[i].close_tag);
                    }
                    if (LineConsist(txtprevline.Split(' '), G.AppSettings.Frames[i].close_tag))
                        return GetTags0();
                    ///////////Sub tags//////////////////////////////////////////////////////
                    for (int j = 0; j < G.AppSettings.Frames[i].subframes.Count; j++)
                    {
                        if (LineConsist(txtprevline.Split(' '), G.AppSettings.Frames[i].subframes[j].init_tag))
                            if (G.AppSettings.Frames[i].subframes[j].body.Count == 1)
                                if (iter > 1)
                                {
                                    if (G.AppSettings.Frames[i].subframes[j].close_tag == "")
                                        return SubFramesToString(G.AppSettings.Frames[i].close_tag);
                                    else return (new List<string>(new string[] { G.AppSettings.Frames[i].subframes[j].close_tag }));
                                }
                                else return BodyToString(G.AppSettings.Frames[i].subframes[j].body);
                            //
                            else//if body is multilines
                                return BodyToString(G.AppSettings.Frames[i].subframes[j].body, G.AppSettings.Frames[i].subframes[j].close_tag);
                        else if (LineConsist(txtprevline.Split(' '), G.AppSettings.Frames[i].subframes[j].close_tag))
                            return SubFramesToString(G.AppSettings.Frames[i].close_tag);
                    }
                }//for


            }//while


            return EmptyList;
        }
        #endregion
        protected void te_TextChanged(object sender, EventArgs e)
        {
            TextEditor te = (sender as TextEditor);
            DocumentContent dc = Utils.AvalonDock.GetActiveDocument(sender as UIElement);
            string fn = System.IO.Path.GetFileName(te.fullpath);
            if (fn != "stage.dat" && fn != "data.txt" && te.oi != null)
            {
                DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
                string txtline = te.Document.GetText(line.Offset, line.Length);
                (te.oi as ObjectInfo).data = Utils.DatFiles.ParseTextWithErrorsReturn
                (te.Text, dc.Tag as string, ref (te.oi as ObjectInfo).errors, null);
                if (txtline.Length != 0)
                    for (int i = 0; i < G.AppSettings.foldtags.Count; i++)
                    {
                        if (LineConsist(txtline.Split(' '), G.AppSettings.foldtags[i].init_tag) || LineConsist(txtline.Split(' '), G.AppSettings.foldtags[i].close_tag))
                        {
                            UpdateFoldings((te.oi as ObjectInfo).data, sender as TextEditor);
                            UpdateComboBoxs(te, (te.oi as ObjectInfo).data);
                        }
                    }
            }
            // UpdateFoldings((te.oi as ObjectInfo).data);
            // UpdateComboBoxs(te, (te.oi as ObjectInfo).data);
            if (dc == null) return;
            string st = (dc as DocumentContent).Title;
            //   if (te.IsModified)
            if (st[st.Length - 1] != '*') (dc as DocumentContent).Title += "*";
        }
        // public delegate void ufac();
        public void UpdateFoldings(DatFileDesc dfd, TextEditor te)
        {
            try
            {
                if (G.AppSettings.isFoldEnable && DockManager.ActiveDocument != null)
                {
                    te.fm.Clear();

                    foldingStrategy.dfd = dfd;
                    foldingStrategy.UpdateFoldings(te.fm, te.Document);

                    //te.fs = (TextSegmentCollection<FoldingSection>)foldingManager.AllFoldings;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        CompletionWindow completionWindow;
        void ta_TextEntered(object sender, TextCompositionEventArgs e)//2
        {
            try
            {
                /*    if (e.Text == " " && NeedShowCompletionWindow(sender as TextArea, e))
                    {
                        ShowCompleteionWindow(sender as TextArea);
                    }*/
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private bool NeedShowCompletionWindow(TextArea ta, TextCompositionEventArgs e)
        {
            //(statusbar.Items[0] as TextBlock).Text = poscurinstr.ToString();            
            if (completionWindow != null) return false;
            DocumentLine dl = ta.Document.GetLineByOffset(ta.Caret.Offset);
            string str = ta.Document.GetText(dl.Offset, dl.Length);

            string[] astr = str.Split(' ');
            astr = G.DeleteEmptyElements(astr);
            if (astr.Length != 0)
                if (astr[0] == "<frame>")
                    if (astr.Length < 2) return false;
            //if (LineConsist(str.Split(' '),"<frame>") 

            int poscurinstr = ta.Caret.Offset - dl.Offset;
            if (poscurinstr < str.TrimEnd().Length) return false;


            if (!char.IsLetter(e.Text[0]) && e.Text[0] != ' ' && e.Text[0] != '<')
                return false;

            return true;
        }
        void ta_TextEntering(object sender, TextCompositionEventArgs e)//1
        {

            /*    if (e.Text[0] != ' ' && NeedShowCompletionWindow(sender as TextArea, e)
                    )
                {
                    ShowCompleteionWindow(sender as TextArea);
                }*/
            /////////////////////////////////////////////////////////////

        }
        private void ShowCompleteionWindow(TextArea sender)
        {
            try
            {
                completionWindow = new CompletionWindow(sender as TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                List<string> ls = GetWordsList((sender as TextArea));
                for (int i = 0; i < ls.Count; i++)
                    data.Add(new MyCompletionData(ls[i]));
                if (ls.Count > 0)
                    completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void dc_Closing<TEventArgs>(object sender, TEventArgs e)
        {
            var child = Utils.AvalonEdit.GetTextEditorFromContent((sender as DocumentContent).Content);
            //(sender as DocumentContent).Content;
            if (child is TextEditor)
            {
                if ((sender as DocumentContent).Title[(sender as DocumentContent).Title.Length - 1] == '*')
                {
                    wSaveOneFile sof = new wSaveOneFile() { Owner = this };
                    sof.ShowDialog();

                    if (sof.Result == 1)
                    {
                        sf_Click(sender, null);
                    }
                    if (sof.Result == 2)
                    {

                    }
                    if (sof.Result == 3)
                        (e as System.ComponentModel.CancelEventArgs).Cancel = true;
                    sof.Close();
                }

            }
            /*       if (child is StackPanel)
                   {
                       if ((child as StackPanel).Children.Count > 0)
                           if ((child as StackPanel).Children[0] is WebBrowser)
                           {
                               ((child as StackPanel).Children[0] as WebBrowser).Dispose();
                           }
                   }*/
        }


        public void te_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as TextEditor).TextArea.TextView.Redraw((sender as TextEditor).CaretOffset, 0);
        }
        public void cbFrameNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((sender as ComboBox).SelectedItem == null) return;
                string whatfind = ((sender as ComboBox).SelectedItem as string).Trim();
                if (whatfind.Length == 0) return;
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                if (te.oi == null) return;
                ObjectInfo oi = te.oi as ObjectInfo;
                int nline = -1;
                if (e == null)
                {
                    DocumentLine dl = te.Document.GetLineByOffset(te.SelectionStart);
                    nline = dl.LineNumber;
                }
                if ((sender as ComboBox).Name != Utils.Const.cbRegion)
                    for (int i = 0; i < oi.data.frames.Count; i++)
                    {
                        if (nline != -1 && oi.data.frames[i].oline <= nline) continue;
                        if (oi.data.frames[i].number.ToString() == whatfind || oi.data.frames[i].caption == whatfind)
                        {
                            te.ScrollToLine(oi.data.frames[i].oline);
                            te.Select(te.Document.Lines[oi.data.frames[i].oline - 1].Offset,
                                      te.Document.Lines[oi.data.frames[i].oline - 1].Length);
                            break;

                        }
                    }
                else
                    for (int i = 0; i < oi.data.regions.Count; i++)
                    {
                        if (nline != -1 && oi.data.regions[i].oline <= nline) continue;
                        if (oi.data.regions[i].caption == whatfind)
                        {
                            te.ScrollToLine(oi.data.regions[i].oline);
                            te.Select(te.Document.Lines[oi.data.regions[i].oline - 1].Offset,
                                      te.Document.Lines[oi.data.regions[i].oline - 1].Length);
                            break;

                        }
                    }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void cbFrameNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                if (e.SystemKey == Key.RightAlt) (sender as ComboBox).IsDropDownOpen = true;
            }
            if (e.Key == Key.Enter) cbFrameNumber_SelectionChanged(sender, null);
            if (e.Key == Key.Escape)
            {
                var te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                if (te is TextEditor) te.Focus();
            }

        }
        public void cbFrameNumber_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                Utils.AvalonDock.GetComboBoxByName(DockManager.ActiveDocument.Content, Utils.Const.cbNumber).Focus();
            }
        }
        public void Document_LineCountChanged(object sender, EventArgs e)
        {
            TextEditor te = Utils.AvalonEdit.FindTextEditorByDocumentText(DockManager, (sender as TextDocument));
            DocumentContent dc = Utils.AvalonDock.GetActiveDocument(te);
            if (te.fullpath.Contains("data.txt") || te.fullpath.Contains("stage.dat")) return;
            if (te.oi != null)
            {
                (te.oi as ObjectInfo).data = Utils.DatFiles.ParseTextWithErrorsReturn
                    (te.Text, dc.Tag as string, ref (te.oi as ObjectInfo).errors, null);
                UpdateFoldings((te.oi as ObjectInfo).data, te);
                UpdateComboBoxs(te, (te.oi as ObjectInfo).data);
            }
        }
        public void dc_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void OpenTextFile(String fn, int numPrj)
        {
            try
            {
                ComboBox cbFrameNumber = new ComboBox() { IsReadOnly = false, IsEditable = true, Width = 50, Name = Utils.Const.cbNumber, Margin = new Thickness(3, 2, 0, 2) };
                ComboBox cbFrameName = new ComboBox() { IsReadOnly = false, IsEditable = true, Width = 150, Name = Utils.Const.cbName, Margin = new Thickness(3, 2, 0, 2) };
                ComboBox cbRegion = new ComboBox() { IsReadOnly = false, IsEditable = true, Width = 150, Name = Utils.Const.cbRegion, Margin = new Thickness(3, 2, 0, 2) };
                //ComboBox cbPic = new ComboBox() { Visibility=System.Windows.Visibility.Hidden, IsReadOnly = false, IsEditable = true, Width = 150, Name = Utils.Const.cbPic, Margin = new Thickness(3, 2, 0, 2) };
                cbFrameNumber.SelectionChanged += new SelectionChangedEventHandler(cbFrameNumber_SelectionChanged);
                cbFrameName.SelectionChanged += new SelectionChangedEventHandler(cbFrameNumber_SelectionChanged);
                cbRegion.SelectionChanged += new SelectionChangedEventHandler(cbFrameNumber_SelectionChanged);
                cbFrameName.KeyDown += new KeyEventHandler(cbFrameNumber_KeyDown);
                cbRegion.KeyDown += new KeyEventHandler(cbFrameNumber_KeyDown);
                cbFrameNumber.KeyDown += new KeyEventHandler(cbFrameNumber_KeyDown);
                //cbPic.LostKeyboardFocus+=new KeyboardFocusChangedEventHandler(cbFrameNumber_LostKeyboardFocus);
                Grid grid = new Grid();
                RowDefinition rd1 = new RowDefinition();
                RowDefinition rd2 = new RowDefinition() { Height = GridLength.Auto };
                if (G.AppSettings.isComboboxesInTop)
                { grid.RowDefinitions.Add(rd2); grid.RowDefinitions.Add(rd1); }
                else
                { grid.RowDefinitions.Add(rd1); grid.RowDefinitions.Add(rd2); }
                WrapPanel wpComboboxes = new WrapPanel();
                wpComboboxes.Children.Add(cbFrameNumber);
                wpComboboxes.Children.Add(cbFrameName);
                wpComboboxes.Children.Add(cbRegion);
                // wpComboboxes.Children.Add(cbPic);
                var dc = new DocumentContent();
                dc.MouseLeftButtonDown += new MouseButtonEventHandler(dc_MouseLeftButtonDown);
                TextEditor te = new TextEditor();
                te.dc = dc;
                grid.Children.Add(te);
                if (System.IO.Path.GetExtension(fn) != ".txt" && !fn.Contains("stage.dat"))
                    grid.Children.Add(wpComboboxes);
                if (G.AppSettings.isComboboxesInTop)
                { Grid.SetRow(te, 1); Grid.SetRow(wpComboboxes, 0); }
                else
                { Grid.SetRow(te, 0); Grid.SetRow(wpComboboxes, 1); }
                dc.Content = grid;

                dc.Title = System.IO.Path.GetFileName(fn);
                dc.Tag = fn + ';' + numPrj.ToString();

                dc.Name = Utils.AvalonDock.ReturnValidName(System.IO.Path.GetFileNameWithoutExtension(fn));
                te.Name = dc.Name;

                te.fullpath = fn;
                te.numProject = numPrj;
                foldingStrategy = new Lf2FoldingStrategy();
                te.Tag = foldingStrategy;
                if (G.AppSettings.isFoldEnable)
                {
                    // if (foldingManager!=null)
                    // FoldingManager.Uninstall(foldingManager);
                    te.fm = FoldingManager.Install(te.TextArea);
                }

                //if (DockManager.ActiveDocument == null)
                //{ DockManager.ActiveDocument = dc; }                

                if (G.AppSettings.isEnabledActiveLineHighlight)
                    te.TextArea.TextView.BackgroundRenderers.Add(new XBackgroundRenderer(te));
                foreach (XBackgroundRenderer br in AppLFStudio.bgRenderer)
                    te.TextArea.TextView.BackgroundRenderers.Add(br);
                dc.Closing += new EventHandler<CancelEventArgs>(dc_Closing);
                te.SyntaxHighlighting =
                HighlightingLoader.Load(new XmlTextReader(programfolder + "lf2.xshd"), HighlightingManager.Instance);
                #region TextEditor Options
                te.Options = G.AppSettings.teAdvOptions;
                try
                { te.Encoding = Encoding.GetEncoding(G.AppSettings.teOptions.EncodingForSavedText); }
                catch { te.Encoding = Encoding.Default; }
                te.FlowDirection = (FlowDirection)G.AppSettings.teOptions.FlowDirection;
                te.FontFamily = new FontFamily(G.AppSettings.teOptions.FontFamily);
                te.FontSize = G.AppSettings.teOptions.FontSize;
                FontStretchConverter fsc = new FontStretchConverter();
                te.FontStretch = (FontStretch)fsc.ConvertFromString(G.AppSettings.teOptions.FontStretch); // Не меняется значение                                                                              
                te.FontStyle = G.GetFontStyleByName(G.AppSettings.teOptions.FontStyle);
                FontWeightConverter fwc = new FontWeightConverter();
                te.FontWeight = (FontWeight)fwc.ConvertFromString(G.AppSettings.teOptions.FontWeight);
                te.WordWrap = G.AppSettings.teOptions.WordWrap;
                te.ShowLineNumbers = G.AppSettings.teOptions.ShowLineNumbers;
                #endregion
                //     this.Dispatcher.BeginInvoke(new Action(delegate()
                //        {
                if (!isNeedDecrypt(fn))
                {
                    FileStream fm = new FileStream(fn, FileMode.Open, FileAccess.ReadWrite);
                    try
                    { te.Load(fm); }
                    finally
                    { fm.Close(); }
                }
                else
                {
                    string ds;
                    if (numPrj == -1)
                        ds = functions.DatFileToPlainText(fn, G.AppSettings.StandardPassword);
                    else ds = functions.DatFileToPlainText(fn, lProjects[numPrj].pass);
                    te.Text = ds;
                }
                /////////////////////////////////////////////////////////////
                if (isOpenManyFiles && DockManager.ActiveDocument == null)
                {
                    #region Crop bitmaps
                    //Stopwatch sw = new Stopwatch();
                    //sw.Start();
                    ObjectInfo oi = FindDatafile(te.numProject, fn);
                    oi.data = Utils.DatFiles.ParseTextWithErrorsReturn(te.Text, fn, ref oi.errors, Utils.Project.GetProject(te.numProject));
                    te.oi = oi;
                    //lbsCropImages.Clear();
                    teCurActiveDocument = te;
                    if (oi.lbiBitmaps == null) oi.lbiCroppedBitmaps = CropBitmaps(oi.data.header.files, numPrj, ref oi.lbiBitmaps);
                    FillComboBoxForDFV(oi);

                    /*BackgroundWorker bwLoadAndCropBitmaps = new BackgroundWorker();
                    bwLoadAndCropBitmaps.DoWork += new DoWorkEventHandler(bwLoadAndCropBitmaps_DoWork);
                    bwLoadAndCropBitmaps.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwLoadAndCropBitmaps_RunWorkerCompleted);
                    bwLoadAndCropBitmaps.RunWorkerAsync();*/

                    //lbsCropImages = FilllbsImages(oi.data.header.files, te.numProject);
                    //sw.Stop();
                    //teOutput.AppendText("Loading and cropped " + oi.data.header.files.Count.ToString() + " files in " + Path.GetFileName(fn) + ". Estimated: " + sw.Elapsed.Milliseconds.ToString() + " ms.");
                    //teOutput.AppendText(Environment.NewLine);
                    //teOutput.ScrollToEnd();
                    #endregion
                }
                if (isOpenManyFiles) DockManager.isAddFirst = false; else DockManager.isAddFirst = true;
                dc.Show(DockManager);
                if (isOpenManyFiles)
                {
                    if (DockManager.MainDocumentPane.Items.Count == 1) dc.Activate();
                }
                else dc.Activate();
                /////////////////////////////////////////
                //te.TextArea.TextEntered += new TextCompositionEventHandler(ta_TextEntered);
                //te.TextArea.TextEntering += new TextCompositionEventHandler(ta_TextEntering);
                te.PreviewKeyDown += new KeyEventHandler(te_PreviewKeyDown);
                te.KeyDown += new KeyEventHandler(te_KeyDown);
                te.TextChanged += new EventHandler(te_TextChanged);
                te.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(te_PreviewMouseLeftButtonDown);
                te.PreviewMouseRightButtonUp += new MouseButtonEventHandler(te_PreviewMouseLeftButtonDown);
                te.Document.LineCountChanged += new EventHandler(Document_LineCountChanged);
                te.TextArea.Caret.PositionChanged += new EventHandler(Caret_PositionChanged);
                //te.TextArea.SelectionChanged+=new EventHandler(TextArea_SelectionChanged);

                if (fn.Contains("stage.dat"))
                {

                }
                else if (fn.Contains("data.txt"))
                {

                }
                else if (fn.Contains("deep_chop.dat")) { }
                else if (System.IO.Path.GetExtension(fn) == ".dat")
                {
                    //       if (te.numProject != -1)
                    //           lProjects[te.numProject].datatxt = Utils.DatFiles.ParseDatatxt(new FileInfo(lProjects[te.numProject].path_to_folder + "\\data\\data.txt"));
                    ObjectInfo oi = FindDatafile(te.numProject, fn);
                    oi.data = Utils.DatFiles.ParseTextWithErrorsReturn(te.Text, fn, ref oi.errors, Utils.Project.GetProject(te.numProject));
                    SendErrorsListToGridErrors(oi);
                    te.oi = oi;
                    if (oi.lbiBitmaps == null)
                        if (oi.data != null && oi.data.header != null)
                            (te.oi as ObjectInfo).lbiCroppedBitmaps = CropBitmaps(oi.data.header.files, numPrj, ref (te.oi as ObjectInfo).lbiBitmaps);


                    foldingStrategy.dfd = oi.data;
                    te.fm.Clear();
                    foldingStrategy.UpdateFoldings(te.fm, te.Document);
                    //   UpdateFoldings(oi.data);
                    UpdateComboBoxs(te, oi.data);
                    if (G.AppSettings.isFoldEnable)
                        te.fs = te.fm.AllFoldings as TextSegmentCollection<FoldingSection>;
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void FillComboBoxForDFV(ObjectInfo oi)
        {
            if (oi.lbiBitmaps == null) return;
            cbCSVBitmap.Items.Clear();
            for (int i = 0; i < oi.lbiBitmaps.Count; i++)
            {
                cbCSVBitmap.Items.Add(new ComboBoxItem()
                {
                    Tag = oi.lbiBitmaps[i],
                    Content = oi.lbiBitmaps[i].UriSource.Segments[oi.lbiBitmaps[i].UriSource.Segments.Length - 1]
                });
            }
            if (cbCSVBitmap.SelectedIndex == -1)
                if (cbCSVBitmap.Items.Count > 0)
                    cbCSVBitmap.SelectedIndex = 0;
        }
        public void Caret_PositionChanged(object sender, EventArgs e)
        {
            DocumentLine line = teCurActiveDocument.Document.GetLineByOffset(teCurActiveDocument.CaretOffset);
            if (CurrentNLine != line.LineNumber)
            {
                DetectWhereCursor();
                CurrentNLine = line.LineNumber;
                ChooseComboboxes(Otvet);
                if (cbEnableRange.IsChecked == false)
                if (CurrentFrameIndex!=-1)
                {

                    #region     Run animate
                    ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                    List<int> npics = new List<int>();
                    List<int> waits = new List<int>();
                    int cfi = CurrentFrameIndex;
                    int pic = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "pic:");
                    int wait = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "wait:");
                    npics.Add(pic); waits.Add(wait);
                    for (int i = 0; i < 399; i++)
                    {
                        int next = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "next:");
                        if (next == 999) break;
                        cfi = WhatFrame(next, oi.data.frames);
                        pic = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "pic:");
                        wait = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "wait:");
                        npics.Add(pic); waits.Add(wait);
                    }
                    bool repeat = (bool)cbRepeat.IsChecked;
                    _sprite = new AnimatedSprite(oi, TimeSpan.FromSeconds(1 / G.AppSettings.baseGameFPS), npics, waits,repeat);
                    Canvas.SetLeft(_sprite, RenderSurface.ActualWidth / 2 - oi.lbiCroppedBitmaps[0].PixelWidth / 2);
                    Canvas.SetTop(_sprite, RenderSurface.ActualHeight / 2 - oi.lbiCroppedBitmaps[0].PixelHeight / 2);
                    if (RenderSurface.Children.Count > 0) RenderSurface.Children.Remove(RenderSurface.Children[0]);
                    RenderSurface.Children.Add(_sprite);
                    //   CompositionTarget.Rendering += OnRender;
                    #endregion

                }
                //  else RenderSurface.Children.Clear();
            }
            else ChooseComboboxes(Otvet);
        }

        public void te_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                //  DetectWhereCursor();
            }
        }
        public void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            DetectWhereCursor();
        }
        public List<BitmapSource> CropBitmaps(List<FileDesc> lfd, int nproj)
        {
            if (nproj == -1) return new List<BitmapSource>(0);
            List<BitmapSource> lbsAll = new List<BitmapSource>();
            List<BitmapSource> result = new List<BitmapSource>();
            string st = "";
            for (int i = 0; i < lfd.Count; i++)
            {
                string path = lProjects[nproj].path_to_folder + "\\" + lfd[i].path;
                if (File.Exists(path))
                {
                    #region Load image files
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(path);
                    bi.EndInit();
                    bi.Freeze();
                    lbsAll.Add(bi);
                    #endregion
                    st += Path.GetFileNameWithoutExtension(lfd[i].path) + ", ";
                    #region Create cropped bitmaps
                    int FrameWidth = lfd[i].width + 1;
                    int FrameHeight = lfd[i].height + 1;
                    int ncol = bi.PixelWidth / (lfd[i].width + 1);
                    int nrow = bi.PixelHeight / (lfd[i].height + 1);
                    int num = ncol * nrow;
                    //  List<BitmapSource> lbi = new List<BitmapSource>();
                    try
                    {
                        for (int j = 0; j < num; j++)
                        {
                            int jdncol = j / ncol;
                            Int32Rect irect = new Int32Rect(FrameWidth * (j - (jdncol) * ncol), FrameHeight * (jdncol), FrameWidth - 1, FrameHeight - 1);
                            CroppedBitmap cb = new CroppedBitmap(bi, irect);
                            cb.Freeze();
                            result.Add(cb);
                        }
                    }
                    catch (Exception) { MessageBox.Show("FilllsbImages error!"); }
                    #endregion
                }
            }
            return result;
        }
        string CreatePathFromDatFile(string p)
        {
            string[] sp = p.Split(new char []{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar});
            sp[0]=sp[0]+'\\';
            sp[sp.Length - 1] = "";
            sp[sp.Length - 2] = "";            
            return Path.Combine(sp);
        }
        public List<CroppedBitmap> CropBitmaps(List<FileDesc> lfd, int nproj, ref List<BitmapImage> lbs)
        {
            //if (nproj == -1) return new List<CroppedBitmap>(0);
            string ptf = "";
            if (nproj == -1)
                ptf = CreatePathFromDatFile(teCurActiveDocument.fullpath);
            else ptf = lProjects[nproj].path_to_folder;

            List<CroppedBitmap> result = new List<CroppedBitmap>();
            if (lbs == null) lbs = new List<BitmapImage>();
            string st = "";
            for (int i = 0; i < lfd.Count; i++)
            {
                string path = ptf + "\\" + lfd[i].path;
                if (File.Exists(path))
                {
                    #region Load image files
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(path);
                    bi.EndInit();
                    bi.Freeze();
                    lbs.Add(bi);
                    #endregion
                    st += Path.GetFileNameWithoutExtension(lfd[i].path) + ", ";
                    #region Create cropped bitmaps
                    int FrameWidth = lfd[i].width + 1;
                    int FrameHeight = lfd[i].height + 1;
                    int ncol = bi.PixelWidth / (lfd[i].width + 1);
                    int nrow = bi.PixelHeight / (lfd[i].height + 1);
                    int num = ncol * nrow;
                    //  List<BitmapSource> lbi = new List<BitmapSource>();
                    try
                    {
                        for (int j = 0; j < num; j++)
                        {
                            int jdncol = j / ncol;
                            Int32Rect irect = new Int32Rect(FrameWidth * (j - (jdncol) * ncol), FrameHeight * (jdncol), FrameWidth - 1, FrameHeight - 1);
                            CroppedBitmap cb = new CroppedBitmap(bi, irect);
                            cb.Freeze();
                            result.Add(cb);
                        }
                    }
                    catch (Exception) { MessageBox.Show("FilllsbImages error!"); }
                    #endregion
                }
            }
            return result;
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void np_Click(object sender, RoutedEventArgs e)
        {
            cnp_Click(sender, e);
        }
        public cProject LoadProject(string fn)
        {
            try
            {
                cProject myObject = cProject.LoadProject(fn);
                lProjects.Add(myObject);
                myObject.datatxt = Utils.DatFiles.ParseDatatxt(new FileInfo(myObject.path_to_folder + "\\data\\data.txt"));
                // ParseAllFiles(myObject); 
                int curProject = lProjects.Count - 1;
                //  tviProjects.Tag = lProjects[curProject].files;
                tviProjects.Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action<ArrayList>)((al) =>
                {
                    tviProjects.Tag = lProjects[curProject].files;
                    lfTreeViewItem tvi = new lfTreeViewItem();
                    //ArrayList al = myObject.files[0] as ArrayList;
                    tvi.HeaderText = System.IO.Path.GetFileNameWithoutExtension((string)(al[0] as ArrayList)[0]);
                    LanguageContext.Binder("etbProj", "Text", "Project '{0}'", tvi.textBlock, EditableTextBlock.TextFormatProperty);
                    tvi.ContextMenu = cmc.cmProject;
                    tvi.Type = lfTreeViewItem.isProject;
                    tvi.Tag = al;
                    if (al.Count > 0) tvi.Items.Add(null);
                    tvi.Expanded += new RoutedEventHandler(tvi_Expanded);
                    tviProjects.Items.Add(tvi);
                }), lProjects[curProject].files);
                //LoadProjectToTreeView(myObject.files[0] as ArrayList, tviProjects);
                return myObject;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); return null; }
        }
        public bool isExistInFolder(lfTreeViewItem parent, object item)
        {

            string name = "";
            if (item is string) name = Path.GetFileName(item as string);
            else name = (item as ArrayList)[0] as string;
            for (int i = 0; i < parent.Items.Count; i++)
            {
                if ((parent.Items[i] as lfTreeViewItem).HeaderText == name) return true;
            }
            return false;
        }
        public void tvi_Expanded(object sender, RoutedEventArgs e)
        {
            //if (tvExplorer.SelectedItem!=lfTreeViewItem.isProjects)
            //if (sender != tvExplorer.SelectedItem ) return;            
            var parent = sender as lfTreeViewItem;
            if (parent == null) return;

            if (!parent.isLoaded)
            {

                //parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                //{
                parent.isLoaded = true;
                ArrayList al = parent.Tag as ArrayList;
                ArrayList item = null;
                if (parent.Type == lfTreeViewItem.isProject) item = al[0] as ArrayList;
                if (parent.Type == lfTreeViewItem.isFolder) item = al;
                if (parent.Items.Count > 0)
                    if (parent.Items[0] == null) parent.Items.RemoveAt(0);
                // Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                //     {
                for (int t = 1; t < item.Count; t++)
                {
                    if (isExistInFolder(parent, item[t])) continue;
                    parent.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action<int>)((i) =>
                    {

                        //while (item[i] == null) { };
                        if (item[i] is ArrayList) //folder
                        {
                            parent.textBlock.PreviewKeyDown += new KeyEventHandler(etb1_KeyDown);
                            lfTreeViewItem tvi = new lfTreeViewItem(lfTreeViewItem.isFolder) { HeaderText = System.IO.Path.GetFileName(((string)((item[i] as ArrayList)[0]))) };
                            tvi.Expanded += new RoutedEventHandler(tvi_Expanded);
                            parent.Items.Add(tvi);
                            tvi.Tag = item[i];
                            parent.textBlock.Tag = tvi.Tag;
                            tvi.ContextMenu = cmc.cmFolder;
                            if ((item[i] as ArrayList).Count > 1)
                                tvi.Items.Add(null);
                        }
                        else   //file
                        {
                            parent.textBlock.PreviewKeyDown += new KeyEventHandler(etb1_KeyDown);
                            lfTreeViewItem tvi2 = new lfTreeViewItem(RestoreRootPath((item[i] as string), WhatProject(parent))) { HeaderText = System.IO.Path.GetFileName((string)item[i]) };
                            parent.Items.Add(tvi2);
                            tvi2.Tag = item[i];
                            parent.textBlock.Tag = tvi2.Tag;
                            if (!isGraphicFile(item[i]))
                                tvi2.ContextMenu = cmc.cmFile;
                            else tvi2.ContextMenu = cmc.cmGraphicFile; //if is Graphic file

                        }
                    }), t);
                }//for
                // }));
                //    }));
            }
        }
        public bool isGraphicFile(object path)
        {
            string ext = Path.GetExtension(path as string);
            string graphExt = "bmp gif png jpg jpeg jpe jfif exif tiff wmp tif";
            string[] ge = graphExt.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ge.Length; i++) if (ext == '.' + ge[i]) return true;
            return false;
        }
        public string DetectTypeForTreeViewItem(string ed)
        {
            if (isNeedDecrypt(ed)) return "DatFile"; else return "PlainTextFile";
        }
        public string DetectType(string fname, int n)
        {
            return RestoreRootPath(fname, n);
        }
        public bool isNeedDecrypt(string fname)
        {
            string extension = System.IO.Path.GetExtension(fname);
            switch (extension)
            {
                case ".dat": return true;
                default: return false;
            }
        }
        public void LoadProjectToTreeView(ArrayList al, lfTreeViewItem root, int rl = 0)
        {
            try
            {
                lfTreeViewItem buf = null;

                root.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    rl++;
                    root.textBlock.PreviewKeyDown += new KeyEventHandler(etb1_KeyDown);
                    lfTreeViewItem tvi = new lfTreeViewItem(lfTreeViewItem.isFolder) { HeaderText = System.IO.Path.GetFileName(((string)(al[0]))) };
                    root.Items.Add(tvi);
                    tvi.Tag = al;
                    root.textBlock.Tag = tvi.Tag;
                    if (rl > 1) //If lfTreeViewItem is root of project
                        tvi.ContextMenu = cmc.cmFolder;
                    else
                    {
                        tvi.HeaderText = System.IO.Path.GetFileNameWithoutExtension((string)al[0]);
                        LanguageContext.Binder("etbProj", "Text", "Project '{0}'", tvi.textBlock, EditableTextBlock.TextFormatProperty);
                        tvi.ContextMenu = cmc.cmProject;
                        tvi.Type = lfTreeViewItem.isProject;
                    }
                    buf = tvi;
                }));

                ////////////////////////////////////////////////////////////////////////                
                root.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action<lfTreeViewItem>)((tvi) =>
                {
                    for (int i = 1; i < al.Count; i++)
                    {
                        if (al[i] is string)
                        {
                            root.textBlock.PreviewKeyDown += new KeyEventHandler(etb1_KeyDown);
                            lfTreeViewItem tvi2 = new lfTreeViewItem(RestoreRootPath((al[i] as string), lProjects.Count - 1)) { HeaderText = System.IO.Path.GetFileName((string)al[i]) };
                            tvi.Items.Add(tvi2);
                            tvi2.Tag = al[i];
                            root.textBlock.Tag = tvi2.Tag;
                            if (!isGraphicFile(al[i]))
                                tvi2.ContextMenu = cmc.cmFile;
                            else tvi2.ContextMenu = cmc.cmGraphicFile;
                        }
                        else LoadProjectToTreeView((ArrayList)al[i], tvi, rl);
                    }//for       
                }), buf);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public string RestoreRootPath(string path, int n)
        {
            if (path[0] == '\\')
                return lProjects[n].path_to_folder + path;
            return path;
        }
        public void RestoreTagDiscInTreeViewItems(ArrayList al, int rl = 0)
        {
            try
            {
                rl++;
                string fd0 = (string)al[0];
                //Console.WriteLine(fd0);
                for (int i = 1; i < (al as ArrayList).Count; i++)
                {
                    if (al[i] is string)
                    {
                        string fd = (string)al[i];
                        Console.WriteLine(fd);
                    }
                    else RestoreTagDiscInTreeViewItems((ArrayList)al[i], rl);
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void CreateProject(string path, string pass)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            lProjects.Add(new cProject());
            lProjects[lProjects.Count - 1].currentpath = path;
            lProjects[lProjects.Count - 1].pass = pass;
            lfTreeViewItem tvi = new lfTreeViewItem(lfTreeViewItem.isProject);
            tvi.IsSelected = true;
            tvi.Expanded += new RoutedEventHandler(tvi_Expanded);
            tvi.HeaderText = name;
            LanguageContext.Binder("etbProj", "Text", "Project '{0}'", tvi.textBlock, EditableTextBlock.TextFormatProperty);

            tvi.ContextMenu = cmc.cmProject;
            tvi.IsExpanded = false;
            tviProjects.Items.Add(tvi);
            ArrayList al = new ArrayList();
            string ed = name;

            al.Add(ed);
            int curProject = lProjects.Count - 1;
            lProjects[curProject].files.Add(al);
            tvi.Tag = lProjects[curProject].files;
            //lProjects[curProject].files[lProjects[curProject].files.Count - 1];
        }
        private void cnp_Click(object sender, RoutedEventArgs e)//Create new project
        {
            try
            {
                var sd = new SaveFileDialog();
                sd.Filter = "Project file(*.lfp)|*.lfp";
                sd.DefaultExt = "lfp";
                if (sd.ShowDialog() == true)
                {
                    G.needCreateProject = true;
                    CreateProject(sd.FileName, "SiuHungIsAGoodBearBecauseHeIsVeryGood");
                    FillsbRunGame();
                    pp_Click(sender, e);
                    cProject.SaveProject(lProjects[lProjects.Count - 1].currentpath, lProjects[lProjects.Count - 1]);
                    lProjects[lProjects.Count - 1].datatxt =
                    Utils.DatFiles.ParseDatatxt(new FileInfo(lProjects[lProjects.Count - 1].path_to_folder + "\\data\\data.txt"));
                    SaveToProjectsTXT(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].currentpath);
                    Utils.tv.RefreshEditableTextBlock(tviProjects);
                    if (lProjects.Count > 0) G.CurrentActiveProject = lProjects.Count - 1;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void andf_Click(object sender, RoutedEventArgs e)// Add new file (treeview context menu)
        {
            try
            {
                if (!File.Exists(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_exe) ||
                    System.IO.Path.GetExtension(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_exe) != ".exe" || !Directory.Exists(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_folder))
                    pp_Click(sender, e);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Crypted Little fighter 2 dat file|*.dat|Plain text file|*.txt";
                sfd.AddExtension = true;
                if (sfd.ShowDialog() == true)
                {
                    //cProject.EntityDesc fd;        
                    lfTreeViewItem temp = null;

                    //if (sfd.FilterIndex == 1)
                    if (System.IO.Path.GetExtension(sfd.FileName) == ".dat")
                    {
                        sfd.DefaultExt = "dat";
                        //    fd = new cProject.EntityDesc(DeletePath(sfd.FileName), true);
                        FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                        temp = CreatelfTreeViewItem(sfd.FileName, (tvExplorer.SelectedItem as lfTreeViewItem), sfd.FileName, true);
                        fs.Close();
                    }
                    else
                    {
                        sfd.DefaultExt = "txt";
                        //     fd = new cProject.EntityDesc(DeletePath(sfd.FileName), false);
                        FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                        temp = CreatelfTreeViewItem(sfd.FileName, (tvExplorer.SelectedItem as lfTreeViewItem), sfd.FileName, false);
                        fs.Close();
                    }
                    cProject.SaveProject(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].currentpath, lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)]);
                    temp.IsSelected = true;
                    openfile_Click(sender, e);
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        public string DeletePath(string path, int curProject)
        {
            int index = path.IndexOf(lProjects[curProject].path_to_folder);
            if (index == -1)
            {
                return path;
            }
            else
                return path.Remove(0, lProjects[curProject].path_to_folder.Length);
        }
        private void myHandler(object sender, MouseButtonEventArgs args)
        {
            try
            {
                int curProject;
                lfTreeViewItem item = null;
                if (args.Source is EditableTextBlock) item = ((args.Source as EditableTextBlock).Parent as StackPanel).Parent as lfTreeViewItem;
                if (args.Source is Image) item = ((args.Source as Image).Parent as StackPanel).Parent as lfTreeViewItem;
                if (args.Source is StackPanel) item = (args.Source as StackPanel).Parent as lfTreeViewItem;
                if (args.Source is lfTreeViewItem) item = args.Source as lfTreeViewItem;
                if (item != null)
                {
                    tvExplorer.Focus();
                    item.Focus();
                    item.IsSelected = true;
                    lfTreeViewItem parent = item;
                    while (true)
                    {
                        if (parent.Type == lfTreeViewItem.isProjects) { (statusbar.Items[0] as TextBlock).Text = "-1"; curProject = -1; break; }
                        if (parent.Type == lfTreeViewItem.isProject)
                        {
                            (statusbar.Items[0] as TextBlock).Text = tviProjects.Items.IndexOf(parent).ToString();
                            curProject = tviProjects.Items.IndexOf(parent);
                            break;
                        }
                        parent = parent.Parent as lfTreeViewItem;
                    }

                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public int WhatProject(lfTreeViewItem item)
        {
            lfTreeViewItem parent = item;
            if (parent == null) return -1;
            while (true)
            {
                if (parent.Type == lfTreeViewItem.isProjects) { (statusbar.Items[0] as TextBlock).Text = "-1"; break; }
                if (parent.Type == lfTreeViewItem.isProject)
                {
                    (statusbar.Items[0] as TextBlock).Text = tviProjects.Items.IndexOf(parent).ToString();
                    return tviProjects.Items.IndexOf(parent);
                }
                parent = parent.Parent as lfTreeViewItem;
            }
            MessageBox.Show("WhatProject return -1;");
            return -1;
        }

        private void aedf_Click(object sender, RoutedEventArgs args)//Add existing  file
        {
            try
            {
                if (!File.Exists(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_exe) ||
                    System.IO.Path.GetExtension(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_exe) != ".exe" ||
                    !Directory.Exists(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].path_to_folder))
                    pp_Click(sender, args);
                OpenFileDialog ofd = new OpenFileDialog(); ofd.Multiselect = true;
                ofd.Filter = "Any files|*.*|Crypted Little fighter 2 dat file|*.dat|Plain text file|*.txt|Litte fighter sprite|*.bmp|Sound|*.wav|Music|*.wma";
                if (ofd.ShowDialog() == true)
                {
                    for (int i = 0; i < ofd.FileNames.Length; i++)
                    {
                        lfTreeViewItem tvi = null;
                        string extension = System.IO.Path.GetExtension(ofd.FileNames[i]);
                        switch (extension)
                        {
                            case ".dat": tvi = CreatelfTreeViewItem(ofd.FileNames[i], tvExplorer.SelectedItem as lfTreeViewItem, ofd.FileNames[i], true); break;
                            case ".txt": tvi = CreatelfTreeViewItem(ofd.FileNames[i], tvExplorer.SelectedItem as lfTreeViewItem, ofd.FileNames[i], false); break;
                            default: tvi = CreatelfTreeViewItem(ofd.FileNames[i], tvExplorer.SelectedItem as lfTreeViewItem, ofd.FileNames[i], false); break;
                        }
                        (tvi.Parent as lfTreeViewItem).IsExpanded = true;
                    }
                    cProject.SaveProject(lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)].currentpath, lProjects[WhatProject(tvExplorer.SelectedItem as lfTreeViewItem)]);
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void tvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        public lfTreeViewItem CreatelfTreeViewItem(string Type, lfTreeViewItem parent, string pathtofile, bool isCrypted)
        {
            if (pathtofile.Length > 0)
            {
                int curproject = WhatProject(parent);
                string st = DeletePath(pathtofile, curproject);
                foreach (lfTreeViewItem t in parent.Items) //check if exist
                {
                    if (t.Tag.ToString() == st) return t;

                }
            }
            object obj = null;
            if (Type == lfTreeViewItem.isFolder || Type == lfTreeViewItem.isProjects)
            {
                string fd = pathtofile;
                obj = new ArrayList();
                (obj as ArrayList).Add(fd);
            }
            else obj = DeletePath(pathtofile, WhatProject(parent));
            object td = (parent).Tag;

            ArrayList alParent = td as ArrayList;
            if (parent.Type == lfTreeViewItem.isProject) alParent = alParent[0] as ArrayList;
            alParent.Add(obj);
            lfTreeViewItem tvi = null;
            if (Type == lfTreeViewItem.isFile)  ///Bad code!!!!!!!!!!
                tvi = new lfTreeViewItem(pathtofile) { HeaderText = System.IO.Path.GetFileName(pathtofile) };
            else tvi = new lfTreeViewItem(Type) { HeaderText = System.IO.Path.GetFileName(pathtofile) };
            if (Type == lfTreeViewItem.isFolder)
                tvi.Expanded += new RoutedEventHandler(tvi_Expanded);
            tvi.Tag = alParent[alParent.Count - 1];
            //(tvExplorer.SelectedItem as lfTreeViewItem).Items.Add(tvi);
            parent.Items.Add(tvi);
            if (Type == lfTreeViewItem.isFolder) tvi.ContextMenu = cmc.cmFolder;
            if (Type == lfTreeViewItem.isFile)
                if (!isGraphicFile(pathtofile))
                    tvi.ContextMenu = cmc.cmFile;
                else tvi.ContextMenu = cmc.cmGraphicFile;
            return tvi;
        }
        private void anf_Click(object sender, RoutedEventArgs args)//Add new folder
        {
            try
            {
                {
                    NeedTypeNameForCreateFolder = true;
                    lfTreeViewItem temp = CreatelfTreeViewItem(lfTreeViewItem.isFolder, (tvExplorer.SelectedItem as lfTreeViewItem), "", false);
                    (tvExplorer.SelectedItem as lfTreeViewItem).IsExpanded = true;
                    temp.IsSelected = true;
                    SetCurrentItemInEditMode(true);

                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #region Mainmenu event dispatchers
        public void LoadListOfOpenFiles()
        {
            //Stopwatch sw = new Stopwatch(); sw.Start();
            isOpenManyFiles = true;
            StreamReader sr = new StreamReader(programfolder + "ofiles.ini", Encoding.UTF8);
            string path;
            int nproject;
            while (true)
            {
                path = sr.ReadLine();
                if (path == null) break;
                if (path.Trim().Length == 0) continue;
                nproject = Convert.ToInt32(G.GetObjectFromTag(path, 1));
                path = G.GetObjectFromTag(path, 0);
                if (!File.Exists(path)) continue;
                isOpenManyFiles = true;
                OpenFile(path, nproject);
            }
            sr.BaseStream.Close();
            sr.Close();
            //sw.Stop();
            //teOutput.AppendText("LoadListOfOpenFiles(): " + sw.Elapsed.TotalSeconds.ToString() + " sec.");
            //teOutput.AppendText(Environment.NewLine);
            //sw.Reset();
        }
        public void SaveListOfOpenFiles()
        {
            StreamWriter f;
            if (!File.Exists(programfolder + "ofiles.ini")) File.Create(programfolder + "ofiles.ini");
            f = new StreamWriter(programfolder + "ofiles.ini");
            ItemCollection ic = Utils.AvalonDock.GetDocuments(DockManager.ActiveDocument);
            for (int i = 0; i < DockManager.Documents.Count; i++) f.WriteLine(DockManager.Documents[i].Tag);
            f.Close();
        }
        private void of_Click(object sender, RoutedEventArgs e)//Open file
        {
            //try
            {
                var od = new OpenFileDialog() { Multiselect = true };
                od.Filter = "Any files|*.*|Crypted Little fighter 2 dat file|*.dat|Plain text file|*.txt|Litte fighter sprite|*.bmp|Sound|*.wav|Music|*.wma";
                if (od.ShowDialog() == true)
                {
                    if (od.FileNames.Length > 1) isOpenManyFiles = true; else isOpenManyFiles = false;
                    for (int i = 0; i < od.FileNames.Length; i++) OpenFile(od.FileNames[i]);
                    //  if (od.FilterIndex == 1)
                    /*       for (int i = 0; i < od.FileNames.Length; i++)
                           {
                               if (System.IO.Path.GetExtension(od.FileNames[i]) == ".dat")
                               {
                                   string ed = od.FileName;
                                   OpenTextFile(od.FileNames[i], -1);

                               }
                               if (System.IO.Path.GetExtension(od.FileNames[i]) == ".txt")
                               {
                                   string ed = od.FileName;
                                   OpenTextFile(od.FileNames[i], -1);
                               }
                               if (System.IO.Path.GetExtension(od.FileNames[i]) == ".bmp")
                               {
                                   OpenLFSprite(od.FileNames[i]);
                               }
                               if (System.IO.Path.GetExtension(od.FileNames[i]) == ".wav" || System.IO.Path.GetExtension(od.FileNames[i]) == ".wma")
                               {
                                   OpenWav(od.FileNames[i]);
                               }

                           }
                                   */
                }
            }
            // catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void OpenWav(string fn, int n)
        {
            var dc = new DocumentContent();
            dc.Name = "name";
            dc.Tag = fn + ';' + n;
            dc.Title = System.IO.Path.GetFileName(fn);
            //   dc.IsFloatingAllowed = false;
            StackPanel sp = new StackPanel()
            {
                Height = 100,
                Width = 100,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,


            };
            Button bPlaySound = new Button() { Content = "Play" };
            //CheckBox cbLoop = new CheckBox() { Content="Loop", Margin=new Thickness(0,5,0,0) };
            bPlaySound.Tag = fn;
            bPlaySound.IsDefault = true;
            bPlaySound.Click += new RoutedEventHandler(bPlaySound_Click);
            sp.Children.Add(bPlaySound);
            //sp.Children.Add(cbLoop);            
            dc.Content = sp;
            dc.Show(DockManager);
            dc.Activate();
        }
        public MyMediaPlayer player = new MyMediaPlayer();
        public void player_MediaEnded(object sender, EventArgs e)
        {
            ((sender as MyMediaPlayer).Tag as Button).Content = "Play";
        }
        public void bPlaySound_Click(object sender, RoutedEventArgs e)
        {
            string fn = (string)(sender as Button).Tag;
            if ((sender as Button).Content.ToString() == "Play")
            {
                player.Open(new Uri(fn, UriKind.Absolute));
                player.Tag = (sender as Button);
                player.MediaEnded += new EventHandler(player_MediaEnded);
                player.Play();
                (sender as Button).Content = "Stop";
            }
            else
                if ((sender as Button).Content.ToString() == "Stop")
                {
                    player.Stop();
                    (sender as Button).Content = "Play";
                }
        }
        public void OpenLFSprite(string fn, int n)
        {
            var dc = new DocumentContent();
            dc.Name = "name";
            dc.Tag = fn + ';' + n;
            dc.Title = System.IO.Path.GetFileName(fn);
            // dc.IsFloatingAllowed = false;
            /////////////////////////////////////////////////////////////////////           
            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Image myImage = new Image() { UseLayoutRounding = true, SnapsToDevicePixels = true };
            sv.Content = myImage;
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            //myBitmapImage.CacheOption = BitmapCacheOption.None;
            myBitmapImage.UriSource = new Uri(fn);
            //            myBitmapImage.DecodePixelWidth = 200;
            myBitmapImage.EndInit();
            myImage.Width = myBitmapImage.PixelWidth;
            myImage.Height = myBitmapImage.PixelHeight;
            myImage.Source = myBitmapImage;
            /////////////////////////////////////////////////////////////////////
            //dc.Tag = ed;
            dc.Content = sv;
            dc.Show(DockManager);
            dc.Activate();

        }
        private void delspaces_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                //(DockManager.ActiveDocument as DocumentContent).Content as TextEditor;
                te.TextChanged -= new EventHandler(te_TextChanged);
                te.Document.LineCountChanged -= new EventHandler(Document_LineCountChanged);
                for (int i = 0; i < te.LineCount; i++)
                {
                    string str = te.Document.GetText(te.Document.Lines[i].Offset, te.Document.Lines[i].Length);
                    int nSpaces = str.Length - str.TrimEnd().Length;
                    te.Document.Remove(te.Document.Lines[i].Offset + te.Document.Lines[i].Length - nSpaces, nSpaces);
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
            finally
            {
                Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).TextChanged += new EventHandler(te_TextChanged);
                Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Document.LineCountChanged += new EventHandler(Document_LineCountChanged);
            }
        }
        private void delemptyline_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                //(DockManager.ActiveDocument as DocumentContent).Content as TextEditor;
                te.TextChanged -= new EventHandler(te_TextChanged);
                te.Document.LineCountChanged -= new EventHandler(Document_LineCountChanged);
                for (int i = 0; i < te.LineCount; i++)
                {
                    string str = te.Document.GetText(te.Document.Lines[i].Offset, te.Document.Lines[i].TotalLength).Trim();
                    if (str.Length == 0)
                    {
                        te.Document.Remove(te.Document.Lines[i].Offset, te.Document.Lines[i].TotalLength);
                        if (i != te.LineCount - 1)
                            i--;
                    }
                }

                te.Document.Remove(te.Document.Lines[te.Document.Lines.Count - 2].Offset + te.Document.Lines[te.Document.Lines.Count - 2].TotalLength - 1, 1);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
            finally
            {
                Utils.AvalonEdit.GetTextEditorFromContent
                    ((DockManager.ActiveDocument as DocumentContent).Content).TextChanged
                    += new EventHandler(te_TextChanged);
                Utils.AvalonEdit.GetTextEditorFromContent
                    ((DockManager.ActiveDocument as DocumentContent).Content).Document.LineCountChanged
                    += new EventHandler(Document_LineCountChanged);
            }
        }
        public void GenerateError()
        {
            List<int> asdf = new List<int>();
            asdf[55] = 4;
        }
        private void ndf_Click(object sender, RoutedEventArgs e)//File->New dat file
        {
            try
            {
                /*      SaveFileDialog sfd = new SaveFileDialog();
                      sfd.Filter = "Crypted Little fighter 2 dat file|*.dat|Plain text file|*.txt";
                      if (sfd.ShowDialog() == true)
                      {
                          string ed;
                          if (sfd.FilterIndex == 1)
                              ed = sfd.FileName;
                          else ed = sfd.FileName;
                          FileStream fs = new FileStream(sfd.FileName, FileMode.Create); fs.Close();

                          OpenTextFile(sfd.FileName, -1);

                      }*/
                lfTreeViewItem sel = tvExplorer.SelectedItem as lfTreeViewItem;
                if (sel.Type != lfTreeViewItem.isProjects && sel.Type != lfTreeViewItem.isFile)
                    andf_Click(sender, e);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }

        }
        private void sf_Click(object sender, RoutedEventArgs e)//Save file menu
        {
            try
            {
                string pth;
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                if (te == null) return;
                string fname = G.GetObjectFromTag(DockManager.ActiveDocument.Tag, 0);
                if (fname[0].CompareTo((char)92) == 0)
                    pth = lProjects[te.numProject].path_to_folder + fname;
                else pth = fname;
                if (isNeedDecrypt(fname)) functions.PlainTextToDatFile(te.Text, pth, Utils.Project.GetProjectPassword(lProjects, te.numProject));
                else te.Save(pth);
                Grid g = new Grid();
                var dc = Utils.AvalonDock.GetActiveDocument(te);
                if (dc is DocumentContent)
                {
                    pth = (dc as DocumentContent).Title;
                    if (pth[pth.Length - 1] == '*')
                        (dc as DocumentContent).Title = (dc as DocumentContent).Title.Remove(pth.Length - 1, 1);
                }
                if (System.IO.Path.GetFileName(fname) != "stage.dat" && System.IO.Path.GetFileName(fname) != "data.txt")
                {
                    ObjectInfo oi = FindDatafile(te.numProject, fname);
                    oi.data = Utils.DatFiles.ParseTextWithErrorsReturn(te.Text, fname, ref oi.errors, Utils.Project.GetProject(te.numProject));
                    SendErrorsListToGridErrors(oi);
                    UpdateFoldings(oi.data, te);
                }

                /*         ObjectInfo oi = null;
                         if (te.numProject == -1)
                         {
                             oi = new ObjectInfo();
                             oi.data = Utils.DatFiles.ParseDatFileWithErrorsReturn2(te.Text, fname, ref oi.errors);
                             SendErrorsListToGridErrors(oi);
                         }
                         else
                         {
                             oi = FindDatafile(te.numProject, fname);
                             if (oi != null)
                             {                        
                                 oi.data = Utils.DatFiles.ParseDatFileWithErrorsReturn2(te.Text, fname,  ref oi.errors,lProjects[te.numProject]);
                                 SendErrorsListToGridErrors();
                             }
                         }               */
                if (te.fullpath.Contains("data.txt"))
                {
                    FileInfo fi = new FileInfo(lProjects[teCurActiveDocument.numProject].path_to_folder + "\\data\\data.txt");
                    lProjects[teCurActiveDocument.numProject].datatxt = Utils.DatFiles.ParseDatatxt(fi);
                }
                DetectWhereCursor();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public ObjectInfo FindDatafile(int nproject, string fn)
        {
            if (nproject == -1) return new ObjectInfo() { };
            for (int i = 0; i < lProjects[nproject].datatxt.lObject.Count; i++)
            {
                if (fn.Contains(lProjects[nproject].datatxt.lObject[i].path))
                    return lProjects[nproject].datatxt.lObject[i];
            }
            return new ObjectInfo(-1, -1, fn, null);
        }
        public void sall_Click(object sender, RoutedEventArgs e)//Save all files
        {
            for (int i = 0; i < DockManager.Documents.Count; i++)
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.Documents[i].Content);

                string pth;
                string ed = G.GetObjectFromTag(DockManager.Documents[i].Tag, 0);
                int curProject = Convert.ToInt32(G.GetObjectFromTag(DockManager.Documents[i].Tag, 1));
                if (ed[0].CompareTo((char)92) == 0)
                    pth = lProjects[curProject].path_to_folder + ed;
                else
                    pth = ed;
                if (isNeedDecrypt(ed))
                {
                    if (te.numProject != -1)
                        functions.PlainTextToDatFile(te.Text, pth, lProjects[curProject].pass);
                    else
                        functions.PlainTextToDatFile(te.Text, pth, G.AppSettings.StandardPassword);
                }
                else
                {
                    if (te != null)
                        te.Save(pth);
                }
                pth = DockManager.Documents[i].Title;
                if (pth[pth.Length - 1] == '*')
                    DockManager.Documents[i].Title = DockManager.Documents[i].Title.Remove(pth.Length - 1, 1);
            }
        }
        private void cf_Click(object sender, RoutedEventArgs e)//Close file
        {
            (DockManager.ActiveDocument as DocumentContent).Close();
        }
        private void caf_Click(object sender, RoutedEventArgs e)//Close all files
        {
            int cnt = DockManager.Documents.Count;
            for (int i = 0; i < cnt; i++)
                (DockManager.Documents[0] as DocumentContent).Close();
        }
        private void aep_Click(object sender, RoutedEventArgs e)// Add existing project
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Project file(*.lfp)|*.lfp"; ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                for (int i = 0; i < ofd.FileNames.Length; i++)
                {
                    //LoadProject(ofd.FileNames[i]);
                    BackgroundWorker bwLoadProject = new BackgroundWorker();
                    bwLoadProject.DoWork += new DoWorkEventHandler(bwLoadProject_DoWork);
                    bwLoadProject.RunWorkerAsync(ofd.FileNames[i]);
                    SaveToProjectsTXT(ofd.FileNames[i]);
                }
                FillsbRunGame();
                if (lProjects.Count > 0) G.CurrentActiveProject = lProjects.Count - 1;
            }
        }
        public void bwLoadProject_DoWork(object sender, DoWorkEventArgs e)
        {

            cProject cp = LoadProject(e.Argument as string);
        }
        private void SaveToProjectsTXT(string fn)
        {
            StreamWriter sw = new StreamWriter("projects.txt", true, System.Text.Encoding.UTF8);
            sw.WriteLine(fn);
            sw.Close();
        }

        private void pp_Click(object sender, RoutedEventArgs e)//Project properties
        {
            try
            {
                int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
                if (curProject == -1) return;
                wProjectOptions wp = new wProjectOptions(lProjects[curProject]);
                wp.ShowDialog();
                cProject.SaveProject(lProjects[curProject].currentpath, lProjects[curProject]);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #endregion
        public void rungame_Click(object sender, RoutedEventArgs e)//Run game from treeview context menu
        {
            try
            {
                int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
                if (!File.Exists(lProjects[curProject].path_to_exe) ||
                 System.IO.Path.GetExtension(lProjects[curProject].path_to_exe) != ".exe" || !Directory.Exists(lProjects[curProject].path_to_folder))
                    pp_Click(sender, e);

                //((sender as System.Windows.Controls.MenuItem).Parent as System.Windows.Controls.ContextMenu).Parent
                RunGame(WhatProject(tvExplorer.SelectedItem as lfTreeViewItem));
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        public void RemoveProjectFromList()
        {
            int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
            (tviProjects as lfTreeViewItem).Items.RemoveAt(curProject);
            lProjects.RemoveAt(curProject);
            if (lProjects.Count == 0) curProject = -1; else curProject = lProjects.Count - 1;
            FillsbRunGame();
            StreamWriter sw = new StreamWriter("projects.txt"); sw.Close();
            for (int i = 0; i < lProjects.Count; i++)
            {
                SaveToProjectsTXT(lProjects[i].currentpath);
            }
        }
        public void removeproject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (MessageBox.Show(this, "Do you want remove project from list?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                if (MessageBoxQuestion.Show(LanguageDictionary.Current.Translate<string>("tRP", "Text", "Do you want remove project from list?")) == MessageBoxQuestion.WPFMessageBoxResult.Yes)
                {
                    RemoveProjectFromList();
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void prj_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lProjects.Count == 0)
                {
                    ((sender as MenuItem).Items[2] as MenuItem).IsEnabled = false;
                    // ((sender as MenuItem).Items[3] as MenuItem).IsEnabled = false;
                    //  ((sender as MenuItem).Items[4] as MenuItem).IsEnabled = false;
                }
                else
                {
                    ((sender as MenuItem).Items[2] as MenuItem).IsEnabled = true;
                    //((sender as MenuItem).Items[3] as MenuItem).IsEnabled = true;
                    // ((sender as MenuItem).Items[4] as MenuItem).IsEnabled = true;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #region Text Edit
        private void undo_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Document.UndoStack.Undo();
        }
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Document.UndoStack.Redo();
        }
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Cut();
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Copy();
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).Paste();
        }
        private void selectall_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).SelectAll();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).TextArea.Document.Remove(Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).SelectionStart, Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).SelectionLength);
        }
        private void removefile_Click(object sender, RoutedEventArgs e)//delete file
        {
            int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
            lfTreeViewItem parent = (lfTreeViewItem)(tvExplorer.SelectedItem as lfTreeViewItem).Parent;
            ArrayList aparent = parent.Tag as ArrayList;
            if (parent.Type == lfTreeViewItem.isProject) aparent = aparent[0] as ArrayList;

            object std = (tvExplorer.SelectedItem as lfTreeViewItem).Tag;
            int index = (aparent as ArrayList).IndexOf(std);
            (aparent as ArrayList).RemoveAt(index);

            parent.Items.RemoveAt(parent.Items.IndexOf(tvExplorer.SelectedItem));
            cProject.SaveProject(lProjects[curProject].currentpath, lProjects[curProject]);
        }
        private void delfolder_Click(object sender, RoutedEventArgs e)//delete folder
        {
            //if (MessageBox.Show("Do you want remove folder?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.No) return;
            if (MessageBoxQuestion.Show(LanguageDictionary.Current.Translate<string>("tDRF", "Text", "Do you want remove folder?")) == MessageBoxQuestion.WPFMessageBoxResult.No) return;
            removefile_Click(sender, e);
        }
        private void edit_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                bool b;
                if (DockManager.Documents.Count == 0) b = false; else b = true;

                for (int i = 0; i < (sender as MenuItem).Items.Count; i++)
                {
                    if ((sender as MenuItem).Items[i] is Separator) continue;
                    ((sender as MenuItem).Items[i] as MenuItem).IsEnabled = b;
                }
                if (b)
                {
                    if (Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).CanUndo == true)
                        ((sender as MenuItem).Items[0] as MenuItem).IsEnabled = true;
                    else ((sender as MenuItem).Items[0] as MenuItem).IsEnabled = false;

                    if (Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content).CanRedo == true)
                        ((sender as MenuItem).Items[1] as MenuItem).IsEnabled = true;
                    else ((sender as MenuItem).Items[1] as MenuItem).IsEnabled = false;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #endregion
        private void openfile_Click(object sender, RoutedEventArgs e)//treeview
        {
            try
            {   // add another file format
                if (tvExplorer.SelectedItem == null) return;
                object td = (tvExplorer.SelectedItem as lfTreeViewItem).Tag;
                if (td is ArrayList) return; //if lfTreeViewItem is folder
                string str = (string)td;
                isOpenManyFiles = false;
                OpenFile(str, WhatProject(tvExplorer.SelectedItem as lfTreeViewItem));
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public bool isAlreadyOpened(string path)
        {
            for (int i = 0; i < DockManager.Documents.Count; i++)
            {
                DocumentContent dc = (DockManager.Documents[i] as DocumentContent);
                string st = G.GetObjectFromTag(dc.Tag, 0);
                if (st == path)
                {
                    (dc).Activate();
                    // MessageBox.Show("File already open", "Warning");
                    MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFAO", "Text", "File already open."));
                    return true;

                }
            }
            return false;
        }
        /*
        public List<List<FileDesc>> GetAllWeapons(int n)
        {
            List<List<FileDesc>> result = new List<List<FileDesc>>();
            return null;
        }*/
        public void bwLoadWeapons_DoWork(object sender, DoWorkEventArgs e)
        {
            bWeaponListUpdate.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                //bWeaponListUpdate.Visibility = System.Windows.Visibility.Hidden;
                bWeaponListUpdate.Opacity = 0.33;
            }));
            Stopwatch sw = new Stopwatch();
            //lProjects[G.CurrentActiveProject].lWeapons.Clear();
            sw.Start();
            #region Loading data.txt
            lProjects[G.CurrentActiveProject].datatxt =
                Utils.DatFiles.ParseDatatxt(new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + "\\data\\data.txt"));
            #endregion
            //lProjects[G.CurrentActiveProject].isWeaponBitmapsLoaded = true;
            //foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
            for (int i = 0; i < lProjects[G.CurrentActiveProject].datatxt.lObject.Count; i++)
            {
                ObjectInfo oi = lProjects[G.CurrentActiveProject].datatxt.lObject[i];
                //if (oi.type == 1 || oi.type == 2 || oi.type == 4 || oi.type == 6)    // 1-light weapon, 2-heavy weapon, 4-throw weapon, 6-drinks
                //if ((oi.id >= 100 && oi.id <= 199)||(oi.id==217 || oi.id==218))
                if (fWType.isRelevant(oi.type.Value))
                    if (fWId.isRelevant(oi.id.Value))
                    {
                        //   if (oi.data == null)
                        oi.data = Utils.DatFiles.ParseDatFileWithErrorsReturn(
                            new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + "\\" + oi.path),
                                         lProjects[G.CurrentActiveProject].pass,
                                         ref oi.errors, lProjects[G.CurrentActiveProject]);
                        /* lfItem item = new lfItem(oi.id.Value, oi.type.Value, lProjects[G.CurrentActiveProject].datatxt.lObject[i].path);
                          #region Fill Width and Height
                          foreach (FileDesc fd in oi.data.header.files)
                          {
                              item.Width.Add(fd.width);
                              item.Height.Add(fd.height);
                          }
                          #endregion
                          item.llbsCroppedImage = CropBitmaps(oi.data.header.files, G.CurrentActiveProject, ref item.lbsImages);
                         */
                        //lProjects[G.CurrentActiveProject].lWeapons.Add(item);
                        //lProjects[G.CurrentActiveProject].lbsCropWeapons.Add(CropBitmaps(oi.data.header.files, G.CurrentActiveProject));
                    }
            }
            sw.Stop();
            e.Result = sw;
            cbTypeItem.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { cbTypeItem_SelectionChanged(null, null); }));

        }
        public void OpenFile(string path, int n = -1)// open file without main menu item
        {
            try
            {
                #region Open file
                string extension = System.IO.Path.GetExtension(path);
                if (path[0] == '\\') path = RestoreRootPath(path, n);
                if (!File.Exists(path))
                {
                    //MessageBox.Show("File don't exist"); 
                    MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("wtFNE", "Text", "File not exist."));
                    return;
                }
                if (isAlreadyOpened(path)) return;
                if (n >= lProjects.Count) return;
                //if (extension==".txt" || extension==".dat") OpenTextFile(path, ptofiles, curProject);
                switch (extension)
                {
                    case ".txt":
                        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                        {
                            OpenTextFile(path, n);
                        }));
                        break;
                    case ".dat":
                        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                        {
                            OpenTextFile(path, n);
                        }));
                        break;
                    case ".bmp": OpenLFSprite(path, n); break;
                    case ".wav": PlaySound(path); break;
                    //OpenWav(path, n); break;
                    case ".wma": PlaySound(path); break;
                    //OpenWav(path, n); break;
                    case ".exe": MyShellExecute(path); break;
                    default: MyShellExecute(path); break;
                }
                #endregion
                #region Save to recent.ini
                string f;
                if (n == -1) f = path;
                else f = path + ';' + Utils.Project.GetNameProject(lProjects[n]) + ';' + n.ToString();
                for (int i = 0; i < RecentFiles.Count; i++)
                {
                    string st = RecentFiles[i];
                    if (st == f) { RecentFiles.Remove(st); break; }
                }
                if (RecentFiles.Count < rfCount)
                    RecentFiles.Insert(0, f);
                else { RecentFiles.RemoveAt(RecentFiles.Count - 1); }
                RecentFilesToMenu();
                RecentFilesToFile();
                #endregion
                #region  Load weapons bitmaps
                if (n != -1)
                {
                    G.OldActiveProject = G.CurrentActiveProject;
                    G.CurrentActiveProject = n;
                }
                if (G.CurrentActiveProject != -1)
                    if (G.CurrentActiveProject != G.OldActiveProject)
                    {
                        #region First time weapon bitmaps loading
                        /*     if (!lProjects[G.CurrentActiveProject].isWeaponBitmapsLoaded)
                        {
                            BackgroundWorker bwLoadWeapons = new BackgroundWorker();
                            bwLoadWeapons.DoWork += new DoWorkEventHandler(bwLoadWeapons_DoWork);
                            bwLoadWeapons.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwLoadWeapons_RunWorkerCompleted);
                            bwLoadWeapons.RunWorkerAsync();
                        }*/
                        #endregion
                        #region First time char bitmaps loading

                        #endregion
                    }


                #endregion
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void PlaySound(string path)
        {
            meAP.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                FileToPlay = path;
                meAP.Source = new Uri(FileToPlay);
                meAP.Play();
            }));
        }
        public void bwLoadWeapons_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopwatch sw = e.Result as Stopwatch;
            teOutput.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                teOutput.AppendText("Weapons bitmap loading and cropped. Estimated: " + sw.Elapsed.TotalSeconds.ToString() + " sec." + Environment.NewLine);
                teOutput.ScrollToEnd();
                cbTypeItem.SelectedIndex = 0;
                //bWeaponListUpdate.Visibility = System.Windows.Visibility.Visible;
                bWeaponListUpdate.Opacity = 1;
            }));
        }
        public void RecentFilesToFile()
        {
            string path = programfolder + "recent.ini";
            if (!File.Exists(path)) return;
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string st in RecentFiles) sw.WriteLine(st);
            sw.Close();
        }
        private void tvExplorer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //myHandler(sender, e);
                if (((sender as TreeView).SelectedItem as lfTreeViewItem) == null) return;
                if (((sender as TreeView).SelectedItem as lfTreeViewItem).Type == "projects") return;
                openfile_Click(sender, null);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void tvExplorer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                renameitem_Click(null, null); //rename file or project
            }
            //SetCurrentItemInEditMode(true);

            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
                cp_Click(sender, e);

        }
        public void etb_LostFocus(object sender, RoutedEventArgs e)
        {


            int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
            lfTreeViewItem parent = (tvExplorer.SelectedItem as lfTreeViewItem).Parent as lfTreeViewItem;
            foreach (lfTreeViewItem t in parent.Items)
            {
                if (t == ((sender as Borgstrup.EditableTextBlock.EditableTextBlock).Parent as System.Windows.Controls.StackPanel).Parent) continue;
                if (t.Type == lfTreeViewItem.isFolder || t.Type == lfTreeViewItem.isProject)
                    if ((t.Tag as ArrayList)[0].ToString() == (sender as EditableTextBlock).Text)
                    {
                        // e.Handled = true;
                        (sender as EditableTextBlock).Text = (sender as EditableTextBlock).oldText;
                        //MessageBox.Show("Folder with this name already exists");
                        MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFNAE", "Text", "Folder with this name already exists."));
                        if (NeedTypeNameForCreateFolder)
                        {
                            NeedTypeNameForCreateFolder = false;
                            removefile_Click(sender, e);
                        }
                        return;
                    }
                if (t.Type == lfTreeViewItem.isFile)
                    if (t.Tag.ToString() == "\\" + (sender as EditableTextBlock).Text)
                    {
                        // e.Handled = true;
                        (sender as EditableTextBlock).Text = (sender as EditableTextBlock).oldText;
                        //MessageBox.Show("File with this name already exists");
                        MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFiNAE", "Text", "File with this name already exists."));
                        if (NeedTypeNameForCreateFolder)
                        {
                            NeedTypeNameForCreateFolder = false;
                            removefile_Click(sender, e);
                        }
                        return;
                    }


            }
            (sender as EditableTextBlock).IsEnabled = false;
            tvExplorer.Focus();
            (sender as EditableTextBlock).Focus();

        }
        public void SetCurrentItemInEditMode(bool EditMode)
        {
            try
            {
                if (tvExplorer.SelectedItem is lfTreeViewItem)
                {
                    lfTreeViewItem tvi = tvExplorer.SelectedItem as lfTreeViewItem;
                    // if (tvi.Type == lfTreeViewItem.isProjects) return;
                    if (((tvi.Header as StackPanel).Children[1] as EditableTextBlock) is EditableTextBlock)
                    {
                        EditableTextBlock etb = ((tvi.Header as StackPanel).Children[1] as EditableTextBlock);
                        etb.IsEnabled = true;
                        etb.PreviewKeyDown += new KeyEventHandler(etb1_KeyDown);
                        //  etb.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(etb_FocusableChanged);
                        etb.LostFocus += new RoutedEventHandler(etb_LostFocus);
                        if (etb.IsEditable) etb.IsInEditMode = EditMode;
                    }
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public EditableTextBlock GetEditableTextBlockFromlfTreeViewItem(Object tvi)
        {
            return (((tvi as lfTreeViewItem).Header as StackPanel).Children[1] as EditableTextBlock);
        }
        public int ChangeValueInProjectFiles(string oldstring, string newstring, ArrayList folderitems)
        {
            for (int i = 1; i < folderitems.Count; i++)
            {
                if (folderitems[i] is ArrayList)
                    ChangeValueInProjectFiles(oldstring, newstring, folderitems[i] as ArrayList);
                else if (oldstring == folderitems[i].ToString()) { folderitems[i] = newstring; return i; }
            }
            return -1;
        }
        private void etb1_KeyDown(object sender, KeyEventArgs e) ///rename file
        {
            try
            {

                if (e.Key == Key.Enter)
                {
                    int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
                    lfTreeViewItem parent = (tvExplorer.SelectedItem as lfTreeViewItem).Parent as lfTreeViewItem;
                    foreach (lfTreeViewItem t in parent.Items)
                    {
                        if (t == ((sender as Borgstrup.EditableTextBlock.EditableTextBlock).Parent as System.Windows.Controls.StackPanel).Parent as lfTreeViewItem) continue;
                        if (t.Type == lfTreeViewItem.isFolder || t.Type == lfTreeViewItem.isProject)
                            if ((t.Tag as ArrayList)[0].ToString() == (sender as EditableTextBlock).Text)
                            {
                                e.Handled = true;
                                //MessageBox.Show("Folder with this name already exists");
                                MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFNAE", "Text", "Folder with this name already exists."));
                                if (NeedTypeNameForCreateFolder)
                                {
                                    NeedTypeNameForCreateFolder = false;
                                    removefile_Click(sender, e);
                                }
                                return;
                            }
                        if (t.Type == lfTreeViewItem.isFile)
                            if (t.Tag.ToString() == "\\" + (sender as EditableTextBlock).Text)
                            {
                                e.Handled = true;
                                //MessageBox.Show("File with this name already exists");
                                MessageBoxInformation.Show(LanguageDictionary.Current.Translate<string>("tFiNAE", "Text", "File with this name already exists."));
                                if (NeedTypeNameForCreateFolder)
                                {
                                    NeedTypeNameForCreateFolder = false;
                                    removefile_Click(sender, e);
                                }
                                return;
                            }
                    }

                    EditableTextBlock etb = sender as EditableTextBlock;
                    object td =
                        //(tvExplorer.SelectedItem as lfTreeViewItem).HeaderText;
                        (tvExplorer.SelectedItem as lfTreeViewItem).Tag;
                    Console.WriteLine("(e.Key == Key.Enter)");
                    if (td is ArrayList)  // virtual folder (not a real folder)
                    {
                        string ed = (string)(td as ArrayList)[0];
                        (td as ArrayList)[0] = etb.Text;
                    }
                    if (td is string)
                    {

                        string newfilename = System.IO.Path.GetDirectoryName(td as string);
                        newfilename += "\\" + etb.Text;
                        if ((td as string)[0].CompareTo((char)92) != 0) System.IO.File.Move(td as string, newfilename);
                        else System.IO.File.Move(lProjects[curProject].path_to_folder + td as string, lProjects[curProject].path_to_folder + newfilename);
                        char[] ch = new char[newfilename.Length];
                        (tvExplorer.SelectedItem as lfTreeViewItem).Tag = newfilename;
                        int result = ChangeValueInProjectFiles(td.ToString(), newfilename, lProjects[curProject].files[0] as ArrayList);

                        // (tvExplorer.SelectedItem as lfTreeViewItem).Tag = td;
                    }

                    NeedTypeNameForCreateFolder = false;
                    cProject.SaveProject(lProjects[curProject].currentpath, lProjects[curProject]);
                    //FillsbRunGame();


                    etb.IsEnabled = false;
                    tvExplorer.Focus();
                    etb.Focus();
                }//enter
                else if (e.Key == Key.Escape)
                {
                    if (NeedTypeNameForCreateFolder)
                    {
                        NeedTypeNameForCreateFolder = false;
                        removefile_Click(sender, e);


                        //(JustCreatelfTreeViewItem.Parent as lfTreeViewItem).Items.Remove(JustCreatelfTreeViewItem);

                    }
                    (sender as EditableTextBlock).IsEnabled = false;
                    tvExplorer.Focus();
                    (sender as EditableTextBlock).Focus();
                    //SaveProject(lProjects[curProject].currentpath);
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }

        }
        #region Drag & Drop for TreeView

        private void tvExplorer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(tvExplorer);
            }
        }
        public ScrollViewer GetScrollViewerFromTreeView(lfTreeViewItem tvi)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(tvi);
            while (parent != null && !(parent is ScrollViewer))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as ScrollViewer;
        }
        private void tvExplorer_MouseMove(object sender, MouseEventArgs e)
        {
            //return;
            try
            {
                double offset = 0;
                double contentHeight = tvExplorer.ActualHeight - 30;
                double contentWidth = tvExplorer.ActualWidth - 30;

                Point currentPosition = e.GetPosition(tvExplorer);
                ScrollViewer sv = GetScrollViewerFromTreeView(tvExplorer.SelectedItem as lfTreeViewItem);
                if (sv != null)
                {
                    contentHeight = sv.ViewportHeight;
                    contentWidth = sv.ViewportWidth;
                    offset = 20;
                }
                if (currentPosition.X < contentWidth - offset && currentPosition.Y < contentHeight - offset)
                    if (e.LeftButton == MouseButtonState.Pressed
                        && !cmc.cmProject.IsVisible && !cmc.cmProjects.IsVisible && !cmc.cmFolder.IsVisible && !cmc.cmFile.IsVisible)
                    {
                        // Point currentPosition = e.GetPosition(tvExplorer);                        
                        if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                            (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                        {
                            draggedItem = (lfTreeViewItem)tvExplorer.SelectedItem;
                            if (draggedItem != null)
                            {

                                DragDropEffects finalDropEffect = DragDrop.DoDragDrop(tvExplorer, tvExplorer.SelectedValue, DragDropEffects.Move);

                                //Checking target is not null and item is dragging(moving)
                                if ((finalDropEffect == DragDropEffects.Move) && (_target != null) && (_target.Type == lfTreeViewItem.isFolder || _target.Type == lfTreeViewItem.isProject))
                                {
                                    // A Move drop was accepted
                                    if (!draggedItem.HeaderText.ToString().Equals(_target.HeaderText.ToString()))
                                    {
                                        CopyItem(draggedItem, _target);
                                        _target = null;
                                        draggedItem = null;
                                    }

                                }
                            }
                        }
                    }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public bool IsSameFolder(lfTreeViewItem di, lfTreeViewItem t)
        {
            bool result = false;
            object td_di = di.Tag;
            object td_t = t.Tag;
            ArrayList al = (td_t as ArrayList);
            for (int i = 0; i < al.Count; i++)
            {
                if (al[i] is string) //if file
                {
                    if ((al[i]) == td_di) { result = true; break; }
                }
                else //if folder
                {
                    if ((al[i] as ArrayList) == td_di) { result = true; break; }
                }
            }
            return result;
        }
        public bool IsSameProject(lfTreeViewItem di, lfTreeViewItem t)
        {
            object td_di = di.Tag;
            object td_t = t.Tag;
            if (WhatProject(di) == WhatProject(t)) return true; else return false;

        }
        private void tvExplorer_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(tvExplorer);
                Console.WriteLine(currentPosition);
                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    if (e.OriginalSource is Image)
                    {
                    }
                    lfTreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                        //Console.WriteLine("Move");
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                        //Console.WriteLine("None");
                    }
                }
                e.Handled = true;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void tvExplorer_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                lfTreeViewItem TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private bool CheckDropTarget(lfTreeViewItem _sourceItem, lfTreeViewItem _targetItem)
        {
            try
            {   //Доработать
                //Check whether the target item is meeting your condition
                if (_targetItem == null) return false;
                bool _isEqual = false;
                if (!_sourceItem.HeaderText.ToString().Equals(_targetItem.HeaderText.ToString()) &&
                    (_targetItem.Type == lfTreeViewItem.isFolder || _targetItem.Type == lfTreeViewItem.isProject) &&
                    (_sourceItem.Type != lfTreeViewItem.isProject) && (_sourceItem.Type != lfTreeViewItem.isProjects) &&
                    !IsSameFolder(_sourceItem, _targetItem) && IsSameProject(_sourceItem, _targetItem) && !isSourceToplevelFolder(_sourceItem, _targetItem))
                {
                    _isEqual = true;
                }
                return _isEqual;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); return false; }
        }
        public bool isSourceToplevelFolder(lfTreeViewItem si, lfTreeViewItem ti)
        {
            bool result = false;
            object si_td = si.Tag;
            object ti_td = ti.Tag;
            if (si_td is string && ti_td is string) return result; //if not folder in folder
            lfTreeViewItem parent = ti.Parent as lfTreeViewItem;
            while (parent != null && parent.Type != lfTreeViewItem.isProject)
            {
                if (parent == si) return true;
                parent = parent.Parent as lfTreeViewItem;

            }
            return result;
        }
        private void CopyItem(lfTreeViewItem _sourceItem, lfTreeViewItem _targetItem)
        {
            string drop = LanguageDictionary.Current.Translate<string>("tDrop", "Text", "Would you like to drop");
            string into = LanguageDictionary.Current.Translate<string>("tInto", "Text", "into");
            string space = " ";
            //Asking user wether he want to drop the dragged lfTreeViewItem here or not
            if (MessageBoxQuestion.Show(drop
                + space
                + _sourceItem.HeaderText.ToString()
                + space
                + into
                + space
                + _targetItem.HeaderText.ToString()
                + "?") == MessageBoxQuestion.WPFMessageBoxResult.Yes)

            //  if (MessageBox.Show("Would you like to drop"+" " + _sourceItem.HeaderText.ToString() + " "+"into"+" " + _targetItem.HeaderText.ToString() + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //adding dragged lfTreeViewItem in target lfTreeViewItem
                    addChild(_sourceItem, _targetItem);
                    object siTag = _sourceItem.Tag;
                    object tiTag = _targetItem.Tag;
                    lfTreeViewItem parent = (lfTreeViewItem)(_sourceItem as lfTreeViewItem).Parent;
                    object td = parent.Tag;
                    ArrayList aparent = (ArrayList)td;
                    object std = (_sourceItem as lfTreeViewItem).Tag;
                    //int index = -1;
                    int index = aparent.IndexOf(std);
                    if (index == -1)
                    {
                    }
                    if (siTag is ArrayList)
                    {
                        (tiTag as ArrayList).Add(siTag);
                    }
                    else
                    {
                        (tiTag as ArrayList).Add(aparent[index]);
                    }
                    aparent.RemoveAt(index);

                    //finding Parent lfTreeViewItem of dragged lfTreeViewItem 
                    lfTreeViewItem ParentItem = FindVisualParent<lfTreeViewItem>(_sourceItem);
                    // if parent is null then remove from TreeView else remove from Parent lfTreeViewItem
                    if (ParentItem == null)
                    {
                        tvExplorer.Items.Remove(_sourceItem);
                    }
                    else
                    {
                        ParentItem.Items.Remove(_sourceItem);
                    }
                    int curProject = WhatProject(tvExplorer.SelectedItem as lfTreeViewItem);
                    cProject.SaveProject(lProjects[curProject].currentpath, lProjects[curProject]);
                }
                catch (Exception ex) { new wException(ex).ShowDialog(); }
            }

        }
        public void addChild(lfTreeViewItem _sourceItem, lfTreeViewItem _targetItem)
        {
            try
            {
                // add item in target lfTreeViewItem 
                lfTreeViewItem item1 = new lfTreeViewItem(_sourceItem.Pathtofile);
                item1.Tag = _sourceItem.Tag;
                item1.HeaderText = _sourceItem.HeaderText;
                _targetItem.Items.Add(item1);
                foreach (lfTreeViewItem item in _sourceItem.Items)
                {
                    addChild(item, item1);
                }
                // SaveProject(lProjects[curProject].currentpath);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            try
            {
                if (child == null)
                {
                    return null;
                }

                UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

                while (parent != null)
                {
                    TObject found = parent as TObject;
                    if (found != null)
                    {
                        return found;
                    }
                    else
                    {
                        parent = VisualTreeHelper.GetParent(parent) as UIElement;
                    }
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }

            return null;
        }
        private lfTreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            lfTreeViewItem container = element as lfTreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as lfTreeViewItem;
            }
            return container;
        }
        #endregion

        #region Fold/Unfold
        private void ClearCurrentTextArea(TextEditor te)
        {
            /*if (textArea != null) {
                        textArea.Caret.PositionChanged -= textArea_Caret_PositionChanged;
                        textArea.LeftMargins.Remove(margin);
                        textArea.TextView.ElementGenerators.Remove(generator);
                        margin = null;
                        generator = null;
                        textArea = null;*/
        }

        private void foldall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);

                //dmp.ContainsActiveContent
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);
                for (; ; )//for (int i = 1; i < fs.Count; i++)
                {
                    try
                    {
                        firstSegment.IsFolded = true;
                    }
                    catch
                    {

                        throw;
                    }

                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void unfoldall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);

                for (; ; )//for (int i = 1; i < fs.Count; i++)
                {
                    firstSegment.IsFolded = false;
                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void fold1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);

                //for (int i = 1; i < fs.Count; i++)
                for (; ; )
                {
                    if (LineConsist(firstSegment.Title.Split(' '), "<frame>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<weapon_strength_list>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<stage>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<bmp_begin>")
                        )
                        firstSegment.IsFolded = true;
                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void unfold1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);

                //for (int i = 1; i < fs.Count; i++)
                for (; ; )
                {
                    if (LineConsist(firstSegment.Title.Split(' '), "<frame>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<weapon_strength_list>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<stage>") ||
                        LineConsist(firstSegment.Title.Split(' '), "<bmp_begin>")
                        )
                        firstSegment.IsFolded = false;
                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void fold2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);

                for (; ; )//for (int i = 1; i < fs.Count; i++)
                {
                    if (LineConsist(firstSegment.Title.Split(' '), "<phase>")
                        )
                        firstSegment.IsFolded = true;
                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void unfold2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                TextSegmentCollection<FoldingSection> fs = te.fs;
                FoldingSection firstSegment = fs.FindFirstSegmentWithStartAfter(0);
                for (; ; )//for (int i = 1; i < fs.Count; i++)
                {
                    if (LineConsist(firstSegment.Title.Split(' '), "<phase>")
                        )
                        firstSegment.IsFolded = false;
                    firstSegment = fs.GetNextSegment(firstSegment);
                    if (firstSegment == null) break;
                }
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        #endregion
        private void SerializeAppSettings()
        {
            adce myObject = new adce();
            XmlSerializer mySerializer = new XmlSerializer(typeof(adce), new Type[] { typeof(adce.Frame), typeof(adce.FrameDesc) });
            StreamWriter myWriter = new StreamWriter("example.xml");
            mySerializer.Serialize(myWriter, myObject);
            myWriter.Close();
        }




        public void UpdateComboBoxs(TextEditor te, DatFileDesc dfd)
        {
            //if (dfd == null) return;
            var cbFrameNumber = Utils.AvalonDock.GetComboBoxByName(te.Parent, Utils.Const.cbNumber);
            if (cbFrameNumber == null) return;
            cbFrameNumber.Items.Clear();
            var cbFrameName = Utils.AvalonDock.GetComboBoxByName(te.Parent, Utils.Const.cbName);
            cbFrameName.Items.Clear();
            var cbRegion = Utils.AvalonDock.GetComboBoxByName(te.Parent, Utils.Const.cbRegion);
            cbRegion.Items.Clear();
            bool povtor = false;
            if (dfd == null) return;
            for (int i = 0; i < dfd.regions.Count; i++) cbRegion.Items.Add(dfd.regions[i].caption);
            for (int i = 0; i < dfd.frames.Count; i++)
            {
                cbFrameNumber.Items.Add(dfd.frames[i].number.ToString());
                foreach (string st in cbFrameName.Items)
                {
                    if (dfd.frames[i].caption == st) povtor = true;
                }
                if (!povtor)
                    cbFrameName.Items.Add(dfd.frames[i].caption);
                else povtor = false;

            }

            /*          Lf2FoldingStrategy fs = te.Tag as Lf2FoldingStrategy;            
                      for (int i = 0; i < fs.framenumbers.Count; i++) cbFrameNumber.Items.Add(fs.framenumbers[i]);
                      for (int i = 0; i < fs.framenames.Count; i++) cbFrameName.Items.Add(fs.framenames[i]);
                      for (int i = 0; i < fs.regions.Count; i++) cbRegion.Items.Add(fs.regions[i]);            */
        }


        private void bFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                TextEditor[] tes = G.GetListOfTextEditors(DockManager);
                if (tes.Length == 0) return;
                fr.Editors = tes;
                TextEditor te = tes[0];
                foreach (TextEditor t in tes)
                    if (t.dc.IsActiveDocument) { te = t; break; }
                fr.CurrentEditor = te;
                fr.ShowAsFind();

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void bFindNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //    TextEditor[] tes = G.GetListOfTextEditors(DockManager);
                //    if (tes.Length == 0) return;
                //    fr.Editors = tes;

                //    fr.CurrentEditor = tes[0];
                fr.FindNext();


            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void gotoline_Click(object sender, RoutedEventArgs e)
        {
            if (DockManager.ActiveDocument == null) return;
            G.gte = Utils.AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
            wGotoLine wgl = new wGotoLine();
            wgl.Owner = this;
            wgl.Show();
        }



        public int WhatFrame(int nframe, List<FrameInfo> lfi)
        {
            if (nframe == int.MaxValue) return -1;
            if (nframe == 1000) return -2;
            for (int i = 0; i < lfi.Count; i++)
            {
                if (lfi[i].number == nframe)
                {
                    return i;
                }
            }
            return -1;
        }
        public int GetPropValueByName(List<PropDesc> lpd, string whatfind, int defvalue = int.MaxValue)
        {
            foreach (PropDesc pd in lpd)
            {
                if (pd.name == whatfind)
                {
                    int result;
                    if (int.TryParse(pd.value, out result)) return result; else return defvalue;
                }
            }
            return defvalue;
        }

        public WhereStand WhatSubTag(FrameInfo fi, int nline, ref int index)
        {
            index = -1;
            if (nline == fi.oline) return WhereStand.frametitle;
            if (nline == fi.cline) return WhereStand.frameend;
            for (int i = 0; i < fi.lnst.ol_bdy.Count; i++)
                if (nline >= fi.lnst.ol_bdy[i] && nline <= fi.lnst.cl_bdy[i])
                { index = i; return WhereStand.bdy; }
            for (int i = 0; i < fi.lnst.ol_itr.Count; i++)
                if (nline >= fi.lnst.ol_itr[i] && nline <= fi.lnst.cl_itr[i])
                { index = i; return WhereStand.itr; }
            if (nline >= fi.lnst.ol_bpoint && nline <= fi.lnst.cl_bpoint)
                return WhereStand.bpoint;
            if (nline >= fi.lnst.ol_wpoint && nline <= fi.lnst.cl_wpoint)
                return WhereStand.wpoint;
            if (nline >= fi.lnst.ol_cpoint && nline <= fi.lnst.cl_cpoint)
                return WhereStand.cpoint;
            if (nline >= fi.lnst.ol_opoint && nline <= fi.lnst.cl_opoint)
                return WhereStand.opoint;
            return WhereStand.frameheader;
        }
        public int WhatImageFile(List<FileDesc> lfd, ref int pic)
        {
            if (pic == int.MaxValue)
                return -1;
            for (int i = 0; i < lfd.Count; i++)
            {
                if (pic >= lfd[i].firstFrame && pic <= lfd[i].lastFrame)
                {
                    pic = pic - lfd[i].firstFrame;
                    return i;
                }
            }
            return -1;
        }
        public void ChooseComboboxes(WhereStand ws)
        {
            if (CurrentFrameIndex == -1) { DisabledEnabledComboboxes(); return; }
            try
            {
                var oi = teCurActiveDocument.oi as ObjectInfo;
                switch (ws)
                {
                    case WhereStand.cpoint:
                        DisabledEnabledComboboxes(cbCKind);
                        int ckind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "kind:");
                        cbCKind.SelectionChanged -= new SelectionChangedEventHandler(cbCKind_SelectionChanged);
                        cbCKind.SelectedIndex = GetIndexByValue(cbCKind, ckind);
                        cbCKind.SelectionChanged += new SelectionChangedEventHandler(cbCKind_SelectionChanged);
                        break;
                    case WhereStand.itr:
                        DisabledEnabledComboboxes(cbItrKind, cbItrEffect);
                        int kind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "kind:");
                        int effect = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "effect:");
                        cbItrKind.SelectionChanged -= new SelectionChangedEventHandler(cbItrKind_SelectionChanged);
                        cbItrEffect.SelectionChanged -= new SelectionChangedEventHandler(cbItrEffect_SelectionChanged);
                        cbItrKind.SelectedIndex = GetIndexByValue(cbItrKind, kind);
                        cbItrEffect.SelectedIndex = GetIndexByValue(cbItrEffect, effect);
                        cbItrKind.SelectionChanged += new SelectionChangedEventHandler(cbItrKind_SelectionChanged);
                        cbItrEffect.SelectionChanged += new SelectionChangedEventHandler(cbItrEffect_SelectionChanged);
                        break;
                    case WhereStand.opoint:
                        DisabledEnabledComboboxes(cbOKind);
                        int okind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "kind:");
                        cbOKind.SelectionChanged -= new SelectionChangedEventHandler(cbOKind_SelectionChanged);
                        cbOKind.SelectedIndex = GetIndexByValue(cbOKind, okind);
                        cbOKind.SelectionChanged += new SelectionChangedEventHandler(cbOKind_SelectionChanged);
                        break;
                    case WhereStand.wpoint:
                        DisabledEnabledComboboxes(cbWKind, cbWCover);
                        //if (CurrentFrameIndex == -1) { cbWKind.SelectedIndex = 0; cbWCover.SelectedIndex = 0; break; }
                        int wkind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "kind:");
                        int cover = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "cover:");
                        cbWKind.SelectionChanged -= new SelectionChangedEventHandler(cbWKind_SelectionChanged);
                        cbWCover.SelectionChanged -= new SelectionChangedEventHandler(cbWCover_SelectionChanged);
                        cbWKind.SelectedIndex = GetIndexByValue(cbWKind, wkind);
                        cbWCover.SelectedIndex = GetIndexByValue(cbWCover, cover);
                        cbWKind.SelectionChanged += new SelectionChangedEventHandler(cbWKind_SelectionChanged);
                        cbWCover.SelectionChanged += new SelectionChangedEventHandler(cbWCover_SelectionChanged);
                        break;
                    case WhereStand.bpoint: DisabledEnabledComboboxes(); break;
                    case WhereStand.frameheader: DisabledEnabledComboboxes(); break;
                    case WhereStand.frametitle: DisabledEnabledComboboxes(); break;
                    case WhereStand.frameend: DisabledEnabledComboboxes(); break;
                    case WhereStand.bdy: DisabledEnabledComboboxes(); break;
                    case WhereStand.none: DisabledEnabledComboboxes(); break;
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        public void DetectWhereCursor()
        {
            try
            {
                if (DockManager.ActiveDocument == null) return;
                if ((DockManager.ActiveDocument as DocumentContent).Content == null) return;
                TextEditor te = AvalonEdit.GetTextEditorFromContent((DockManager.ActiveDocument as DocumentContent).Content);
                
                if (te == null) return;
                //if (te.numProject == -1) 
                //{ dcCanvas.frame.status = Error.notProject; dcCanvas.RedrawFrames(); return; }
                DocumentLine line = te.Document.GetLineByOffset(te.CaretOffset);
                int index = Utils.DatFileTextEditor.WhatFrameFromLineNumber(te.oi as ObjectInfo, line.LineNumber);
                if (index == -1) { CurrentFrameIndex = -1; dcCanvas.ClearFrames(); Otvet = WhereStand.none; lfiWDS.SelectingIndex = -1; lfiCSV.SelectingIndex = -1; return; }
                CurrentFrameIndex = index;

                ObjectInfo oi = te.oi as ObjectInfo;

                //cbFiles.InvalidateVisual();

                Otvet = WhatSubTag(oi.data.frames[index], line.LineNumber, ref Objectindex);
                if (Otvet == WhereStand.none) lfiWDS.SelectingIndex = -1;

                PrepareDataForFrameAndDraw(dcCanvas.frame, te.oi as ObjectInfo, index, Otvet, Objectindex);
                #region FrameNext
                if (dcCanvas.isShowFrameNext)
                {
                    #region next frame
                    int nextvalue = GetPropValueByName(oi.data.frames[index].header, "next:");
                    if (nextvalue == 999) nextvalue = 0;
                    int indexn = WhatFrame(nextvalue, oi.data.frames);
                    PrepareDataForFrameAndDraw(dcCanvas.framenext, te.oi as ObjectInfo, indexn, Otvet, Objectindex);
                    #endregion
                }
                #endregion
                dcCanvas.UpdateFramesPosition();
            }
            catch (Exception ex)
            {
                teOutput.AppendText("Some errors in SGV!");
                teOutput.AppendText(Environment.NewLine);
                teOutput.ScrollToEnd();
            }
        }
        public ObjectInfo GetWeaponByIndexInDataTXT(int index)
        {
            int i = 0;
            foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
            {
                if (oi.type == 1 || oi.type == 2 || oi.type == 4 || oi.type == 6)
                    if (oi.id >= 100 && oi.id <= 199)
                    {
                        if (i == index) return oi;
                        i++;
                    }
            }
            return null;
        }
        public ObjectInfo GetWeaponFromlfItem(lfItem item)
        {
            foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
            {
                if (oi.id == item.id) return oi;
            }
            return null;
        }
        public void PrepareDataForFrameAndDraw(lfDrawingVisual dv, ObjectInfo oi, int index, WhereStand ws, int objectindex)
        {
            #region Clear values
            dv.frame = null;
            #endregion
            #region Invalid next frame
            if (index == -1) { dv.status = Error.InvalidNextFrame; dv.Draw(); return; }
            #endregion
            #region next: 1000
            if (index == -2) { dv.status = Error.Next1000; dv.Draw(); return; }
            #endregion
            int pic = GetPropValueByName(oi.data.frames[index].header, "pic:");
            #region Invalid pic index
            if (pic >= oi.lbiCroppedBitmaps.Count || pic < 0)
            {
                dv.status = Error.InvalidPic; dv.Draw();
                //  return;
            }
            else dv.status = Error.None;
            #endregion
            if (dv.status != Error.InvalidPic)
            {
                # region Draw bitmap
                BitmapSource sbs = oi.lbiCroppedBitmaps[pic];
                dv.Width1 = sbs.PixelWidth;
                dv.Height1 = sbs.PixelHeight;
                ColorKeyBitmap ckb = null;
                if (G.AppSettings.isSGVTransparent)
                {
                    ckb = new ColorKeyBitmap() { TransparentColor = (Color)ColorConverter.ConvertFromString(G.AppSettings.SGVColorKey) };
                    ckb.Source = sbs;
                    dv.frame = ckb;
                }
                else dv.frame = sbs;
                #endregion
            }
            if (ws == WhereStand.wpoint || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
            {
                #region Weapon
                ObjectInfo oiWeapon = lfiWDS.item;
                if (oiWeapon != null)
                {
                    int weaponact = GetPropValueByName(oi.data.frames[index].wpoint, "weaponact:");
                    int windex = WhatFrame(weaponact, oiWeapon.data.frames);
                    dv.Cover = GetPropValueByName(oi.data.frames[index].wpoint, "cover:");
                    dv.WeaponBitmapOffsetX = int.MaxValue;
                    dv.WeaponBitmapOffsetY = int.MaxValue;
                    int wpic = int.MaxValue;
                    if (windex >= 0 && windex < oiWeapon.data.frames.Count)
                    // if (weaponact >= 0 && weaponact < oiWeapon.data.frames.Count)
                    {
                        dv.WeaponBitmapOffsetX = GetPropValueByName(oiWeapon.data.frames[windex].wpoint, "x:");
                        dv.WeaponBitmapOffsetY = GetPropValueByName(oiWeapon.data.frames[windex].wpoint, "y:");
                        wpic = GetPropValueByName(oiWeapon.data.frames[windex].header, "pic:");
                    }
                    if (dv == dcCanvas.frame)
                    {
                        //int pic = GetPropValueByName(oi.data.frames[index].header, "pic:");
                        if (pic != int.MaxValue)
                        {
                            lfiCSV.SelectingIndex = pic;
                            SwitchCombobox(lfiCSV, cbCSVBitmap, lfiCSV.SelectingIndex);
                        }
                    }
                    if (wpic != int.MaxValue)
                    {
                        if (dv == dcCanvas.frame)
                        {
                            lfiWDS.SelectingIndex = wpic;
                            SwitchCombobox(lfiWDS, cbFiles, lfiWDS.SelectingIndex);
                        }
                        if (wpic > 0 && wpic < lfiWDS.item.lbiCroppedBitmaps.Count)
                        {
                            dv.WeaponBitmap = new ColorKeyBitmap() { TransparentColor = (Color)ColorConverter.ConvertFromString(G.AppSettings.SGVColorKey) };
                            dv.WeaponBitmap.Source = lfiWDS.item.lbiCroppedBitmaps[wpic];
                        }
                        else dv.WeaponBitmap = null;

                        //lProjects[G.CurrentActiveProject].lWeapons[CurrentWeaponIndex].llbsCroppedImage[wpic];                     
                    }
                }
                #endregion
            }
            else { dv.WeaponBitmap = null; }
            //List<Rect> lrectBody
            dv.lrectBody = new List<Rect>();
            if (ws == WhereStand.bdy || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
            {
                List<List<PropDesc>> llpd = oi.data.frames[index].bdy;
                if (llpd != null && llpd.Count > 0)
                    for (int i = 0; i < llpd.Count; i++)
                    {
                        Rect rectBdy = new Rect(GetPropValueByName(llpd[i], "x:"), GetPropValueByName(llpd[i], "y:"),
                                                GetPropValueByName(llpd[i], "w:"), GetPropValueByName(llpd[i], "h:"));
                        if (i == objectindex || objectindex == -1 || G.AppSettings.isShowAllBdy || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                            dv.lrectBody.Add(rectBdy);
                    }
            }
            else dv.lrectBody = null;
            //dv.lrectBody = lrectBody;
            //List<Rect> lrectItr = 
            dv.lrectItr = new List<Rect>();
            if (ws == WhereStand.itr || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
            {
                var llpd = oi.data.frames[index].itr;
                if (llpd != null && llpd.Count > 0)
                    for (int i = 0; i < llpd.Count; i++)
                    {
                        Rect rectItr = new Rect(GetPropValueByName(llpd[i], "x:"), GetPropValueByName(llpd[i], "y:"),
                                                GetPropValueByName(llpd[i], "w:"), GetPropValueByName(llpd[i], "h:"));
                        if (i == objectindex || objectindex == -1 || G.AppSettings.isShowAllItr || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                            dv.lrectItr.Add(rectItr);
                    }
            }
            else dv.lrectItr = null;
            //dv.lrectItr = lrectItr;
            if (ws == WhereStand.bpoint || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                dv.bpoint = new Point(GetPropValueByName(oi.data.frames[index].bpoint, "x:"), GetPropValueByName(oi.data.frames[index].bpoint, "y:"));
            else dv.bpoint = null;
            if (ws == WhereStand.wpoint || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                dv.wpoint = new Point(GetPropValueByName(oi.data.frames[index].wpoint, "x:"), GetPropValueByName(oi.data.frames[index].wpoint, "y:"));
            else dv.wpoint = new Point(int.MaxValue, int.MaxValue);
            if (ws == WhereStand.frameheader || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                dv.center = new Point(GetPropValueByName(oi.data.frames[index].header, "centerx:"), GetPropValueByName(oi.data.frames[index].header, "centery:"));
            else dv.center = null;
            if (ws == WhereStand.opoint || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
            {
                dv.opoint = new Point(GetPropValueByName(oi.data.frames[index].opoint, "x:"), GetPropValueByName(oi.data.frames[index].opoint, "y:"));
                if (dv.opoint.Value.X != int.MaxValue && dv.opoint.Value.Y != int.MaxValue && dv == dcCanvas.frame)
                {
                    {
                        int oid = GetPropValueByName(oi.data.frames[index].opoint, "oid:");
                        int action = GetPropValueByName(oi.data.frames[index].opoint, "action:");
                        if (oid != int.MaxValue)
                        {
                            #region Combobox
                            for (int i = 0; i < cbSODatFile.Items.Count; i++)
                            {
                                ObjectInfo oiOpoint = (cbSODatFile.Items[i] as ComboBoxItem).Tag as ObjectInfo;
                                if (oiOpoint.id == oid)
                                {


                                    cbSODatFile.SelectedIndex = i;
                                    int oindex = WhatFrame(action, oiOpoint.data.frames);
                                    if (oindex == -1) break;
                                    int opic = int.MaxValue;
                                    if (oindex != int.MaxValue)
                                        opic = GetPropValueByName(oiOpoint.data.frames[oindex].header, "pic:");
                                    if (opic != int.MaxValue)
                                    {
                                        lfiSO.SelectingIndex = opic;
                                        SwitchCombobox(lfiSO, cbSOBitmap, opic);

                                        dv.OpointBitmapOffsetX = GetPropValueByName(oiOpoint.data.frames[oindex].header, "centerx:");
                                        dv.OpointBitmapOffsetY = GetPropValueByName(oiOpoint.data.frames[oindex].header, "centery:");
                                        if (dv.OpointBitmapOffsetX != int.MaxValue && dv.OpointBitmapOffsetY != int.MaxValue)
                                        {
                                            var ckbOpoint = new ColorKeyBitmap() { TransparentColor = (Color)ColorConverter.ConvertFromString(G.AppSettings.SGVColorKey) };
                                            ckbOpoint.Source = oiOpoint.lbiCroppedBitmaps[opic];
                                            dv.OpointBitmap = ckbOpoint;
                                        }
                                        else dv.OpointBitmap = null;

                                    }
                                    break;
                                }
                            }
                            #endregion
                        }
                    }
                }
                else { if (dv == dcCanvas.frame) lfiSO.SelectingIndex = -1; }
            }
            else { dv.opoint = null; if (dv == dcCanvas.frame) lfiSO.SelectingIndex = -1; }
            if (ws == WhereStand.cpoint || ws == WhereStand.frametitle || ws == WhereStand.frameend || G.AppSettings.isShowAllinSGV || dv == dcCanvas.framenext)
                dv.cpoint = new Point(GetPropValueByName(oi.data.frames[index].cpoint, "x:"), GetPropValueByName(oi.data.frames[index].cpoint, "y:"));
            else dv.cpoint = null;
            dv.Draw();
        }
        public void SwitchCombobox(lfImage lfi, ComboBox cb, int index)
        {
            if (index == int.MaxValue) return;
            if (lfi.item == null) return;
            int ncol = 0;
            int nrow = 0;
            int num = ncol * nrow;
            int sum = num;
            int min = 0;
            int max = 0;
            for (int i = 0; i < lfi.item.lbiBitmaps.Count; i++)
            {
                ncol = lfi.item.lbiBitmaps[i].PixelWidth / (lfi.item.data.header.files[i].width + 1);
                nrow = lfi.item.lbiBitmaps[i].PixelHeight / (lfi.item.data.header.files[i].height + 1);
                num = ncol * nrow;
                max += num;
                if (index >= min && index < max) { cb.SelectedIndex = i; return; }
                min = max;
            }
            cb.SelectedIndex = 0;
        }
        private void MainGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                sf_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
            {
                sall_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                of_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            {
                cf_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                caf_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Q)
            {
                ndf_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.G)
            {
                gotoline_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                miPE_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F11)
            {

                var pbe = new PngBitmapEncoder();
                pbe.Frames.Add(BitmapFrame.Create(Utils.Graphic.ToImageSource(this)));
                using (var fs = new FileStream("c:\\screenshot.png", FileMode.Create))
                {
                    pbe.Save(fs);
                }
            }
            if (e.Key == Key.F5) sbRunGame_Click(sender, e);
            if (e.Key == Key.F6)
            {
                Utils.Graphic.SaveControlToFile(Utils.Graphic.ToImageSource(dcCanvas), @"C:\Users\wirwl\Desktop\canvas.png");
            }

            //SerializeAppSettings();

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.OemMinus)
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                int value;
                try { value = Convert.ToInt32(te.SelectedText); }
                catch { return; }
                value--;
                int ss = te.SelectionStart;
                te.Document.Replace(te.SelectionStart, te.SelectionLength, value.ToString(), OffsetChangeMappingType.RemoveAndInsert);
                te.Select(ss, value.ToString().Length);
                DetectWhereCursor();

            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.OemPlus)
            {
                TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
                int value;
                try { value = Convert.ToInt32(te.SelectedText); }
                catch { return; }
                value++;
                int ss = te.SelectionStart;
                te.Document.Replace(te.SelectionStart, te.SelectionLength, value.ToString(), OffsetChangeMappingType.RemoveAndInsert);
                te.Select(ss, value.ToString().Length);
                DetectWhereCursor();
            }
            if (e.Key == Key.F9)
            {
                ComboBox cb = null;
                if (DockManager.ActiveDocument != null)
                    cb = Utils.AvalonDock.GetComboBoxByName(DockManager.ActiveDocument.Content, Utils.Const.cbNumber);
                if (cb != null) cb.Focus();
            };
            if (e.Key == Key.F11)
            {
                ComboBox cb = null;
                if (DockManager.ActiveDocument != null)
                    cb = Utils.AvalonDock.GetComboBoxByName(DockManager.ActiveDocument.Content, Utils.Const.cbRegion);
                if (cb != null) cb.Focus();
            }
            if (e.Key == Key.F12)
            {
                /*    Tools tools=new Tools();
                    tools.tools.Add(new Tool() {arguments="-d",initial_dir="c:",path=@"c:\test",promt_arguments=true,title="title" });
                    Tools.SaveProject(@"c:\\tools.ini", tools);*/

                //adce.Save(@"c:\main.ini", G.AppSettings);
            }
        }
        private void miPE_Click(object sender, RoutedEventArgs e)
        {
            switch (dcPE.State)
            {
                case DockableContentState.Hidden: dcPE.Show(); break;
                case DockableContentState.DockableWindow: dcPE.Hide(); break;
                case DockableContentState.Docked: dcPE.Hide(); break;
                case DockableContentState.AutoHide: dcPE.Hide(); break;
            }
        }
        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            textView.EnsureVisualLines();
            TextEditor editor = (DockManager.ActiveDocument as DocumentContent).Content as TextEditor;
            var line = editor.Document.GetLineByOffset(editor.CaretOffset);
            var segment = new TextSegment { StartOffset = line.Offset, EndOffset = line.EndOffset };
            Brush background = new SolidColorBrush(Color.FromArgb(255, 0xE9, 0xEC, 0xFA));
            Pen border = new Pen(null, 1);
            foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
            {
                drawingContext.DrawRoundedRectangle(background, border, new Rect(r.Location, new Size(textView.ActualWidth, r.Height)), 3, 3);
            }
        }
        private void tvExplorer_KeyDown_1(object sender, KeyEventArgs e)
        {
            try
            {
                EditableTextBlock etb = GetEditableTextBlockFromlfTreeViewItem(tvExplorer.SelectedItem);
                if (etb.IsEditable) return;
                if (e.Key == Key.Enter)
                {
                    openfile_Click(sender, e);
                }
                if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None) removefile_Click(sender, e);

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void miGetList_Click(object sender, RoutedEventArgs e)
        {
            List<string> ls = new List<string>();
            foreach (EncodingInfo s in Encoding.GetEncodings())
                ls.Add(s.Name);
            wGetListCodepages glc = new wGetListCodepages(ls) { Owner = this };
            glc.Show();
        }
        private void miGetListofFonts_Click(object sender, RoutedEventArgs e)
        {
            List<string> ls = new List<string>();
            foreach (FontFamily s in Fonts.SystemFontFamilies)
                ls.Add(s.Source);
            wGetListCodepages glc = new wGetListCodepages(ls) { Owner = this };
            glc.Show();
        }

        private void wMainWindow_Closed(object sender, EventArgs e)
        {
            //adce.Save(programfolder + "main.xml", G.AppSettings);
            SaveCurrentSettings();
            WindowCollection wc = Application.Current.Windows;
            for (int i = 0; i < wc.Count; i++)
            {
                if (wc[i] != this)
                    wc[i].Close();
            }

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                List<string> ls = new List<string>();
                for (int i = 0; i < DockManager.Documents.Count; i++)
                {
                    if (!((DockManager.Documents[i] as DocumentContent).Content is TextEditor)) continue;
                    TextEditor te = ((DockManager.Documents[i] as DocumentContent).Content as TextEditor);
                    DocumentContent dc = te.Parent as DocumentContent;
                    if (dc.Title[dc.Title.Length - 1] == '*')
                    {
                    }
                }
                if (ls.Count > 0)
                {
                    wSaveManyFiles smf = new wSaveManyFiles(ls) { Owner = this };
                    smf.ShowDialog();
                    if (smf.Result == 1)
                    {
                        sall_Click(sender, null);
                    }
                    if (smf.Result == 2)
                    {

                    }
                    if (smf.Result == 3)
                    {
                        e.Cancel = true;
                    }
                    smf.Close();
                }
                SaveListOfOpenFiles();
                DockManager.SaveLayout(programfolder + "layout.xml");

                if (e.Cancel == false)
                {
                    if (G.ServerThread != null)
                        G.ServerThread.Abort();
                    //Application.Current.Shutdown();
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void wb_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(sender as WebBrowser, true); // make it silent
        }
        public void sp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WebBrowser wb = (sender as StackPanel).Tag as WebBrowser;
            if (wb == null) return;
            wb.Height = (sender as StackPanel).ActualHeight;
            wb.Width = (sender as StackPanel).ActualWidth;
        }
        private void miShowWebBrowser_Click(object sender, RoutedEventArgs e)
        {
            DocumentContent dc = new DocumentContent();
            dc.Name = "WebBrowser";
            dc.Title = dc.Name;
            dc.Closing += new EventHandler<CancelEventArgs>(dc_Closing);
            //  dc.SizeChanged+=new SizeChangedEventHandler(dc_SizeChanged);
            //dc.Height = dmp.Height;
            // dc.Width = dmp.Width;
            StackPanel sp = new StackPanel();
            sp.SizeChanged += new SizeChangedEventHandler(sp_SizeChanged);
            // sp.Height = 300; sp.Width = 300;
            // sp.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            // sp.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            WebBrowser wb = new WebBrowser();
            sp.Tag = wb;
            wb.Navigated += new System.Windows.Navigation.NavigatedEventHandler(wb_Navigated);
            wb.Navigate("http://lfforever.ru");

            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //sv.Margin = new Thickness(0); 
            sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            sv.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            sv.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            sv.VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
            //sv.Height = 300; sv.Width = 300;
            sv.Background = new SolidColorBrush(Colors.Red);

            sp.Children.Add(wb);
            //sv.Content = 
            //new Image() {Height=60,Width=60 };            
            dc.Content = sp;
            dc.Show(DockManager);
            dc.Activate();
            wb.Height = sp.ActualHeight;
            wb.Width = sp.ActualWidth;
        }
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private void gErrors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                gfErrors gfe = (sender as DataGrid).SelectedItem as gfErrors;
                if (gfe == null) return;
                int line = gfe.Line;
                TextEditor te = null;
                var dc = Utils.AvalonDock.GetDocumentContentByName(DockManager, gfe.fullName);
                if (dc != null)
                {
                    te = Utils.AvalonEdit.GetTextEditorFromContent(dc.Content);
                    DockManager.ActiveDocument = dc;
                    dc.Activate();
                    DocumentLine dl = te.Document.GetLineByNumber(line);
                    te.UpdateLayout();
                    te.Select(dl.Offset, dl.Length);
                    te.ScrollToLine(line);
                }
                else
                {
                    isOpenManyFiles = false;
                    OpenFile(gfe.fullName, Utils.Project.GetProjectIndex(lProjects, gfe.Project));
                    Tasks.Add(new TaskHighlight(gfe.fullName, gfe.Line));
                    //dc = DockManager.Documents[0] as DocumentContent;
                    //DockManager.ActiveDocument as DocumentContent;
                    //te = Utils.AvalonEdit.GetTextEditorFromContent(dc.Content);
                }

            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void wMainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                if (e.SystemKey == Key.F10)
                {
                    e.Handled = true;
                    ComboBox cb = null;
                    if (DockManager.ActiveDocument == null) return;
                    cb = Utils.AvalonDock.GetComboBoxByName(DockManager.ActiveDocument.Content, Utils.Const.cbName);
                    if (cb != null) cb.Focus();
                }
            }
        }

        private void teOutput_KeyDown(object sender, KeyEventArgs e)
        {
            TextEditor te = (sender as TextEditor);
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
                te.Clear();
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.None)
            {
                DocumentLine dl = te.Document.GetLineByOffset(te.SelectionStart);
                te.Document.Remove(dl.Offset, dl.TotalLength);
            }
        }

        private void miErrors_Click(object sender, RoutedEventArgs e)
        {
            switch (dcErrors.State)
            {
                case DockableContentState.Hidden: dcErrors.Show(); break;
                case DockableContentState.DockableWindow: dcErrors.Hide(); break;
                case DockableContentState.Docked: dcErrors.Hide(); break;
                case DockableContentState.AutoHide: dcErrors.Hide(); break;
            }
        }

        private void miDebug_Click(object sender, RoutedEventArgs e)
        {
            switch (dcOutput.State)
            {
                case DockableContentState.Hidden: dcOutput.Show(); break;
                case DockableContentState.DockableWindow: dcOutput.Hide(); break;
                case DockableContentState.Docked: dcOutput.Hide(); break;
                case DockableContentState.AutoHide: dcOutput.Hide(); break;
            }
        }

        private void DockManager_ActiveDocumentChanged(object sender, EventArgs e) //changeactivedocument
        {

            //teOutput.AppendText("adc"+Environment.NewLine);
            // teOutput.ScrollToEnd();
            if (DockManager.ActiveDocument == null) return;
            teCurActiveDocument = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
            TextEditor te = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
            if (te == null) return;
            ObjectInfo oi = te.oi as ObjectInfo;
            if (te != null)
            {
                if (oi == null)
                {

                    oi = FindDatafile(te.numProject, te.fullpath);
                    string text = te.Text;
                    // if (oi.id == -1) text = functions.DatFileToPlainText(te.fullpath, G.AppSettings.StandardPassword);
                    oi.data = Utils.DatFiles.ParseTextWithErrorsReturn(text, te.fullpath, ref oi.errors, Utils.Project.GetProject(te.numProject));
                    SendErrorsListToGridErrors(oi);
                    te.oi = oi;

                }
                if (oi.lbiBitmaps == null)
                    if (oi.data != null && oi.data.header != null)
                        oi.lbiCroppedBitmaps = CropBitmaps(oi.data.header.files, G.CurrentActiveProject, ref oi.lbiBitmaps);
                FillComboBoxForDFV(oi);
                /*    BackgroundWorker bwLoadAndCropBitmaps = new BackgroundWorker();
                    bwLoadAndCropBitmaps.DoWork += new DoWorkEventHandler(bwLoadAndCropBitmaps_DoWork);
                    bwLoadAndCropBitmaps.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwLoadAndCropBitmaps_RunWorkerCompleted);
                    bwLoadAndCropBitmaps.RunWorkerAsync();*/
                //Stopwatch sw = new Stopwatch(); sw.Start();
                //lbsCropImages = FilllbsImages(oi.data.header.files, te.numProject);
                //sw.Stop();
                //teOutput.AppendText("Loading and cropped " + oi.data.header.files.Count.ToString() + " files in " + DockManager.ActiveDocument.Title + ". Estimated: " + sw.Elapsed.Milliseconds.ToString() + " ms.");
                //teOutput.AppendText(Environment.NewLine);
                //teOutput.ScrollToEnd();
            }
            if (fr != null)
                if (fr.Editors != null)
                    if (DockManager.ActiveDocument != null)
                        fr.CurrentEditor = Utils.AvalonEdit.GetTextEditorFromContent(DockManager.ActiveDocument.Content);
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].PathToFile == teCurActiveDocument.fullpath)
                {
                    Utils.AvalonEdit.HighlightText(teCurActiveDocument, Tasks[i].nline);
                    Tasks.RemoveAt(i);
                }
            }
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            new wnd.wAbout().ShowDialog();
        }

        private void miHotkeys_Click(object sender, RoutedEventArgs e)
        {
            new wnd.wHotkeys().ShowDialog();
        }

        private void SetDefaultTheme(object sender, RoutedEventArgs e)
        {
            ThemeFactory.ResetTheme();
        }

        private void ChangeCustomTheme(object sender, RoutedEventArgs e)
        {
            string uri = (string)((MenuItem)sender).Tag;
            ThemeFactory.ChangeTheme(new Uri(uri, UriKind.RelativeOrAbsolute));

        }

        private void ChangeStandardTheme(object sender, RoutedEventArgs e)
        {
            //   MainMenu.Style.Resources.Clear();
            //    MainMenu.Resources = null;
            //   MainMenu.Style = null;

            string name = (string)((MenuItem)sender).Tag;
            ThemeFactory.ChangeTheme(name);
            G.AppSettings.ColorForStandardTheme = "";
            G.AppSettings.AdvancedTheme = "";
            adce.Save(programfolder + "main.xml", G.AppSettings);
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {

            ThemeFactory.ChangeColors((Color)ColorConverter.ConvertFromString(((MenuItem)sender).Header.ToString()));

        }
        /*
          private void MenuItem_Click(object sender, RoutedEventArgs e)
          {
            //  string st = (sender as MenuItem).Tag as string;
            //  ThemeFactory.ChangeTheme(new Uri(programfolder+ "Themes\\"+st, UriKind.RelativeOrAbsolute));
              string st = programfolder + @"Themes\Standard\aero.normalcolor.xaml";
              ThemeFactory.ChangeTheme(new Uri(st, UriKind.RelativeOrAbsolute));
          }
          */
        private void ChangeColor(object sender, SelectionChangedEventArgs e)
        {
            WPFBrush obj = (sender as ComboBox).SelectedItem as WPFBrush;
            ThemeFactory.ChangeColors((Color)ColorConverter.ConvertFromString(obj.Hex));

            G.AppSettings.ColorForStandardTheme = obj.Hex;
            G.AppSettings.AdvancedTheme = "";
            adce.Save(programfolder + "main.xml", G.AppSettings);

            /*
            Color color = (Color)ColorConverter.ConvertFromString(obj.Hex);
            ResourceDictionary rd = 
                //Application.Current.Resources;
                new ResourceDictionary();
            rd.Source = new Uri(@"C:\main\projects\LFStudio\LFStudio\bin\Release\Themes\dev2010.xaml");
                //new Uri("/AvalonDock;component/themes/" + baseTheme + ".xaml", UriKind.RelativeOrAbsolute);
            ThemeFactory.ChangeKeysInResourceDictionary(rd, color);
            foreach (ResourceDictionary rd2 in rd.MergedDictionaries) ThemeFactory.ChangeKeysInResourceDictionary(rd2, color);            
            ThemeFactory.ResetTheme();
            Application.Current.Resources.MergedDictionaries.Add(rd);                               
             */
            /////////////////////////////////////////////////////////////////////////

        }
        /*
        private void miSelectLang_Click(object sender, RoutedEventArgs e)
        {
            LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("ru-RU");
        }

        private void miEng_Click(object sender, RoutedEventArgs e)
        {
            LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
        }
        */
        private void miAuto_Click(object sender, RoutedEventArgs e)
        {
            Utils.Menu.AbordIsCheckedMenuItems(miLang);
            (sender as MenuItem).IsChecked = true;
            string curlang = CultureInfo.InstalledUICulture.IetfLanguageTag;
            var ld = LanguageDictionary.GetDictionary(CultureInfo.GetCultureInfo(curlang));
            if (ld == LanguageDictionary.Null) curlang = "en-US";
            LanguageContext.Instance.Culture = CultureInfo.GetCultureInfo(curlang);
            G.AppSettings.Language = "Autolang";
            Utils.tv.RefreshEditableTextBlock(tviProjects);
            adce.Save(programfolder + "main.xml", G.AppSettings);
        }





        private void teOutput_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void DockManager_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void bNoFocus_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.focusTo = WhereFocused.NotSpecified;
            G.AppSettings.sgvFocusTo = 0;
            SaveCurrentSettings();
        }

        private void bFocusToFrame_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.focusTo = WhereFocused.Frame;
            G.AppSettings.sgvFocusTo = 1;
            SaveCurrentSettings();
            UpdateSGVFocus();
        }

        private void bFocusToFrameNext_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.focusTo = WhereFocused.NextFrame;
            G.AppSettings.sgvFocusTo = 3;
            SaveCurrentSettings();
            UpdateSGVFocus();
        }

        private void bFocusBetweenFrames_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.focusTo = WhereFocused.BetweenFrames;
            G.AppSettings.sgvFocusTo = 2;
            SaveCurrentSettings();
            UpdateSGVFocus();
        }
        public void UpdateSGVFocus()
        {
            //string hor = svSGV.GetValue(ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty).ToString();
            //string vert = svSGV.GetValue(ScrollViewer.ComputedVerticalScrollBarVisibilityProperty).ToString();
            double vw = zpSGV.ViewportWidth;
            double vh = zpSGV.ViewportHeight;
            if (dcCanvas.Height > zpSGV.ActualHeight) // появлися вертикальный скролбар
            {
            }
            if (dcCanvas.Width > zpSGV.ActualWidth) //появился горизонтальный скролбар
            {
            }
            //"Collapsed"
            //"Visible"        
            Vector v = dcCanvas.ApplyFocusSetting(vw, vh, zpSGV.ContentScale);
            if (v.X == -1) return;
            zpSGV.ContentOffsetX = v.X / zpSGV.ContentScale;
            zpSGV.ContentOffsetY = v.Y / zpSGV.ContentScale;
        }

        private void bIncCanvasSize_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.Width += 50;
            dcCanvas.Height += 50;
            G.AppSettings.startCanvasWidth = (int)dcCanvas.Width;
            G.AppSettings.startCanvasHeight = (int)dcCanvas.Height;
            SaveCurrentSettings();
            UpdateSGVFocus();
        }

        private void bDecCanvasSize_Click(object sender, RoutedEventArgs e)
        {
            dcCanvas.Width -= 50;
            dcCanvas.Height -= 50;
            G.AppSettings.startCanvasWidth = (int)dcCanvas.Width;
            G.AppSettings.startCanvasHeight = (int)dcCanvas.Height;
            SaveCurrentSettings();
            UpdateSGVFocus();
        }
        public Point IncDecForPoints(List<PropDesc> bi, Point p)
        {
            int x = GetPropValueByName(bi, "x:");
            int y = GetPropValueByName(bi, "y:");
            if (x == int.MaxValue || y == int.MaxValue) return new Point(int.MaxValue, int.MaxValue);
            return (new Point(x + p.X, y + p.Y));
        }
        public Rect IncDecForBdyAndItr(List<PropDesc> bi, int[] rect)
        {
            int x = GetPropValueByName(bi, "x:");
            int y = GetPropValueByName(bi, "y:");
            int w = GetPropValueByName(bi, "w:");
            int h = GetPropValueByName(bi, "h:");
            if (x == int.MaxValue || y == int.MaxValue || w == int.MaxValue || h == int.MaxValue) return Rect.Empty;
            int newwidth = w + rect[2];
            int newheight = h + rect[3];
            if (newwidth < 0) newwidth = 0;
            if (newheight < 0) newheight = 0;
            return new Rect(x + rect[0], y + rect[1], newwidth, newheight);
        }
        private void dcSGV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                if (Otvet == WhereStand.bdy)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, 0, +f, 0 });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { +f, 0, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentBdyInCurrentFrame(teCurActiveDocument, 0, newRect, Objectindex);
                }
                if (Otvet == WhereStand.itr)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, 0, +f, 0 });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { +f, 0, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, newRect, Objectindex);
                }
                if (Otvet == WhereStand.bpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeBpointInCurrentFrame(teCurActiveDocument, x + f, y);
                }
                if (Otvet == WhereStand.wpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, x + f, y, GetValueFromComboBox(cbWCover));
                }
                if (Otvet == WhereStand.cpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, x + f, y);
                }
                if (Otvet == WhereStand.opoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, x + f, y);
                }
                if (Otvet == WhereStand.frameheader)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centerx:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centery:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCenterXYInCurrentFrame(teCurActiveDocument, x + f, y);
                }
            }
            if (e.Key == Key.A)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                if (Otvet == WhereStand.bdy)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, 0, -f, 0 });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { -f, 0, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentBdyInCurrentFrame(teCurActiveDocument, 0, newRect, Objectindex);
                }
                if (Otvet == WhereStand.itr)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, 0, -f, 0 });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { -f, 0, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, newRect, Objectindex);
                }
                if (Otvet == WhereStand.bpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeBpointInCurrentFrame(teCurActiveDocument, x - f, y);
                }
                if (Otvet == WhereStand.wpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, x - f, y, GetValueFromComboBox(cbWCover));
                }
                if (Otvet == WhereStand.cpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, x - f, y);
                }
                if (Otvet == WhereStand.opoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, x - f, y);
                }
                if (Otvet == WhereStand.frameheader)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centerx:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centery:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCenterXYInCurrentFrame(teCurActiveDocument, x - f, y);
                }

            }
            if (e.Key == Key.W)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                if (Otvet == WhereStand.bdy)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, 0, 0, -f });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, -f, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentBdyInCurrentFrame(teCurActiveDocument, 0, newRect, Objectindex);
                }
                if (Otvet == WhereStand.itr)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, 0, 0, -f });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, -f, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, newRect, Objectindex);
                }
                if (Otvet == WhereStand.bpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeBpointInCurrentFrame(teCurActiveDocument, x, y - f);
                }
                if (Otvet == WhereStand.wpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, x, y - f, GetValueFromComboBox(cbWCover));
                }
                if (Otvet == WhereStand.cpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, x, y - f);
                }
                if (Otvet == WhereStand.opoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, x, y - f);
                }
                if (Otvet == WhereStand.frameheader)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centerx:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centery:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCenterXYInCurrentFrame(teCurActiveDocument, x, y - f);
                }

            }
            if (e.Key == Key.S)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                if (Otvet == WhereStand.bdy)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift) newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, 0, 0, +f });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].bdy[Objectindex], new int[4] { 0, +f, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentBdyInCurrentFrame(teCurActiveDocument, 0, newRect, Objectindex);
                }
                if (Otvet == WhereStand.itr)
                {
                    Rect newRect;
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    if (Keyboard.Modifiers == ModifierKeys.Shift) newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, 0, 0, +f });
                    else newRect = IncDecForBdyAndItr(oi.data.frames[CurrentFrameIndex].itr[Objectindex], new int[4] { 0, +f, 0, 0 });
                    Utils.DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, newRect, Objectindex);
                }
                if (Otvet == WhereStand.bpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].bpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeBpointInCurrentFrame(teCurActiveDocument, x, y + f);
                }
                if (Otvet == WhereStand.wpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, x, y + f, GetValueFromComboBox(cbWCover));
                }
                if (Otvet == WhereStand.cpoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, x, y + f);
                }
                if (Otvet == WhereStand.opoint)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "y:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, x, y + f);
                }
                if (Otvet == WhereStand.frameheader)
                {
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centerx:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].header, "centery:");
                    int f = 1;
                    if (Keyboard.IsKeyToggled(Key.CapsLock)) f = 5;
                    Utils.DatFileTextEditor.ChangeCenterXYInCurrentFrame(teCurActiveDocument, x, y + f);
                }

            }
            /////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////
            if (e.Key == Key.X)
            {
                if (dcCanvas.frame.frame == null) return;
                zpSGV.ContentOffsetX = dcCanvas.frame.Offset.X - zpSGV.ViewportWidth / 2 + dcCanvas.frame.frame.PixelWidth / 2;
                zpSGV.ContentOffsetY = dcCanvas.frame.Offset.Y - zpSGV.ViewportHeight / 2 + dcCanvas.frame.frame.PixelHeight / 2;
            }
            if (e.Key == Key.C)
            {
                if (dcCanvas.frame.frame == null) return;
                if (dcCanvas.framenext.frame == null) return;
                zpSGV.ContentOffsetX = ((dcCanvas.framenext.Offset.X - zpSGV.ViewportWidth / 2 + dcCanvas.framenext.frame.PixelWidth / 2) + (dcCanvas.frame.Offset.X - zpSGV.ViewportWidth / 2 + dcCanvas.frame.frame.PixelWidth / 2)) / 2;
                zpSGV.ContentOffsetY = ((dcCanvas.framenext.Offset.Y - zpSGV.ViewportHeight / 2 + dcCanvas.framenext.frame.PixelHeight / 2) + (dcCanvas.framenext.Offset.Y - zpSGV.ViewportHeight / 2 + dcCanvas.framenext.frame.PixelHeight / 2)) / 2;
            }
            if (e.Key == Key.V)
            {
                if (dcCanvas.framenext.frame == null) return;
                zpSGV.ContentOffsetX = dcCanvas.framenext.Offset.X - zpSGV.ViewportWidth / 2 + dcCanvas.framenext.frame.PixelWidth / 2;
                zpSGV.ContentOffsetY = dcCanvas.framenext.Offset.Y - zpSGV.ViewportHeight / 2 + dcCanvas.framenext.frame.PixelHeight / 2;
            }
            DetectWhereCursor();
        }
        public void DisabledEnabledComboboxes(ComboBox cb1 = null, ComboBox cb2 = null)
        {
            cbOKind.Visibility = Visibility.Collapsed;
            cbCKind.Visibility = Visibility.Collapsed;
            cbWCover.Visibility = Visibility.Collapsed;
            cbWKind.Visibility = Visibility.Collapsed;
            cbItrEffect.Visibility = Visibility.Collapsed;
            cbItrKind.Visibility = Visibility.Collapsed;
            if (cb1 != null)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                cb1.Visibility = Visibility.Visible;
            }
            if (cb2 != null)
            {
                cb2.Visibility = Visibility.Visible;
            }
        }
        private void rbBdy_Click(object sender, RoutedEventArgs e)
        {
            //cbKind.Visibility = Visibility.Hidden;
            //cbEffect.Visibility = Visibility.Hidden;            
            DisabledEnabledComboboxes();
            dcCanvas.WhatTool = WhatTool.Bdy;
            dcCanvas.SelectRegion.scbBkg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.BodyBackgroundColor));
            (dcCanvas.SelectRegion.pen.Brush as SolidColorBrush).Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.BodyBorderColor);
            if (G.AppSettings.isEnableDashStyle)
                dcCanvas.SelectRegion.pen.DashStyle = new DashStyle(G.AppSettings.BodyDashes, 0);
        }
        public void FillComboBox(ref ComboBox cb, List<string> ls)
        {
            cb.Items.Clear();
            foreach (string s in ls) cb.Items.Add(s);
        }
        private void rbItr_Click(object sender, RoutedEventArgs e)
        {
            //cbKind.Visibility = System.Windows.Visibility.Visible;
            //cbEffect.Visibility = Visibility.Visible;
            DisabledEnabledComboboxes(cbItrKind, cbItrEffect);
            dcCanvas.WhatTool = WhatTool.Itr;
            //FillComboBox(ref cbKind, icItr);
            //  cbItrKind.SelectedIndex = G.AppSettings.lastKindItrIndex;
            //  cbItrEffect.SelectedIndex = G.AppSettings.lastEffectItrIndex;

            dcCanvas.SelectRegion.scbBkg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBackgroundColor));
            //dcCanvas.SelectRegion.scbBkg.Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBackgroundColor);
            (dcCanvas.SelectRegion.pen.Brush as SolidColorBrush).Color = (Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBorderColor);
            if (G.AppSettings.isEnableDashStyle)
                dcCanvas.SelectRegion.pen.DashStyle = new DashStyle(G.AppSettings.ItrDashes, 0);
            // Utils.DatFileTextEditor.PasteItrInCurrentFrame(teCurActiveDocument, 0, new Int32Rect(10, 15, 25, 45));
            //     dcCanvas.SelectRegion.scb = new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBackgroundColor));          
            //     dcCanvas.SelectRegion.pen = new Pen(new SolidColorBrush(((Color)ColorConverter.ConvertFromString(G.AppSettings.ItrBorderColor))), 1) 
            //{ DashStyle = new DashStyle(G.AppSettings.ItrDashes, 0) };
        }

        private void rbBpoint_Click(object sender, RoutedEventArgs e)
        {
            //cbKind.Visibility = System.Windows.Visibility.Hidden;
            //cbEffect.Visibility = Visibility.Hidden;
            DisabledEnabledComboboxes();
            dcCanvas.WhatTool = WhatTool.Bpoint;
            dcCanvas.SelectRegion.scbBkg = new SolidColorBrush(Colors.Red);
            dcCanvas.Cursor = Cursors.None;

        }

        private void rbWpoint_Click(object sender, RoutedEventArgs e)
        {
            DisabledEnabledComboboxes(cbWKind, cbWCover);
            //cbKind.Visibility = System.Windows.Visibility.Visible;
            //cbEffect.Visibility = Visibility.Visible;
            dcCanvas.WhatTool = WhatTool.Wpoint;
            //FillComboBox(ref cbKind, icWpoint);
            //FillComboBox(ref cbEffect, lsWpoint);
            // cbWKind.SelectedIndex = G.AppSettings.lastKindWpointIndex;
            // cbWCover.SelectedIndex = G.AppSettings.lastCoverWpointIndex;
            dcCanvas.Cursor = Cursors.None;
        }

        private void rbCpoint_Click(object sender, RoutedEventArgs e)
        {
            DisabledEnabledComboboxes(cbCKind);
            //cbKind.Visibility = System.Windows.Visibility.Visible;
            //cbEffect.Visibility = Visibility.Hidden;
            dcCanvas.WhatTool = WhatTool.Cpoint;
            //FillComboBox(ref cbKind, icCpoint);
            // cbCKind.SelectedIndex = G.AppSettings.lastKindCpointIndex;
            dcCanvas.Cursor = Cursors.None;
        }
        private void rbOpoint_Click(object sender, RoutedEventArgs e)
        {
            DisabledEnabledComboboxes(cbOKind);
            //cbKind.Visibility = System.Windows.Visibility.Visible;
            //cbEffect.Visibility = Visibility.Hidden;
            dcCanvas.WhatTool = WhatTool.Opoint;
            //FillComboBox(ref cbKind, icOpoint);
            //  cbOKind.SelectedIndex = G.AppSettings.lastKindOpointIndex;
            dcCanvas.Cursor = Cursors.None;
        }
        private void rbCenter_Click(object sender, RoutedEventArgs e)
        {
            DisabledEnabledComboboxes();
            //cbKind.Visibility = System.Windows.Visibility.Hidden;
            //cbEffect.Visibility = Visibility.Hidden;
            dcCanvas.WhatTool = WhatTool.CenterXY;
            dcCanvas.Cursor = Cursors.None;
        }
        public int GetValueFromComboBox(ComboBox cb)
        {
            string s = cb.SelectedItem as string;
            if (s == null) return 0;
            string[] m = s.Split(' ');
            return Convert.ToInt32(m[0]);
        }
        public int GetIndexByValue(ComboBox cb, int value)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                string s = cb.Items[i] as string;
                if (s == null) return 0;
                string[] m = s.Split(' ');
                if (m[0] == value.ToString()) return i;
            }
            return 0;
        }
        private void zpSGV_MouseMove(object sender, MouseEventArgs e)
        {
            Point mp = e.GetPosition(dcCanvas);
            mp = new Point((int)mp.X, (int)mp.Y);
            tbCoor.Text = "x: " + (mp.X - dcCanvas.frame.Offset.X) + " y: " + (mp.Y - dcCanvas.frame.Offset.Y);
            if (dcCanvas.mouseButtonDown == MouseButton.Right && dcCanvas.mouseHandlingMode == WhatTool.Hand)
            {
                Point curContentMousePoint = e.GetPosition(dcCanvas);
                Vector dragOffset = (curContentMousePoint - origContentMouseDownPointDouble);
                dragOffset = Utils.MathUtils.DoubleVectorToIntVector(dragOffset);
                zpSGV.isPanning = true;
                zpSGV.ContentOffsetX -= dragOffset.X;
                zpSGV.ContentOffsetY -= dragOffset.Y;
                e.Handled = true;
            }
            if (dcCanvas.WhatTool == WhatTool.Bpoint)
            {
                //dcCanvas.rectBlood.X = mp.X; dcCanvas.rectBlood.Y = mp.Y;
                dcCanvas.SelectRegion.DrawBlood(mp.X, mp.Y);
            }
            if (dcCanvas.WhatTool == WhatTool.Wpoint && e.LeftButton == MouseButtonState.Pressed)
            {
                dcCanvas.frame.wpoint = new Point(mp.X - dcCanvas.frame.Offset.X, mp.Y - dcCanvas.frame.Offset.Y);
                //dcCanvas.frame.WeaponBitmapOffsetX = (int)(mp.X - dcCanvas.frame.Offset.X);
                //dcCanvas.frame.WeaponBitmapOffsetY = (int)(mp.Y - dcCanvas.frame.Offset.Y);
                dcCanvas.frame.Draw();
            }
            if (dcCanvas.WhatTool == WhatTool.Wpoint)
            {
                dcCanvas.SelectRegion.DrawPoint(mp.X, mp.Y);
                if (dcCanvas.isFirstMouseMove == true)
                {
                    dcCanvas.SelectRegion.StartAnimation(G.AppSettings.FromColorWpoint, G.AppSettings.ToColorWpoint, G.AppSettings.DurationWpoint, G.AppSettings.isAutoReverseWpoint);
                    dcCanvas.isFirstMouseMove = false;
                }
            }
            if (dcCanvas.WhatTool == WhatTool.Cpoint)
            {
                if (dcCanvas.isFirstMouseMove == true)
                {
                    dcCanvas.SelectRegion.DrawPoint(mp.X, mp.Y);
                    dcCanvas.SelectRegion.StartAnimation(G.AppSettings.FromColorCpoint, G.AppSettings.ToColorCpoint, G.AppSettings.DurationCpoint, G.AppSettings.isAutoReverseCpoint);
                    dcCanvas.isFirstMouseMove = false;
                }
                else
                {
                    dcCanvas.rectPoint.X = mp.X;
                    dcCanvas.rectPoint.Y = mp.Y;
                    dcCanvas.SelectRegion.DrawPoint(mp.X, mp.Y);
                }
            }
            if (dcCanvas.WhatTool == WhatTool.Opoint)
            {
                if (dcCanvas.isFirstMouseMove == true)
                {
                    dcCanvas.SelectRegion.DrawPoint(mp.X, mp.Y);
                    dcCanvas.SelectRegion.StartAnimation(G.AppSettings.FromColorOpoint, G.AppSettings.ToColorOpoint, G.AppSettings.DurationOpoint, G.AppSettings.isAutoReverseOpoint);
                    dcCanvas.isFirstMouseMove = false;
                }
                else
                {
                    dcCanvas.rectPoint.X = mp.X;
                    dcCanvas.rectPoint.Y = mp.Y;
                    dcCanvas.SelectRegion.DrawPoint(mp.X, mp.Y);
                }
            }
            #region centerx: centery:
            if (dcCanvas.WhatTool == WhatTool.CenterXY)
            {
                dcCanvas.SelectRegion.DrawPointAndShadow(mp.X, mp.Y);
                if (dcCanvas.isFirstMouseMove == true)
                {
                    dcCanvas.SelectRegion.StartAnimation(G.AppSettings.FromColorCenter, G.AppSettings.ToColorCenter, G.AppSettings.DurationCenter, G.AppSettings.isAutoReverseCenter);
                    dcCanvas.isFirstMouseMove = false;
                }
            }
            #endregion
            if (dcCanvas.mouseButtonDown == MouseButton.Left && dcCanvas.mouseHandlingMode == WhatTool.Hand)
            {
            }
            else
                if (dcCanvas.mouseHandlingMode == WhatTool.Bdy)
                {
                    if (dcCanvas.isNeedDrawSelectionRectangle)
                    {
                        dcCanvas.rectSel = dcCanvas.SelectRegion.NormalizeWidthHeight(startX, mp.X,
                                                                                      startY, mp.Y);
                        tbCoor.Text = "x: " + (dcCanvas.rectSel.X - dcCanvas.frame.Offset.X) + " y: " + (dcCanvas.rectSel.Y - dcCanvas.frame.Offset.Y) + " w: " + dcCanvas.rectSel.Width + " h: " + dcCanvas.rectSel.Height;
                        dcCanvas.SelectRegion.Draw(dcCanvas.rectSel);

                    }
                }
                else if (dcCanvas.mouseHandlingMode == WhatTool.Itr)
                {
                    if (dcCanvas.isNeedDrawSelectionRectangle)
                    {
                        dcCanvas.rectSel = dcCanvas.SelectRegion.NormalizeWidthHeight(startX, mp.X,
                                                                                      startY, mp.Y);
                        tbCoor.Text = "x: " + (dcCanvas.rectSel.X - dcCanvas.frame.Offset.X) + " y: " + (dcCanvas.rectSel.Y - dcCanvas.frame.Offset.Y) + " w: " + dcCanvas.rectSel.Width + " h: " + dcCanvas.rectSel.Height;
                        dcCanvas.SelectRegion.Draw(dcCanvas.rectSel);

                    }

                }
                else if (dcCanvas.mouseHandlingMode == WhatTool.Bdy)
                {

                }
                else if (dcCanvas.mouseHandlingMode == WhatTool.Zooming)
                {
                    Point curZoomAndPanControlMousePoint = e.GetPosition(zpSGV);
                    Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                    double dragThreshold = 10;
                    if (dcCanvas.mouseButtonDown == MouseButton.Left &&
                        (Math.Abs(dragOffset.X) > dragThreshold ||
                         Math.Abs(dragOffset.Y) > dragThreshold))
                    {
                        //
                        // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                        // initiate drag zooming mode where the user can drag out a rectangle to select the area
                        // to zoom in on.
                        //
                        //dcCanvas.mouseHandlingMode = dcMouseHandlingMode.DragZooming;
                        //Point curContentMousePoint = e.GetPosition(dcCanvas);
                        //      InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
                    }
                    e.Handled = true;
                }
        }
        private void zpSGV_MouseDown(object sender, MouseButtonEventArgs e)
        {
            zpSGV.Focus();
            //   Keyboard.Focus(dcCanvas);
            dcCanvas.mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(zpSGV);
            origZoomAndPanControlMouseDownPoint = new Point((int)origZoomAndPanControlMouseDownPoint.X, (int)origZoomAndPanControlMouseDownPoint.Y);
            origContentMouseDownPoint = e.GetPosition(dcCanvas);
            origContentMouseDownPointDouble = origContentMouseDownPoint;
            origContentMouseDownPoint = new Point((int)origContentMouseDownPoint.X, (int)origContentMouseDownPoint.Y);

            if (dcCanvas.mouseButtonDown == MouseButton.Right) dcCanvas.mouseHandlingMode = WhatTool.Hand;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                // Shift + left- or right-down initiates zooming mode.
                dcCanvas.mouseHandlingMode = WhatTool.Zooming;
            }
            else if (dcCanvas.mouseButtonDown == MouseButton.Left)
            {
                if (dcCanvas.WhatTool == WhatTool.Hand) dcCanvas.mouseHandlingMode = WhatTool.Hand;
                if (dcCanvas.WhatTool == WhatTool.Bdy)
                {
                    dcCanvas.mouseHandlingMode = WhatTool.Bdy;
                    dcCanvas.isNeedDrawSelectionRectangle = true;
                    startX = origContentMouseDownPoint.X;
                    startY = origContentMouseDownPoint.Y;
                }
                if (dcCanvas.WhatTool == WhatTool.Itr)
                {
                    dcCanvas.mouseHandlingMode = WhatTool.Itr;
                    dcCanvas.isNeedDrawSelectionRectangle = true;
                    startX = origContentMouseDownPoint.X;
                    startY = origContentMouseDownPoint.Y;
                }

                if (dcCanvas.WhatTool == WhatTool.Bpoint)
                {
                    //dcCanvas.isNeedDrawSelectionRectangle = false;
                    //dcCanvas.SelectRegion.Draw(Rect.Empty);

                    Utils.DatFileTextEditor.ChangeBpointInCurrentFrame(teCurActiveDocument, (int)(origContentMouseDownPoint.X - dcCanvas.frame.Offset.X),
                        (int)(origContentMouseDownPoint.Y - dcCanvas.frame.Offset.Y));
                }
                if (dcCanvas.WhatTool == WhatTool.Cpoint)
                {
                    Utils.DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbCKind), (int)(origContentMouseDownPoint.X - dcCanvas.frame.Offset.X),
                        (int)(origContentMouseDownPoint.Y - dcCanvas.frame.Offset.Y));
                }
                if (dcCanvas.WhatTool == WhatTool.Wpoint)
                {
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument,
                                                                       GetValueFromComboBox(cbWKind),
                                                                       (int)(origContentMouseDownPoint.X - dcCanvas.frame.Offset.X),
                                                                       (int)(origContentMouseDownPoint.Y - dcCanvas.frame.Offset.Y), GetValueFromComboBox(cbWCover));
                    //dcCanvas.frame.WeaponBitmap = new ColorKeyBitmap() { TransparentColor = (Color)ColorConverter.ConvertFromString(G.AppSettings.SGVColorKey) };
                    //dcCanvas.frame.WeaponBitmap.Source = lProjects[G.CurrentActiveProject].lbsCropWeapons[0][0];
                    //dcCanvas.frame.Draw();
                }
                if (dcCanvas.WhatTool == WhatTool.Opoint)
                    Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbOKind), (int)(origContentMouseDownPoint.X - dcCanvas.frame.Offset.X),
                        (int)(origContentMouseDownPoint.Y - dcCanvas.frame.Offset.Y));
                if (dcCanvas.WhatTool == WhatTool.CenterXY)
                {
                    Utils.DatFileTextEditor.ChangeCenterXYInCurrentFrame(teCurActiveDocument, (int)(origContentMouseDownPoint.X - dcCanvas.frame.Offset.X),
                        (int)(origContentMouseDownPoint.Y - dcCanvas.frame.Offset.Y));
                }

            }

            if (dcCanvas.mouseHandlingMode != WhatTool.None)
            {
                // Capture the mouse so that we eventually receive the mouse up event.
                zpSGV.CaptureMouse();
                e.Handled = true;
            }
        }
        private void zpSGV_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dcCanvas.mouseButtonDown == MouseButton.Left)
            {
                if (dcCanvas.WhatTool == WhatTool.Bdy)
                {
                    dcCanvas.isNeedDrawSelectionRectangle = false;
                    dcCanvas.SelectRegion.RemoveFromCanvas();
                    Utils.DatFileTextEditor.PasteBdyInCurrentFrame(teCurActiveDocument, 0, dcCanvas.Converter(dcCanvas.rectSel));
                }
                if (dcCanvas.WhatTool == WhatTool.Itr)
                {
                    dcCanvas.isNeedDrawSelectionRectangle = false;
                    dcCanvas.SelectRegion.Draw(Rect.Empty);
                    Utils.DatFileTextEditor.PasteItrInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbItrKind), GetValueFromComboBox(cbItrEffect), dcCanvas.Converter(dcCanvas.rectSel));
                }
                if (dcCanvas.WhatTool == WhatTool.Bpoint)
                {
                    dcCanvas.SelectRegion.Draw(Rect.Empty);
                }
                if (dcCanvas.WhatTool == WhatTool.Wpoint)
                {
                    Point p = e.GetPosition(dcCanvas);
                    Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbWKind),
                        (int)(p.X - dcCanvas.frame.Offset.X),
                        (int)(p.Y - dcCanvas.frame.Offset.Y), GetValueFromComboBox(cbWCover));
                    dcCanvas.SelectRegion.RemoveFromCanvas();
                }
                if (dcCanvas.WhatTool == WhatTool.Cpoint)
                {
                    dcCanvas.SelectRegion.Draw(Rect.Empty);
                }
                if (dcCanvas.WhatTool == WhatTool.Opoint)
                {
                    dcCanvas.SelectRegion.Draw(Rect.Empty);
                }
                if (dcCanvas.WhatTool == WhatTool.CenterXY)
                {
                    dcCanvas.SelectRegion.RemoveFromCanvas();
                }
            }
            #region Garbich
            if (dcCanvas.mouseHandlingMode != WhatTool.None)
            {
                if (dcCanvas.mouseHandlingMode == WhatTool.Zooming)
                {
                    if (dcCanvas.mouseButtonDown == MouseButton.Left)
                    {

                        // Shift + left-click zooms in on the content.
                        // ZoomIn(origContentMouseDownPoint);
                    }
                    else if (dcCanvas.mouseButtonDown == MouseButton.Right)
                    {
                        // Shift + left-click zooms out from the content.
                        // ZoomOut(origContentMouseDownPoint);
                    }
                }
            #endregion
                zpSGV.ReleaseMouseCapture();
                dcCanvas.mouseHandlingMode = WhatTool.None;
                zpSGV.isPanning = false;
                e.Handled = true;
                //zpSGV.UpdateContentViewportSize();
            }
        }
        /*        private void cbKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (cbKind.SelectedIndex == -1) return;
                    switch (dcCanvas.WhatTool)
                    {
                        case WhatTool.Itr: G.AppSettings.lastKindItrIndex = cbKind.SelectedIndex; break;
                        case WhatTool.Cpoint: G.AppSettings.lastKindCpointIndex = cbKind.SelectedIndex; break;
                        case WhatTool.Wpoint: G.AppSettings.lastKindWpointIndex = cbKind.SelectedIndex; break;
                        case WhatTool.Opoint: G.AppSettings.lastKindOpointIndex = cbKind.SelectedIndex; break;
                    }
                    adce.Save(programfolder + "main.xml", G.AppSettings);
                }
                private void cbEffect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (cbEffect.SelectedIndex == -1) return;
                    if (dcCanvas.WhatTool == WhatTool.Itr) G.AppSettings.lastEffectItrIndex = cbEffect.SelectedIndex;
                    if (dcCanvas.WhatTool == WhatTool.Wpoint) G.AppSettings.lastCoverWpointIndex = cbEffect.SelectedIndex;
                    adce.Save(programfolder + "main.xml", G.AppSettings);
                }*/
        private void zpSGV_MouseLeave(object sender, MouseEventArgs e)
        {
            dcCanvas.isFirstMouseMove = true;
            dcCanvas.SelectRegion.RemoveFromCanvas();
        }
        public void SaveCurrentSettings()
        {
            adce.Save(programfolder + "main.xml", G.AppSettings);
        }
        private void cbScale_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            zpSGV.ContentScale = Convert.ToInt32(cbScale.SelectedItem);
            lfDrawingVisual.scaleFactor = zpSGV.ContentScale;
            lfSelectRegion.scaleFactor = zpSGV.ContentScale;
            dcCanvas.RedrawFrames();
            G.AppSettings.scaleIndex = cbScale.SelectedIndex;
            UpdateSGVFocus();
            SaveCurrentSettings();
        }
        private void dcSGV_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSGVFocus();
        }
        private void svSGV_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSGVFocus();
        }
        private void dcCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isFirstMMForKFocus)
            {
                isFirstMMForKFocus = false;
                zpSGV.Focus();
                //this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle,(Action)(()=>{}));            
                //ChooseComboboxes(Otvet);
            }
        }
        private void dcCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            isFirstMMForKFocus = true;
        }

        private void dcErrors_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SaveFileDialog ofd = new SaveFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    string s = "";
                    using (StreamWriter sw = new StreamWriter(new FileStream(ofd.FileName, FileMode.Create)))
                    {
                        foreach (ObjectInfo loi in lProjects[0].datatxt.lObject)
                            foreach (gfErrors g in loi.errors)
                                s += g.N.ToString() + " " + g.Description.ToString() + " " + g.Line + " " + g.File + Environment.NewLine;
                        sw.Write(s);
                    }
                }
            }
        }

        private void tbShowAll_Click(object sender, RoutedEventArgs e)
        {
            G.AppSettings.isShowAllinSGV = tbShowAll.IsChecked.Value;
            if (G.AppSettings.isShowAllinSGV) { tbShowAllBdy.IsEnabled = false; tbShowAllItr.IsEnabled = false; }
            else { tbShowAllBdy.IsEnabled = true; tbShowAllItr.IsEnabled = true; }
            DetectWhereCursor();
            SaveCurrentSettings();
        }

        private void tbShowAllBdy_Click(object sender, RoutedEventArgs e)
        {
            G.AppSettings.isShowAllBdy = tbShowAllBdy.IsChecked.Value;
            DetectWhereCursor();
            SaveCurrentSettings();
        }

        private void tbShowAllItr_Click(object sender, RoutedEventArgs e)
        {
            G.AppSettings.isShowAllItr = tbShowAllItr.IsChecked.Value;
            DetectWhereCursor();
            SaveCurrentSettings();
        }

        private void bSettings_MouseMove(object sender, MouseEventArgs e)
        {

            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvCS", "Text", "Reload bitmaps");
        }

        private void rbBdy_MouseMove(object sender, MouseEventArgs e)
        {

            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDBR", "Text", "Define bdy region"); ;
        }

        private void rbItr_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDIR", "Text", "Define itr region");
        }

        private void rbBpoint_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDB", "Text", "Define bpoint");
        }

        private void rbWpoint_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDW", "Text", "Define wpoint");
        }

        private void rbCpoint_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDC", "Text", "Define cpoint");
        }

        private void rbOpoint_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDO", "Text", "Define opoint");
        }

        private void rbCenter_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDCC", "Text", "Define centerx and centery");
        }

        private void bIncCanvasSize_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvICS", "Text", "Increase canvas size");
        }

        private void bDecCanvasSize_MouseMove(object sender, MouseEventArgs e)
        {

            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvDCS", "Text", "Decrease canvas size");
        }

        private void bNoFocus_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvNF", "Text", "No focus");
        }

        private void bFocusToFrame_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvFTF", "Text", "Focus to frame");
        }

        private void bFocusBetweenFrames_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvFBF", "Text", "Focus between frames");
        }

        private void bFocusToFrameNext_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvFNF", "Text", "Focus to next frame");
        }

        private void tbShowAll_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvSAO", "Text", "Draw all objects");
        }

        private void tbShowAllBdy_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvSAB", "Text", "Draw all bdy's");
        }

        private void tbShowAllItr_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvSAI", "Text", "Draw all itr's");
        }

        private void cbScale_MouseMove(object sender, MouseEventArgs e)
        {
            tbCoor.Text = LanguageDictionary.Current.Translate<string>("tsgvSC", "Text", "Scale canvas");
        }

        private void bSettings_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("It will be in next version!", "Information");
            if (bSettings.Opacity == 1)
            {
                BackgroundWorker bwLoadAndCropBitmaps = new BackgroundWorker();
                bwLoadAndCropBitmaps.DoWork += new DoWorkEventHandler(bwLoadAndCropBitmaps_DoWork);
                bwLoadAndCropBitmaps.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwLoadAndCropBitmaps_RunWorkerCompleted);
                bwLoadAndCropBitmaps.RunWorkerAsync();
            }
        }

        private void miSGV_Click(object sender, RoutedEventArgs e)
        {
            switch (dcSGV.State)
            {
                case DockableContentState.Hidden: dcSGV.Show(); break;
                case DockableContentState.DockableWindow: dcSGV.Hide(); break;
                case DockableContentState.Docked: dcSGV.Hide(); break;
                case DockableContentState.AutoHide: dcSGV.Hide(); break;
            }
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelLoad_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbTypeItem_SelectionChanged(object sender, SelectionChangedEventArgs e)    //1
        {
            try
            {
                if (G.CurrentActiveProject == -1) return;
                cbItems.Items.Clear(); cbFiles.Items.Clear();
                if (cbTypeItem.SelectedItem == null) return;
                if (cbTypeItem.SelectedIndex == 0) // light weapons
                {
                    /*        for (int i = 0; i < lProjects[G.CurrentActiveProject].lWeapons.Count; i++)
                            {
                                if (lProjects[G.CurrentActiveProject].lWeapons[i].Type == 1)
                                {
                                    //lProjects[G.CurrentActiveProject].lWeapons[i].
                                    cbItems.Items.Add(new ComboBoxItem()
                                    {
                                        Tag = lProjects[G.CurrentActiveProject].lWeapons[i],
                                        Content = lProjects[G.CurrentActiveProject].lWeapons[i].Title
                                    });
                                }
              

                            }*/
                    foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
                    {
                        if (oi.type == 1)
                            cbItems.Items.Add(new ComboBoxItem()
                            {
                                Tag = oi,
                                Content = Path.GetFileName(oi.path)
                            });
                    }
                }
                if (cbTypeItem.SelectedIndex == 1) // heavy weapons
                {
                    foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
                    {
                        if (oi.type == 2)
                            cbItems.Items.Add(new ComboBoxItem()
                            {
                                Tag = oi,
                                Content = Path.GetFileName(oi.path)
                            });
                    }
                }
                if (cbTypeItem.SelectedIndex == 2) // throw weapons
                {
                    foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
                    {
                        if (oi.type == 4)
                            cbItems.Items.Add(new ComboBoxItem()
                            {
                                Tag = oi,
                                Content = Path.GetFileName(oi.path)
                            });
                    }
                }
                if (cbTypeItem.SelectedIndex == 3) // drinks weapons
                {
                    foreach (ObjectInfo oi in lProjects[G.CurrentActiveProject].datatxt.lObject)
                    {
                        if (oi.type == 6)
                            cbItems.Items.Add(new ComboBoxItem()
                            {
                                Tag = oi,
                                Content = Path.GetFileName(oi.path)
                            });
                    }
                }
                if (cbItems.Items.Count > 0) cbItems.SelectedIndex = 0;
                DetectWhereCursor();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void cbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)   //2
        {
            try
            {
                // DetectWhereCursor();
                if (G.CurrentActiveProject == -1) return;
                cbFiles.Items.Clear();
                if (cbItems.SelectedItem == null) return;
                ObjectInfo weapon = (cbItems.SelectedItem as ComboBoxItem).Tag as ObjectInfo;
                //ObjectInfo oi = (cbItems.SelectedItem as ComboBoxItem).Tag as ObjectInfo;
                lfiWDS.item = weapon;
                //if (weapon == null) return;
                if (lProjects[G.CurrentActiveProject].datatxt == null)
                    lProjects[G.CurrentActiveProject].datatxt = Utils.DatFiles.ParseDatatxt(new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + "\\data\\data.txt"));
                if (weapon.data == null)
                    weapon.data = Utils.DatFiles.ParseDatFileWithErrorsReturn(
                        new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + "\\" + weapon.path),
                                     lProjects[G.CurrentActiveProject].pass,
                                     ref weapon.errors, lProjects[G.CurrentActiveProject]);
                if (weapon.lbiBitmaps == null) weapon.lbiCroppedBitmaps = CropBitmaps(weapon.data.header.files, G.CurrentActiveProject, ref weapon.lbiBitmaps);
                for (int i = 0; i < weapon.lbiBitmaps.Count; i++)
                {
                    cbFiles.Items.Add(
                        new ComboBoxItem()
                        {
                            Tag = weapon.lbiBitmaps[i],
                            Content = weapon.lbiBitmaps[i].UriSource.Segments[weapon.lbiBitmaps[i].UriSource.Segments.Length - 1]
                        }
                        );
                }
                if (cbFiles.Items.Count > 0) cbFiles.SelectedIndex = 0;
                DetectWhereCursor();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void cbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)    //3
        {
            try
            {
                if (G.CurrentActiveProject == -1) return;
                if (cbFiles.Items.Count == 0) return;
                if (cbFiles.SelectedItem == null) return;
                BitmapImage bi = (cbFiles.SelectedItem as ComboBoxItem).Tag as BitmapImage;
                lfiWDS.Source = bi;
                lfiWDS.BitmapIndex = cbFiles.SelectedIndex;
                lfiWDS.SelectingIndex = lfiWDS.SelectingIndex;
                DetectWhereCursor();
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
            /*  if (CurrentFrameIndex != -1)
              {
                  ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                  int weaponact = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "weaponact:");
                  ObjectInfo oiWeapon = GetWeaponFromlfItem(lfiWDS.item);
                  int windex = WhatFrame(weaponact, oiWeapon.data.frames);
                  int wpic = int.MaxValue;
                  if (windex >= 0 && windex < oiWeapon.data.frames.Count)
                  wpic = GetPropValueByName(oiWeapon.data.frames[windex].header, "pic:");
                  lfiWDS.SelectingIndex = wpic;
              }*/
        }

        private void bWeaponListUpdate_MouseMove(object sender, MouseEventArgs e)
        {
            //tbPic.Text = "Reload weapons and drinks list";
            tbPic.Text = LanguageDictionary.Current.Translate<string>("wdReload", "Text", "Reload weapons and drinks list");
        }

        private void bWeaponListUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (bWeaponListUpdate.Opacity == 1)
            {
                /*    BackgroundWorker bwLoadWeapons = new BackgroundWorker();
                    bwLoadWeapons.DoWork += new DoWorkEventHandler(bwLoadWeapons_DoWork);
                    bwLoadWeapons.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwLoadWeapons_RunWorkerCompleted);
                    bwLoadWeapons.RunWorkerAsync();*/
            }
        }

        private void iStop_MouseDown(object sender, RoutedEventArgs e)
        {
            meAP.Stop();
        }

        private void iPause_MouseDown(object sender, RoutedEventArgs e)
        {
            meAP.Pause();
        }

        private void meAP_MediaEnded(object sender, RoutedEventArgs e)
        {
            meAP.Stop();
        }

        private void iPlay_MouseDown(object sender, RoutedEventArgs e)
        {
            meAP.Play();
        }

        private void bAP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
                meAP.Pause();
        }

        private void miAP_Click(object sender, RoutedEventArgs e)
        {
            switch (dcAP.State)
            {
                case DockableContentState.Hidden: dcAP.Show(); break;
                case DockableContentState.DockableWindow: dcAP.Hide(); break;
                case DockableContentState.Docked: dcAP.Hide(); break;
                case DockableContentState.AutoHide: dcAP.Hide(); break;
            }
        }

        private void miWD_Click(object sender, RoutedEventArgs e)
        {
            switch (dcWD.State)
            {
                case DockableContentState.Hidden: dcWD.Show(); break;
                case DockableContentState.DockableWindow: dcWD.Hide(); break;
                case DockableContentState.Docked: dcWD.Hide(); break;
                case DockableContentState.AutoHide: dcWD.Hide(); break;
            }
        }

        private void cbCKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFrameIndex != -1 && Otvet == WhereStand.cpoint)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "x:");
                int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].cpoint, "y:");
                DatFileTextEditor.ChangeCpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbCKind), x, y);
            }
        }

        private void cbOKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFrameIndex != -1 && Otvet == WhereStand.opoint)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "x:");
                int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].opoint, "y:");
                DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbOKind), x, y);
            }
        }

        private void cbWCover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbWCover.IsFocused)
                if (CurrentFrameIndex != -1 && Otvet == WhereStand.wpoint)
                {
                    ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                    int kind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "kind:");
                    int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                    int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                    DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, kind, x, y, GetValueFromComboBox(cbWCover));
                }
        }

        private void cbWKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFrameIndex != -1 && Otvet == WhereStand.wpoint)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "x:");
                int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "y:");
                int cover = GetPropValueByName(oi.data.frames[CurrentFrameIndex].wpoint, "cover:");
                DatFileTextEditor.ChangeWpointInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbWKind), x, y, cover);
            }
        }

        private void cbItrEffect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (CurrentFrameIndex != -1 && Otvet == WhereStand.itr)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                int kind = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "kind:");
                int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "x:");
                int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "y:");
                int w = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "w:");
                int h = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "h:");
                DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbItrKind), new Rect(x, y, w, h), Objectindex, GetValueFromComboBox(cbItrEffect));
            }

        }

        private void cbItrKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentFrameIndex != -1 && Otvet == WhereStand.itr)
            {
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                int x = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "x:");
                int y = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "y:");
                int w = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "w:");
                int h = GetPropValueByName(oi.data.frames[CurrentFrameIndex].itr[Objectindex], "h:");
                DatFileTextEditor.ChangeCurrentItrInCurrentFrame(teCurActiveDocument, GetValueFromComboBox(cbItrKind), new Rect(x, y, w, h), Objectindex);
            }
        }

        private void cbCSVBitmap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbCSVBitmap.Items.Count == 0) return;
                if (cbCSVBitmap.SelectedItem == null) return;
                BitmapImage bi = (cbCSVBitmap.SelectedItem as ComboBoxItem).Tag as BitmapImage;
                lfiCSV.Source = bi;
                lfiCSV.item = teCurActiveDocument.oi as ObjectInfo;
                lfiCSV.BitmapIndex = cbCSVBitmap.SelectedIndex;
                lfiCSV.SelectingIndex = lfiCSV.SelectingIndex;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }

        }
        private void lfiCSV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Utils.DatFileTextEditor.ChangePicInCurrentFrame(teCurActiveDocument, lfiCSV.pic);
            DetectWhereCursor();
        }

        private void bCSVListUpdate_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void bCSVListUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (teCurActiveDocument == null) return;
                ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
                oi.lbiBitmaps = null;
                if (oi.lbiBitmaps == null) oi.lbiCroppedBitmaps = CropBitmaps(oi.data.header.files, G.CurrentActiveProject, ref oi.lbiBitmaps);
                FillComboBoxForDFV(oi);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void bSOListUpdate_MouseMove(object sender, MouseEventArgs e)
        {
            tbSOPic.Text = LanguageDictionary.Current.Translate<string>("tSO", "Text", "Reload");
        }

        private void bSOListUpdate_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSpawnObjectsLoading();
        }

        private void cbSOBitmap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbSOBitmap.Items.Count == 0) return;
                if (cbSOBitmap.SelectedItem == null) return;
                ObjectInfo oi = (cbSOBitmap.SelectedItem as ComboBoxItem).Tag as ObjectInfo;
                lfiSO.Source = oi.lbiBitmaps[cbSOBitmap.SelectedIndex];
                lfiSO.item = oi;
                lfiSO.BitmapIndex = cbSOBitmap.SelectedIndex;
                lfiSO.SelectingIndex = lfiSO.SelectingIndex;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        private void lfiCSV_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }
        private void cbSOAll_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void cbSOBitmap_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
        private void cbSODatFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ObjectInfo oi = (cbSODatFile.SelectedItem as ComboBoxItem).Tag as ObjectInfo;
                if (oi.data == null)
                    oi.data = DatFiles.ParseDatFileWithErrorsReturn(new FileInfo(lProjects[G.CurrentActiveProject].path_to_folder + '\\' + oi.path),
                                                  lProjects[G.CurrentActiveProject].pass, ref oi.errors, lProjects[G.CurrentActiveProject]);
                if (oi.lbiBitmaps == null)
                    oi.lbiCroppedBitmaps = CropBitmaps(oi.data.header.files, G.CurrentActiveProject, ref oi.lbiBitmaps);

                cbSOBitmap.Items.Clear();

                for (int i = 0; i < oi.data.header.files.Count; i++)
                {
                    cbSOBitmap.Items.Add(
                        new ComboBoxItem()
                        {
                            Content = Path.GetFileName(oi.data.header.files[i].path),
                            Tag = oi
                        }
                        );
                }
                if (cbSOBitmap.Items.Count > 0) cbSOBitmap.SelectedIndex = 0;
                /////////////////////////////////////////////////////////////              
                Utils.DatFileTextEditor.ChangeOpointInCurrentFrame(teCurActiveDocument, oi.id.Value);
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }

        private void lfiSO_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void RenderSurface_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_sprite == null) return;
            if (teCurActiveDocument == null) return;
            if (teCurActiveDocument.oi == null) return;
            ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
            Canvas.SetLeft(_sprite, RenderSurface.ActualWidth / 2 - oi.lbiCroppedBitmaps[0].PixelWidth / 2);
            Canvas.SetTop(_sprite, RenderSurface.ActualHeight / 2 - oi.lbiCroppedBitmaps[0].PixelHeight / 2);
        }

        private void cbEnableRange_Click(object sender, RoutedEventArgs e)
        {
            if (cbEnableRange.IsChecked == true)
            {
                tbRange.IsEnabled = true;
                //cbEnableFrame.IsEnabled = true;
                bPlayAnim.IsEnabled = true;
               // cbRepeat.IsEnabled = true;
                RenderSurface.Children.Clear();
            }
            else
            {
                tbRange.IsEnabled = false;
             //   cbEnableFrame.IsEnabled = false;
                bPlayAnim.IsEnabled = false;
             //   cbRepeat.IsEnabled = false;
                RenderSurface.Children.Clear();
            }
        }

        private void tbBaseFPS_KeyUp(object sender, KeyEventArgs e)
        {
            int newvalue;
            bool result = int.TryParse(tbBaseFPS.Text, out newvalue);
            if (result) G.AppSettings.baseGameFPS = newvalue;
        }
        private List<int> GetWaitsFromOI(ObjectInfo oi)
        {
            List<int> result = new List<int>();
            for (int i=0;i<999;i++)
            {
               
            }
            return null;
        }
        private void bPlayAnim_Click(object sender, RoutedEventArgs e)
        {
            #region     Run animate
            #region Prepare list of indexes
            
            var range = new lfFiltr(G.mainWindow.tbRange.Text.ToString());
            // npics = range.allvalues;           
            #endregion
                if (teCurActiveDocument == null) return;
            ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
            if (oi == null) return;
            _sprite = new AnimatedSprite(oi, TimeSpan.FromSeconds(1 / G.AppSettings.baseGameFPS),range.allvalues,null,(bool)cbRepeat.IsChecked);

            Canvas.SetLeft(_sprite, RenderSurface.ActualWidth / 2 - oi.lbiCroppedBitmaps[0].PixelWidth / 2);
            Canvas.SetTop(_sprite, RenderSurface.ActualHeight / 2 - oi.lbiCroppedBitmaps[0].PixelHeight / 2);
            if (RenderSurface.Children.Count > 0) RenderSurface.Children.Remove(RenderSurface.Children[0]);
            RenderSurface.Children.Add(_sprite);

            //CompositionTarget.Rendering += OnRender;
            #endregion
        }
        private void OnRender(object sender, EventArgs e)
        {
            if (_sprite == null) return;
            _sprite.InvalidateVisual();
        }

        private void bGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (teCurActiveDocument == null) return;
            ObjectInfo oi=teCurActiveDocument.oi as ObjectInfo;
            ocESA.Clear();
            //dgESA.Items.Clear();
            int count = 0;
            int.TryParse(tbCountNewRows.Text, out count);            
            int nframe = 0;
            int.TryParse(tbStartNFrame.Text, out nframe);
            for (int i = 0; i < count; i++)
            {
                dgExtentedSpriteAnimator esa = new dgExtentedSpriteAnimator();
                esa.Hit_a = 0;
                esa.Hit_d = 0;
                esa.Hit_j = 0;
                esa.NFrame = nframe;
                #region Comment
                if (oi != null)
                {
                    int indexOI = WhatFrame(nframe, oi.data.frames);
                    if (indexOI != -1)                    
                        esa.Comment = oi.data.frames[indexOI].caption;                    
                }
                #endregion
                if (i != count - 1)
                    esa.Next = esa.NFrame + 1;
                else
                    esa.Next = 999;
                int state = 15;
                int.TryParse(tbState.Text, out state);
                esa.State = state;
                double wait = 0;
                double.TryParse(tbWait.Text, out wait);
                if (cbSeconds.IsChecked == true) wait *= G.AppSettings.baseGameFPS;
                esa.Wait = (int)wait;
                nframe++;
                ocESA.Add(esa);
            }
        }

        private void bEachWait_Click(object sender, RoutedEventArgs e)
        {
            foreach (dgExtentedSpriteAnimator esa in ocESA)
            {
                double wait = 0;
                //int.TryParse(tbWait.Text, out wait);
                double.TryParse(tbWait.Text, out wait);
                if (cbSeconds.IsChecked == true) wait *= G.AppSettings.baseGameFPS;
                esa.Wait = (int)wait;
            }
            dgESA.Items.Refresh();
        }

        private void bState_Click(object sender, RoutedEventArgs e)
        {
            foreach (dgExtentedSpriteAnimator esa in ocESA)
            {
                int state = 15;
                int.TryParse(tbState.Text, out state);
                esa.State = state;
            }
            dgESA.Items.Refresh();
        }

        private void bTT_Click(object sender, RoutedEventArgs e)
        {
            double tt = 0;
            //int.TryParse(tbTT.Text, out tt);
            double.TryParse(tbTT.Text, out tt);
            if (cbSeconds.IsChecked==true) tt = tt * G.AppSettings.baseGameFPS;
            double value = tt / ocESA.Count;
            foreach (dgExtentedSpriteAnimator esa in ocESA)
            esa.Wait = (int)value;            
            dgESA.Items.Refresh();
        }

        private void bbindNext_Click(object sender, RoutedEventArgs e)
        {
            for (int i=0;i<ocESA.Count;i++)            
            {
                if (i == ocESA.Count - 1) {ocESA[i].Next = 999; break;}
                ocESA[i].Next = ocESA[i + 1].NFrame;
            }
            dgESA.Items.Refresh();
        }
        public int WhatFrameESA(int nframe)
        {
            for (int i = 0; i < ocESA.Count; i++)
            {
                if (ocESA[i].NFrame == nframe) return i;
            }
            return -1;
        }
        private void bAnimate_Click(object sender, RoutedEventArgs e)
        {

            
            #region     Run animate
            ObjectInfo oi = teCurActiveDocument.oi as ObjectInfo;
            List<int> npics = new List<int>();
            List<int> waits = new List<int>();
            if (ocESA.Count < 1) return;
            int nframe=ocESA[0].NFrame;
            int indexOI = WhatFrame(nframe, oi.data.frames);
            int pic = G.mainWindow.GetPropValueByName(oi.data.frames[indexOI].header, "pic:");
            int wait = ocESA[0].Wait;
            npics.Add(pic); waits.Add(wait);            
            int next=ocESA[0].Next;
            for (int i = 0; i < 399; i++)
            {                
                if (next == 999) break;
                int indexESA = WhatFrameESA(next);
                if (indexESA == -1) break;
                indexOI=WhatFrame(next, oi.data.frames);
                if (indexOI == -1) break;
                pic = G.mainWindow.GetPropValueByName(oi.data.frames[indexOI].header, "pic:");
                wait = ocESA[indexESA].Wait;
                npics.Add(pic); waits.Add(wait);
                next = ocESA[indexESA].Next;
            }
            _sprite = new AnimatedSprite(oi, TimeSpan.FromSeconds(1 / G.AppSettings.baseGameFPS), npics, waits, (bool)cbRepeat.IsChecked);
            Canvas.SetLeft(_sprite, RenderSurface.ActualWidth / 2 - oi.lbiCroppedBitmaps[0].PixelWidth / 2);
            Canvas.SetTop(_sprite, RenderSurface.ActualHeight / 2 - oi.lbiCroppedBitmaps[0].PixelHeight / 2);
            if (RenderSurface.Children.Count > 0) RenderSurface.Children.Remove(RenderSurface.Children[0]);
            RenderSurface.Children.Add(_sprite);
            //CompositionTarget.Rendering += OnRender;
            #endregion
            
        }
        private int IndexGridRow(DataGridRow dgr)
        {
            for (int i = 0; i < ocESA.Count; i++)
            { 
                //if (dgr=dgESA.cell)
            }
                return -1;
            
        }
        private void dgESA_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Column ==dgtcNFrame)
                {
                    ObjectInfo oi=teCurActiveDocument.oi as ObjectInfo;
                    var v = e.EditingElement as TextBox;
                    int nframe = -1;
                    int.TryParse(v.Text,out nframe);
                    int indexOI = WhatFrame(nframe, oi.data.frames);
                    var dgesa = e.Row.Item as dgExtentedSpriteAnimator;
                    if (indexOI!=-1)
                    dgesa.Comment = oi.data.frames[indexOI].caption;
                    try
                    {
                        dgESA.Dispatcher.BeginInvoke
                       (new Action(() =>dgESA.Items.Refresh()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    }
                    catch (Exception)
                    {
                        teOutput.AppendText("Error when refresh data grid"+Environment.NewLine);
                    }
                }
            }            
        }
       
      
        private void bGif_Click(object sender, RoutedEventArgs e)
        {
            var oi=teCurActiveDocument.oi as ObjectInfo;
            if (oi == null) return;
            if (_sprite == null)
            {
                MessageBox.Show
               ("To create gif you need create a sequence of sprites press button 'Animate' in SA window or ESA window");
                return;
            }
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();            
            dlg.DefaultExt = ".gif"; // Default file extension
            dlg.Filter = "Gif document (*.gif)|*.gif"; // Filter files by extension            
            Nullable<bool> result = dlg.ShowDialog();            
            if (result == true)
            {                
                AnimatedGifEncoder gif = new AnimatedGifEncoder();
                gif.Start(dlg.FileName);
                gif.SetDelay((int)(1 / G.AppSettings.baseGameFPS));
                //-1:no repeat,0:always repeat
                gif.SetRepeat(0);
                for (int i = 0; i < _sprite.sprites.Count; i++)
                {
                    var cb = oi.lbiCroppedBitmaps[_sprite.sprites[i]];
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(cb.PixelWidth, cb.PixelHeight);
                    gif.AddFrame(Graphic.CroppedBitmapToImage(cb));
                }
                gif.Finish();
            }
            /*GifHelper fs = new GifHelper();
            fs.Start();
            for (int i = 0; i < _sprite.sprites.Count; i++)
                fs.AddFrame(oi.lbiCroppedBitmaps[_sprite.sprites[i]]);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();            
            dlg.DefaultExt = ".gif"; // Default file extension
            dlg.Filter = "Gif document (.gif)|*.gif"; // Filter files by extension
            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
               fs.SaveToFile( dlg.FileName);
            }*/

        }
    }//class
}           //namespace
