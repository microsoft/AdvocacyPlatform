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
    /// Converts an InstallerActionType value to a bool value.
    /// </summary>
    public class InstallerActionTypeToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts an InstallerActionType value to a bool value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The InstallerActionType to look for.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>True if the passed value is equal to the converter parameter and false if not.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            InstallerActionType compareValueA = (InstallerActionType)int.Parse(parameter.ToString());
            InstallerActionType compareValueB = (InstallerActionType)value;

            return compareValueA == compareValueB;
        }

        /// <summary>
        /// Converts a bool value to an InstallerActionType value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">The integer equivalent of the InstallerActionType to look for.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>An InstallerActionType converted from the converter parameter.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (InstallerActionType)int.Parse(parameter.ToString());
        }
    }
}
