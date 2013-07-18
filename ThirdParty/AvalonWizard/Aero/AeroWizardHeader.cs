#region License
// Copyright © Pavel Fedarovich, 2010-2012
// 
// This file is part of AvalonWizard.
//  
// You may at your option receive a license to Avalon Wizard under 
// either the terms of the Apache License version 2.0 or 
// the GNU Lesser General Public License (LGPL) version 2.1 or any later version.
//  
// AvalonWizard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//  
// You may obtain a copy of the Apache License at [http://www.apache.org/licenses/LICENSE-2.0].
// You may obtain a copy of the LGPL at [http://www.gnu.org/licenses/].
#endregion

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using AvalonWizard.Extensions;

namespace AvalonWizard.Aero
{
    /// <summary>
    /// Represent wizard's header in Aero Wizard style.
    /// </summary>
    public class AeroWizardHeader : Control
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public AeroWizardHeader()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        static AeroWizardHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AeroWizardHeader), 
                new FrameworkPropertyMetadata(typeof(AeroWizardHeader)));

            IsTabStopProperty.OverrideMetadata(typeof(AeroWizardHeader), new FrameworkPropertyMetadata(false));
        }

        #region [IsCompositionEnabled]

        /// <summary>
        /// Gets the value indicating whether DWM composition is enabled.
        /// </summary>
        public bool IsCompositionEnabled
        {
            get { return (bool)GetValue(IsCompositionEnabledProperty); }
            private set { SetValue(IsCompositionEnabledPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsCompositionEnabledPropertyKey =
            DependencyProperty.RegisterReadOnly("IsCompositionEnabled", typeof(bool), typeof(AeroWizardHeader),
                                                new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies <see cref="IsCompositionEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCompositionEnabledProperty =
            IsCompositionEnabledPropertyKey.DependencyProperty;       

        #endregion [IsCompositionEnabled]

        #region [IsActive]

        /// <summary>
        /// Gets the value indicating whether the wizard window is active.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActivePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsActivePropertyKey =
            DependencyProperty.RegisterReadOnly("IsActive", typeof (bool), typeof (AeroWizardHeader),
                                                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
            IsActivePropertyKey.DependencyProperty;

        #endregion [IsActive]

        #region [GlowStyle]

        /// <summary>
        /// Gets or sets Aero glow style for the wizard header.
        /// </summary>
        public AeroGlowStyle GlowStyle
        {
            get { return (AeroGlowStyle)GetValue(GlowStyleProperty); }
            set { SetValue(GlowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="GlowStyle"/> dependency property.
        /// </summary>
        public static DependencyProperty GlowStyleProperty = DependencyProperty.Register(
            "GlowStyle", typeof (AeroGlowStyle), typeof (AeroWizardHeader), new PropertyMetadata(AeroGlowStyle.BlurredText));

        #endregion

        #region [Private Members]

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                HwndSource.AddHook(WndProc);

                DependencyPropertyDescriptor isActiveDesc = DependencyPropertyDescriptor.FromProperty(
                    Window.IsActiveProperty, typeof(Window));
                isActiveDesc.AddValueChanged(window, OnWindowActiveChanged);
            }

            SetGlass();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                HwndSource.RemoveHook(WndProc);

                DependencyPropertyDescriptor isActiveDesc = DependencyPropertyDescriptor.FromProperty(
                    Window.IsActiveProperty, typeof(Window));
                isActiveDesc.RemoveValueChanged(window, OnWindowActiveChanged);
            }

            ResetGlass();
        }

        private void OnWindowActiveChanged(object sender, EventArgs e)
        {
            IsActive = ParentWindow.IsActive;
        }

        private void SetGlass()
        {
            IsCompositionEnabled = DesktopWindowManager.IsCompositionEnabled;
            if (ParentWindow != null)
            {
                if (IsCompositionEnabled)
                {
                    orinalWindowBackground = ParentWindow.Background;
                    ParentWindow.Background = Brushes.Transparent;
                    HwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
                    Matrix dpiMatrix = HwndSource.CompositionTarget.TransformToDevice;
                    double dpiY = dpiMatrix.M22;

                    // Create a margin structure
                    var margins = new WinApi.Margins
                                      {
                                          Left = 0,
                                          Right = 0,
                                          Top = (int)Math.Round(this.ActualHeight * dpiY, MidpointRounding.AwayFromZero),
                                          Bottom = 0
                                      };

                    DesktopWindowManager.ExtendFrameIntoClientArea(HwndSource.Handle, ref margins);
                }

                DesktopWindowManager.SetTitleAndIconVisibility(HwndSource.Handle, false);
            }
        }

        private void ResetGlass()
        {
            if (ParentWindow != null)
            {
                if (IsCompositionEnabled)
                {
                    ParentWindow.Background = orinalWindowBackground;
                    DesktopWindowManager.ResetGlass(HwndSource.Handle);
                }

                DesktopWindowManager.SetTitleAndIconVisibility(HwndSource.Handle, true);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg.In(WinApi.WM_DWMCOMPOSITIONCHANGED, WinApi.WM_DWMNCRENDERINGCHANGED))
            {
                IsCompositionEnabled = DesktopWindowManager.IsCompositionEnabled;
                if (IsCompositionEnabled)
                {
                    SetGlass();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Invoked when an unhandled MouseLeftButtonDown routed event is raised on this element.
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The MouseButtonEventArgs that contains the event data. 
        /// The event data reports that the left mouse button was pressed.
        /// </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (ParentWindow != null)
            {
                ParentWindow.DragMove();
            }
        }

        private Window ParentWindow
        {
            get
            {
                return window ?? (window = Window.GetWindow(this));
            }
        }

        private HwndSource HwndSource
        {
            get
            {
                return hwndSource ?? (hwndSource = PresentationSource.FromVisual(ParentWindow) as HwndSource);
            }
        }

        private Window window;

        private HwndSource hwndSource;

        private Brush orinalWindowBackground;

        #endregion [Private Members]
    }
}
