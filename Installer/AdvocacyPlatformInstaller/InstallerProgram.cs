// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Application class.
    /// </summary>
    public class InstallerProgram
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                App.Main();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"A fatal error occurred: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

                StringBuilder errorOut = new StringBuilder();

                errorOut.AppendLine(ex.Message);
                errorOut.AppendLine(ex.StackTrace);

                File.WriteAllText(@".\error.log", errorOut.ToString());
            }
        }
    }
}
