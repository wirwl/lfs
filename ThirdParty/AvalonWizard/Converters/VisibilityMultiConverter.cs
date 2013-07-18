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
    /// Converts multiply <see cref="Visibility"/> values to a single one.
    /// </summary>
    public class VisibilityMultiConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        /// A converted value.
        /// If the method returns null, the valid null value is used.
        /// A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.
        /// A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use. See remark for more info.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <remarks>
        /// <para>
        /// The default behavior results in <see cref="Visibility.Visible"/> if ALL passed values are
        /// <see cref="Visibility.Visible"/> and to <see cref="Visibility.Collapsed"/> otherwise. 
        /// This behavior may be changed by <paramref name="parameter"/>.
        /// </para>
        /// <para>The parameter argument can be one of the following values:</para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>ANY</term>
        ///         <description>The result is <see cref="Visibility.Visible"/> if ANY of the passed values is <see cref="Visibility.Visible"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>HIDE</term>
        ///         <description>Convert <c>false</c> to <c>Visibility.Hidden</c> instead of <c>Visibility.Collapsed</c>.</description>
        ///     </item>
        /// </list>
        /// <para>You may use both values, separated by comma.</para>
        /// </remarks>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var visibityValues = values.Cast<Visibility>();
                String[] parts = parameter.ToString().ToUpperInvariant().Split(new[] {','});
                bool collapse = parts.All(p => p != "HIDE");
                bool all = parts.All(p => p != "ANY");
                bool visible = all ?
                    visibityValues.All(v => v == Visibility.Visible) :
                    visibityValues.Any(v => v == Visibility.Visible);

                return visible ?
                    Visibility.Visible :
                    (collapse ? Visibility.Collapsed : Visibility.Hidden);
            }
            catch
            {
                // Do nothing.
            }
            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">The value that the binding target produces.
        /// </param><param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.
        /// </param><param name="parameter">The converter parameter to use.
        /// </param><param name="culture">The culture to use in the converter.
        /// </param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
