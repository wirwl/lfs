using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LFStudio.Types;
using System.Windows;

namespace LFStudio.Controls
{
    public class lfImage : Image
    {       
        //public int selectedPic=-1;
        public TextBlock tbPic;
        private int startIndexForPic=0;
        public int pic=-1;
        //public double mw;
        //public double mh;
        public int old_col=-1;
        public int old_row=-1;
        public int col = -1;
        public int row = -1;
        public Rect rectMouse = Rect.Empty;
        private int _BitmapIndex = -1;
        public int BitmapIndex
        {
            get { return _BitmapIndex; }
            set 
            { 
                _BitmapIndex = value; 
                startIndexForPic=0;
                for (int i = 0; i < value; i++)
                {

                    int ncol = item.lbiBitmaps[i].PixelWidth / (item.data.header.files[i].width + 1);
                    int nrow = item.lbiBitmaps[i].PixelHeight / (item.data.header.files[i].height + 1);
                    int num = ncol * nrow;
                    startIndexForPic += num;
                }
            }
        }
        public ObjectInfo item;
        public Rect rectSel;
        public Pen pSel;
        public SolidColorBrush scbBkg;
        public SolidColorBrush scbSel;
        public Pen pMouse;
        public SolidColorBrush scbbMouse;
        public int SelectingIndexForMouse=-1;
        private int si = -1;
        public int SelectingIndex
        {
            set 
            {
                this.InvalidateVisual();
                si = value; 
                rectSel = Rect.Empty;
                if (value == int.MaxValue) return;
                if (value == -1) return;
                if (item == null) return;
                if (BitmapIndex == -1) return;
                int FrameWidth = item.data.header.files[BitmapIndex].width + 1;
                int FrameHeight = item.data.header.files[BitmapIndex].height + 1;
                int ncol = OriginalWidth / (item.data.header.files[BitmapIndex].width + 1);
                int nrow = OriginalHeight / (item.data.header.files[BitmapIndex].height + 1);
                int num = ncol * nrow;
                if (value>=startIndexForPic && value < num + startIndexForPic)
                    for (int j = 0; j < num; j++)
                    {
                        if (j == value - startIndexForPic)
                        {
                            int jdncol = j / ncol;
                            rectSel = new Rect(FrameWidth * (j - (jdncol) * ncol), FrameHeight * (jdncol), FrameWidth, FrameHeight);
                            this.tbPic.Text = "pic: " + (j+startIndexForPic).ToString();
                            break;
                        }
                    }
                else rectSel = Rect.Empty;
                
            }
            get { return si; }
        }        
        public int OriginalWidth;
        public int OriginalHeight;
     
        public lfImage()
        {            
            scbSel = new SolidColorBrush(Colors.Yellow);
            if (scbSel.CanFreeze) scbSel.Freeze();
            pSel = new Pen(scbSel, 1);
            scbBkg = new SolidColorBrush(new Color(){A=0x3F,R=0xFF,G=0xFF,B=0});
            pMouse = new Pen(new SolidColorBrush(Colors.White),1);
                
            scbbMouse = new SolidColorBrush(new Color() { A = 0x3F, R = 0xFF, G = 0xFF, B = 0xFF });
        }
        public ImageSource Source
        {
            get
            {
                return (ImageSource)base.GetValue(SourceProperty);
            }
            set
            {
                //base.SetValue(ActualWidthProperty,(value as BitmapImage).Width);
                OriginalWidth = (value as BitmapImage).PixelWidth;
                OriginalHeight = (value as BitmapImage).PixelHeight;
                base.SetValue(SourceProperty, value);
            }
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            double mw = this.ActualWidth / OriginalWidth;
            double mh = this.ActualHeight / OriginalHeight;    
            if (SelectingIndex != -1 && !rectSel.IsEmpty)
            {               
                           
                dc.DrawRectangle(scbBkg, null, new Rect(mw * rectSel.X, mh * (rectSel.Y-0), mw * rectSel.Width, mh * rectSel.Height));
            }
            if (!rectMouse.IsEmpty)
            {              
                dc.DrawRectangle(scbbMouse, null, new Rect(mw*rectMouse.X,mh*rectMouse.Y,mw*rectMouse.Width,mh*rectMouse.Height));
            }
        }
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);
                //if (item.Height.Count == 0 || item.Width.Count == 0) return;
                if (item.data.header.files.Count == 0) return;
                Point p = e.MouseDevice.GetPosition(this);
               // G.mainWindow.teOutput.AppendText(p.ToString() + Environment.NewLine);
               // G.mainWindow.teOutput.ScrollToEnd();
                double mw = this.ActualWidth / OriginalWidth;
                double mh = this.ActualHeight / OriginalHeight;
                int ActualItemHeight = (int)((item.data.header.files[BitmapIndex].height + 1) * mh);                
                int ActualItemWidth = (int)((item.data.header.files[BitmapIndex].width + 1) * mw);
                //G.mainWindow.teOutput.AppendText(mw.ToString()+ Environment.NewLine);
                //G.mainWindow.teOutput.ScrollToEnd();
                col = (int)(p.X / ((ActualItemWidth)));
                row = (int)(p.Y / ((ActualItemHeight)));
                //G.mainWindow.teOutput.AppendText(col.ToString() + Environment.NewLine);
                //G.mainWindow.teOutput.ScrollToEnd();

                int maxcol = OriginalWidth / item.data.header.files[BitmapIndex].width;
                int maxrow = OriginalHeight / item.data.header.files[BitmapIndex].height;
                if (col >= maxcol) col = maxcol - 1;
                if (row >= maxrow)
                    row = maxrow - 1;
                if (col != old_col || row != old_row)
                {
                    rectMouse = new Rect(col * (item.data.header.files[BitmapIndex].width + 1), row * (item.data.header.files[BitmapIndex].height + 1), item.data.header.files[BitmapIndex].width + 1, item.data.header.files[BitmapIndex].height + 1);
//                    G.mainWindow.teOutput.AppendText(rectMouse.ToString() + Environment.NewLine);
//                    G.mainWindow.teOutput.ScrollToEnd();
                    this.InvalidateVisual();
                    pic = (row) * maxcol + col + startIndexForPic;
                    tbPic.Text = "pic: " + pic.ToString();
                }

                old_col = col;
                old_row = row;
            }
            catch (Exception ex) { new wException(ex).ShowDialog(); }
        }
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
           
          //  Utils.DatFileTextEditor.ChangeWpointInCurrentFrame(G.mainWindow.teCurActiveDocument, weaponact);
        }
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            rectMouse = Rect.Empty;
            old_col = -1; old_row = -1;
            if (this.SelectingIndex == -1)
                tbPic.Text = "";
            else
                tbPic.Text = "pic: "+this.SelectingIndex.ToString();
            this.InvalidateVisual();
        }
    }//class
}//namespace
