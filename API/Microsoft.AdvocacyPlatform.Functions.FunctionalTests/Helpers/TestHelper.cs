// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Helper class for functional tests.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Gets configuration information.
        /// </summary>
        /// <returns>The configuration information.</returns>
        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                    .AddJsonFile(GlobalConstants.LocalJsonSettingsFileName, optional: true, reloadOnChange: false)
                    .AddJsonFile(GlobalConstants.JsonAppSettingsFileName, optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// Initializes test.
        /// </summary>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="reset">Flag indicating if previous run information should be cleared from tracking file.</param>
        /// <returns>A tuple with sequential and concurrent test configuration information.</returns>
        public static Tuple<TestConfiguration, ConcurrentTestConfiguration> InitializeTest(ConsoleLogger log, bool reset = false)
        {
            log.LogInformation($"Loading test configuration from '{GlobalTestConstants.TestConfigurationFilePath}'...");
            TestConfiguration config = TestConfiguration.Load(GlobalTestConstants.TestConfigurationFilePath);

            log.LogInformation($"Loading concurrent test configuration from '{GlobalTestConstants.ConcurrentTestConfigurationFilePath}'...");
            ConcurrentTestConfiguration concurrentConfig = ConcurrentTestConfiguration.Load(GlobalTestConstants.ConcurrentTestConfigurationFilePath);

            if (!string.IsNullOrWhiteSpace(config.CallSid) && reset)
            {
                log.LogInformation("Previous run information present. Clearing out previous information...");
                config.CallSid = null;
                config.RecordingUri = null;

                config.Save(GlobalTestConstants.TestConfigurationFilePath);
                log.LogInformation("Test configuration saved to clean state.");
            }

            bool uncleanTestConfig = false;
            foreach (TestConfiguration configuration in concurrentConfig.TestConfigurationQueue)
            {
                if (!string.IsNullOrWhiteSpace(configuration.CallSid) && reset)
                {
                    uncleanTestConfig = true;
                    log.LogInformation("Previous run information present. Clearing out previous information...");
                    configuration.CallSid = null;
                    configuration.RecordingUri = null;
                }

                if (uncleanTestConfig)
                {
                    concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
                    log.LogInformation("Test configuration saved to clean state.");
                }
            }

            return new Tuple<TestConfiguration, ConcurrentTestConfiguration>(config, concurrentConfig);
        }
    }
}
