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
using System.Windows;
using System.Windows.Data;

namespace AvalonWizard.Converters
{
    /// <summary>
    /// Invertes the <see cref="bool"/> values.
    /// </summary>
    public class BooleanInverterConvertor : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.
        /// </param><param name="targetType">The type of the binding target property.
        /// </param><param name="parameter">The converter parameter to use.
        /// </param><param name="culture">The culture to use in the converter.
        /// </param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = !System.Convert.ToBoolean(value, culture);
                return result;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
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
            try
            {
                var result = !System.Convert.ToBoolean(value, culture);
                return result;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        #endregion
    }
}
