/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class ConfigurationBuilderTests
    {
        private TestSection _testSection;

        [TestInitialize]
        public void Setup()
        {
            _testSection = TestSection.Create();
        }

        private static List<int> GetIntList(string value)
        {
            return value.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        }

        private static List<CultureInfo> GetCultureList(string cultureNames)
        {
            return cultureNames.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(name => new CultureInfo(name)).ToList();
        }
        private static IConfigurationBuilder StageBuilder(IOddsFeedConfigurationSection section)
        {
            return new TokenSetter(new TestSectionProvider(section))
                  .SetAccessTokenFromConfigFile()
                  .SelectStaging()
                  .LoadFromConfigFile();
        }

        private static IConfigurationBuilder StageBuilder(string token)
        {
            return new TokenSetter(new TestSectionProvider(null))
                  .SetAccessToken(token)
                  .SelectStaging();
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

        [TestMethod]
        public void access_token_has_correct_value()
        {
            _testSection.AccessToken = "my_token";
            Assert.AreEqual(_testSection.AccessToken, StageBuilder(_testSection).Build().AccessToken);
            Assert.AreEqual(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().AccessToken);
            Assert.AreEqual(_testSection.AccessToken, ProductionBuilder(_testSection).Build().AccessToken);
            Assert.AreEqual(_testSection.AccessToken, ReplayBuilder(_testSection).Build().AccessToken);
            Assert.AreEqual(_testSection.AccessToken, CustomBuilder(_testSection).Build().AccessToken);

            Assert.AreEqual("token", StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.AreEqual("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.AreEqual("token", ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.AreEqual("token", ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AccessToken);
            Assert.AreEqual("token", CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AccessToken);
        }

        [TestMethod]
        public void inactivity_seconds_has_correct_value()
        {
            _testSection.InactivitySeconds = 100;
            Assert.AreEqual(_testSection.InactivitySeconds, StageBuilder(_testSection).Build().InactivitySeconds);
            Assert.AreEqual(_testSection.InactivitySeconds, IntegrationBuilder(_testSection).Build().InactivitySeconds);
            Assert.AreEqual(_testSection.InactivitySeconds, ProductionBuilder(_testSection).Build().InactivitySeconds);
            Assert.AreEqual(SdkInfo.MaxInactivitySeconds, ReplayBuilder(_testSection).Build().InactivitySeconds);
            Assert.AreEqual(_testSection.InactivitySeconds, CustomBuilder(_testSection).Build().InactivitySeconds);

            Assert.AreEqual(80, StageBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.AreEqual(80, IntegrationBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.AreEqual(80, ProductionBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);
            Assert.AreEqual(80, CustomBuilder(_testSection).SetInactivitySeconds(80).Build().InactivitySeconds);

            Assert.AreEqual(SdkInfo.MinInactivitySeconds, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.AreEqual(SdkInfo.MinInactivitySeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.AreEqual(SdkInfo.MinInactivitySeconds, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.AreEqual(SdkInfo.MaxInactivitySeconds, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().InactivitySeconds);
            Assert.AreEqual(SdkInfo.MinInactivitySeconds, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().InactivitySeconds);
        }

        [TestMethod]
        public void default_locale_has_correct_value()
        {
            _testSection.DefaultLanguage = "de";
            var cultureEn = new CultureInfo("en");
            var cultureDe = new CultureInfo("de");

            Assert.AreEqual(cultureDe, StageBuilder(_testSection).Build().DefaultLocale);
            Assert.AreEqual(cultureDe, IntegrationBuilder(_testSection).Build().DefaultLocale);
            Assert.AreEqual(cultureDe, ProductionBuilder(_testSection).Build().DefaultLocale);
            Assert.AreEqual(cultureDe, ReplayBuilder(_testSection).Build().DefaultLocale);
            Assert.AreEqual(cultureDe, CustomBuilder(_testSection).Build().DefaultLocale);

            Assert.AreEqual(cultureEn, StageBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, ProductionBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, ReplayBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, CustomBuilder(_testSection).SetDefaultLanguage(cultureEn).Build().DefaultLocale);

            Assert.AreEqual(cultureEn, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DefaultLocale);
            Assert.AreEqual(cultureEn, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DefaultLocale);

            Assert.AreEqual(cultureDe, StageBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.AreEqual(cultureDe, IntegrationBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.AreEqual(cultureDe, ProductionBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.AreEqual(cultureDe, ReplayBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
            Assert.AreEqual(cultureDe, CustomBuilder(_testSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLocale);
        }

        [TestMethod]
        public void locales_have_correct_value()
        {
            _testSection.DefaultLanguage = "it";
            _testSection.SupportedLanguages = "it,de,en";

            Assert.IsTrue(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(StageBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.SupportedLanguages).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            Assert.IsTrue(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(StageBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(IntegrationBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(ProductionBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(ReplayBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));
            Assert.IsTrue(GetCultureList(_testSection.DefaultLanguage).SequenceEqual(CustomBuilder(_testSection).SetSupportedLanguages(null).Build().Locales));

            _testSection.DefaultLanguage = "sl";
            var langString = "sl," + _testSection.SupportedLanguages;
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(StageBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            _testSection.DefaultLanguage = "de";
            langString = "de," + _testSection.SupportedLanguages.Replace(",de", "");
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(StageBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(IntegrationBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(ProductionBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(ReplayBuilder(_testSection).Build().Locales));
            Assert.IsTrue(GetCultureList(langString).SequenceEqual(CustomBuilder(_testSection).Build().Locales));

            Assert.IsTrue(GetCultureList("en").SequenceEqual(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.IsTrue(GetCultureList("en").SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.IsTrue(GetCultureList("en").SequenceEqual(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.IsTrue(GetCultureList("en").SequenceEqual(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Locales));
            Assert.IsTrue(GetCultureList("en").SequenceEqual(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Locales));
        }

        [TestMethod]
        public void disabled_producers_have_correct_value()
        {
            _testSection.DisabledProducers = "1,3";
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(StageBuilder(_testSection).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder(_testSection).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(ProductionBuilder(_testSection).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(ReplayBuilder(_testSection).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(CustomBuilder(_testSection).Build().DisabledProducers));

            Assert.IsNull(StageBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.IsNull(IntegrationBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.IsNull(StageBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.IsNull(StageBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);
            Assert.IsNull(StageBuilder(_testSection).SetDisabledProducers(null).Build().DisabledProducers);

            Assert.IsNull(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.IsNull(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.IsNull(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.IsNull(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().DisabledProducers);
            Assert.IsNull(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DisabledProducers);

            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(StageBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).Build().DisabledProducers));
            Assert.IsTrue(GetIntList(_testSection.DisabledProducers).SequenceEqual(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetDisabledProducers(GetIntList(_testSection.DisabledProducers)).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().DisabledProducers));
        }

        [TestMethod]
        public void max_recovery_time_has_correct_value()
        {
            _testSection.MaxRecoveryTime = 1000;

            Assert.AreEqual(_testSection.MaxRecoveryTime, StageBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, ProductionBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, ReplayBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, CustomBuilder(_testSection).Build().MaxRecoveryTime);

            Assert.AreEqual(1400, StageBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.AreEqual(1400, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.AreEqual(1400, ProductionBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);
            Assert.AreEqual(1400, CustomBuilder(_testSection).SetMaxRecoveryTime(1400).Build().MaxRecoveryTime);

            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().MaxRecoveryTime);

            Assert.AreEqual(_testSection.MaxRecoveryTime, StageBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, IntegrationBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, ProductionBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
            Assert.AreEqual(_testSection.MaxRecoveryTime, CustomBuilder(_testSection).SetMaxRecoveryTime(1400).LoadFromConfigFile().Build().MaxRecoveryTime);
        }

        [TestMethod]
        public void node_id_has_correct_value()
        {
            _testSection.NodeId = 15;

            Assert.AreEqual(_testSection.NodeId, StageBuilder(_testSection).Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, IntegrationBuilder(_testSection).Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, ProductionBuilder(_testSection).Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, ReplayBuilder(_testSection).Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, CustomBuilder(_testSection).Build().NodeId);

            Assert.AreEqual(0, StageBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.AreEqual(0, IntegrationBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.AreEqual(0, ProductionBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.AreEqual(0, ReplayBuilder(_testSection).SetNodeId(0).Build().NodeId);
            Assert.AreEqual(0, CustomBuilder(_testSection).SetNodeId(0).Build().NodeId);

            Assert.AreEqual(0, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.AreEqual(0, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.AreEqual(0, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.AreEqual(0, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);
            Assert.AreEqual(0, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().NodeId);

            Assert.AreEqual(_testSection.NodeId, StageBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, IntegrationBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, ProductionBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, ReplayBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
            Assert.AreEqual(_testSection.NodeId, CustomBuilder(_testSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        }

        [TestMethod]
        public void environment_has_correct_value()
        {
            _testSection.UseIntegrationEnvironment = true;

            Assert.AreEqual(SdkEnvironment.Integration, StageBuilder(_testSection).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Integration, IntegrationBuilder(_testSection).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Production, ProductionBuilder(_testSection).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Replay, ReplayBuilder(_testSection).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Custom, CustomBuilder(_testSection).Build().Environment);

            Assert.AreEqual(SdkEnvironment.Integration, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Integration, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Production, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Replay, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Custom, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Environment);
        }

        [TestMethod]
        public void exception_handling_strategy_has_correct_value()
        {
            _testSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;

            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, StageBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, ProductionBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, ReplayBuilder(_testSection).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, CustomBuilder(_testSection).Build().ExceptionHandlingStrategy);

            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, StageBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, ProductionBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, ReplayBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, CustomBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).Build().ExceptionHandlingStrategy);

            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ExceptionHandlingStrategy);
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().ExceptionHandlingStrategy);

            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, StageBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, IntegrationBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, ProductionBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, ReplayBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
            Assert.AreEqual(_testSection.ExceptionHandlingStrategy, CustomBuilder(_testSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.CATCH).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        }

        [TestMethod]
        public void messaging_host_has_correct_value()
        {
            _testSection.Host = "mq.localhost.local";

            Assert.AreEqual(SdkInfo.IntegrationHost, StageBuilder(_testSection).Build().Host);
            Assert.AreEqual(SdkInfo.IntegrationHost, IntegrationBuilder(_testSection).Build().Host);
            Assert.AreEqual(SdkInfo.ProductionHost, ProductionBuilder(_testSection).Build().Host);
            Assert.AreEqual(SdkInfo.ReplayHost, ReplayBuilder(_testSection).Build().Host);
            Assert.AreEqual(_testSection.Host, CustomBuilder(_testSection).Build().Host);

            Assert.AreEqual(SdkInfo.IntegrationHost, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.AreEqual(SdkInfo.IntegrationHost, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.AreEqual(SdkInfo.ProductionHost, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.AreEqual(SdkInfo.ReplayHost, ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Host);
            Assert.AreEqual(_testSection.Host, CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Host);

            Assert.AreEqual("mq1.localhost.local", CustomBuilder(_testSection).SetMessagingHost("mq1.localhost.local").Build().Host);
            Assert.AreEqual("mq.localhost.local", CustomBuilder(_testSection).SetMessagingHost("mq1.localhost.local").LoadFromConfigFile().Build().Host);
        }

        [TestMethod]
        public void port_has_correct_value()
        {
            _testSection.Port = 2250;
            _testSection.UseSSL = true;

            Assert.AreEqual(SdkInfo.DefaultHostPort, StageBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, ProductionBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, ReplayBuilder(_testSection).Build().Port);
            Assert.AreEqual(_testSection.Port, CustomBuilder(_testSection).Build().Port);

            _testSection.UseSSL = false;
            Assert.AreEqual(SdkInfo.DefaultHostPort, StageBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, IntegrationBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, ProductionBuilder(_testSection).Build().Port);
            Assert.AreEqual(SdkInfo.DefaultHostPort, ReplayBuilder(_testSection).Build().Port);
            Assert.AreEqual(_testSection.Port, CustomBuilder(_testSection).Build().Port);

            _testSection.Port = 0;
            Assert.AreEqual(SdkInfo.DefaultHostPort + 1, CustomBuilder(_testSection).Build().Port);
            _testSection.UseSSL = true;
            Assert.AreEqual(SdkInfo.DefaultHostPort, CustomBuilder(_testSection).Build().Port);
            _testSection.Port = 2250;
            Assert.AreEqual(_testSection.Port, CustomBuilder(_testSection).LoadFromConfigFile().Build().Port);
        }

        [TestMethod]
        public void username_has_correct_value()
        {
            _testSection.Username = "username";
            _testSection.AccessToken = "token";

            Assert.AreEqual(_testSection.AccessToken, StageBuilder(_testSection).Build().Username);
            Assert.AreEqual(_testSection.AccessToken, IntegrationBuilder(_testSection).Build().Username);
            Assert.AreEqual(_testSection.AccessToken, ProductionBuilder(_testSection).Build().Username);
            Assert.AreEqual(_testSection.AccessToken, ReplayBuilder(_testSection).Build().Username);
            Assert.AreEqual(_testSection.Username, CustomBuilder(_testSection).Build().Username);

            Assert.AreEqual("token", StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.AreEqual("token", IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.AreEqual("token", ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.AreEqual("token", ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Username);
            Assert.AreEqual("token", CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().Username);

            Assert.AreEqual("username", CustomBuilder(_testSection).Build().Username);
            Assert.AreEqual(_testSection.Username, CustomBuilder(_testSection).LoadFromConfigFile().Build().Username);
        }

        [TestMethod]
        public void password_has_correct_value()
        {
            Assert.AreEqual(_testSection.Password, CustomBuilder(_testSection).LoadFromConfigFile().Build().Password);
            Assert.AreEqual(_testSection.Password, CustomBuilder(_testSection).Build().Password);

            _testSection.Password = null;

            Assert.AreEqual(null, StageBuilder(_testSection).Build().Password);
            Assert.AreEqual(null, IntegrationBuilder(_testSection).Build().Password);
            Assert.AreEqual(null, ProductionBuilder(_testSection).Build().Password);
            Assert.AreEqual(null, ReplayBuilder(_testSection).Build().Password);
            Assert.AreEqual(_testSection.Password, CustomBuilder(_testSection).Build().Password);

            Assert.IsNull(CustomBuilder(_testSection).Build().Password);
        }

        [TestMethod]
        public void virtual_host_has_correct_value()
        {
            _testSection.VirtualHost = null;

            Assert.IsNull(StageBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(ProductionBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(ReplayBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(CustomBuilder(_testSection).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);

            Assert.IsNull(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.IsNull(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.IsNull(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.IsNull(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().VirtualHost);
            Assert.IsNull(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);

            _testSection.VirtualHost = "my_virtual_host";
            Assert.IsNull(StageBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(IntegrationBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(ProductionBuilder(_testSection).Build().VirtualHost);
            Assert.IsNull(ReplayBuilder(_testSection).Build().VirtualHost);
            Assert.AreEqual(_testSection.VirtualHost, CustomBuilder(_testSection).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);
            Assert.AreEqual("virtual_host", CustomBuilder(_testSection).SetVirtualHost("virtual_host").SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().VirtualHost);
        }

        [TestMethod]
        public void host_with_protocol_causes_an_exception()
        {
            var exceptionThrown = false;
            try
            {
                CustomBuilder("token").SetMessagingHost("http://mq.localhost.local").Build();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                Assert.Fail("MessagingHost with protocol specification 'http://' should cause an exception");
            }
            exceptionThrown = false;
            try
            {
                CustomBuilder("token").SetMessagingHost("https://mq.localhost.local").Build();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                Assert.Fail("MessagingHost with protocol specification 'https://' should cause an exception");
            }
        }

        [TestMethod]
        public void use_messaging_ssl_has_correct_value()
        {
            _testSection.UseSSL = false;

            Assert.IsTrue(StageBuilder(_testSection).Build().UseSsl);
            Assert.IsTrue(IntegrationBuilder(_testSection).Build().UseSsl);
            Assert.IsTrue(ProductionBuilder(_testSection).Build().UseSsl);
            Assert.IsTrue(ReplayBuilder(_testSection).Build().UseSsl);
            Assert.IsFalse(CustomBuilder(_testSection).Build().UseSsl);

            Assert.IsTrue(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.IsTrue(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.IsTrue(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.IsTrue(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseSsl);
            Assert.IsTrue(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseSsl);

            Assert.IsTrue(CustomBuilder(_testSection).UseMessagingSsl(true).Build().UseSsl);
            Assert.IsFalse(CustomBuilder(_testSection).UseMessagingSsl(true).LoadFromConfigFile().Build().UseSsl);
            Assert.IsFalse(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).UseMessagingSsl(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseSsl);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void missing_messaging_host_throw_exception()
        {
            Assert.IsNull(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetApiHost(_testSection.ApiHost).Build().ApiHost);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void missing_api_host_throw_exception()
        {
            Assert.IsNull(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).Build().ApiHost);
        }

        [TestMethod]
        public void api_host_has_correct_value()
        {
            _testSection.ApiHost = "api.localhost.local";

            Assert.AreEqual(SdkInfo.IntegrationApiHost, StageBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(SdkInfo.IntegrationApiHost, IntegrationBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(SdkInfo.ProductionApiHost, ProductionBuilder(_testSection).Build().ApiHost);
            Assert.IsNull(ReplayBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(_testSection.ApiHost, CustomBuilder(_testSection).Build().ApiHost);

            Assert.AreEqual(SdkInfo.IntegrationApiHost, StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.AreEqual(SdkInfo.IntegrationApiHost, IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.AreEqual(SdkInfo.ProductionApiHost, ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            Assert.IsNull(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);
            //Assert.IsNull(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).Build().ApiHost);

            Assert.AreEqual(_testSection.ApiHost, CustomBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(_testSection.ApiHost, CustomBuilder(_testSection).LoadFromConfigFile().Build().ApiHost);
            Assert.AreEqual("api.localhost.local", CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetApiHost("api.localhost.local").SetMessagingHost(_testSection.Host).Build().ApiHost);
        }

        [TestMethod]
        public void api_host_with_protocol_causes_an_exception()
        {
            var exceptionThrown = false;
            try
            {
                CustomBuilder("token").SetApiHost("http://api.localhost.local").Build();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                Assert.Fail("ApiHost with protocol specification 'http://' should cause an exception");
            }
            exceptionThrown = false;
            try
            {
                CustomBuilder("token").SetApiHost("https://api.localhost.local").Build();
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
            {
                Assert.Fail("ApiHost with protocol specification 'https://' should cause an exception");
            }
        }

        [TestMethod]
        public void use_api_ssl_has_correct_value()
        {
            _testSection.UseApiSSL = false;

            Assert.IsTrue(StageBuilder(_testSection).Build().UseApiSsl);
            Assert.IsTrue(IntegrationBuilder(_testSection).Build().UseApiSsl);
            Assert.IsTrue(ProductionBuilder(_testSection).Build().UseApiSsl);
            Assert.IsTrue(ReplayBuilder(_testSection).Build().UseApiSsl);
            Assert.IsFalse(CustomBuilder(_testSection).Build().UseApiSsl);

            Assert.IsTrue(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.IsTrue(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.IsTrue(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.IsTrue(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().UseApiSsl);
            Assert.IsTrue(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseApiSsl);

            Assert.IsTrue(CustomBuilder(_testSection).UseApiSsl(true).Build().UseApiSsl);
            Assert.IsFalse(CustomBuilder(_testSection).UseApiSsl(true).LoadFromConfigFile().Build().UseApiSsl);
            Assert.IsFalse(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).UseApiSsl(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().UseApiSsl);
        }

        [TestMethod]
        public void enforce_max_after_age_has_correct_value()
        {
            _testSection.AdjustAfterAge = false;

            Assert.IsFalse(StageBuilder(_testSection).Build().AdjustAfterAge);
            Assert.IsFalse(IntegrationBuilder(_testSection).Build().AdjustAfterAge);
            Assert.IsFalse(ProductionBuilder(_testSection).Build().AdjustAfterAge);
            Assert.IsFalse(ReplayBuilder(_testSection).Build().AdjustAfterAge);
            Assert.IsFalse(CustomBuilder(_testSection).Build().AdjustAfterAge);

            Assert.IsFalse(StageBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.IsFalse(IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.IsFalse(ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.IsFalse(ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().AdjustAfterAge);
            Assert.IsFalse(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AdjustAfterAge);

            Assert.IsTrue(CustomBuilder(_testSection).SetAdjustAfterAge(true).Build().AdjustAfterAge);
            Assert.IsFalse(CustomBuilder(_testSection).SetAdjustAfterAge(true).LoadFromConfigFile().Build().AdjustAfterAge);
            Assert.IsTrue(CustomBuilder(_testSection).LoadFromConfigFile().SetAdjustAfterAge(true).Build().AdjustAfterAge);
            Assert.IsFalse(CustomBuilder(_testSection).LoadFromConfigFile().SetAdjustAfterAge(true).LoadFromConfigFile().Build().AdjustAfterAge);
            Assert.IsFalse(CustomBuilder("token").SetDefaultLanguage(TestData.Culture).SetAdjustAfterAge(false).SetMessagingHost(_testSection.Host).SetApiHost(_testSection.ApiHost).Build().AdjustAfterAge);
        }
    }
}
