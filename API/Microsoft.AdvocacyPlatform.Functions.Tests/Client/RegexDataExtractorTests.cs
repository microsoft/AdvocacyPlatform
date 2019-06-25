// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AdvocacyPlatform.Functions.Module;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Module;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for RegexDataExtractor.
    /// </summary>
    [TestClass]
    public class RegexDataExtractorTests
    {
        private static MockLogger _log = new MockLogger();
        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new RegexDataExtractorModule())
            .Build();

        /// <summary>
        /// Gets the mock logger instance.
        /// </summary>
        public static MockLogger Log => _log;

        /// <summary>
        /// Gets the dependency injection container.
        /// </summary>
        public static IServiceProvider Container => _container;

        /// <summary>
        /// Tests success for ExtractDateTime.
        /// </summary>
        [TestMethod]
        public void ExtractDateTimeSuccess()
        {
            List<Tuple<string, DateInfo>> inputsAndResults = new List<Tuple<string, DateInfo>>()
            {
                new Tuple<string, DateInfo>("your next Master hearing date January 19th 2018 at 3 p.m. for Gymboree", new DateInfo(2018, 1, 19, 15, 0)),
                new Tuple<string, DateInfo>("twenty third street january seventh at ate a.m.", new DateInfo(2019, 1, 7, 8, 0)),
                new Tuple<string, DateInfo>("march avenue february two thousand and twenty at nine a.m.", new DateInfo(2020, 2, 1, 9, 0)),
                new Tuple<string, DateInfo>("april thirteen two thousand and nineteen at four thirty AM", new DateInfo(2019, 4, 13, 4, 30)),
                new Tuple<string, DateInfo>("judge may smith at address involving march and 23rd st on May 10th 2018 at 3 p.m.", new DateInfo(2018, 5, 10, 15, 0)),
            };

            RegexDataExtractor dataExtractor = Container.GetService<IDataExtractor>() as RegexDataExtractor;

            foreach (Tuple<string, DateInfo> inputAndResult in inputsAndResults)
            {
                Console.WriteLine($"TEST TRANSCRIPT: {inputAndResult.Item1}");

                List<DateInfo> results = dataExtractor.ExtractDateTimes(inputAndResult.Item1, Log);

                DateInfo result = results.First();

                Assert.AreEqual(inputAndResult.Item2.Year, result.Year);
                Assert.AreEqual(inputAndResult.Item2.Month, result.Month);
                Assert.AreEqual(inputAndResult.Item2.Day, result.Day);
                Assert.AreEqual(inputAndResult.Item2.Hour, result.Hour);
                Assert.AreEqual(inputAndResult.Item2.Minute, result.Minute);
            }
        }

        /// <summary>
        /// Tests success for ExtractDateTime with example transcripts.
        /// </summary>
        [TestMethod]
        public void ExtractDateTimeRealSuccess()
        {
            List<Tuple<string, DateInfo>> inputsAndResults = new List<Tuple<string, DateInfo>>()
            {
                new Tuple<string, DateInfo>("your next Master hearing date January 19th 2018 at 3 p.m. for Gymboree", new DateInfo(2018, 1, 19, 15, 0)),
                new Tuple<string, DateInfo>("twenty third street january seventh at ate a.m.", new DateInfo(2019, 1, 7, 8, 0)),
                new Tuple<string, DateInfo>("march avenue february two thousand and twenty at nine a.m.", new DateInfo(2020, 2, 1, 9, 0)),
                new Tuple<string, DateInfo>("april thirteen two thousand and nineteen at four thirty AM", new DateInfo(2019, 4, 13, 4, 30)),
                new Tuple<string, DateInfo>("judge may smith at address involving march and 23rd st on May 10th 2018 at 3 p.m.", new DateInfo(2018, 5, 10, 15, 0)),
                new Tuple<string, DateInfo>("The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021.At one PM before Judge Jeremiah Johnson at 100, Montgomery St Suite.Eight hundred San Francisco CA. 104.For your next hearing date press one.For case processing information press 2. 4 decision information press 3.For case appeal information press 4.Or filing information press 5.", new DateInfo(2021, 12, 13, 13, 0)),
            };

            RegexDataExtractor dataExtractor = Container.GetService<IDataExtractor>() as RegexDataExtractor;

            foreach (Tuple<string, DateInfo> inputAndResult in inputsAndResults)
            {
                Console.WriteLine($"TEST TRANSCRIPT: {inputAndResult.Item1}");

                List<DateInfo> results = dataExtractor.ExtractDateTimes(inputAndResult.Item1, Log);

                DateInfo result = results.First();

                Assert.AreEqual(inputAndResult.Item2.Year, result.Year);
                Assert.AreEqual(inputAndResult.Item2.Month, result.Month);
                Assert.AreEqual(inputAndResult.Item2.Day, result.Day);
                Assert.AreEqual(inputAndResult.Item2.Hour, result.Hour);
                Assert.AreEqual(inputAndResult.Item2.Minute, result.Minute);
            }
        }
    }
}
