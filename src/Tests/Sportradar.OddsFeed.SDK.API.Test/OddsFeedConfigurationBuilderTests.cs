/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    [Obsolete]
    public class OddsFeedConfigurationBuilderTests
    {
        private TestSection _testSection;

        [TestInitialize]
        public void Setup()
        {
            _testSection = TestSection.Create();
        }

        private static List<CultureInfo> GetCultureList(params string[] cultureNames)
        {
            return cultureNames.Select(name => new CultureInfo(name)).ToList();
        }

        private static OddsFeedConfigurationBuilder CreateBuilder(IOddsFeedConfigurationSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }
            return new OddsFeedConfigurationBuilder(new TestSectionProvider(section));
        }

        private static OddsFeedConfigurationBuilder CreateBuilder(
            string token,
            int inactivitySeconds,
            params string[] cultureNames)
        {
            var builder = new OddsFeedConfigurationBuilder(null)
                .SetAccessToken(token)
                .SetInactivitySeconds(inactivitySeconds);

            if (cultureNames != null && cultureNames.Any())
            {
                foreach (var cultureName in cultureNames)
                {
                    builder.AddLocale(new CultureInfo(cultureName));
                }
            }
            return (OddsFeedConfigurationBuilder)builder;
        }

        [TestMethod]
        public void access_token_has_correct_value()
        {
            _testSection.AccessToken = "my_token";
            Assert.AreEqual("my_token", CreateBuilder(_testSection).Build().AccessToken);
            Assert.AreEqual("my_token", CreateBuilder("my_token", 30, "en").Build().AccessToken);
            Assert.AreEqual("my_token_1", ((IOddsFeedConfigurationBuilder)CreateBuilder(_testSection).SetAccessToken("my_token_1")).Build().AccessToken);
        }

        [TestMethod]
        public void inactivity_seconds_has_correct_value()
        {
            _testSection.InactivitySeconds = 55;
            Assert.AreEqual(55, CreateBuilder(_testSection).Build().InactivitySeconds);
            Assert.AreEqual(45, CreateBuilder("token", 45, "en").Build().InactivitySeconds);
            Assert.AreEqual(35, CreateBuilder(_testSection).SetInactivitySeconds(35).Build().InactivitySeconds);
        }

        [TestMethod]
        public void default_locale_and_locales_have_correct_value()
        {
            _testSection.SupportedLanguages = "it,de,en";
            Assert.IsTrue(GetCultureList("it", "de","en").SequenceEqual(CreateBuilder(_testSection).Build().Locales));
            Assert.AreEqual(new CultureInfo("it"), CreateBuilder(_testSection).Build().DefaultLocale);

            Assert.IsTrue(GetCultureList("de", "en").SequenceEqual(CreateBuilder(_testSection).RemoveLocale(new CultureInfo("it")).Build().Locales));
            Assert.AreEqual(new CultureInfo("de"), CreateBuilder(_testSection).RemoveLocale(new CultureInfo("it")).Build().DefaultLocale);

            Assert.IsTrue(GetCultureList("it", "de", "en", "sl").SequenceEqual(CreateBuilder(_testSection).AddLocale(new CultureInfo("sl")).Build().Locales));
            Assert.AreEqual(new CultureInfo("it"), CreateBuilder(_testSection).AddLocale(new CultureInfo("sl")).Build().DefaultLocale);
        }

        [TestMethod]
        public void disabled_producers_have_correct_value()
        {
            _testSection.DisabledProducers = null;
            Assert.IsNull(CreateBuilder(_testSection).Build().DisabledProducers);
            Assert.IsNull(CreateBuilder("token", 30, "en").Build().DisabledProducers);

            _testSection.DisabledProducers = "1,3";
            Assert.IsTrue(new List<int>(new []{1,3}).SequenceEqual(CreateBuilder(_testSection).Build().DisabledProducers));
        }

        [TestMethod]
        public void max_recovery_time_has_correct_value()
        {
            _testSection.MaxRecoveryTime = 1000;

            Assert.AreEqual(1000, CreateBuilder(_testSection).Build().MaxRecoveryTime);
            Assert.AreEqual(SdkInfo.MaxRecoveryExecutionInSeconds, CreateBuilder("token", 30, "en").Build().MaxRecoveryTime);
            Assert.AreEqual(1200, CreateBuilder(_testSection).SetMaxRecoveryTime(1200).Build().MaxRecoveryTime);
        }

        [TestMethod]
        public void node_id_has_correct_value()
        {
            _testSection.NodeId = 55;
            Assert.AreEqual(55, CreateBuilder(_testSection).Build().NodeId);
            Assert.AreEqual(0, CreateBuilder("token", 30, "en").Build().NodeId);
            Assert.AreEqual(10, CreateBuilder("token", 30, "en").SetNodeId(10).Build().NodeId);
            Assert.AreEqual(20, CreateBuilder(_testSection).SetNodeId(20).Build().NodeId);
        }

        [TestMethod]
        public void environment_has_correct_value()
        {
            _testSection.UseIntegrationEnvironment = false;
            Assert.AreEqual(SdkEnvironment.Production, CreateBuilder(_testSection).Build().Environment);

            _testSection.UseIntegrationEnvironment = true;
            Assert.AreEqual(SdkEnvironment.Integration, CreateBuilder(_testSection).Build().Environment);

            Assert.AreEqual(SdkEnvironment.Production, CreateBuilder("token", 30, "en").Build().Environment);
            Assert.AreEqual(SdkEnvironment.Integration, CreateBuilder("token", 30, "en").SetUseStagingEnvironment(true).Build().Environment);
            Assert.AreEqual(SdkEnvironment.Integration, CreateBuilder("token", 30, "en").SetUseIntegrationEnvironment(true).Build().Environment);

            _testSection.UseIntegrationEnvironment = true;
            Assert.AreEqual(SdkEnvironment.Production, CreateBuilder(_testSection).SetUseIntegrationEnvironment(false).Build().Environment);

            _testSection.UseStagingEnvironment = true;
            Assert.AreEqual(SdkEnvironment.Production, CreateBuilder(_testSection).SetUseStagingEnvironment(false).Build().Environment);
        }

        [TestMethod]
        public void exception_handling_strategy_has_correct_value()
        {
            _testSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, CreateBuilder(_testSection).Build().ExceptionHandlingStrategy);

            _testSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.THROW;
            Assert.AreEqual(ExceptionHandlingStrategy.THROW, CreateBuilder(_testSection).Build().ExceptionHandlingStrategy);

            Assert.AreEqual(ExceptionHandlingStrategy.CATCH, CreateBuilder("token", 30, "en").Build().ExceptionHandlingStrategy);
        }

        [TestMethod]
        public void messaging_host_has_correct_value()
        {
            _testSection.Host = null;

            _testSection.UseIntegrationEnvironment = false;
            Assert.AreEqual(SdkInfo.ProductionHost, CreateBuilder(_testSection).Build().Host);

            _testSection.UseIntegrationEnvironment = true;
            Assert.AreEqual(SdkInfo.IntegrationHost, CreateBuilder(_testSection).Build().Host);

            _testSection.Host = "mq.localhost.local";
            Assert.AreEqual("mq.localhost.local", CreateBuilder(_testSection).Build().Host);

            Assert.AreEqual(SdkInfo.ProductionHost, CreateBuilder("token", 30, "en").Build().Host);
            Assert.AreEqual(SdkInfo.IntegrationHost, CreateBuilder("token", 30, "en").SetUseIntegrationEnvironment(true).Build().Host);
            Assert.AreEqual("mq.localhost.local", CreateBuilder("token", 30, "en").SetHost("mq.localhost.local").Build().Host);

            _testSection.Host = null;
            Assert.AreEqual(SdkInfo.ProductionHost, CreateBuilder(_testSection).SetUseIntegrationEnvironment(false).Build().Host);

            _testSection.UseIntegrationEnvironment = false;
            _testSection.Host = "mq.localhost.local";
            Assert.AreEqual(SdkInfo.ProductionHost, CreateBuilder(_testSection).SetHost(null).Build().Host);
        }

        [TestMethod]
        public void port_has_correct_value()
        {
            _testSection.Port = 10;

            _testSection.UseSSL = false;
            Assert.AreEqual(5672, CreateBuilder(_testSection).Build().Port);

            _testSection.UseSSL = true;
            Assert.AreEqual(SdkInfo.DefaultHostPort, CreateBuilder(_testSection).Build().Port);
            Assert.AreEqual(5672, CreateBuilder(_testSection).SetUseSsl(false).Build().Port);
        }

        [TestMethod]
        public void username_has_correct_value()
        {
            _testSection.AccessToken = "my_token_1";
            _testSection.Username = "username";
            Assert.AreEqual("my_token_1", CreateBuilder(_testSection).Build().Username);
            Assert.AreEqual("my_token_2", CreateBuilder("my_token_2", 30, "en").Build().Username);
        }

        [TestMethod]
        public void password_has_correct_value()
        {
            _testSection.Password = "password";
            Assert.IsNull(CreateBuilder(_testSection).Build().Password);
            Assert.IsNull(CreateBuilder("token", 30, "en").Build().Password);
        }

        [TestMethod]
        public void virtual_host_has_correct_value()
        {
            _testSection.VirtualHost = null;
            Assert.IsNull(CreateBuilder(_testSection).Build().VirtualHost);

            _testSection.VirtualHost = "virtual_host";
            Assert.AreEqual("virtual_host", CreateBuilder(_testSection).Build().VirtualHost);

            Assert.IsNull(CreateBuilder("token", 30, "en").Build().VirtualHost);
            Assert.AreEqual("virtual_host", CreateBuilder("token", 30, "en").SetVirtualHost("virtual_host").Build().VirtualHost);

            _testSection.VirtualHost = "virtual_host";
            Assert.IsNull(CreateBuilder(_testSection).SetVirtualHost(null).Build().VirtualHost);
        }

        [TestMethod]
        public void use_ssl_has_correct_value()
        {
            _testSection.UseSSL = true;
            Assert.IsTrue(CreateBuilder(_testSection).Build().UseSsl);

            _testSection.UseSSL = false;
            Assert.IsFalse(CreateBuilder(_testSection).Build().UseSsl);
            Assert.IsTrue(CreateBuilder(_testSection).SetUseSsl(true).Build().UseSsl);

            Assert.IsTrue(CreateBuilder("token", 30, "en").Build().UseSsl);
        }

        [TestMethod]
        public void api_host_has_correct_value()
        {
            _testSection.ApiHost = null;
            _testSection.UseIntegrationEnvironment = false;

            Assert.AreEqual(SdkInfo.ProductionApiHost, CreateBuilder(_testSection).Build().ApiHost);

            _testSection.UseIntegrationEnvironment = true;
            Assert.AreEqual(SdkInfo.IntegrationApiHost, CreateBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(SdkInfo.ProductionApiHost, CreateBuilder(_testSection).SetUseIntegrationEnvironment(false).Build().ApiHost);

            _testSection.ApiHost = "api.localhost.local";
            _testSection.UseIntegrationEnvironment = true;
            Assert.AreEqual(_testSection.ApiHost, CreateBuilder(_testSection).Build().ApiHost);
            Assert.AreEqual(SdkInfo.IntegrationApiHost, CreateBuilder(_testSection).SetApiHost(null).Build().ApiHost);

            Assert.AreEqual(SdkInfo.ProductionApiHost, CreateBuilder("token", 30, "en").Build().ApiHost);
            Assert.AreEqual(SdkInfo.IntegrationApiHost, CreateBuilder("token", 30, "en").SetUseIntegrationEnvironment(true).Build().ApiHost);

            Assert.AreEqual("api.localhost.local", CreateBuilder("token", 30, "en").SetApiHost("api.localhost.local").Build().ApiHost);
            Assert.AreEqual("api.localhost.local", CreateBuilder("token", 30, "en").SetApiHost("http://api.localhost.local").Build().ApiHost);
            Assert.AreEqual("api.localhost.local", CreateBuilder("token", 30, "en").SetApiHost("https://api.localhost.local").Build().ApiHost);
        }

        [TestMethod]
        public void use_api_ssl_has_correct_value()
        {
            _testSection.UseApiSSL = false;

            Assert.IsTrue(CreateBuilder(_testSection).Build().UseApiSsl);
            Assert.IsTrue(CreateBuilder("token", 30, "en").Build().UseApiSsl);
            Assert.IsTrue(CreateBuilder(_testSection).SetUseIntegrationEnvironment(true).Build().UseApiSsl);

            Assert.IsTrue(CreateBuilder(_testSection).SetApiHost("api.localhost.local").Build().UseApiSsl);
            Assert.IsFalse(CreateBuilder(_testSection).SetApiHost("http://api.localhost.local").Build().UseApiSsl);
            Assert.IsTrue(CreateBuilder(_testSection).SetApiHost("https://api.localhost.local").Build().UseApiSsl);
        }
    }
}
