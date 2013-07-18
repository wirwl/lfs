using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.DwayneNeed.Media.Imaging;
using System.Windows.Media.Animation;
using Tomers.WPF.Localization;
namespace LFStudio.Controls
{
    public enum Error { None, InvalidNextFrame, Next1000, InvalidPic, notProject }
    public class lfDrawingVisual:DrawingVisual
    {
        public int OpointBitmapOffsetY=0;
        public int OpointBitmapOffsetX=0;
        public BitmapSource OpointBitmap;
        public int Cover = 0;
        public ColorKeyBitmap WeaponBitmap;        
        public int WeaponBitmapOffsetX = 0;
        public int WeaponBitmapOffsetY = 0;

        public static double scaleFactor = 1;
    
        public Vector position;
        public int Width1;
        public int Height1;
       
        private Error _status;
        public Error status
        {
            get { return _status; }
            set { if (value != _status) { _status = value;  } }
        }
        private Point _wpoint = new Point(int.MaxValue,int.MaxValue);
        public Point wpoint
        {
            get { return _wpoint; }
            set
            {
                if (value != _wpoint)
                {
                    _wpoint = value;                    
                }
            }
        }
        private Point? _opoint = null;
        public Point? opoint
        {
            get { return _opoint; }
            set
            {
                if (value != _opoint)
                {
                    _opoint = value;
                    
                }
            }
        }
        private Point? _cpoint = null;
        public Point? cpoint
        {
            get { return _cpoint; }
            set
            {
                if (value != _cpoint)
                {
                    _cpoint = value;
                    
                }
            }
        }
        private Point? _bpoint = null;
        public Point? bpoint
        {
            get { return _bpoint; }
            set
            {
                if (value != _bpoint)
                {
                    _bpoint = value;
                    
                }
            }
        }

        public List<Rect> _lrectItr = null;
        public List<Rect> lrectItr
        {
            get { return _lrectItr; }
            set
            {
                if (value != _lrectItr)
                {
                    _lrectItr = value;
                    
                }
            }
        }
        public List<Rect> _lrectBody = null;
        public List<Rect> lrectBody
        {
            get { return _lrectBody; }
            set
            {
                if (value != _lrectBody)
                {
                    _lrectBody = value;
                    
                }
            }
        }
        private BitmapSource _frame = null;
        public BitmapSource frame
        {
            get { return _frame; }
            set
            {
                if (value != _frame)
                {
                    _frame = value;
                    
                }
            }
        }
        public ColorKeyBitmap shadow = null;
        private Point? _center = null;
        public Point? center
        {
            get { return _center; }
            set
            {
                  if (value != _center)
                {
                    _center = value;
                    
                }
            }
        }

        public void Draw()
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                #region Check errors
                if (frame == null)
                {
                    #region status
                    switch (status)
                    {
                        case Error.InvalidNextFrame:
                            dc.DrawText(Utils.Graphic.GetFormattedText("invalid next:", Colors.Blue), new Point(0, this.Height1 / 2 - 12)); return;
                        case Error.Next1000:
                            dc.DrawText(Utils.Graphic.GetFormattedText("next: 1000", Colors.Blue), new Point(0, this.Height1 / 2 - 12)); return;
                        case Error.InvalidPic:
                            Height1 = 15;
                            Width1 = 80;
                            dc.DrawText(Utils.Graphic.GetFormattedText("invalid pic", Colors.Red), new Point(0, 0)); break;
                        case Error.notProject:
                              dc.DrawText(Utils.Graphic.GetFormattedText(
                                  LanguageDictionary.Current.Translate<string>("tsgvNPrj", "Text", "This feature work only in context of project system"), Colors.Red), new Point(0, 0)); return;
                    }
                    #endregion
                   // return;
                }
                #endregion                         
                #region Draw shadow
                if (center != null)
                    if (center.Value.X != int.MaxValue && center.Value.Y != int.MaxValue)
                    {                        
                        DrawAnimPoint(dc, center, G.AppSettings.FromColorCenter, G.AppSettings.ToColorCenter, G.AppSettings.DurationCenter, G.AppSettings.isAutoReverseCenter);
                        if (shadow != null)
                            dc.DrawImage(shadow, new Rect(center.Value.X - 18, center.Value.Y - 4, shadow.PixelWidth, shadow.PixelHeight));
                    }
                #endregion
                if (Cover >0 && Cover!=int.MaxValue)
                #region Wpoint
                    if (wpoint != null)
                        if (wpoint.X != int.MaxValue && wpoint.Y != int.MaxValue)
                        {
                            if (WeaponBitmapOffsetX!=int.MaxValue && WeaponBitmapOffsetY!=int.MaxValue)
                            DrawWeapon(dc, WeaponBitmapOffsetX, WeaponBitmapOffsetY);
                            DrawAnimPoint(dc, wpoint, G.AppSettings.FromColorWpoint, G.AppSettings.ToColorWpoint,
                                                      G.AppSettings.DurationWpoint, G.AppSettings.isAutoReverseWpoint);
                        }
                    #endregion
                #region Draw frame
                if (frame!=null)
                dc.DrawImage(frame, new Rect(0, 0, frame.PixelWidth, frame.PixelHeight));
                #endregion                             
                #region Draw bdy
                if (lrectBody != null)
                    foreach (Rect rectBody in lrectBody)
                        if (rectBody.X != int.MaxValue && rectBody.Y != int.MaxValue && rectBody.Width != int.MaxValue && rectBody.Height != int.MaxValue)
                        {                                                         
                            Pen bpen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.BodyBorderColor)), 1);
                            if (G.AppSettings.isEnableDashStyle)
                                bpen.DashStyle = new DashStyle(G.AppSettings.BodyDashes, 0);
                            Rect newRect = new Rect(rectBody.X + 1, rectBody.Y + 1, rectBody.Width, rectBody.Height);
                            if (scaleFactor > 1) newRect = new Rect(rectBody.X + 0.5, rectBody.Y + 0.5, rectBody.Width, rectBody.Height);
                            
                            dc.DrawRectangle(new SolidColorBrush((Color)ColorConverter.ConvertFromString(G.AppSettings.BodyBackgroundColor)),
                                            bpen,
                                             newRect);
                        }
                #endregion
                #region Draw Itr
                if (lrectItr != null)
                    foreach (Rect rectItr in lrectItr)
                    {
                        if (rectItr.X != int.MaxValue && rectItr.Y != int.MaxValue && rectItr.Width != int.MaxValue && rectItr.Height != int.MaxValue)
                        {
                            Rect newRect = new Rect(rectItr.X + 1, rectItr.Y + 1, rectItr.Width, rectItr.Height);
                            if (scaleFactor > 1) newRect = new Rect(rectItr.X + 0.5, rectItr.Y + 0.5, rectItr.Width, rectItr.Height);
                            dc.DrawRectangle(G.mainWindow.ibrush, G.mainWindow.ipen, newRect);
                        }
                    }
                #endregion
                #region Draw Bpoint
                if (bpoint != null)
                    if (bpoint.Value.X != int.MaxValue && bpoint.Value.Y != int.MaxValue)
                    {
                        Rect rectBlood = new Rect(bpoint.Value.X,bpoint.Value.Y,1,3);
                        SolidColorBrush scbBkg= new SolidColorBrush(Colors.Red);
                        GuidelineSet guidelines = new GuidelineSet();
                        double halfPenWidth = 0.5;
                        guidelines.GuidelinesX.Add(rectBlood.Left + halfPenWidth);
                        guidelines.GuidelinesX.Add(rectBlood.Right + halfPenWidth);
                        guidelines.GuidelinesY.Add(rectBlood.Top + halfPenWidth);
                        guidelines.GuidelinesY.Add(rectBlood.Bottom + halfPenWidth);
                        dc.PushGuidelineSet(guidelines);
                        dc.DrawRectangle(scbBkg, null, rectBlood);
                        dc.Pop();
                    }
                #endregion
                if (Cover==0 && Cover!=int.MaxValue)
                #region Wpoint
                if (wpoint != null)
                    if (wpoint.X != int.MaxValue && wpoint.Y != int.MaxValue)
                    {
                        if (WeaponBitmapOffsetX != int.MaxValue && WeaponBitmapOffsetY != int.MaxValue)
                        DrawWeapon(dc, WeaponBitmapOffsetX, WeaponBitmapOffsetY);
                        DrawAnimPoint(dc, wpoint, G.AppSettings.FromColorWpoint, G.AppSettings.ToColorWpoint,
                                                  G.AppSettings.DurationWpoint, G.AppSettings.isAutoReverseWpoint);
                    }
                #endregion
                #region Opoint
                if (opoint != null)
                    if (opoint.Value.X != int.MaxValue && opoint.Value.Y != int.MaxValue)
                    {
                        if (OpointBitmapOffsetX != int.MaxValue && OpointBitmapOffsetY != int.MaxValue)
                            DrawOpoint(dc, OpointBitmapOffsetX, OpointBitmapOffsetY);
                        DrawAnimPoint(dc, opoint, G.AppSettings.FromColorOpoint, G.AppSettings.ToColorOpoint, G.AppSettings.DurationOpoint, G.AppSettings.isAutoReverseOpoint);                      
                    }
                #endregion
                #region Cpoint
                if (cpoint != null)
                    if (cpoint.Value.X != int.MaxValue && cpoint.Value.Y != int.MaxValue)
                    {                        
                        DrawAnimPoint(dc, cpoint, G.AppSettings.FromColorCpoint, G.AppSettings.ToColorCpoint, G.AppSettings.DurationCpoint, G.AppSettings.isAutoReverseCpoint);
                    }
                #endregion                
            }
        }
        private void DrawWeapon(DrawingContext dc, int x, int y)
        {
            if (WeaponBitmap == null) return;           
            if (x!=int.MaxValue && y!=int.MaxValue)
            dc.DrawImage(WeaponBitmap, new Rect(wpoint.X-x,wpoint.Y-y,WeaponBitmap.PixelWidth, WeaponBitmap.PixelHeight));
        }
        private void DrawOpoint(DrawingContext dc, int x, int y)
        {
            if (OpointBitmap == null) return;
            if (x != int.MaxValue && y != int.MaxValue)
                dc.DrawImage(OpointBitmap, new Rect(opoint.Value.X - x, opoint.Value.Y - y, OpointBitmap.PixelWidth, OpointBitmap.PixelHeight));
        }
        public static void DrawAnimPoint(DrawingContext dc, Point? point, string fromColor, string toColor, double duration, bool isAutoReverse)
        {
            SolidColorBrush scbW = new SolidColorBrush(Colors.Black);
            ColorAnimation myAnimation = new ColorAnimation((Color)ColorConverter.ConvertFromString(fromColor),
                                                            (Color)ColorConverter.ConvertFromString(toColor),
                                                            new Duration(TimeSpan.FromSeconds(duration)));
            myAnimation.AutoReverse = isAutoReverse;
            myAnimation.RepeatBehavior = RepeatBehavior.Forever;
            scbW.BeginAnimation(SolidColorBrush.ColorProperty, myAnimation);
            Pen wpen = new Pen(scbW, 1);
            Rect rectW = new Rect(point.Value.X, point.Value.Y, 1, 1);
            double halfPenWidth = (wpen.Thickness*scaleFactor) / 2;
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rectW.Left + halfPenWidth);
            guidelines.GuidelinesX.Add(rectW.Right + halfPenWidth);
            guidelines.GuidelinesY.Add(rectW.Top + halfPenWidth);
            guidelines.GuidelinesY.Add(rectW.Bottom + halfPenWidth);
            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(scbW, null, rectW);
            dc.Pop();
        }
    }
}
