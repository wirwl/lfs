using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Globalization;
using System.IO;

namespace LFStudio.Utils
{
   public static class Graphic
    {
       public static FormattedText GetFormattedText(string text, Color color)
       {
          return new FormattedText(text, 
                             CultureInfo.CurrentCulture, 
                             FlowDirection.LeftToRight, 
                             new Typeface("Verdana"),12, new SolidColorBrush(color));

       }
       public static void DrawPoint(DrawingContext dc, Point? point,string fromColor, string toColor, double duration, bool isAutoReverse)
       {
           SolidColorBrush scbW = new SolidColorBrush(Colors.Black);
           ColorAnimation myAnimation = new ColorAnimation((Color)ColorConverter.ConvertFromString(fromColor),
                                                           (Color)ColorConverter.ConvertFromString(toColor),
                                                           new Duration(TimeSpan.FromSeconds(duration)));
           myAnimation.AutoReverse = isAutoReverse;
           myAnimation.RepeatBehavior = RepeatBehavior.Forever;
           scbW.BeginAnimation(SolidColorBrush.ColorProperty, myAnimation);
           Pen wpen = new Pen(scbW, 1);
           Rect rectW = new Rect(point.Value.X, point.Value.Y, 0.1, 0.1);
           double halfPenWidth = wpen.Thickness / 2;
           GuidelineSet guidelines = new GuidelineSet();
           guidelines.GuidelinesX.Add(rectW.Left + halfPenWidth);
           guidelines.GuidelinesX.Add(rectW.Right + halfPenWidth);
           guidelines.GuidelinesY.Add(rectW.Top + halfPenWidth);
           guidelines.GuidelinesY.Add(rectW.Bottom + halfPenWidth);
           dc.PushGuidelineSet(guidelines);

           dc.DrawRectangle(null, wpen, rectW);
           dc.Pop();
       }
        public static Color Light(Color baseColor, byte value)
        {
            var r = baseColor.R;
            var g = baseColor.G;
            var b = baseColor.B;

            if ((r + value) > 255) r = 255;
            else r += value;

            if ((g + value) > 255) g = 255;
            else g += value;

            if ((b + value) > 255) b = 255;
            else b += value;

            return Color.FromArgb(0xFF,r, g, b);
        }
        public static Color Dark(Color baseColor, byte value)
        {
            var r = baseColor.R;
            var g = baseColor.G;
            var b = baseColor.B;

            if ((r - value) < 0) r = 0;
            else r -= value;

            if ((g - value) < 0) g = 0;
            else g -= value;

            if ((b - value) < 0) b = 0;
            else b -= value;

            return Color.FromArgb(0xFF,r, g, b);
        }
      public static Color PickColor(BitmapSource Source,double x, double y)
       {
           if (Source == null)
               throw new InvalidOperationException("Image Source not set");

           BitmapSource bitmapSource = Source as BitmapSource;
           double ActualWidth = bitmapSource.PixelWidth;
           double ActualHeight = bitmapSource.PixelHeight;
           if (bitmapSource != null)
           { // Get color from bitmap pixel.
               // Convert coopdinates from WPF pixels to Bitmap pixels and restrict them by the Bitmap bounds.
               x *= bitmapSource.PixelWidth / ActualWidth;
               if ((int)x > bitmapSource.PixelWidth - 1)
                   x = bitmapSource.PixelWidth - 1;
               else if (x < 0)
                   x = 0;
               y *= bitmapSource.PixelHeight / ActualHeight;
               if ((int)y > bitmapSource.PixelHeight - 1)
                   y = bitmapSource.PixelHeight - 1;
               else if (y < 0)
                   y = 0;

               // Lee Brimelow approach (http://thewpfblog.com/?p=62).
               //byte[] pixels = new byte[4];
               //CroppedBitmap cb = new CroppedBitmap(bitmapSource, new Int32Rect((int)x, (int)y, 1, 1));
               //cb.CopyPixels(pixels, 4, 0);
               //return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);

               // Alternative approach
               if (bitmapSource.Format == PixelFormats.Indexed4)
               {
                   byte[] pixels = new byte[1];
                   int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 3) / 4;
                   bitmapSource.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, stride, 0);

                   Debug.Assert(bitmapSource.Palette != null, "bitmapSource.Palette != null");
                   Debug.Assert(bitmapSource.Palette.Colors.Count == 16, "bitmapSource.Palette.Colors.Count == 16");
                   return bitmapSource.Palette.Colors[pixels[0] >> 4];
               }
               else if (bitmapSource.Format == PixelFormats.Indexed8)
               {
                   byte[] pixels = new byte[1];
                   int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
                   bitmapSource.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, stride, 0);

                   Debug.Assert(bitmapSource.Palette != null, "bitmapSource.Palette != null");
                   Debug.Assert(bitmapSource.Palette.Colors.Count == 256, "bitmapSource.Palette.Colors.Count == 256");
                   return bitmapSource.Palette.Colors[pixels[0]];
               }
               else
               {
                   byte[] pixels = new byte[4];
                   int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
                   bitmapSource.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, stride, 0);

                   return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
               }
               // TODO There are other PixelFormats which processing should be added if desired.
           }

   /*        DrawingImage drawingImage = Source as DrawingImage;
           if (drawingImage != null)
           { // Get color from drawing pixel.
               RenderTargetBitmap targetBitmap = TargetBitmap;
               Debug.Assert(targetBitmap != null, "targetBitmap != null");

               // Convert coopdinates from WPF pixels to Bitmap pixels and restrict them by the Bitmap bounds.
               x *= targetBitmap.PixelWidth / ActualWidth;
               if ((int)x > targetBitmap.PixelWidth - 1)
                   x = targetBitmap.PixelWidth - 1;
               else if (x < 0)
                   x = 0;
               y *= targetBitmap.PixelHeight / ActualHeight;
               if ((int)y > targetBitmap.PixelHeight - 1)
                   y = targetBitmap.PixelHeight - 1;
               else if (y < 0)
                   y = 0;

               // TargetBitmap is always in PixelFormats.Pbgra32 format.
               // Pbgra32 is a sRGB format with 32 bits per pixel (BPP). Each channel (blue, green, red, and alpha)
               // is allocated 8 bits per pixel (BPP). Each color channel is pre-multiplied by the alpha value. 
               byte[] pixels = new byte[4];
               int stride = (targetBitmap.PixelWidth * targetBitmap.Format.BitsPerPixel + 7) / 8;
               targetBitmap.CopyPixels(new Int32Rect((int)x, (int)y, 1, 1), pixels, stride, 0);
               return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
           }
           */
           throw new InvalidOperationException("Unsupported Image Source Type");
       }
       public static BitmapSource ToImageSource(FrameworkElement obj)
       {
           // Save current canvas transform
           Transform transform = obj.LayoutTransform;
           obj.LayoutTransform = null;

           // fix margin offset as well
           Thickness margin = obj.Margin;
           obj.Margin = new Thickness(0, 0,
                margin.Right - margin.Left, margin.Bottom - margin.Top);

           // Get the size of canvas
           Size size = new Size(obj.ActualWidth, obj.ActualHeight);

           // force control to Update
           obj.Measure(size);
           obj.Arrange(new Rect(size));                  
           RenderTargetBitmap bmp = new RenderTargetBitmap(
               (int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

           bmp.Render(obj);

           // return values as they were before
           obj.LayoutTransform = transform;
           obj.Margin = margin;
           return bmp;
       }
       public static void SaveControlToFile(BitmapSource bs, string path)
       {
           FileStream stream = new FileStream(path, FileMode.Create);
           PngBitmapEncoder encoder = new PngBitmapEncoder();
           encoder.Frames.Add(BitmapFrame.Create(bs));
           encoder.Save(stream);
           stream.Close();
       }
       public static System.Drawing.Image CroppedBitmapToImage(CroppedBitmap cb)
       {
             MemoryStream mStream= new MemoryStream();
             BmpBitmapEncoder jEncoder = new BmpBitmapEncoder(); 	 
	         jEncoder.Frames.Add(BitmapFrame.Create(cb));  	         
	         jEncoder.Save(mStream);
             System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(mStream);	         
             return bmp;
       }
    }
}
