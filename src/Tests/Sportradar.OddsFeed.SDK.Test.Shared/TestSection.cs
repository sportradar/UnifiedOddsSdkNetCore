/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestSection : IOddsFeedConfigurationSection
    {
        public static TestSection Create()
        {
            return new TestSection(
                TestData.AccessToken,
                25,
                "mq.localhost.com",
                "virtual_host",
                5000,
                "username",
                "password",
                "api.localhost.com",
                false,
                false,
                "en,de,it",
                "en",
                false,
                0,
                0,
                "logs",
                true,
                ExceptionHandlingStrategy.THROW,
                "1,3",
                1150,
                SdkInfo.DefaultIntervalBetweenRecoveryRequests,
                11,
                false,
                30,
                30);
        }

        internal static IOddsFeedConfigurationSection MinimalIntegrationSection = new TestSection(
            TestData.AccessToken,
            25,
            null,
            null,
            0,
            null,
            null,
            null,
            true,
            true,
            "en,de,it",
            null,
            false,
            0,
            0,
            null,
            true,
            ExceptionHandlingStrategy.CATCH,
            null,
            SdkInfo.MaxRecoveryExecutionInSeconds,
            SdkInfo.DefaultIntervalBetweenRecoveryRequests,
            0,
            false,
            30,
            30);

        internal static IOddsFeedConfigurationSection MinimalProductionSection = new TestSection(
            TestData.AccessToken,
            25,
            null,
            null,
            0,
            null,
            null,
            null,
            true,
            true,
            "en,de,it",
            null,
            false,
            0,
            0,
            null,
            false,
            ExceptionHandlingStrategy.CATCH,
            null,
            SdkInfo.MaxRecoveryExecutionInSeconds,
            SdkInfo.DefaultIntervalBetweenRecoveryRequests,
            0,
            false,
            30,
            30);

        internal static IOddsFeedConfigurationSection IntegrationSection = new TestSection(
            TestData.AccessToken,
            25,
            "stgmq.localhost.com",
            "virtual_host",
            5000,
            "username",
            "password",
            "stgapi.localhost.com",
            false,
            false,
            "en,de,it",
            "en",
            false,
            0,
            0,
            "logs",
            true,
            ExceptionHandlingStrategy.THROW,
            "1,3",
            1150,
            SdkInfo.DefaultIntervalBetweenRecoveryRequests,
            11,
            true,
            30,
            30);

        internal static IOddsFeedConfigurationSection ProductionSection = new TestSection(
            TestData.AccessToken,
            25,
            "mq.localhost.com",
            "virtual_host",
            5000,
            "username",
            "password",
            "api.localhost.com",
            false,
            false,
            "en,de,it",
            "en",
            false,
            0,
            0,
            "logs",
            false,
            ExceptionHandlingStrategy.THROW,
            "1,3",
            1150,
            SdkInfo.DefaultIntervalBetweenRecoveryRequests,
            11,
            true,
            30,
            30);

        internal static readonly IOddsFeedConfigurationSection DefaultSection = new TestSection(
            TestData.AccessToken,
            25,
            "stgmq.localhost.com",
            "virtual_host",
            5671,
            "myTokenAkaUsername",
            string.Empty,
            EnvironmentManager.GetApiHost(SdkEnvironment.Integration),
            true,
            true,
            "en,de,hu",
            "en",
            false,
            0,
            0,
            "logs",
            true,
            ExceptionHandlingStrategy.THROW,
            null,
            600,
            SdkInfo.DefaultIntervalBetweenRecoveryRequests,
            33,
            false,
            30,
            30);

        public string AccessToken { get; set; }
        public int InactivitySeconds { get; set; }
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiHost { get; set; }
        public bool UseSSL { get; set; }
        public bool UseApiSSL { get; set; }
        public string SupportedLanguages { get; set; }
        public string DefaultLanguage { get; set; }
        public bool StatisticsEnabled { get; set; }
        public int StatisticsTimeout { get; set; }
        public int StatisticsRecordLimit { get; set; }
        public string SdkLogConfigPath { get; set; }
        public bool UseStagingEnvironment { get; set; }
        public bool UseIntegrationEnvironment { get; set; }
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; set; }
        public string DisabledProducers { get; set; }
        public int MaxRecoveryTime { get; set; }
        public int MinIntervalBetweenRecoveryRequests { get; set; }
        public int NodeId { get; set; }
        public bool AdjustAfterAge { get; set; }

        /// <summary>
        /// Gets a value specifying timeout set for HTTP responses
        /// </summary>
        public int HttpClientTimeout { get; set; }

        /// <summary>
        /// Gets a value specifying timeout set for recovery HTTP responses
        /// </summary>
        public int RecoveryHttpClientTimeout { get; set; }

        /// <inheritdoc />
        public SdkEnvironment? UfEnvironment { get; }

        public TestSection(string accessToken, int inactivitySeconds, string host, string virtualHost, int port, string username, string password, string apiHost, bool useSSL, bool useApiSSL, string supportedLanguages, string defaultLanguage, bool statisticsEnabled, int statisticsTimeout, int statisticsRecordLimit, string sdkLogConfigPath, bool useIntegrationEnvironment, ExceptionHandlingStrategy exceptionHandlingStrategy, string disabledProducers, int maxRecoveryTime, int minIntervalBetweenRecoveryRequests, int nodeId, bool adjustAfterAge, int httpClientTimeout, int recoveryHttpClientTimeout, SdkEnvironment? ufEnvironment = null)
        {
            AccessToken = accessToken;
            InactivitySeconds = inactivitySeconds;
            Host = host;
            VirtualHost = virtualHost;
            Port = port;
            Username = username;
            Password = password;
            ApiHost = apiHost;
            UseSSL = useSSL;
            UseApiSSL = useApiSSL;
            SupportedLanguages = supportedLanguages;
            DefaultLanguage = defaultLanguage;
            StatisticsEnabled = statisticsEnabled;
            StatisticsTimeout = statisticsTimeout;
            StatisticsRecordLimit = statisticsRecordLimit;
            SdkLogConfigPath = sdkLogConfigPath;
            UseIntegrationEnvironment = useIntegrationEnvironment;
            ExceptionHandlingStrategy = exceptionHandlingStrategy;
            DisabledProducers = disabledProducers;
            MaxRecoveryTime = maxRecoveryTime;
            MinIntervalBetweenRecoveryRequests = minIntervalBetweenRecoveryRequests;
            NodeId = nodeId;
            AdjustAfterAge = adjustAfterAge;
            HttpClientTimeout = httpClientTimeout;
            RecoveryHttpClientTimeout = recoveryHttpClientTimeout;
            if (ufEnvironment != null)
            {
                UfEnvironment = ufEnvironment;
            }
            else
            {
                UfEnvironment = useIntegrationEnvironment ? SdkEnvironment.Integration : SdkEnvironment.Production;
            }
        }
    }

    internal class TestSectionProvider : IConfigurationSectionProvider
    {
        private readonly IOddsFeedConfigurationSection _section;

        public TestSectionProvider(IOddsFeedConfigurationSection section)
        {
            _section = section;
        }

        public IOddsFeedConfigurationSection GetSection()
        {
            return _section;
        }
    }
}