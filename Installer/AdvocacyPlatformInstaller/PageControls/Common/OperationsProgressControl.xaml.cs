// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// User control for tracking operation execution progress.
    /// </summary>
    public partial class OperationsProgressControl : UserControl
    {
        /// <summary>
        /// Sets the operations source list.
        /// </summary>
        public static readonly DependencyProperty OperationsSourceProperty = DependencyProperty.Register("OperationsSource", typeof(OperationsProgress), typeof(OperationsProgressControl), new PropertyMetadata(new OperationsProgress()));

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationsProgressControl"/> class.
        /// </summary>
        public OperationsProgressControl()
        {
            InitializeComponent();

            DataContext = this;
        }

        /// <summary>
        /// Gets or sets the model representing operations progress.
        /// </summary>
        public OperationsProgress OperationsSource
        {
            get
            {
                return (OperationsProgress)GetValue(OperationsSourceProperty);
            }

            set
            {
                SetValue(OperationsSourceProperty, value);
            }
        }
    }
}
