// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for helper classes.
    /// </summary>
    [TestClass]
    public class HelperTests
    {
        /// <summary>
        /// Tests success for NewId().
        /// </summary>
        [TestMethod]
        public void NewIdSuccess()
        {
            string id = Helpers.NewId();
            int expectedLength = 4;

            Assert.IsNotNull(id);
            Assert.AreEqual(expectedLength, id.Length, $"Value should have a length of {expectedLength}!");
        }

        /// <summary>
        /// Tests success for NewId() with a sharedIdName.
        /// </summary>
        [TestMethod]
        public void NewIdSharedSuccess()
        {
            string expectedSharedIdName = "shared4";
            int expectedLength = 4;

            string id1 = Helpers.NewId(expectedSharedIdName);

            Assert.IsNotNull(id1);
            Assert.AreEqual(expectedLength, id1.Length, $"Value should have a length of {expectedLength}!");

            string id2 = Helpers.NewId(expectedSharedIdName);

            Assert.IsNotNull(id2);
            Assert.AreEqual(expectedLength, id2.Length, $"Value should have a length of {expectedLength}!");
            Assert.AreEqual(id1, id2, $"Values should be equal ('{id1}' != '{id2}')!");
        }

        /// <summary>
        /// Tests success for NewId() wit custom length of 6 and a sharedIdName.
        /// </summary>
        [TestMethod]
        public void NewIdSharedLength6Success()
        {
            string expectedSharedIdName = "shared6";
            int expectedLength = 6;

            string id1 = Helpers.NewId(expectedSharedIdName, expectedLength);

            Assert.IsNotNull(id1);
            Assert.AreEqual(expectedLength, id1.Length, $"Value should have a length of {expectedLength}!");

            string id2 = Helpers.NewId(expectedSharedIdName, expectedLength);

            Assert.IsNotNull(id2);
            Assert.AreEqual(expectedLength, id1.Length, $"Value should have a length of {expectedLength}!");
            Assert.AreEqual(id1, id2, $"Values should be equal ('{id1}' != '{id2}')!");
        }

        /// <summary>
        /// Tests success for NewId() for two consecutive ids.
        /// </summary>
        [TestMethod]
        public void NewIdTwoIdsSuccess()
        {
            int expectedLength = 4;

            GetUniqueIds(2, expectedLength);
        }

        /// <summary>
        /// Tests success for NewId() with five consecutive ids.
        /// </summary>
        [TestMethod]
        public void NewIdFiveIdsSuccess()
        {
            int expectedLength = 4;

            GetUniqueIds(5, expectedLength);
        }

        /// <summary>
        /// Tests success for NewId() with five consecutive ids with a custom length of 6.
        /// </summary>
        [TestMethod]
        public void NewIdFiveIdsLength6Success()
        {
            int expectedLength = 6;

            GetUniqueIds(5, expectedLength);
        }

        private HashSet<string> GetUniqueIds(int count = 2, int idLength = 4)
        {
            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < count; i++)
            {
                string newId = Helpers.NewId(idLength: idLength);

                Assert.AreEqual(idLength, newId.Length, $"Value should have a length of {idLength}!");
                Assert.IsTrue(ids.Add(newId), $"Could not add id '{newId}' which means it is not unique!");
            }

            return ids;
        }
    }
}
