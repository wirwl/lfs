using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.DwayneNeed.Media.Imaging;

namespace GraphEngine
{
    class AnimSprite
    {
        public int fps;
        public int count;
        public int frno;
        public bool isAnim = false;
        public Color ColorKey;
        public List<ColorKeyBitmap> bitmaps;
        //public ColorKeyBitmap bitmap;
        public DrawingVisual body;

        //public bool PositionChanged = false;

        private double _x;
        public double x
        {
            get { return _x; }
            set
            {
                if (_x != value)
                { _x = value; Draw(value, y); }
            }
        }
        private double _y;
        public double y
        {
            get { return _y; }
            set
            {
                if (_y != value)
                { _y = value; Draw(x, value); }
            }
        }
        public double Width, Height;
        public AnimSprite(List<string> paths, double x = 0, double y = 0, int fps = 24, string colorkey = "#FF000000")
        {
            body = new DrawingVisual();
            this.ColorKey = (Color)ColorConverter.ConvertFromString(colorkey);
            count = paths.Count;
            bitmaps = new List<ColorKeyBitmap>(count);
            foreach (string st in paths)
            {
                bitmaps.Add(new ColorKeyBitmap() { TransparentColor = this.ColorKey, Source = new BitmapImage(new Uri(st)) });
            }
            frno = 0;
            Width = bitmaps[0].PixelWidth;
            Height = bitmaps[0].PixelHeight;
            this.x = x; this.y = y;
            this.fps=fps;
        }
        public void Draw(double x, double y,int frno=0)
        {
            
            using (DrawingContext dc = this.body.RenderOpen())
            {
                dc.DrawImage(bitmaps[frno], new Rect(x, y, Width, Height));                
            }
        }
        public void Start()
        {
            this.isAnim = true;
        }
        public void Stop()
        {
            this.isAnim = false;
        }
    }
}

