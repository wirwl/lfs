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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AvalonWizard.Converters
{
    /// <summary>
    /// Converts <see cref="bool"/> to <see cref="Visibility"/>.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. See remarks for the supported values.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <remarks>
        /// <para>
        /// The default behavior results in <see cref="Visibility.Visible"/> if the passed values is <c>true</c>
        /// and to <see cref="Visibility.Collapsed"/> otherwise. 
        /// This behavior may be changed by <paramref name="parameter"/>.
        /// </para>
        /// <para>The parameter argument can be one of the following values:</para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>INVERT</term>
        ///         <description>Inverts the value.</description>
        ///     </item>
        ///     <item>
        ///         <term>HIDE</term>
        ///         <description>Convert <c>false</c> to <c>Visibility.Hidden</c> instead of <c>Visibility.Collapsed</c>.</description>
        ///     </item>
        /// </list>
        /// <para>You may use both values, separated by comma.</para>
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            bool collapse = true;
            if (parameter != null)
            {
                String[] parts = parameter.ToString().ToUpperInvariant().Split(',');
                invert = parts.Any(part => part == "INVERT");
                collapse = parts.All(part => part != "HIDE");
            }

            bool visible = (bool)value ^ invert;
            return visible ? Visibility.Visible : 
                (collapse ? Visibility.Collapsed : Visibility.Hidden);
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.
        /// </param><param name="targetType">The type to convert to.
        /// </param><param name="parameter">The converter parameter to use.
        /// </param><param name="culture">The culture to use in the converter.
        /// </param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
