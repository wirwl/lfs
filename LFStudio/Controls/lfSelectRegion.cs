using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace LFStudio.Controls.SelectRegion
{
    public class lfSelectRegion:lfDrawingVisual
    {        
        public Rect rectPoint;
        public Rect rectBlood;
        public bool isNeedStartAnimation=false;
        public Pen pen;
        public SolidColorBrush scbBkg;
        public SolidColorBrush scbPen;
        double halfPenWidth = 0.5;
        public lfSelectRegion()
        { 
            scbPen=new SolidColorBrush(Colors.Transparent);
            pen = new Pen(scbPen, 1);
            scbBkg = new SolidColorBrush(Colors.Transparent);
            rectPoint = new Rect(0, 0, 1, 1);
            rectBlood = new Rect(0,0,1,3);
        }
        public void DrawPointAndWeapon(double x, double y)
        {
            rectPoint.X = x; rectPoint.Y = y;
            using (DrawingContext dc = this.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(rectPoint.Left + halfPenWidth);
                guidelines.GuidelinesX.Add(rectPoint.Right + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Top + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Bottom + halfPenWidth);
                dc.PushGuidelineSet(guidelines);
                dc.DrawRectangle(scbBkg, null, rectPoint);
                dc.Pop();
            }
        }
        public void DrawBlood(double x, double y)
        {
            rectBlood.X = x; rectBlood.Y = y;
            using (DrawingContext dc = this.RenderOpen())
            {
                halfPenWidth = (pen.Thickness * 1) / 2;
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(rectBlood.Left + halfPenWidth);
                guidelines.GuidelinesX.Add(rectBlood.Right + halfPenWidth);
                guidelines.GuidelinesY.Add(rectBlood.Top + halfPenWidth);
                guidelines.GuidelinesY.Add(rectBlood.Bottom + halfPenWidth);
                dc.PushGuidelineSet(guidelines);
                dc.DrawRectangle(scbBkg, null, rectBlood);
                dc.Pop();

            }
        }
        public void DrawPoint(double x, double y)
        {
            rectPoint.X = x; rectPoint.Y = y;
            using (DrawingContext dc = this.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(rectPoint.Left + halfPenWidth);
                guidelines.GuidelinesX.Add(rectPoint.Right + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Top + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Bottom + halfPenWidth);
                dc.PushGuidelineSet(guidelines);
                dc.DrawRectangle(scbBkg, null, rectPoint);
                dc.Pop();                
            }
        }
        public void DrawPointAndShadow(double x, double y)
        {
            rectPoint.X = x; rectPoint.Y = y;
            using (DrawingContext dc = this.RenderOpen())
            {
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(rectPoint.Left + halfPenWidth);
                guidelines.GuidelinesX.Add(rectPoint.Right + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Top + halfPenWidth);
                guidelines.GuidelinesY.Add(rectPoint.Bottom + halfPenWidth);
                dc.PushGuidelineSet(guidelines);
                dc.DrawRectangle(scbBkg, null, rectPoint);
                dc.Pop();
                if (shadow!=null)
                dc.DrawImage(shadow, new Rect(x - 18, y - 4, shadow.PixelWidth, shadow.PixelHeight));
            }
        }
        public void Draw(Rect rect)
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                if (!rect.IsEmpty)
                {
                    Rect newRect = new Rect(rect.X + 1, rect.Y + 1, rect.Width, rect.Height);
                    if (scaleFactor > 1) newRect = new Rect(rect.X + 0.5, rect.Y + 0.5, rect.Width, rect.Height);
                    dc.DrawRectangle(scbBkg, pen, newRect);                   
                }
            }
        }
        public void StartAnimation(string fromColor, string toColor, double duration, bool isAutoReverse)
        {
            ColorAnimation myAnimation = new ColorAnimation((Color)ColorConverter.ConvertFromString(fromColor),
                                                        (Color)ColorConverter.ConvertFromString(toColor),
                                                        new Duration(TimeSpan.FromSeconds(duration)));
            myAnimation.AutoReverse = isAutoReverse;
            myAnimation.RepeatBehavior = RepeatBehavior.Forever;
            scbBkg.BeginAnimation(SolidColorBrush.ColorProperty, myAnimation);
        }
        public Rect NormalizeWidthHeight(double x1, double x2, double y1, double y2)
        {
            double rx = x1;
            double ry = y1;
            double rw = x2 - x1;
            double rh = y2 - y1;
            if (x2 < x1) 
            { rx = x2; rw = x1 - x2; }
            if (y2 < y1) { ry = y2; rh = y1 - y2; }
            return new Rect(rx,ry,rw,rh);
        }
        public void RemoveFromCanvas()
        {
            this.Draw(Rect.Empty);
        }
    }
}
