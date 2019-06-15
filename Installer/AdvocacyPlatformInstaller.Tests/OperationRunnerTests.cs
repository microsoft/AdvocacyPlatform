// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the OperationRunnner class.
    /// </summary>
    [TestClass]
    public class OperationRunnerTests
    {
        private WizardPageMock _wizardPage;

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            InstallerModel dataModel = new InstallerModel(null);

            _wizardPage = new WizardPageMock(
                dataModel,
                new InstallerWizard(
                    null,
                    null,
                    dataModel,
                    true));
        }

        /// <summary>
        /// Test success of a single operation run.
        /// </summary>
        [TestMethod]
        public void OperationRunnerSingleOperationSuccess()
        {
            OperationRunner runner = new OperationRunner(
                null,
                _wizardPage);

            int execution = 0;

            runner.Operations.Enqueue(new Operation()
            {
                Name = "SingleOperation",
                OperationFunction = (context) =>
                {
                    execution++;

                    return null;
                },
                ValidateFunction = (context) =>
                {
                    return context.LastOperationStatusCode == 0;
                },
            });

            runner.RunOperations();

            Assert.AreEqual(1, execution, $"Unexpected number of operations executed ('1' != '{execution}')!");
        }

        /// <summary>
        /// Test success of two consecutive operations.
        /// </summary>
        [TestMethod]
        public void OperationRunnerTwoOperationsSuccess()
        {
            OperationRunner runner = new OperationRunner(
                null,
                _wizardPage);

            int execution = 0;

            for (int i = 0; i < 2; i++)
            {
                runner.Operations.Enqueue(new Operation()
                {
                    Name = $"Operation_{i}",
                    OperationFunction = (context) =>
                    {
                        execution++;

                        return null;
                    },
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                });
            }

            runner.RunOperations();

            Assert.AreEqual(2, execution, $"Unexpected number of operations executed ('1' != '{execution}')!");
        }

        /// <summary>
        /// Test success of five consecutive operations.
        /// </summary>
        [TestMethod]
        public void OperationRunnerFiveOperationsSuccess()
        {
            OperationRunner runner = new OperationRunner(
                null,
                _wizardPage);

            int execution = 0;

            for (int i = 0; i < 5; i++)
            {
                runner.Operations.Enqueue(new Operation()
                {
                    Name = $"Operation_{i}",
                    OperationFunction = (context) =>
                    {
                        execution++;

                        return null;
                    },
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                });
            }

            runner.RunOperations();

            Assert.AreEqual(5, execution, $"Unexpected number of operations executed ('1' != '{execution}')!");
        }

        /// <summary>
        /// Test failure of third operation in a set of five consecutive operations.
        /// </summary>
        [TestMethod]
        public void OperationRunnerFiveOperationsExceptionOnThird()
        {
            OperationRunner runner = new OperationRunner(
                null,
                _wizardPage);

            int execution = 0;
            bool handledException = false;

            for (int i = 0; i < 5; i++)
            {
                if (i == 2)
                {
                    runner.Operations.Enqueue(new Operation()
                    {
                        Name = $"Operation_{i}",
                        OperationFunction = (context) =>
                        {
                            execution++;

                            throw new Exception("Halt here!");
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            handledException = true;
                        },
                    });
                }
                else
                {
                    runner.Operations.Enqueue(new Operation()
                    {
                        Name = $"Operation_{i}",
                        OperationFunction = (context) =>
                        {
                            execution++;

                            return null;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                    });
                }
            }

            runner.RunOperations();

            Assert.AreEqual(3, execution, $"Unexpected number of operations executed ('1' != '{execution}')!");
            Assert.IsTrue(handledException, "Operation should have been passed exception thrown!");
        }

        /// <summary>
        /// Test validation failure of fourth operation in a set of five consecutive operations.
        /// </summary>
        [TestMethod]
        public void OperationRunnerFiveOperationsValidationFailOnFourth()
        {
            OperationRunner runner = new OperationRunner(
                null,
                _wizardPage);

            int execution = 0;

            for (int i = 0; i < 5; i++)
            {
                if (i == 3)
                {
                    runner.Operations.Enqueue(new Operation()
                    {
                        Name = $"Operation_{i}",
                        OperationFunction = (context) =>
                        {
                            execution++;

                            return null;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == -1;
                        },
                    });
                }
                else
                {
                    runner.Operations.Enqueue(new Operation()
                    {
                        Name = $"Operation_{i}",
                        OperationFunction = (context) =>
                        {
                            execution++;

                            return null;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                    });
                }
            }

            runner.RunOperations();

            Assert.AreEqual(4, execution, $"Unexpected number of operations executed ('1' != '{execution}')!");
        }
    }
}
