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
    /// Converts an operation status code (int) value to a Color value.
    /// </summary>
    public class OperationStatusCodeToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts an operation status code (int) value to a Color value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">Specifies the Color to use as the default.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>Blue if InProgress, Green if CompletedSuccessfully, Red if Failed or Unknown, and the Color passed as the converter parameter if Not Started or any other value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OperationStatusCode status = (OperationStatusCode)value;

            switch (status)
            {
                case OperationStatusCode.InProgress:
                    return new SolidColorBrush(Colors.Blue);

                case OperationStatusCode.CompletedSuccessfully:
                    return new SolidColorBrush(Colors.Green);

                case OperationStatusCode.Failed:
                case OperationStatusCode.Unknown:
                    return new SolidColorBrush(Colors.Red);

                case OperationStatusCode.NotStarted:
                default:
                    return new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString((string)parameter));
            }
        }

        /// <summary>
        /// Converts a Color value to an operation status code (int) value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type (not used).</param>
        /// <param name="parameter">A converter parameter.</param>
        /// <param name="culture">Culture information used to affect the behavior of the conversion.</param>
        /// <returns>Throws a NotImplementedException.</returns>
        [Obsolete("This functionality was not implemented. Do not use!", true)]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
