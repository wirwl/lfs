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

using System.Windows;

namespace AvalonWizard
{
    /// <summary>
    /// A helper class providing attached properties for Aero Wizard style.
    /// </summary>
    public class AeroWizard : DependencyObject
    {
        #region [GlowStyle]

        /// <summary>
        /// Gets the Aero glow style of window header.
        /// </summary>
        public static AeroGlowStyle GetGlowStyle(DependencyObject obj)
        {
            return (AeroGlowStyle)obj.GetValue(GlowStyleProperty);
        }

        /// <summary>
        /// Sets the Aero glow style of window header.
        /// </summary>
        public static void SetGlowStyle(DependencyObject obj, AeroGlowStyle value)
        {
            obj.SetValue(GlowStyleProperty, value);
        }

        /// <summary>
        /// Identifies GlowStyle attached property.
        /// </summary>
        public static readonly DependencyProperty GlowStyleProperty = DependencyProperty.RegisterAttached(
            "GlowStyle", typeof(AeroGlowStyle), typeof(AeroWizard), new UIPropertyMetadata(AeroGlowStyle.BlurredText));

        #endregion
    }
}
