using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using LFStudio.Utils;

namespace LFStudio.Controls
{
    public class AnimatedSprite : FrameworkElement
    {
        #region Fields

        public List<int> sprites = new List<int>();
        public List<int> waits = new List<int>();

        private readonly Visual _spriteCore;

        private DateTime _lastTick;

        private readonly int _renderRegionWidth;

        private readonly int _renderRegionHeight;

        #endregion

        #region Constructor


        public AnimatedSprite(ObjectInfo oi, TimeSpan frameDisplayTime, List<int> npics, List<int> waits,bool r)
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            sprites = npics; 
            this.waits = waits;
      //      if (G.AppSettings.isRangeEnabled)
          

            int pic = 0;
            if (G.mainWindow.CurrentFrameIndex >= 0)
                pic = G.mainWindow.GetPropValueByName(oi.data.frames[G.mainWindow.CurrentFrameIndex].header, "pic:");
            _renderRegionWidth = oi.lbiCroppedBitmaps[pic].PixelWidth;
            _renderRegionHeight = oi.lbiCroppedBitmaps[pic].PixelHeight;
            _spriteCore = new AnimatedSpriteDrawingCore(oi, frameDisplayTime,npics,waits,r);
            _lastTick = DateTime.Now;
        }

        #endregion

        #region Properties

        public AnimatedSpriteDrawingCore Sprite { get { return _spriteCore as AnimatedSpriteDrawingCore; } }

        #endregion

        #region FrameworkElement

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _spriteCore;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(_renderRegionWidth, _renderRegionHeight);
        }

        #endregion

        #region Methods

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Sprite == null) return;
            var elapsedTime = DateTime.Now - _lastTick;
            Sprite.Render(elapsedTime);
            _lastTick = DateTime.Now;
        }
        private void OnLoaded(object sender, EventArgs e)
        {
            AddVisualChild(_spriteCore);
            AddLogicalChild(_spriteCore);
        }
        private void OnUnloaded(object sender, EventArgs e)
        {
            RemoveVisualChild(_spriteCore);
            RemoveLogicalChild(_spriteCore);
        }

        #endregion
    }
}
