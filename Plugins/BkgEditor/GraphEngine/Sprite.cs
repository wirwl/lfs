using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.DwayneNeed.Media.Imaging;

namespace GraphEngine
{
    class Sprite
    {
        public Color ColorKey;
        public ColorKeyBitmap bitmap;
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
        public Sprite(string path, double x = 0, double y = 0, string colorkey = "#FF000000")
        {
            body = new DrawingVisual();            
            this.ColorKey = (Color)ColorConverter.ConvertFromString(colorkey);
            bitmap = new ColorKeyBitmap() { TransparentColor =this.ColorKey };
            bitmap.Source = new BitmapImage(new Uri(path));
            Width = bitmap.PixelWidth;
            Height = bitmap.PixelHeight;            
            this.x = x; this.y = y;                        
        }
        public void Draw(double x, double y)
        {
            using (DrawingContext dc = this.body.RenderOpen())
            {
                dc.DrawImage(bitmap, new Rect(x, y, Width, Height));                                
            }
        }
    }
}
