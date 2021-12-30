/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using System;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class OddsFeedConfigurationInternalTests
    {
        private TestSection _testSection;
        private BookmakerDetailsProvider _defaultBookmakerDetailsProvider;

        [TestInitialize]
        public void Setup()
        {
            _testSection = TestSection.Create();
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                                  new TestDataFetcher(),
                                                                                  new Deserializer<bookmaker_details>(),
                                                                                  new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            _defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void exception_is_thrown_if_replay_is_enabled_once_api_config_is_loaded()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                .SetAccessToken(TestData.AccessToken)
                .SelectIntegration()
                .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            internalConfig.EnableReplayServer();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void exception_is_if_api_config_is_loaded_more_than_once()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                .SetAccessToken(TestData.AccessToken)
                .SelectIntegration()
                .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            internalConfig.Load();
        }

        [TestMethod]
        public void values_forwarded_from_public_config_have_correct_values()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(_testSection))
                .SetAccessTokenFromConfigFile()
                .SelectCustom()
                .LoadFromConfigFile()
                .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(publicConfig.AccessToken, internalConfig.AccessToken);
            Assert.AreEqual(publicConfig.InactivitySeconds, internalConfig.InactivitySeconds);
            Assert.AreEqual(publicConfig.DefaultLocale, internalConfig.DefaultLocale);
            Assert.IsTrue(publicConfig.Locales.SequenceEqual(internalConfig.Locales));
            if (publicConfig.DisabledProducers == null)
            {
                Assert.IsNull(internalConfig.DisabledProducers);
            }
            else
            {
                Assert.IsTrue(publicConfig.DisabledProducers.SequenceEqual(internalConfig.DisabledProducers));
            }
            Assert.AreEqual(publicConfig.MaxRecoveryTime, internalConfig.MaxRecoveryTime);
            Assert.AreEqual(publicConfig.NodeId, internalConfig.NodeId);
            // Environment is tested separately
            Assert.AreEqual(publicConfig.ExceptionHandlingStrategy, internalConfig.ExceptionHandlingStrategy);
            // Host is tested separately
            // VirtualHost is tested separately
            Assert.AreEqual(publicConfig.Username, internalConfig.Username);
            Assert.AreEqual(publicConfig.Password, internalConfig.Password);
            Assert.AreEqual(publicConfig.Port, internalConfig.Port);
            Assert.AreEqual(publicConfig.UseSsl, internalConfig.UseSsl);
            // ApiHost is tested separately
            Assert.AreEqual(publicConfig.UseApiSsl, internalConfig.UseApiSsl);
            Assert.AreEqual(internalConfig.UseApiSsl ? "https://" + internalConfig.ApiHost : "http://" + internalConfig.ApiHost, internalConfig.ApiBaseUri);
            Assert.AreEqual("api.localhost.com/v1/replay", internalConfig.ReplayApiHost);
        }

        [TestMethod]
        public void environment_has_correct_value()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.AreEqual(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectProduction()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.AreEqual(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectReplay()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.AreEqual(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectCustom()
                          .SetDefaultLanguage(TestData.Culture)
                          .SetMessagingHost(_testSection.Host)
                          .SetApiHost(_testSection.ApiHost)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.AreEqual(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(SdkEnvironment.Replay, internalConfig.Environment);
        }

        [TestMethod]
        public void host_has_correct_value()
        {
            var builder = new TokenSetter(new TestSectionProvider(_testSection)).SetAccessTokenFromConfigFile();

            var publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(publicConfig.Host, internalConfig.Host);

            publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.AreEqual(publicConfig.Host, internalConfig.Host);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), internalConfig.Host);

            publicConfig = builder.SelectProduction().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(publicConfig.Host, internalConfig.Host);

            publicConfig = builder.SelectReplay().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), internalConfig.Host);

            publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(publicConfig.Host, internalConfig.Host);
        }

        [TestMethod]
        public void virtual_host_has_correct_value()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(TestConfigurationInternal.GetBookmakerDetails().VirtualHost, internalConfig.VirtualHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectCustom()
                          .SetVirtualHost("virtual_host")
                          .SetDefaultLanguage(TestData.Culture)
                          .SetMessagingHost(_testSection.Host)
                          .SetApiHost(_testSection.ApiHost)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.AreEqual(publicConfig.VirtualHost, internalConfig.VirtualHost);
        }

        [TestMethod]
        public void api_host_has_correct_value()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectProduction()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectCustom()
                          .SetDefaultLanguage(TestData.Culture)
                          .SetMessagingHost(_testSection.Host)
                          .SetApiHost(_testSection.ApiHost)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectReplay()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

        }

        [TestMethod]
        public void production_api_host_is_selected_when_access_to_integration_does_not_work()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectReplay()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();

            var bookmakerProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                            new TestDataFetcher(),
                                                                            new Deserializer<bookmaker_details>(),
                                                                            new BookmakerDetailsMapperFactory());
            var integrationUrl = publicConfig.UseApiSsl
                ? "https://" + EnvironmentManager.GetApiHost(SdkEnvironment.Integration)
                : "http://" + EnvironmentManager.GetApiHost(SdkEnvironment.Integration);
            bookmakerProviderMock.Setup(x => x.GetData(integrationUrl)).Throws(new CommunicationException("Exception message", integrationUrl, null));

            var prodUrl = publicConfig.UseApiSsl
                ? "https://" + EnvironmentManager.GetApiHost(SdkEnvironment.Production)
                : "http://" + EnvironmentManager.GetApiHost(SdkEnvironment.Production);
            bookmakerProviderMock.Setup(x => x.GetData(prodUrl)).Returns(TestConfigurationInternal.GetBookmakerDetails());

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, bookmakerProviderMock.Object);
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Production), internalConfig.ApiHost);
        }

        [TestMethod]
        public void production_api_host_is_selected_on_use_replay_when_access_to_integration_does_not_work()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();

            var bookmakerProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                            new TestDataFetcher(),
                                                                            new Deserializer<bookmaker_details>(),
                                                                            new BookmakerDetailsMapperFactory());
            var integrationUrl = publicConfig.UseApiSsl
                ? "https://" + EnvironmentManager.GetApiHost(SdkEnvironment.Integration)
                : "http://" + EnvironmentManager.GetApiHost(SdkEnvironment.Integration);
            bookmakerProviderMock.Setup(x => x.GetData(integrationUrl)).Throws(new CommunicationException("Exception message", integrationUrl, null));

            var prodUrl = publicConfig.UseApiSsl
                ? "https://" + EnvironmentManager.GetApiHost(SdkEnvironment.Production)
                : "http://" + EnvironmentManager.GetApiHost(SdkEnvironment.Production);
            bookmakerProviderMock.Setup(x => x.GetData(prodUrl)).Returns(TestConfigurationInternal.GetBookmakerDetails());

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, bookmakerProviderMock.Object);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.AreEqual(EnvironmentManager.GetApiHost(SdkEnvironment.Production), internalConfig.ApiHost);
        }
    }
}
