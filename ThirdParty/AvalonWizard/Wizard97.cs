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
using System.Windows;
using System.Windows.Media;

namespace AvalonWizard
{
    /// <summary>
    /// A helper class providing attached properties for Wizard 97 style.
    /// </summary>
    public class Wizard97 : DependencyObject
    {
        #region [Subtitle]

        /// <summary>
        /// Gets the <see cref="WizardPage"/> subtitle.
        /// </summary>
        public static String GetSubtitle(DependencyObject obj)
        {
            return (String)obj.GetValue(SubtitleProperty);
        }

        /// <summary>
        /// Sets the <see cref="WizardPage"/> subtitle.
        /// </summary>
        public static void SetSubtitle(DependencyObject obj, String value)
        {
            obj.SetValue(SubtitleProperty, value);
        }

        /// <summary>
        /// Identifies Subtitle attached property.
        /// </summary>
        /// <seealso cref="GetSubtitle"/>
        /// <seealso cref="SetSubtitle"/>
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.RegisterAttached("Subtitle", typeof(String), typeof(Wizard97), new UIPropertyMetadata(null));

        #endregion [Subtitle]

        #region [Icon]

        /// <summary>
        /// Gets the <see cref="WizardPage"/> icon.
        /// </summary>
        public static ImageSource GetIcon(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(IconProperty);
        }

        /// <summary>
        /// Sets the <see cref="WizardPage"/> icon.
        /// </summary>
        /// <remarks>The icon dimensions must be 48x48.</remarks>
        public static void SetIcon(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Identifies Subtitle attached property.
        /// </summary>
        /// <seealso cref="GetIcon"/>
        /// <seealso cref="SetIcon"/>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(Wizard97), new UIPropertyMetadata(null));

        #endregion [Icon]
    }
}
