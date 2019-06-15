// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AdvocacyPlatform.Functions.Module;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Module;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Twilio.Rest.Api.V2010.Account;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Tests for TwilioCallWrapper.
    /// </summary>
    [TestClass]
    public class TwilioCallWrapperTests
    {
        /// <summary>
        /// The base URL for making calls with the Twilio Markup Language.
        /// </summary>
        public const string TwiMLBaseUrl = "http://twimlets.com/echo?Twiml=";

        /// <summary>
        /// The test account SID to use.
        /// </summary>
        public const string TestAccountSid = "ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        /// <summary>
        /// The name of the secret containing the Twilio local number to use to make calls.
        /// </summary>
        public const string TwilioLocalNumberSecretNameAppSettingName = "twilioLocalNumberSecretName";

        /// <summary>
        /// The name of the secret containing the Twilio account SID.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// The name of the secret containing the Twilio auth token.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// The name of the application setting specifying the authorization authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// The name of the application setting specifying the Twilio ML template.
        /// </summary>
        public const string TwiMLTemplateAppSettingName = "twilioTwiMLTemplate";

        /// <summary>
        /// Path to the template file for call responses.
        /// </summary>
        public const string CallResponseTemplatePath = @".\Templates\Response\Twilio\CallResponseTemplate.json";

        /// <summary>
        /// Path to the template file for a composite recording response.
        /// </summary>
        public const string RecordingCompositeTemplatePath = @".\Templates\Response\Twilio\Composite\RecordingCompositeTemplate.json";

        /// <summary>
        /// Key to replace in template with a composite recording response.
        /// </summary>
        public const string RecordingCompositesKey = "recordingsComposite";

        /// <summary>
        /// Path to the template file for a recording response.
        /// </summary>
        public const string RecordingResponseTemplatePath = @".\Templates\Response\Twilio\RecordingResponseTemplate.json";

        private static MockLogger _log = new MockLogger();

        private IConfiguration _config;
        private IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientMockModule())
            .RegisterModule(new AzureKeyVaultSecretStoreMockModule())
            .RegisterModule(new TwilioModule())
            .Build();

        /// <summary>
        /// Gets the mock logger instance.
        /// </summary>
        public static MockLogger Log => _log;

        /// <summary>
        /// Gets the dependency injection container.
        /// </summary>
        public IServiceProvider Container => _container;

        /// <summary>
        /// Registers an expected response message for a URI.
        /// </summary>
        /// <param name="requestUri">The expected URI.</param>
        /// <param name="response">The expected response.</param>
        public void RegisterExpectedUriResponse(string requestUri, HttpResponseMessage response)
        {
            HttpClientMock client = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            client.RegisterExpectedResponseMessage(requestUri, response);
        }

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            _config = TestHelper.GetConfiguration();
        }

        /// <summary>
        /// Initializes mock secret store.
        /// </summary>
        public void InitializeSecrets()
        {
            AzureKeyVaultSecretStoreMock secretStore = Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedTwilioAccountSidSecret = new Secret(_config[TwilioAccountSidSecretNameAppSettingName], TestHelper.CreateSecureString("mock-account-sid"));

            secretStore.RegisterExpectedSecret(expectedTwilioAccountSidSecret);

            Secret expectedTwilioAuthTokenSecret = new Secret(_config[TwilioAuthTokenSecretNameAppSettingName], TestHelper.CreateSecureString("mock-auth-token"));

            secretStore.RegisterExpectedSecret(expectedTwilioAuthTokenSecret);
        }

        /// <summary>
        /// Tests a successful GetTwilioUri() call.
        /// </summary>
        [TestMethod]
        public void GetTwilioUriSuccess()
        {
            string uriFromRecording = "/recording/234234234.mp3";
            string expectedResult = $"https://api.twilio.com{uriFromRecording}";

            InitializeSecrets();

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            Assert.AreEqual(expectedResult, callWrapper.GetTwilioUri(uriFromRecording).AbsoluteUri, false);
        }

        /// <summary>
        /// Tests a successful PlaceAndRecordCallAsync() call.
        /// </summary>
        [TestMethod]
        public void PlaceAndRecordCallAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "in-progress";
            string numberToCall = "5555555";
            string twilioLocalNumber = "5555656";

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(numberToCall, "call", expectedResponse);

            Uri twiMLUrl = FormatTwiMLUrl(_config[TwiMLTemplateAppSettingName], new Dictionary<string, string>());

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            string callSid = callWrapper.PlaceAndRecordCallAsync(
                twiMLUrl,
                numberToCall,
                twilioLocalNumber,
                Log).Result;

            Assert.AreEqual(expectedCallSid, callSid, false);
        }

        /// <summary>
        /// Tests a successful FetchCallAsync() call.
        /// </summary>
        [TestMethod]
        public void FetchCallAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000002";
            string expectedStatusText = "in-progress";

            CallResource.StatusEnum expectedStatus = CallResource.StatusEnum.InProgress;

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatusText },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            CallResource call = callWrapper.FetchCallAsync(expectedCallSid, Log).Result;

            Assert.IsNotNull(call);
            Assert.AreEqual(expectedCallSid, call.Sid);
            Assert.AreEqual(expectedStatus, call.Status);
        }

        /// <summary>
        /// Tests a successful FetchStatusAsync() call.
        /// </summary>
        [TestMethod]
        public void FetchStatusAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000003";
            string expectedStatusText = "in-progress";

            CallResource.StatusEnum expectedStatus = CallResource.StatusEnum.InProgress;

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatusText },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            CallResource.StatusEnum status = callWrapper.FetchStatusAsync(expectedCallSid, Log).Result;

            Assert.AreEqual(expectedStatus, status);
        }

        /// <summary>
        /// Tests a successful HangupCallAsync() call.
        /// </summary>
        [TestMethod]
        public void HangupCallAsyncCompleted()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000004";
            string expectedStatusText = "completed";

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatusText },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            bool success = callWrapper.HangupCallAsync(expectedCallSid, Log).Result;

            Assert.AreEqual(false, success);
        }

        /// <summary>
        /// Tests a successful HangupCallAsync() call for a call in-progress.
        /// </summary>
        [TestMethod]
        public void HangupCallAsyncInProgress()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000005";
            string expectedStatusText = "in-progress";

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatusText },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            bool success = callWrapper.HangupCallAsync(expectedCallSid, Log).Result;

            Assert.AreEqual(true, success);
        }

        /// <summary>
        /// Tests a successful HangupCallAsync() call for queued call.
        /// </summary>
        [TestMethod]
        public void HangupCallAsyncQueued()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000006";
            string expectedStatusText = "queued";

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatusText },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            bool success = callWrapper.HangupCallAsync(expectedCallSid, Log).Result;

            Assert.AreEqual(true, success);
        }

        /// <summary>
        /// Tests a successful FetchRecordingsAsync() call.
        /// </summary>
        [TestMethod]
        public void FetchRecordingsAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000007";
            string[] recordingSids =
            {
                "RE10000000000000000000000000000001",
                "RE10000000000000000000000000000002",
            };
            string[] recordingUris =
            {
                "https://storage/recordings/234234/file_2015_10_09_10_11_12.wav",
                "https://storage/recordings/212234/file_2016_09_02_01_00_13.wav",
            };

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                RecordingResponseTemplatePath,
                RecordingCompositesKey,
                RecordingCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "pageSize", "2" },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "accountSid", TestAccountSid },
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[0] },
                        { "recordingUri", recordingUris[0] },
                    },
                    new Dictionary<string, string>()
                    {
                        { "accountSid", TestAccountSid },
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[1] },
                        { "recordingUri", recordingUris[1] },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            IList<RecordingResource> recordings = callWrapper.FetchRecordingsAsync(expectedCallSid, Log).Result;

            Assert.AreEqual(2, recordings.Count);

            for (int index = 0; index < recordingSids.Length; index++)
            {
                Assert.AreEqual(TestAccountSid, recordings[index].AccountSid);
                Assert.AreEqual(expectedCallSid, recordings[index].CallSid);
                Assert.AreEqual(recordingSids[index], recordings[index].Sid);
                Assert.AreEqual(recordingUris[index], recordings[index].Uri);
            }
        }

        /// <summary>
        /// Tests a successful DeleteCallAsync() call.
        /// </summary>
        [TestMethod]
        public void DeleteCallAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000008";

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "delete", expectedResponse);

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            bool success = callWrapper.DeleteCallAsync(expectedCallSid, Log).Result;

            Assert.IsTrue(success);
        }

        /// <summary>
        /// Tests a successful DeleteRecordingsAsync() call.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsAsyncSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000009";
            string[] recordingSids =
            {
                "RE10000000000000000000000000000003",
                "RE10000000000000000000000000000004",
            };
            string[] recordingUris =
            {
                "https://storage/recordings/234234/file_2015_10_09_10_11_12.wav",
                "https://storage/recordings/212234/file_2016_09_02_01_00_13.wav",
            };

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                RecordingResponseTemplatePath,
                RecordingCompositesKey,
                RecordingCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "pageSize", "2" },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "accountSid", TestAccountSid },
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[0] },
                        { "recordingUri", recordingUris[0] },
                    },
                    new Dictionary<string, string>()
                    {
                        { "accountSid", TestAccountSid },
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[1] },
                        { "recordingUri", recordingUris[1] },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty);
            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedRecordingsResponse);

            foreach (string recordingSid in recordingSids)
            {
                httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSid, "delete", expectedResponse);
            }

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            callWrapper.InitializeAsync(
                _config[TwilioAccountSidSecretNameAppSettingName],
                _config[TwilioAuthTokenSecretNameAppSettingName],
                _config[AuthorityAppSettingName]).Wait();

            bool success = callWrapper.DeleteRecordingsAsync(expectedCallSid, Log).Result;

            Assert.IsTrue(success);
        }

        /// <summary>
        /// Tests a successful GetFullRecordingUri() call.
        /// </summary>
        [TestMethod]
        public void GetFullRecordingUriSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000010";
            string expectedRecordingSid = "RE10000000000000000000000000000005";
            string expectedRecordingRelativeUri = $"/2010-04-01/Accounts/ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX/Recordings/{expectedRecordingSid}";
            string expectedRecordingUri = $"{TwilioCallWrapper.TwilioUriBase}{expectedRecordingRelativeUri}";

            RecordingResource recording = RecordingResource.FromJson(
                TemplateManager.Load(RecordingCompositeTemplatePath, new Dictionary<string, string>()
                {
                    { "accountSid", TestAccountSid },
                    { "callSid", expectedCallSid },
                    { "recordingSid", expectedRecordingSid },
                    { "recordingUri", expectedRecordingRelativeUri },
                }));

            TwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapper;

            Uri recordingUri = callWrapper.GetFullRecordingUri(recording, Log);

            Assert.IsNotNull(recordingUri);
            Assert.AreEqual(expectedRecordingUri, recordingUri.AbsoluteUri);
        }

        private static Uri FormatTwiMLUrl(string twiMLTemplate, Dictionary<string, string> parameters)
        {
            StringBuilder twiMLUrl = new StringBuilder(twiMLTemplate);

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                twiMLUrl = twiMLUrl.Replace($"{{{parameter.Key}}}", parameter.Value);
            }

            string encodeTwiMLUrl = HttpUtility.UrlEncode(twiMLUrl.ToString());

            return new Uri($"{TwiMLBaseUrl}{encodeTwiMLUrl}");
        }
    }
}
