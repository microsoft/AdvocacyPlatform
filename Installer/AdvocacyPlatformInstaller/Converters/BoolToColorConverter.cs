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
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converts bool value to Color value.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a bool value to a Color value.
        /// </summary>
        /// <param name="value">The bool value to convert.</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">A value converter parameter.</param>
        /// <param name="culture">Culture info for conversion behavior.</param>
        /// <returns>Green if true, Yellow if null, and Red if false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolValue = (bool?)value;

            if (boolValue.HasValue && boolValue.Value)
            {
                return new SolidColorBrush(Colors.Green);
            }
            else if (!boolValue.HasValue)
            {
                return new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                return new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Converts a Color value to a bool value.
        /// </summary>
        /// <param name="value">The Color value to convert.</param>
        /// <param name="targetType">The type to convert to (not used).</param>
        /// <param name="parameter">A value converter parameter.</param>
        /// <param name="culture">Culture info for conversion behavior.</param>
        /// <returns>Throws a NotImplementedException.</returns>
        [Obsolete("This functionality was not implemented. Do not use!", true)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
