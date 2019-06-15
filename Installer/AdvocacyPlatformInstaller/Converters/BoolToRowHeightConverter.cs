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
    /// Converts a bool value to a row height.
    /// </summary>
    public class BoolToRowHeightConverter : IValueConverter
    {
        /// <summary>
        /// Converts a bool value to a row height.
        /// </summary>
        /// <param name="value">The bool value.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The row height (int) to convert the bool value to if true.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>The converter parameter as an integer (row height) if true, and 0 if null or false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? boolValue = (bool?)value;

            if (boolValue.HasValue && boolValue.Value)
            {
                return int.Parse(parameter.ToString());
            }
            else if (!boolValue.HasValue)
            {
                return 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts a row height value to a bool value.
        /// </summary>
        /// <param name="value">The bool value.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The row height (int) to convert the bool value to if true.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>Throws a NotImplementException.</returns>
        [Obsolete("This functionality was not implemented. Do not use!", true)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
