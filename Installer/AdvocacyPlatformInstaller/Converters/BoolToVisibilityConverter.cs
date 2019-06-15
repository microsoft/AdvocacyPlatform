// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts a bool value to a value in the Visibility enum.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a bool value to a value in the Visibility enum.
        /// </summary>
        /// <param name="value">The bool value to convert.</param>
        /// <param name="targetType">The target type to convert to (not used).</param>
        /// <param name="parameter">A converter parameter (not used).</param>
        /// <param name="culture">Culture information for affecting the behavior of the conversion.</param>
        /// <returns>Visible if true, Collapsed if null, and Hidden if false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolValue = (bool?)value;

            if (boolValue.HasValue && boolValue.Value)
            {
                return Visibility.Visible;
            }
            else if (!boolValue.HasValue)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Converts a Visibility value to a bool value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The target type to convert to (not used).</param>
        /// <param name="parameter">A converter parameter (not used).</param>
        /// <param name="culture">Culture information for affecting the behavior of the conversion.</param>
        /// <returns>Throws a NotImplementedException.</returns>
        [Obsolete("This functionality was not implemented. Do not use!", true)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
