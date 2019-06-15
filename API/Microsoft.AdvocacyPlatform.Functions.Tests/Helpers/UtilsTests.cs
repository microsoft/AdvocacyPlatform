// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for Utils.
    /// </summary>
    [TestClass]
    public class UtilsTests
    {
        /// <summary>
        /// Tests successful HourWithMinuteToTime() calls.
        /// </summary>
        [TestMethod]
        public void HourWithMinuteToTimeSuccess()
        {
            string result = Utils.HourWithMinuteToTime("four fifteen");

            Assert.AreEqual("4:15", result);

            result = Utils.HourWithMinuteToTime("six thirty");

            Assert.AreEqual("6:30", result);

            result = Utils.HourWithMinuteToTime("one forty five");

            Assert.AreEqual("1:45", result);
        }

        /// <summary>
        /// Tests successful OrdinalsToOrdinals() calls.
        /// </summary>
        [TestMethod]
        public void OrdinalsToOrdinalsSuccess()
        {
            string result = Utils.OrdinalsToOrdinals("first");

            Assert.AreEqual("1st", result);

            result = Utils.OrdinalsToOrdinals("third");

            Assert.AreEqual("3rd", result);

            result = Utils.OrdinalsToOrdinals("fourth");

            Assert.AreEqual("4th", result);

            result = Utils.OrdinalsToOrdinals("eighteenth");

            Assert.AreEqual("18th", result);

            result = Utils.OrdinalsToOrdinals("twenty second");

            Assert.AreEqual("22nd", result);

            result = Utils.OrdinalsToOrdinals("twenty third");

            Assert.AreEqual("23rd", result);

            result = Utils.OrdinalsToOrdinals("thirty first");

            Assert.AreEqual("31st", result);
        }

        /// <summary>
        /// Tests successful ReplaceHomonyms() calls.
        /// </summary>
        [TestMethod]
        public void ReplaceHomonymsSuccess()
        {
            string result = Utils.ReplaceHomonyms("won");

            Assert.AreEqual("one", result);

            result = Utils.ReplaceHomonyms("too");

            Assert.AreEqual("two", result);

            result = Utils.ReplaceHomonyms("to");

            Assert.AreEqual("two", result);

            result = Utils.ReplaceHomonyms("tree");

            Assert.AreEqual("three", result);

            result = Utils.ReplaceHomonyms("for");

            Assert.AreEqual("four", result);

            result = Utils.ReplaceHomonyms("ate");

            Assert.AreEqual("eight", result);

            result = Utils.ReplaceHomonyms("fort");

            Assert.AreEqual("fourth", result);

            result = Utils.ReplaceHomonyms("forth");

            Assert.AreEqual("fourth", result);

            result = Utils.ReplaceHomonyms("fit");

            Assert.AreEqual("fifth", result);

            result = Utils.ReplaceHomonyms("tent");

            Assert.AreEqual("tenth", result);
        }

        /// <summary>
        /// Tests a successful WordnumsToNums() calls.
        /// </summary>
        [TestMethod]
        public void WordnumsToNums()
        {
            string result = Utils.WordnumsToNums("thirty one");

            Assert.AreEqual("31", result);

            result = Utils.WordnumsToNums("thirty");

            Assert.AreEqual("30", result);

            result = Utils.WordnumsToNums("twenty eight");

            Assert.AreEqual("28", result);

            result = Utils.WordnumsToNums("nineteen");

            Assert.AreEqual("19", result);

            result = Utils.WordnumsToNums("eight teen");

            Assert.AreEqual("18", result);

            result = Utils.WordnumsToNums("twelve");

            Assert.AreEqual("12", result);

            result = Utils.WordnumsToNums("eleven");

            Assert.AreEqual("11", result);

            result = Utils.WordnumsToNums("seven");

            Assert.AreEqual("7", result);

            result = Utils.WordnumsToNums("one");

            Assert.AreEqual("1", result);

            result = Utils.WordnumsToNums("zero");

            Assert.AreEqual("0", result);

            result = Utils.WordnumsToNums("oh");

            Assert.AreEqual("0", result);
        }

        /// <summary>
        /// Tests a successful YearsToDigits() calls.
        /// </summary>
        [TestMethod]
        public void YearsToDigits()
        {
            string result = Utils.YearsToDigits("two thousand seventeen");

            Assert.AreEqual(", 2017", result);

            result = Utils.YearsToDigits("two thousand and seventeen");

            Assert.AreEqual(", 2017", result);

            result = Utils.YearsToDigits("two thousand and nineteen");

            Assert.AreEqual(", 2019", result);

            result = Utils.YearsToDigits("two thousand nineteen");

            Assert.AreEqual(", 2019", result);
        }
    }
}
