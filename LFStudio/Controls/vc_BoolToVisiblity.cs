/// <copyright>
///   Distrubited under The Code Project Open License (CPOL)
///   The main points subject to the terms of the License are:
/// 
/// 	Source Code and Executable Files can be used in commercial applications;
/// 	Source Code and Executable Files can be redistributed; and
/// 	Source Code can be modified to create derivative works.
/// 	No claim of suitability, guarantee, or any warranty whatsoever is provided.
/// 	The software is provided "as-is".
/// 	Provides copyright protection: True
/// 	Can be used in commercial applications: True
/// 	Bug fixes / extensions must be released to the public domain: False
/// 	Provides an explicit patent license: True
/// 	Can be used in closed source applications: True
/// 	Is a viral licence: False
///
///	Copyright (c) 2007-2008 Kavan Jalal Shaban.
/// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace LFStudio.Control
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class vc_BoolToVisiblity : IValueConverter
    {
        public static vc_BoolToVisiblity Instance = new vc_BoolToVisiblity();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = ((bool)value);

            if (parameter is bool)
            {
                if ((bool)parameter)
                {
                    bValue = !bValue;
                }
            }

            if (bValue)
            {
                return (Visibility.Visible);
            }
            else
            {
                return (Visibility.Collapsed);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vValue = ((Visibility)value);
            if (vValue == Visibility.Hidden)
            {
                vValue = Visibility.Collapsed;
            }

            if (parameter is bool)
            {
                if ((bool)parameter)
                {
                    if (vValue == Visibility.Visible)
                    {
                        vValue = Visibility.Collapsed;
                    }
                    else
                    {
                        vValue = Visibility.Visible;
                    }
                }
            }
            return (vValue == Visibility.Visible);
        }
    }
}
