using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;
using LFStudio;
using BkgEditor;
using ObjectInstaller;

namespace GraphEngine
{
    class Engine
    {
        public double Camera=0;
        public Color BackgroundColor = Colors.Teal;
        public List<AnimSprite> animobjects = new List<AnimSprite>();
        public DrawingCanvas Source;
        DateTime oldTime;
        int fcount = 0;
        DateTime time;

        public double dt;
        public Engine(DrawingCanvas c)
        {
            Source = c;            
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            time = DateTime.Now;
            
           
        }
        public void AddToScene(object obj)
        {
            if (obj is AnimSprite)
            {
                animobjects.Add(obj as AnimSprite);
                Source.AddVisual((obj as AnimSprite).body);
            }
            if (obj is Sprite)
            {
                //animobjects.Add(obj as Sprite);
                Source.AddVisual((obj as Sprite).body);
            }            

        }
        public void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            DateTime nw = DateTime.Now;
            dt = (nw - oldTime).TotalSeconds;
            oldTime = nw;

            //  Debug.WriteLine(dt);
     /*       fcount++;
            if ((DateTime.Now - time).Seconds >= 1)
            {
                time = DateTime.Now;
                Debug.WriteLine(fcount);
                fcount = 0;
            }*/
            foreach (AnimSprite spr in animobjects)
            {
                if (spr.isAnim)
                if ((nw - time).TotalSeconds >= 1.0/spr.fps)
                {
                    time = nw;
                    spr.frno++;
                    if (spr.frno >= spr.count) spr.frno = 0;
                    spr.Draw(spr.x, spr.y, spr.frno);
                }
            }
        }
    }
}
