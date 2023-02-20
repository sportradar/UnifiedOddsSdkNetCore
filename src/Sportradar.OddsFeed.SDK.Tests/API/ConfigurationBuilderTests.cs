/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class ConfigurationBuilderTests
    {
        private readonly TestSection _testSection;

        public ConfigurationBuilderTests()
        {
            _testSection = TestSection.Create();
        }

        private static List<int> GetIntList(string value)
        {
            return value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        }

        private static List<CultureInfo> GetCultureList(string cultureNames)
        {
            return cultureNames.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(name => new CultureInfo(name)).ToList();
        }

        private static IConfigurationBuilder IntegrationBuilder(IOddsFeedConfigurationSection section)
        {
            return new TokenSetter(new TestSectionProvider(section))
                .SetAccessTokenFromConfigFile()
                .SelectIntegration()
                .LoadFromConfigFile();
        }

        private static IConfigurationBuilder IntegrationBuilder(string token)
        {
            return new TokenSetter(new TestSectionProvider(null))
                .SetAccessToken(token)
                .SelectIntegration();
        }

        private static IConfigurationBuilder ProductionBuilder(IOddsFeedConfigurationSection section)
        {
            return new TokenSetter(new TestSectionProvider(section))
                .SetAccessTokenFromConfigFile()
                .SelectProduction()
                .LoadFromConfigFile();
        }

        private static IConfigurationBuilder ProductionBuilder(string token)
        {
            return new TokenSetter(new TestSectionProvider(null))
                .SetAccessToken(token)
                .SelectProduction();
        }

        private static IReplayConfigurationBuilder ReplayBuilder(IOddsFeedConfigurationSection section)
        {
            return new TokenSetter(new TestSectionProvider(section))
                .SetAccessTokenFromConfigFile()
                .SelectReplay()
                .LoadFromConfigFile();
        }

        private static IReplayConfigurationBuilder ReplayBuilder(string token)
        {
            return new TokenSetter(new TestSectionProvider(null))
                .SetAccessToken(token)
                .SelectReplay();
        }

        private static ICustomConfigurationBuilder CustomBuilder(IOddsFeedConfigurationSection section)
        {
            return new TokenSetter(new TestSectionProvider(section))
                .SetAccessTokenFromConfigFile()
                .SelectCustom()
                .LoadFromConfigFile();
        }

        private static ICustomConfigurationBuilder CustomBuilder(string token)
        {
            return new TokenSetter(new TestSectionProvider(null))
                  .SetAccessToken(token)
                  .SelectCustom();
        }

        [Fact]
        public void AccessTokenHasCorrectValue()
        {
            _testSection.AccessToken = "my_token";
            Assert.Equal(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().AccessToken);
            Assert.Equal(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().AccessToken);
            Assert.Equal(_testSection.AccessToken, ProductionBuilder(_testSection).Build().AccessToken);
            Assert.Equal(_testSection.AccessToken, ReplayBuilder(_testSection).Build().AccessToken);
            Assert.Equal(_testSection.AccessToken, CustomBuilder(_testSection).Build().AccessToken);

            Assert.Equal("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.Equal("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.Equal("token", ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.Equal("token", ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.Equal("token", CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AccessToken);
        }

        [Fact]
        public void InactivitySecondsHasCorrectValue()
        {
            _testSection.InactivitySeconds = 100;
            Assert.Equal(_testSection.InactivitySeconds, IntegrationBuilder(_testSection).Build().InactivitySeconds);
            Assert.Equal(_testSection.InactivitySeconds, IntegrationBuilder(_testSection).Build().InactivitySeconds);
            Assert.Equal(_testSection.InactivitySeconds, ProductionBuilder(_testSection).Build().InactivitySeconds);
            Assert.Equal(SdkInfo.MaxInactivitySeconds, ReplayBuilder(_testSection).Build().InactivitySeconds);
            Assert.Equal(_testSection.InactivitySeconds, CustomBuilder(_testSection).Build().InactivitySeconds);

            Assert.Equal(80, IntegrationBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.Equal(80, IntegrationBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.Equal(80, ProductionBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.Equal(80, CustomBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);

            Assert.Equal(SdkInfo.MinInactivitySeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.Equal(SdkInfo.MinInactivitySeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.Equal(SdkInfo.MinInactivitySeconds, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.Equal(SdkInfo.MaxInactivitySeconds, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.Equal(SdkInfo.MinInactivitySeconds, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().InactivitySeconds);
        }

        [Fact]
        public void DefaultLocaleHasCorrectValue()
        {
            _testSection.DefaultLanguage = "de";
            var cultureEn = new CultureInfo("en");
            var cultureDe = new CultureInfo("de");

            Assert.Equal(cultureDe, IntegrationBuilder(_testSection).Build().DefaultLocale);
            Assert.Equal(cultureDe, IntegrationBuilder(_testSection).Build().DefaultLocale);
            Assert.Equal(cultureDe, ProductionBuilder(_testSection).Build().DefaultLocale);
            Assert.Equal(cultureDe, ReplayBuilder(_testSection).Build().DefaultLocale);
            Assert.Equal(cultureDe, CustomBuilder(_testSection).Build().DefaultLocale);

            Assert.Equal(cultureEn, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.Equal(cultureEn, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.Equal(cultureEn, ProductionBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.Equal(cultureEn, ReplayBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.Equal(cultureEn, CustomBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);

            Assert.Equal(cultureEn, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.Equal(cultureEn, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.Equal(cultureEn, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.Equal(cultureEn, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.Equal(cultureEn, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DefaultLocale);

            Assert.Equal(cultureDe, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.Equal(cultureDe, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.Equal(cultureDe, ProductionBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.Equal(cultureDe, ReplayBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.Equal(cultureDe, CustomBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
        }

        [Fact]
        public void LocalesHasCorrectValue()
        {
            _testSection.DefaultLanguage = "it";
            _testSection.SupportedLanguages = "it,de,en";

            Assert.True(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            Assert.True(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(IntegrationBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.True(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(IntegrationBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.True(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(ProductionBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.True(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(ReplayBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.True(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(CustomBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));

            _testSection.DefaultLanguage = "sl";
            var langString = "sl," + _testSection.SupportedLanguages;
            Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            _testSection.DefaultLanguage = "de";
            langString = "de," + _testSection.SupportedLanguages.Replace(",de", "");
            Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.True(GetCultureList(langString).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            Assert.True(GetCultureList("en").SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.True(GetCultureList("en").SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.True(GetCultureList("en").SequenceEqual(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.True(GetCultureList("en").SequenceEqual(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.True(GetCultureList("en").SequenceEqual(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Locales));
        }

        [Fact]
        public void DisabledProducersHasCorrectValue()
        {
            _testSection.DisabledProducers = "1,3";
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder(_testSection).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder(_testSection).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(ProductionBuilder(_testSection).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(ReplayBuilder(_testSection).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(CustomBuilder(_testSection).Build().DisabledProducers));

            Assert.Null(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.Null(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.Null(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.Null(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.Null(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);

            Assert.Null(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.Null(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.Null(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.Null(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.Null(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DisabledProducers);

            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.True(GetIntList(_testSection.DisabledProducers).SequenceEqual(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DisabledProducers));
        }

        [Fact]
        public void MaxRecoveryTimeHasCorrectValue()
        {
            _testSection.MaxRecoveryTime = 1000;

            Assert.Equal(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, ProductionBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, ReplayBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, CustomBuilder(_testSection).Build().MaxRecoveryTime);

            Assert.Equal(1400, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.Equal(1400, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.Equal(1400, ProductionBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.Equal(1400, CustomBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);

            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.Equal(SdkInfo.MaxRecoveryExecutionInSeconds, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().MaxRecoveryTime);

            Assert.Equal(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, ProductionBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.Equal(_testSection.MaxRecoveryTime, CustomBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
        }

        [Fact]
        public void NodeIdHasCorrectValue()
        {
            _testSection.NodeId = 15;

            Assert.Equal(_testSection.NodeId, IntegrationBuilder(_testSection).Build().NodeId);
            Assert.Equal(_testSection.NodeId, IntegrationBuilder(_testSection).Build().NodeId);
            Assert.Equal(_testSection.NodeId, ProductionBuilder(_testSection).Build().NodeId);
            Assert.Equal(_testSection.NodeId, ReplayBuilder(_testSection).Build().NodeId);
            Assert.Equal(_testSection.NodeId, CustomBuilder(_testSection).Build().NodeId);

            Assert.Equal(0, IntegrationBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.Equal(0, IntegrationBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.Equal(0, ProductionBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.Equal(0, ReplayBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.Equal(0, CustomBuilder(_testSection).SetNodeId(0).Build().NodeId);

            Assert.Equal(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.Equal(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.Equal(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.Equal(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.Equal(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);

            Assert.Equal(_testSection.NodeId, IntegrationBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.Equal(_testSection.NodeId, IntegrationBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.Equal(_testSection.NodeId, ProductionBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.Equal(_testSection.NodeId, ReplayBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.Equal(_testSection.NodeId, CustomBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        }

        [Fact]
        public void EnvironmentHasCorrectValue()
        {
            _testSection.UseIntegrationEnvironment = true;

            Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder(_testSection).Build().Environment);
            Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder(_testSection).Build().Environment);
            Assert.Equal(SdkEnvironment.Production, ProductionBuilder(_testSection).Build().Environment);
            Assert.Equal(SdkEnvironment.Replay, ReplayBuilder(_testSection).Build().Environment);
            Assert.Equal(SdkEnvironment.Custom, CustomBuilder(_testSection).Build().Environment);

            Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.Equal(SdkEnvironment.Production, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.Equal(SdkEnvironment.Replay, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.Equal(SdkEnvironment.Custom, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Environment);
        }

        [Fact]
        public void ExceptionHandlingStrategyHasCorrectValue()
        {
            _testSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;

            Assert.Equal(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, ProductionBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, ReplayBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, CustomBuilder(_testSection).Build().ExceptionHandlingStrategy);

            Assert.Equal(ExceptionHandlingStrategy.CATCH, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, ProductionBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, ReplayBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, CustomBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);

            Assert.Equal(ExceptionHandlingStrategy.CATCH, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.Equal(ExceptionHandlingStrategy.CATCH, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().ExceptionHandlingStrategy);

            Assert.Equal(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, ProductionBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, ReplayBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.Equal(_testSection.ExceptionHandlingStrategy, CustomBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        }

        [Fact]
        public void MessagingHostHasCorrectValue()
        {
            _testSection.Host = "mq.localhost.local";

            Assert.Equal(_testSection.Host, IntegrationBuilder(_testSection).Build().Host);
            Assert.Equal(_testSection.Host, IntegrationBuilder(_testSection).Build().Host);
            Assert.Equal(_testSection.Host, ProductionBuilder(_testSection).Build().Host);
            Assert.Equal(_testSection.Host, ReplayBuilder(_testSection).Build().Host);
            Assert.Equal(_testSection.Host, CustomBuilder(_testSection).Build().Host);

            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Production), ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.Equal(_testSection.Host, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Host);

            Assert.Equal("mq1.localhost.local", CustomBuilder(_testSection).SetMessagingHost("mq1.localhost.local").Build().Host);
            Assert.Equal("mq.localhost.local", CustomBuilder(_testSection).SetMessagingHost("mq1.localhost.local").LoadFromConfigFile().Build().Host);
        }

        [Fact]
        public void PortHasCorrectValue()
        {
            _testSection.Port = 2250;
            _testSection.UseSSL = true;

            Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(_testSection).Build().Port);
            Assert.Equal(_testSection.Port, CustomBuilder(_testSection).Build().Port);

            _testSection.UseSSL = false;
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(_testSection).Build().Port);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(_testSection).Build().Port);
            Assert.Equal(_testSection.Port, CustomBuilder(_testSection).Build().Port);

            _testSection.Port = 0;
            Assert.Equal(EnvironmentManager.DefaultMqHostPort + 1, CustomBuilder(_testSection).Build().Port);
            _testSection.UseSSL = true;
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, CustomBuilder(_testSection).Build().Port);
            _testSection.Port = 2250;
            Assert.Equal(_testSection.Port, CustomBuilder(_testSection).LoadFromConfigFile().Build().Port);
        }

        [Fact]
        public void UsernameHasCorrectValue()
        {
            _testSection.Username = "username";
            _testSection.AccessToken = "token";

            Assert.Equal(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().Username);
            Assert.Equal(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().Username);
            Assert.Equal(_testSection.AccessToken, ProductionBuilder(_testSection).Build().Username);
            Assert.Equal(_testSection.AccessToken, ReplayBuilder(_testSection).Build().Username);
            Assert.Equal(_testSection.Username, CustomBuilder(_testSection).Build().Username);

            Assert.Equal("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.Equal("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.Equal("token", ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.Equal("token", ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.Equal("token", CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Username);

            Assert.Equal("username", CustomBuilder(_testSection).Build().Username);
            Assert.Equal(_testSection.Username, CustomBuilder(_testSection).LoadFromConfigFile().Build().Username);
        }

        [Fact]
        public void PasswordHasCorrectValue()
        {
            Assert.Equal(_testSection.Password, CustomBuilder(_testSection).LoadFromConfigFile().Build().Password);
            Assert.Equal(_testSection.Password, CustomBuilder(_testSection).Build().Password);

            _testSection.Password = null;

            Assert.Null(IntegrationBuilder(_testSection).Build().Password);
            Assert.Null(IntegrationBuilder(_testSection).Build().Password);
            Assert.Null(ProductionBuilder(_testSection).Build().Password);
            Assert.Null(ReplayBuilder(_testSection).Build().Password);
            Assert.Equal(_testSection.Password, CustomBuilder(_testSection).Build().Password);

            Assert.Null(CustomBuilder(_testSection).Build().Password);
        }

        [Fact]
        public void VirtualHostHasCorrectValue()
        {
            _testSection.VirtualHost = null;

            Assert.Null(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.Null(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.Null(ProductionBuilder(_testSection).Build().VirtualHost);
            Assert.Null(ReplayBuilder(_testSection).Build().VirtualHost);
            Assert.Null(CustomBuilder(_testSection).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);

            Assert.Null(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.Null(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.Null(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.Null(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.Null(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);

            _testSection.VirtualHost = "my_virtual_host";
            Assert.Null(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.Null(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.Null(ProductionBuilder(_testSection).Build().VirtualHost);
            Assert.Null(ReplayBuilder(_testSection).Build().VirtualHost);
            Assert.Equal(_testSection.VirtualHost, CustomBuilder(_testSection).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);
            Assert.Equal("virtual_host", CustomBuilder(_testSection).SetVirtualHost("virtual_host").SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);
        }

        [Fact]
        public void HostWithProtocolCausesAnException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost("http://mq.localhost.local").Build());
            Assert.Contains("must not contain protocol specification", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost("https://mq.localhost.local").Build());
            Assert.Contains("must not contain protocol specification.", ex.Message);
        }

        [Fact]
        public void ApiHostWithProtocolCausesException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost("global.api.betradar.com").SetApiHost("http://api.localhost.local").Build());
            Assert.Contains("must not contain protocol specification", ex.Message);

            ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost("global.api.betradar.com").SetApiHost("https://api.localhost.local").Build());
            Assert.Contains("must not contain protocol specification.", ex.Message);
        }

        [Fact]
        public void UseMessagingSslHasCorrectValue()
        {
            _testSection.UseSSL = false;

            Assert.True(IntegrationBuilder(_testSection).Build().UseSsl);
            Assert.True(IntegrationBuilder(_testSection).Build().UseSsl);
            Assert.True(ProductionBuilder(_testSection).Build().UseSsl);
            Assert.True(ReplayBuilder(_testSection).Build().UseSsl);
            Assert.False(CustomBuilder(_testSection).Build().UseSsl);

            Assert.True(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.True(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.True(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.True(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.True(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseSsl);

            Assert.True(CustomBuilder(_testSection).UseMessagingSsl(true).Build().UseSsl);
            Assert.False(CustomBuilder(_testSection).UseMessagingSsl(true).LoadFromConfigFile().Build().UseSsl);
            Assert.False(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).UseMessagingSsl(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseSsl);
        }

        [Fact]
        public void MissingMessagingHostThrowException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetApiHost(_testSection.ApiHost).Build());
            Assert.Contains("MessagingHost is missing", ex.Message);
        }

        [Fact]
        public void MissingApiHostThrowException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.ApiHost).Build());
            Assert.Contains("ApiHost is missing", ex.Message);
        }

        [Fact]
        public void ApiHostHasCorrectValue()
        {
            _testSection.ApiHost = "api.localhost.local";

            Assert.Equal(_testSection.ApiHost, IntegrationBuilder(_testSection).Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, IntegrationBuilder(_testSection).Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, ProductionBuilder(_testSection).Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, ReplayBuilder(_testSection).Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, CustomBuilder(_testSection).Build().ApiHost);

            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Replay), ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);

            Assert.Equal(_testSection.ApiHost, CustomBuilder(_testSection).Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, CustomBuilder(_testSection).LoadFromConfigFile().Build().ApiHost);
            Assert.Equal(_testSection.ApiHost, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetApiHost(_testSection.ApiHost).SetMessagingHost(_testSection.Host).Build().ApiHost);
        }

        [Fact]
        public void UseApiSslHasCorrectValue()
        {
            _testSection.UseApiSSL = false;

            Assert.True(IntegrationBuilder(_testSection).Build().UseApiSsl);
            Assert.True(IntegrationBuilder(_testSection).Build().UseApiSsl);
            Assert.True(ProductionBuilder(_testSection).Build().UseApiSsl);
            Assert.True(ReplayBuilder(_testSection).Build().UseApiSsl);
            Assert.False(CustomBuilder(_testSection).Build().UseApiSsl);

            Assert.True(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.True(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.True(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.True(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.True(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseApiSsl);

            Assert.True(CustomBuilder(_testSection).UseApiSsl(true).Build().UseApiSsl);
            Assert.False(CustomBuilder(_testSection).UseApiSsl(true).LoadFromConfigFile().Build().UseApiSsl);
            Assert.False(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).UseApiSsl(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseApiSsl);
        }

        [Fact]
        public void EnforceMaxAfterAgeHasCorrectValue()
        {
            _testSection.AdjustAfterAge = false;

            Assert.False(IntegrationBuilder(_testSection).Build().AdjustAfterAge);
            Assert.False(IntegrationBuilder(_testSection).Build().AdjustAfterAge);
            Assert.False(ProductionBuilder(_testSection).Build().AdjustAfterAge);
            Assert.False(ReplayBuilder(_testSection).Build().AdjustAfterAge);
            Assert.False(CustomBuilder(_testSection).Build().AdjustAfterAge);

            Assert.False(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.False(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.False(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.False(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.False(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AdjustAfterAge);

            Assert.True(CustomBuilder(_testSection).SetAdjustAfterAge(true).Build().AdjustAfterAge);
            Assert.False(CustomBuilder(_testSection).SetAdjustAfterAge(true).LoadFromConfigFile().Build().AdjustAfterAge);
            Assert.True(CustomBuilder(_testSection).LoadFromConfigFile().SetAdjustAfterAge(true).Build().AdjustAfterAge);
            Assert.False(CustomBuilder(_testSection).LoadFromConfigFile().SetAdjustAfterAge(true).LoadFromConfigFile().Build().AdjustAfterAge);
            Assert.False(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetAdjustAfterAge(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AdjustAfterAge);
        }

        [Fact]
        public void HttpClientTimeoutHasCorrectValue()
        {
            _testSection.HttpClientTimeout = 50;
            Assert.Equal(_testSection.HttpClientTimeout, IntegrationBuilder(_testSection).Build().HttpClientTimeout);
            Assert.Equal(_testSection.HttpClientTimeout, ProductionBuilder(_testSection).Build().HttpClientTimeout);
            Assert.Equal(_testSection.HttpClientTimeout, ReplayBuilder(_testSection).Build().HttpClientTimeout);
            Assert.Equal(_testSection.HttpClientTimeout, CustomBuilder(_testSection).Build().HttpClientTimeout);

            Assert.Equal(80, IntegrationBuilder(_testSection).SetHttpClientTimeout(80).Build().HttpClientTimeout);
            Assert.Equal(80, ProductionBuilder(_testSection).SetHttpClientTimeout(80).Build().HttpClientTimeout);
            Assert.Equal(80, ReplayBuilder(_testSection).SetHttpClientTimeout(80).Build().HttpClientTimeout);
            Assert.Equal(80, CustomBuilder(_testSection).SetHttpClientTimeout(80).Build().HttpClientTimeout);

            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().HttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().HttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().HttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().HttpClientTimeout);
        }

        [Fact]
        public void RecoveryHttpClientTimeoutHasCorrectValue()
        {
            _testSection.RecoveryHttpClientTimeout = 50;
            Assert.Equal(_testSection.RecoveryHttpClientTimeout, IntegrationBuilder(_testSection).Build().RecoveryHttpClientTimeout);
            Assert.Equal(_testSection.RecoveryHttpClientTimeout, ProductionBuilder(_testSection).Build().RecoveryHttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, ReplayBuilder(_testSection).Build().RecoveryHttpClientTimeout);
            Assert.Equal(_testSection.RecoveryHttpClientTimeout, CustomBuilder(_testSection).Build().RecoveryHttpClientTimeout);

            Assert.Equal(80, IntegrationBuilder(_testSection).SetRecoveryHttpClientTimeout(80).Build().RecoveryHttpClientTimeout);
            Assert.Equal(80, ProductionBuilder(_testSection).SetRecoveryHttpClientTimeout(80).Build().RecoveryHttpClientTimeout);
            Assert.Equal(80, CustomBuilder(_testSection).SetRecoveryHttpClientTimeout(80).Build().RecoveryHttpClientTimeout);

            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().RecoveryHttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().RecoveryHttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().RecoveryHttpClientTimeout);
            Assert.Equal(SdkInfo.DefaultHttpClientTimeout, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().RecoveryHttpClientTimeout);
        }

        [Fact]
        public void MinIntervalBetweenRecoveryRequestsHasCorrectValue()
        {
            _testSection.MinIntervalBetweenRecoveryRequests = 100;

            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, IntegrationBuilder(_testSection).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, ProductionBuilder(_testSection).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(SdkInfo.DefaultIntervalBetweenRecoveryRequests, ReplayBuilder(_testSection).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, CustomBuilder(_testSection).Build().MinIntervalBetweenRecoveryRequests);

            Assert.Equal(140, IntegrationBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(140, ProductionBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(140, CustomBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).Build().MinIntervalBetweenRecoveryRequests);

            Assert.Equal(SdkInfo.DefaultIntervalBetweenRecoveryRequests, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(SdkInfo.DefaultIntervalBetweenRecoveryRequests, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(SdkInfo.DefaultIntervalBetweenRecoveryRequests, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(SdkInfo.DefaultIntervalBetweenRecoveryRequests, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().MinIntervalBetweenRecoveryRequests);

            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, IntegrationBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).LoadFromConfigFile().Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, ProductionBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).LoadFromConfigFile().Build().MinIntervalBetweenRecoveryRequests);
            Assert.Equal(_testSection.MinIntervalBetweenRecoveryRequests, CustomBuilder(_testSection).SetMinIntervalBetweenRecoveryRequests(140).LoadFromConfigFile().Build().MinIntervalBetweenRecoveryRequests);
        }

        [Fact]
        public void LoadBasicAppConfig()
        {
            var accessToken = "AccessToken";
            var config = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en' />".ToSdkConfiguration();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.Production,
                                  "en",
                                  1,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.Production),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.Production),
                                  0,
                                  accessToken,
                                  string.Empty,
                                  string.Empty);
        }

        [Fact]
        public void LoadEnvironmentProxyTokyoAppConfig()
        {
            var accessToken = "AccessToken";
            var config = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en' ufEnvironment='ProxyTokyo' />".ToSdkConfiguration();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.ProxyTokyo,
                                  "en",
                                  1,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.ProxyTokyo),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.ProxyTokyo),
                                  0,
                                  accessToken,
                                  string.Empty,
                                  string.Empty);
        }

        [Fact]
        public void BuilderEnvironmentProxyTokyo()
        {
            var accessToken = "AccessToken";
            var section = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en' ufEnvironment='ProxyTokyo' />".ToSection();
            var config = new TokenSetter(new TestSectionProvider(section)).BuildFromConfigFile();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.ProxyTokyo,
                                  "en",
                                  1,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.ProxyTokyo),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.ProxyTokyo),
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken,
                                  string.Empty,
                                  string.Empty);
        }

        [Fact]
        public void BuilderEnvironmentGlobalProduction()
        {
            var accessToken = "AccessToken";
            var section = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en,de' nodeId='11' />".ToSection();
            var config = new TokenSetter(new TestSectionProvider(section))
                         .SetAccessTokenFromConfigFile()
                         .SelectEnvironment(SdkEnvironment.GlobalProduction)
                         .LoadFromConfigFile()
                         .SetAdjustAfterAge(true)
                         .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.THROW)
                         .SetHttpClientTimeout(45)
                         .SetInactivitySeconds(45)
                         .SetMaxRecoveryTime(750)
                         .SetMinIntervalBetweenRecoveryRequests(45)
                         .SetRecoveryHttpClientTimeout(65)
                         .Build();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.GlobalProduction,
                                  "en",
                                  2,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.GlobalProduction),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.GlobalProduction),
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken,
                                  null,
                                  null,
                                  true,
                                  true,
                                  45,
                                  750,
                                  45,
                                  11,
                                  0,
                                  ExceptionHandlingStrategy.THROW,
                                  true,
                                  45,
                                  65);
        }

        [Fact]
        public void BuilderEnvironmentGlobalIntegration()
        {
            var accessToken = "AccessToken";
            var section = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en,de' nodeId='11' />".ToSection();
            var config = new TokenSetter(new TestSectionProvider(section))
                         .SetAccessTokenFromConfigFile()
                         .SelectEnvironment(SdkEnvironment.GlobalIntegration)
                         .LoadFromConfigFile()
                         .SetAdjustAfterAge(true)
                         .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.THROW)
                         .SetHttpClientTimeout(45)
                         .SetInactivitySeconds(45)
                         .SetMaxRecoveryTime(750)
                         .SetMinIntervalBetweenRecoveryRequests(45)
                         .SetRecoveryHttpClientTimeout(65)
                         .Build();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.GlobalIntegration,
                                  "en",
                                  2,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.GlobalIntegration),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.GlobalIntegration),
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken,
                                  null,
                                  null,
                                  true,
                                  true,
                                  45,
                                  750,
                                  45,
                                  11,
                                  0,
                                  ExceptionHandlingStrategy.THROW,
                                  true,
                                  45,
                                  65);
        }

        [Fact]
        public void BuilderEnvironmentReplay()
        {
            var accessToken = "AccessToken";
            var section = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en,de' nodeId='11' />".ToSection();
            var config = new TokenSetter(new TestSectionProvider(section))
                         .SetAccessTokenFromConfigFile()
                         .SelectEnvironment(SdkEnvironment.Replay)
                         .LoadFromConfigFile()
                         .SetAdjustAfterAge(true)
                         .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.THROW)
                         .SetHttpClientTimeout(45)
                         .SetInactivitySeconds(45)
                         .SetMaxRecoveryTime(750)
                         .SetMinIntervalBetweenRecoveryRequests(45)
                         .SetRecoveryHttpClientTimeout(65)
                         .Build();
            Assert.Equal(SdkEnvironment.Replay, config.Environment);
            Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), config.ApiHost);
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.Replay,
                                  "en",
                                  2,
                                  EnvironmentManager.GetMqHost(SdkEnvironment.Replay),
                                  EnvironmentManager.GetApiHost(SdkEnvironment.Replay),
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken,
                                  null,
                                  null,
                                  true,
                                  true,
                                  45,
                                  750,
                                  45,
                                  11,
                                  0,
                                  ExceptionHandlingStrategy.THROW,
                                  true,
                                  45,
                                  65);
        }

        [Fact]
        public void BuilderEnvironmentCustom()
        {
            var accessToken = "AccessToken";
            var section = $"<oddsFeedSection accessToken='{accessToken}' supportedLanguages='en,de' />".ToSection();
            var config = new TokenSetter(new TestSectionProvider(section))
                         .SetAccessTokenFromConfigFile()
                         .SelectCustom()
                         .LoadFromConfigFile()
                         .SetMessagingHost("mq.local.com")
                         .SetApiHost("api.local.com")
                         .Build();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.Custom,
                                  "en",
                                  2,
                                  "mq.local.com",
                                  "api.local.com",
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken);

            config = new TokenSetter(new TestSectionProvider(section))
                     .SetAccessTokenFromConfigFile()
                     .SelectCustom()
                     .SetMessagingHost("mq.local.com")
                     .SetApiHost("api.local.com")
                     .LoadFromConfigFile()
                     .Build();
            ValidateConfiguration(config,
                                  accessToken,
                                  SdkEnvironment.Custom,
                                  "en",
                                  2,
                                  "mq.local.com",
                                  "api.local.com",
                                  EnvironmentManager.DefaultMqHostPort,
                                  accessToken);
        }

        private void ValidateConfiguration(IOddsFeedConfiguration config,
                                           string accessToken,
                                           SdkEnvironment environment,
                                           string defaultCulture,
                                           int wantedCultures,
                                           string mqHost,
                                           string apiHost,
                                           int port,
                                           string username,
                                           string password = null,
                                           string virtualHost = null,
                                           bool useMqSsl = true,
                                           bool useApiSsl = true,
                                           int inactivitySeconds = SdkInfo.MinInactivitySeconds,
                                           int maxRecoveryExecutionInSeconds = SdkInfo.MaxRecoveryExecutionInSeconds,
                                           int minIntervalBetweenRecoveryRequests = SdkInfo.DefaultIntervalBetweenRecoveryRequests,
                                           int nodeId = 0,
                                           int disabledProducers = 0,
                                           ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH,
                                           bool adjustAfterAge = false,
                                           int httpClientTimeout = SdkInfo.DefaultHttpClientTimeout,
                                           int recoveryHttpClientTimeout = SdkInfo.DefaultHttpClientTimeout)
        {
            Assert.NotNull(config);
            Assert.Equal(accessToken, config.AccessToken);
            Assert.Equal(environment, config.Environment);
            Assert.Equal(defaultCulture, config.DefaultLocale.TwoLetterISOLanguageName);
            Assert.Equal(wantedCultures, config.Locales.Count());
            Assert.Equal(mqHost, config.Host);
            Assert.Equal(apiHost, config.ApiHost);
            Assert.Equal(port, config.Port);
            Assert.Equal(username, config.Username);
            Assert.Equal(password, config.Password);
            Assert.Equal(virtualHost, config.VirtualHost);
            Assert.Equal(useMqSsl, config.UseSsl);
            Assert.Equal(useApiSsl, config.UseApiSsl);
            Assert.Equal(inactivitySeconds, config.InactivitySeconds);
            Assert.Equal(maxRecoveryExecutionInSeconds, config.MaxRecoveryTime);
            Assert.Equal(minIntervalBetweenRecoveryRequests, config.MinIntervalBetweenRecoveryRequests);
            Assert.Equal(nodeId, config.NodeId);
            Assert.Equal(disabledProducers, config.DisabledProducers?.Count() ?? 0);
            Assert.Equal(exceptionHandlingStrategy, config.ExceptionHandlingStrategy);
            Assert.Equal(adjustAfterAge, config.AdjustAfterAge);
            Assert.Equal(httpClientTimeout, config.HttpClientTimeout);
            Assert.Equal(recoveryHttpClientTimeout, config.RecoveryHttpClientTimeout);
        }
    }
}
