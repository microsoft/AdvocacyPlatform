// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests implemented ValueValidators.
    /// </summary>
    [TestClass]
    public class ValueValidatorTests
    {
        /// <summary>
        /// Tests a call to the AINValueValidator() for a valid AIN.
        /// </summary>
        [TestMethod]
        public void AINValueValidatorSuccess()
        {
            string expectedInputId = "555444555";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsTrue(isValid, "The validator should have reported this input as valid!");
            Assert.IsNotNull(actualAcceptedInputId, "The accepted input id should not be null!");
            Assert.AreEqual(expectedInputId, actualAcceptedInputId, $"Unexpected accepted input id ('{expectedInputId}' != '{actualAcceptedInputId}')");
        }

        /// <summary>
        /// Tests a call to the AINValueValidator() for an invalid AIN (too many digits).
        /// </summary>
        [TestMethod]
        public void AINValueValidatorFailTooManyDigits()
        {
            string expectedInputId = "5554445553333";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsFalse(isValid, "The validator should have reported this input as invalid!");
            Assert.IsNull(actualAcceptedInputId, "The accepted input id should be null!");
        }

        /// <summary>
        /// Tests a call to the AINValueValidator() for an invalid AIN (alphanumeric).
        /// </summary>
        [TestMethod]
        public void AINValueValidatorFailAlphanumeric()
        {
            string expectedInputId = "555A44555";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsFalse(isValid, "The validator should have reported this input as invalid!");
            Assert.IsNull(actualAcceptedInputId, "The accepted input id should be null!");
        }

        /// <summary>
        /// Tests a call to the AINValueValidator() for a valid AIN with dashes in the value.
        /// </summary>
        [TestMethod]
        public void AINValueValidatorDashSuccess()
        {
            string expectedInputId = "555-444-555";
            string expectedAcceptedInputId = "555444555";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsTrue(isValid, "The validator should have reported this input as valid!");
            Assert.IsNotNull(actualAcceptedInputId, "The accepted input id should not be null!");
            Assert.AreEqual(expectedAcceptedInputId, actualAcceptedInputId, $"Unexpected accepted input id ('{expectedAcceptedInputId}' != '{actualAcceptedInputId}')");
        }

        /// <summary>
        /// Tests a call to the AINValueValidator() for an invalid AIN with dashes in the value (too many digits).
        /// </summary>
        [TestMethod]
        public void AINValueValidatorDashFailTooManyDigits()
        {
            string expectedInputId = "555-444-555-555";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsFalse(isValid, "The validator should have reported this input as invalid!");
            Assert.IsNull(actualAcceptedInputId, "The accepted input id should be null!");
        }

        /// <summary>
        /// Tests a call to the AINValueValidator() for an invalid AIN with dashes in the value (alphanumeric).
        /// </summary>
        [TestMethod]
        public void AINValueValidatorDashFailAlphanumeric()
        {
            string expectedInputId = "555-444-A55";

            AINValueValidator validator = new AINValueValidator();

            string actualAcceptedInputId = null;

            bool isValid = validator.Validate(expectedInputId, out actualAcceptedInputId);

            Assert.IsFalse(isValid, "The validator should have reported this input as invalid!");
            Assert.IsNull(actualAcceptedInputId, "The accepted input id should be null!");
        }
    }
}
