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
    /// Converts an int value to a bool value.
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an int value to a bool value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The value to look for.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>Return true if the value is equal to the converter parameter and false if not.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int compareValueA = int.Parse(parameter.ToString());
            int compareValueB = int.Parse(value.ToString());

            return compareValueA == compareValueB;
        }

        /// <summary>
        /// Converts a bool value to an int value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The value to look for.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>The converter parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
