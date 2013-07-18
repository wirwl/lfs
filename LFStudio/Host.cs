using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;
using LFStudio.Controls;
using Wpf.Controls;

namespace LFStudio
{  
    public class Host
    {                 
        public Grid AppGrid;
        public MainWindow wMain;
        public object Tag;
        public SplitButton sbRunGame;
        public List<IBackgroundRenderer> bgRenderer;
     //   public override  void BeforeSaveFile()
      //  {

//        }
    }
}
