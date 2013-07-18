using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using LFStudio.Utils;

namespace LFStudio.Controls
{
    public class AnimatedSpriteDrawingCore : DrawingVisual
    {
        #region Fields
        ObjectInfo oi;
        // Исходное изображение
        private readonly BitmapSource _spriteBitmapSource;

        // Кадры спрайта
        private readonly List<CroppedBitmap> _spriteFramesOnBitmap;

        // Количество кадров
        private readonly int _framesCount;

        // Время в течениикоторого должен отображается кадр
        private readonly TimeSpan _frameDisplayTime;

        // Время в течении которого отображается спрайт
        private TimeSpan _displayedFrameElapsedTime;

        // Ширина кадра
        private readonly int _frameWidth;

        // Высота кадра
        private readonly int _frameHeight;

        // Позиция рамки
        private Int32Rect _framePosition;

        // Область в которой происходит рендеринг спрайта
        private Rect _renderRegionRect;

        // Индекс текущего кадра
        private int _currentFrame;
     //   int cfi;
      //  int curWait;
        List<int> sprites;
        List<int> waits;
        private bool repeat;
        #endregion
        #region Constructor

        public AnimatedSpriteDrawingCore(ObjectInfo oi, TimeSpan frameDisplayTime, List<int> npics, List<int> _waits, bool r)
        {
            this.oi = oi;
            sprites = npics;
            waits = _waits;
            repeat = r;
            //curWait = 0;
            _currentFrame = 0;
            _spriteFramesOnBitmap = oi.lbiCroppedBitmaps;
            _frameWidth = _spriteFramesOnBitmap[sprites[_currentFrame]].PixelWidth;
            _frameHeight = _spriteFramesOnBitmap[sprites[_currentFrame]].PixelHeight;
            _framePosition = new Int32Rect(0, 0, _frameWidth, _frameHeight);
            _frameDisplayTime = frameDisplayTime;
            _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[sprites[_currentFrame]].PixelWidth, 
                                                  _spriteFramesOnBitmap[sprites[_currentFrame]].PixelHeight));
           /* if (G.mainWindow.bPlayAnim.IsEnabled == false)
            {
                this.oi = oi;
                cfi = 0;
                if (G.mainWindow.CurrentFrameIndex >= 0)
                    cfi = G.mainWindow.CurrentFrameIndex;
                int pic = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "pic:");
                curWait = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "wait:");
                _currentFrame = pic;
                _spriteFramesOnBitmap = oi.lbiCroppedBitmaps;
                _frameWidth = _spriteFramesOnBitmap[pic].PixelWidth;
                _frameHeight = _spriteFramesOnBitmap[pic].PixelHeight;
                _framePosition = new Int32Rect(0, 0, _frameWidth, _frameHeight);
                _frameDisplayTime = frameDisplayTime;
                _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[pic].PixelWidth, _spriteFramesOnBitmap[pic].PixelHeight));
            }
            else
            {
                range = new lfFiltr(G.mainWindow.tbRange.Text.ToString());
                this.oi = oi;
                _currentFrame = range.allvalues[0];
                _spriteFramesOnBitmap = oi.lbiCroppedBitmaps;
                _frameWidth = _spriteFramesOnBitmap[_currentFrame].PixelWidth;
                _frameHeight = _spriteFramesOnBitmap[_currentFrame].PixelHeight;
                _framePosition = new Int32Rect(0, 0, _frameWidth, _frameHeight);
                _frameDisplayTime = frameDisplayTime;
                _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[_currentFrame].PixelWidth, _spriteFramesOnBitmap[_currentFrame].PixelHeight));
            }
            */
        }

        #endregion
        #region Properties

        public bool IsFreazed { get; set; }

        #endregion
        #region Methods

        public void Render(TimeSpan elapsedTime)
        {
            if (IsFreazed) return;
            HandleElapsedTime(elapsedTime);
            if (IsFreazed) return;
            RenderCurrentFrame();
        }

      

      /*  public void SetRenderRegionSize(Size size)
        {
            _renderRegionRect = new Rect(size);
        }*/

        private void RenderCurrentFrame()
        {
            using (var context = RenderOpen())
            {
                if (_currentFrame<sprites.Count)
                    if (sprites[_currentFrame]<_spriteFramesOnBitmap.Count)
                context.DrawImage(_spriteFramesOnBitmap[sprites[_currentFrame]], _renderRegionRect);
             //   if (range != null)
                {
                   
                }
            }

        }

        private void HandleElapsedTime(TimeSpan elapsedTime)
        {
            _displayedFrameElapsedTime += elapsedTime;
            int w = 0;
            if (waits != null)
                if (_currentFrame < waits.Count)
                    w = waits[_currentFrame];
            if (_displayedFrameElapsedTime >= (_frameDisplayTime + TimeSpan.FromSeconds(w / G.AppSettings.baseGameFPS)))
            {
                //curWait = 0;
                if (_currentFrame < sprites.Count)
                    if (sprites[_currentFrame] < _spriteFramesOnBitmap.Count)
                _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[sprites[_currentFrame]].PixelWidth,
                                                      _spriteFramesOnBitmap[sprites[_currentFrame]].PixelHeight));
                _currentFrame++;
                if (_currentFrame > sprites.Count - 1)
                    if (repeat == false)
                        this.IsFreazed = true;
                    else _currentFrame = 0;

                /*    if (G.mainWindow.bPlayAnim.IsEnabled == false)
                    {
                        #region calc next frame index
                        if (cfi == -1) {  return; }
                        int next = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "next:");
              //          if (next == 999) 
              //              next = 0;
                        cfi = G.mainWindow.WhatFrame(next, oi.data.frames);
                        int pic = -1;
                        if (cfi < 0) cfi = -cfi;
                        if (cfi > oi.data.frames.Count - 1 || cfi < 0)
                            G.mainWindow.teOutput.AppendText("Wrong frame index(HandleElapsedTime)" + Environment.NewLine);
                        else
                        {
                            pic = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "pic:");
                            curWait = G.mainWindow.GetPropValueByName(oi.data.frames[cfi].header, "wait:");
                            _currentFrame = pic;
                            _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[pic].PixelWidth, _spriteFramesOnBitmap[pic].PixelHeight));
                        }
                        #endregion
                    }
                    else
                    {
                        curWait = 0;
                        _renderRegionRect = new Rect(new Size(_spriteFramesOnBitmap[_currentFrame].PixelWidth, 
                                                              _spriteFramesOnBitmap[_currentFrame].PixelHeight));
                       // if (G.mainWindow.bPlayAnim.IsEnabled == true)
                        {
                            _currentFrame++;
                            if (_currentFrame > range.allvalues.Count - 1)
                                if (G.mainWindow.cbRepeat.IsChecked == false)
                                    this.IsFreazed = true;
                                else _currentFrame = 0;
                        }
                    }
                 */
                _displayedFrameElapsedTime = TimeSpan.FromMilliseconds(0);
            }
        }

     /*   private void SlideFrame()
        {
            if (_currentFrame == (_framesCount - 1))
            {
                _currentFrame = 1;
            }
            else
            {
                _currentFrame = Math.Min(_framesCount - 1, _currentFrame + 1);
            }
        }*/

        #endregion
    }
}
