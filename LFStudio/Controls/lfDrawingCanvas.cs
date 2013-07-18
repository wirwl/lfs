using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using LFStudio;
using LFStudio.Controls.SelectRegion;
namespace LFStudio.Controls
{

    public enum WhatTool {None, Hand, Bdy, Itr, Bpoint, Wpoint, Cpoint, Opoint,CenterXY, Zooming }
    public enum WhereFocused {NotSpecified, Frame, BetweenFrames, NextFrame }
    public class lfDrawingCanvas:Panel
    {     
        public bool isFirstMouseMove=true;
        public bool isNeedDrawSelectionRectangle = false;
        public Rect rectSel;
        public Rect rectBlood;
        public Rect rectPoint;
        public lfSelectRegion SelectRegion;
        public MouseButton mouseButtonDown;
        public WhatTool mouseHandlingMode;
        public double Width
        {
            get 
            {
                return (double)GetValue(WidthProperty);
            }
            set
            {
                if (value < 0) value = 0;
                SetValue(WidthProperty, value);               
                UpdateFramesPosition();
            }
        }
        public double Height
        {
            get
            {
                return (double)GetValue(HeightProperty);
            }
            set
            {
                if (value < 0) value = 0;
                SetValue(HeightProperty, value);                
                UpdateFramesPosition();
            }
        }
        private WhatTool _WhatTool;
        public WhatTool WhatTool
        {
            get { return _WhatTool; }
            set 
              { 
                _WhatTool = value;
                switch (value)
                {
                    case WhatTool.Hand: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Bdy: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Bpoint: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Cpoint: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Itr: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Opoint: this.Cursor = Cursors.Arrow; break;
                    case WhatTool.Wpoint: this.Cursor = Cursors.Arrow; break;
                }
              }
        }
        public WhereFocused focusTo=WhereFocused.Frame;
        public bool isHorizontal = true;
        public bool isShowFrameNext=true;
        private lfDrawingVisual _frame=null;
        public lfDrawingVisual frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
                AddVisual(_frame);
            }
        }
        private lfDrawingVisual _framenext=null;
        public lfDrawingVisual framenext
        {
            get { return _framenext; }
            set
            {
                _framenext = value;
                AddVisual(_framenext);
            }
        }
        private List<Visual> visuals = new List<Visual>();
     
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }
        public lfDrawingCanvas()
        {
            rectBlood = new Rect(0,0,1,3);
            rectPoint = new Rect(0,0,1,1);
        }
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }
        public void DeleteVisual(Visual visual)
        {
            visuals.Remove(visual);

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;
        }
        private List<DrawingVisual> hits = new List<DrawingVisual>();
        public List<DrawingVisual> GetVisuals(Geometry region)
        {
            hits.Clear();
            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = new HitTestResultCallback(this.HitTestCallback);
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }
        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            DrawingVisual visual = result.VisualHit as DrawingVisual;
            if (visual != null &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }
        public Vector GetPosXYForFrame(double wframe, double hframe)
        {
            if (!isShowFrameNext)
                return new Vector((Width / 2) - (wframe / 2), (Height / 2) - (hframe / 2));
            else          return new Vector((Width / 4) - (wframe / 2), (Height / 2) - (hframe / 2));
        }
       
        public Vector SetFrameToCenter(double wframe, double hframe)
        {
            if (!isShowFrameNext)
                return new Vector((int)((Width / 2) - (wframe / 2)), (int)((Height / 2) - (hframe / 2)));
            else return new Vector((int)((Width / 4) - (wframe / 2)), (int)((Height / 2) - (hframe / 2)));
        }
        public Vector SetFrameNextToCenter(double wframe, double hframe)
        {
            return new Vector((int)((Width / 4) - (wframe / 2) + (Width / 2)),(int) ((Height / 2) - (hframe / 2)));
        }
        public void UpdateFramesPosition()
        {
            if (this.frame.frame != null)
            this.frame.Offset = this.SetFrameToCenter(this.frame.frame.PixelWidth, this.frame.frame.PixelHeight);
            if (this.framenext.frame == null) return;
            if (this.isShowFrameNext)
                this.framenext.Offset = this.SetFrameNextToCenter(this.framenext.frame.PixelWidth, this.framenext.frame.PixelHeight);
        }
        public Vector ApplyFocusSettingScale(double vw, double vh, double maxw,double maxh, double scaleFactor)
        {
            if (focusTo == WhereFocused.NotSpecified) return new Vector(-1, -1);
            if (focusTo == WhereFocused.Frame || !isShowFrameNext)
            {   
                double realWidth=Width*scaleFactor;
                double realFameWidth=this.frame.frame.PixelWidth*scaleFactor;

                double one=realWidth / maxw;               
                if (isShowFrameNext)
                return new Vector((int)(one*(realWidth/4-realFameWidth/2)),(int)(maxh/2));
                else return new Vector((int)(maxw / 2), (int)(maxh / 2));

            }
            if (focusTo == WhereFocused.BetweenFrames)
            {

                return new Vector((int)(maxw / 2), (int)(maxh / 2));
            }
            return new Vector(0,0);
        }
        public Vector ApplyFocusSetting(double width, double height, double scaleFactor)
        {
            if (focusTo == WhereFocused.NotSpecified) return new Vector(-1, -1);
            if (focusTo == WhereFocused.Frame || !isShowFrameNext)
            {
                double fw = 0;
                double fx =  (Width*scaleFactor) / 2;
                double fh = 0;
                double fy =  (Height*scaleFactor) / 2;
                if (isShowFrameNext) { fx =  (Width*scaleFactor) / 4; fy =  (Height*scaleFactor) / 2; }
                if (frame.frame != null)
                {
                    fw = frame.frame.PixelWidth*scaleFactor; fx =  frame.Offset.X*scaleFactor;
                    fh = frame.frame.PixelHeight*scaleFactor; fy = frame.Offset.Y*scaleFactor;
                }
                return new Vector((int)(fx - width / 2 + fw / 2), (int)(fy - height / 2 + fh / 2));
                //return new Vector((int)(width/2-fw/2),(int)(height/2-fh/2));
            }
            
            if (focusTo == WhereFocused.NextFrame)
               // if (isShowFrameNext)
            {
                double fw2 = 0;
                double fx2 = Width * scaleFactor / 2;
                double fh2 = 0;
                double fy2 = Height * scaleFactor / 2;
                fx2 = Width * scaleFactor / 4; fy2 = Height * scaleFactor / 2;
                if (framenext.frame != null) 
                { 
                    fw2 = framenext.frame.PixelWidth * scaleFactor; fx2 = framenext.Offset.X * scaleFactor;
                    fh2 = framenext.frame.PixelHeight * scaleFactor; fy2 = framenext.Offset.Y * scaleFactor; 
                }
                return new Vector((int)(fx2 - width / 2 + fw2 / 2), (int)(fy2 - height / 2 + fh2 / 2));            
            }
            if (focusTo == WhereFocused.BetweenFrames)
               // if (isShowFrameNext)
            {
                double fw = 0;
                double fx = Width * scaleFactor / 2;
                double fh = 0;
                double fy = Height * scaleFactor / 2;
                if (isShowFrameNext) { fx = Width * scaleFactor / 4; fy = Height * scaleFactor / 2; }
                if (frame.frame != null)
                { fw = frame.frame.PixelWidth * scaleFactor; fx = frame.Offset.X * scaleFactor; 
                  fh = frame.frame.PixelHeight * scaleFactor; fy = frame.Offset.Y * scaleFactor; }
                double fw2 = 0;
                double fx2 = Width * scaleFactor / 2;
                double fh2 = 0;
                double fy2 = Height * scaleFactor / 2;
                fx2 = Width * scaleFactor / 4; fy2 = Height * scaleFactor / 2;
                if (framenext.frame != null)
                {
                    fw2 = framenext.frame.PixelWidth * scaleFactor; fx2 = framenext.Offset.X * scaleFactor;
                    fh2 = framenext.frame.PixelHeight * scaleFactor; fy2 = framenext.Offset.Y * scaleFactor;
                }
                return new Vector((int)(((fx2 - width / 2 + fw2 / 2)+(fx - width / 2 + fw / 2))/2),
                                  ((int)((fy2 - height / 2 + fh2 / 2) + (fy - height / 2 + fh / 2)) / 2));

            }
            return new Vector(-1,-1);

        }
        public Rect Converter(Rect rect)
        {
            return new Rect(rect.X - frame.Offset.X, rect.Y - frame.Offset.Y, rect.Width, rect.Height);
        }
        public void RedrawFrames(bool isNeedRedrawNextFrame=true)
        {
            frame.Draw();
            if (isNeedRedrawNextFrame)
            if (isShowFrameNext) framenext.Draw();
        }
        public void ClearFrame()
        { using (DrawingContext dc = frame.RenderOpen()) { } }
        public void ClearFramenext()
        { using (DrawingContext dc = framenext.RenderOpen()) { } }
        public void ClearFrames()
        {
            using (DrawingContext dc = frame.RenderOpen()) { }
            using (DrawingContext dc = framenext.RenderOpen()) { }
        }        
    }
}
