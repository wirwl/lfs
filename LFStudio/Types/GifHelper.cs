using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace LFStudio.Types
{
    class GifHelper
    {
        private BitmapEncoder fEncoder;
        protected BitmapEncoder Encoder
        {
            get { return fEncoder; }
        }

        public void AddFrame(BitmapFrame frame)
        {   
            Encoder.Frames.Add(frame);
            
        }

        public void AddFrame(BitmapSource source)
        {
            AddFrame(BitmapFrame.Create(source));
        }

        public void AddFrame(FrameworkElement element)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.ActualWidth,
            (int)element.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);
            bmp.Render(element);
            AddFrame(bmp);
        }
        protected virtual BitmapEncoder CreateEncoder()
        {                                    
            return new GifBitmapEncoder();
        }

        public void SaveToFile(String fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                Encoder.Save(fs);
            }
        }

        public void Start()
        {
            fEncoder = CreateEncoder();
        }
    }
}