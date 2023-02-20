/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Globalization;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class OddsFeedConfigurationInternalTests
    {
        private readonly TestSection _testSection;
        private readonly BookmakerDetailsProvider _defaultBookmakerDetailsProvider;
        private const string NonGlobalReplayMqHost = "replaymq.betradar.com";

        public OddsFeedConfigurationInternalTests()
        {
            _testSection = TestSection.Create();
            var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                new TestDataFetcher(),
                new Deserializer<bookmaker_details>(),
                new BookmakerDetailsMapperFactory());
            bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestConfigurationInternal.GetBookmakerDetails());
            _defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;
        }

        // [Fact]
        // [ExpectedException(typeof(InvalidOperationException))]
        // public void exception_is_thrown_if_replay_is_enabled_once_api_config_is_loaded()
        // {
        //     var publicConfig = new TokenSetter(new TestSectionProvider(null))
        //         .SetAccessToken(TestData.AccessToken)
        //         .SelectIntegration()
        //         .Build();
        //
        //     var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
        //     internalConfig.Load();
        //     internalConfig.EnableReplayServer();
        // }
        //
        // [Fact]
        // [ExpectedException(typeof(InvalidOperationException))]
        // public void exception_is_if_api_config_is_loaded_more_than_once()
        // {
        //     var publicConfig = new TokenSetter(new TestSectionProvider(null))
        //         .SetAccessToken(TestData.AccessToken)
        //         .SelectIntegration()
        //         .Build();
        //
        //     var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
        //     internalConfig.Load();
        //     internalConfig.Load();
        // }

        [Fact]
        public void ValuesForwardedFromPublicConfigHaveCorrectValues()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(_testSection))
                .SetAccessTokenFromConfigFile()
                .SelectCustom()
                .LoadFromConfigFile()
                .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(publicConfig.AccessToken, internalConfig.AccessToken);
            Assert.Equal(publicConfig.InactivitySeconds, internalConfig.InactivitySeconds);
            Assert.Equal(publicConfig.DefaultLocale, internalConfig.DefaultLocale);
            Assert.True(publicConfig.Locales.SequenceEqual(internalConfig.Locales));
            if (publicConfig.DisabledProducers == null)
            {
                Assert.Null(internalConfig.DisabledProducers);
            }
            else
            {
                Assert.True(publicConfig.DisabledProducers.SequenceEqual(internalConfig.DisabledProducers));
            }
            Assert.Equal(publicConfig.MaxRecoveryTime, internalConfig.MaxRecoveryTime);
            Assert.Equal(publicConfig.NodeId, internalConfig.NodeId);
            // Environment is tested separately
            Assert.Equal(publicConfig.ExceptionHandlingStrategy, internalConfig.ExceptionHandlingStrategy);
            // Host is tested separately
            // VirtualHost is tested separately
            Assert.Equal(publicConfig.Username, internalConfig.Username);
            Assert.Equal(publicConfig.Password, internalConfig.Password);
            Assert.Equal(publicConfig.Port, internalConfig.Port);
            Assert.Equal(publicConfig.UseSsl, internalConfig.UseSsl);
            // ApiHost is tested separately
            Assert.Equal(publicConfig.UseApiSsl, internalConfig.UseApiSsl);
            Assert.Equal(internalConfig.UseApiSsl ? "https://" + internalConfig.ApiHost : "http://" + internalConfig.ApiHost, internalConfig.ApiBaseUri);
            Assert.Equal("api.localhost.com/v1/replay", internalConfig.ReplayApiHost);
        }

        [Fact]
        public void EnvironmentHasCorrectValue()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.Equal(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectProduction()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.Equal(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectReplay()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.Equal(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(SdkEnvironment.Replay, internalConfig.Environment);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectCustom()
                          .SetDefaultLanguage(TestData.Culture)
                          .SetMessagingHost(_testSection.Host)
                          .SetApiHost(_testSection.ApiHost)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.Equal(publicConfig.Environment, internalConfig.Environment);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(SdkEnvironment.Replay, internalConfig.Environment);
        }

        [Fact]
        public void ExceptionIsThrownIfReplayIsEnabledOnceApiConfigIsLoaded()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                               .SetAccessToken(TestData.AccessToken)
                               .SelectIntegration()
                               .SetDefaultLanguage(new CultureInfo(_testSection.DefaultLanguage))
                               .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();

            var ex = Assert.Throws<InvalidOperationException>(() => internalConfig.EnableReplayServer());
            Assert.Equal("Replay server cannot be enabled once the API configuration is loaded.", ex.Message);
        }

        [Fact]
        public void ExceptionIsIfApiConfigIsLoadedMoreThanOnce()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                               .SetAccessToken(TestData.AccessToken)
                               .SelectIntegration()
                               .SetDefaultLanguage(new CultureInfo(_testSection.DefaultLanguage))
                               .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();

            var ex = Assert.Throws<InvalidOperationException>(() => internalConfig.Load());
            Assert.Equal("The API configuration is already loaded", ex.Message);
        }

        [Fact]
        public void MqHostHasCorrectValue()
        {
            var builder = new TokenSetter(new TestSectionProvider(_testSection)).SetAccessTokenFromConfigFile();

            var publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(publicConfig.Host, internalConfig.Host);

            publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            Assert.Equal(publicConfig.Host, internalConfig.Host);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), internalConfig.Host);

            publicConfig = builder.SelectProduction().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(publicConfig.Host, internalConfig.Host);

            publicConfig = builder.SelectReplay().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), internalConfig.Host);

            publicConfig = builder.SelectIntegration().SetDefaultLanguage(TestData.Culture).Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(publicConfig.Host, internalConfig.Host);
        }

        [Fact]
        public void VirtualHostHasCorrectValue()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();

            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.Load();
            Assert.Equal(TestConfigurationInternal.GetBookmakerDetails().VirtualHost, internalConfig.VirtualHost);

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
            Assert.Equal(publicConfig.VirtualHost, internalConfig.VirtualHost);
        }

        [Fact]
        public void ApiHostHasCorrectValue()
        {
            var publicConfig = new TokenSetter(new TestSectionProvider(null))
                              .SetAccessToken(TestData.AccessToken)
                              .SelectIntegration()
                              .SetDefaultLanguage(TestData.Culture)
                              .Build();
            var internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectProduction()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

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
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

            publicConfig = new TokenSetter(new TestSectionProvider(null))
                          .SetAccessToken(TestData.AccessToken)
                          .SelectReplay()
                          .SetDefaultLanguage(TestData.Culture)
                          .Build();
            internalConfig = new OddsFeedConfigurationInternal(publicConfig, _defaultBookmakerDetailsProvider);
            internalConfig.EnableReplayServer();
            internalConfig.Load();
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), internalConfig.ApiHost);

        }

        [Fact]
        public void ProductionApiHostIsSelectedWhenAccessToIntegrationDoesNotWork()
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
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), internalConfig.ApiHost);
        }

        [Fact]
        public void ProductionApiHostIsSelectedOnUseReplayWhenAccessToIntegrationDoesNotWork()
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
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), internalConfig.ApiHost);
        }

        [Fact]
        public void ShouldNotUseReplayMqServerInNonReplayEnvironment()
        {
            var publicConfig = new Mock<IOddsFeedConfiguration>();
            publicConfig.Setup(c => c.Environment).Returns(SdkEnvironment.Production);

            var config = new OddsFeedConfigurationInternal(publicConfig.Object, MockBookmakerProvider().Object);

            Assert.NotEqual(NonGlobalReplayMqHost, config.Host);
            Assert.Contains("UseReplayServer=False", config.ToString());
        }

        [Fact]
        public void ReplayEnvironmentShouldUseReplayMqServer()
        {
            var publicConfig = new Mock<IOddsFeedConfiguration>();
            publicConfig.Setup(c => c.Environment).Returns(SdkEnvironment.Replay);

            var config = new OddsFeedConfigurationInternal(publicConfig.Object, MockBookmakerProvider().Object);

            Assert.Equal(NonGlobalReplayMqHost, config.Host);
            Assert.Contains("UseReplayServer=True", config.ToString());
        }

        [Fact]
        public void EnabledReplayInNonReplayEnvironmentShouldSwitchToUsingReplayMqServer()
        {
            var publicConfig = new Mock<IOddsFeedConfiguration>();
            publicConfig.Setup(c => c.Environment).Returns(SdkEnvironment.Production);
            var config = new OddsFeedConfigurationInternal(publicConfig.Object, MockBookmakerProvider().Object);

            config.EnableReplayServer();

            Assert.Equal(NonGlobalReplayMqHost, config.Host);
            Assert.Contains("UseReplayServer=True", config.ToString());
        }

        private Mock<BookmakerDetailsProvider> MockBookmakerProvider()
        {
            return new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                            new TestDataFetcher(),
                                                                            new Deserializer<bookmaker_details>(),
                                                                            new BookmakerDetailsMapperFactory());
        }
    }
}
