// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Model for representing a concurrent test configuration.
    /// </summary>
    public class ConcurrentTestConfiguration
    {
        private IEnumerable<TestConfiguration> _testConfigurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentTestConfiguration"/> class.
        /// </summary>
        public ConcurrentTestConfiguration()
        {
        }

        /// <summary>
        /// Gets or sets the desired number of threads to run on.
        /// </summary>
        public int NumberOfThreads { get; set; }

        /// <summary>
        /// Gets or sets the test configuration list.
        /// </summary>
        public IEnumerable<TestConfiguration> TestConfigurations
        {
            get => _testConfigurations;
            set
            {
                _testConfigurations = value;

                TestConfigurationQueue = new ConcurrentQueue<TestConfiguration>(_testConfigurations);
            }
        }

        /// <summary>
        /// Gets or sets the test configuration queue.
        /// </summary>
        [JsonIgnore]
        public ConcurrentQueue<TestConfiguration> TestConfigurationQueue { get; set; }

        /// <summary>
        /// Loads a test configuration from file.
        /// </summary>
        /// <param name="filePath">Path to the test configuration file.</param>
        /// <returns>The test configuration.</returns>
        public static ConcurrentTestConfiguration Load(string filePath)
        {
            return JsonConvert.DeserializeObject<ConcurrentTestConfiguration>(
                File.ReadAllText(filePath));
        }

        /// <summary>
        /// Saves the configuration to file.
        /// </summary>
        /// <param name="filePath">Path to the test configuration file to save to.</param>
        public void Save(string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this));
        }
    }
}
