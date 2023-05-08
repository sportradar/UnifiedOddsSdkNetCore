/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestConfigurationInternal : IOddsFeedConfigurationInternal
    {
        public TestConfigurationInternal(IOddsFeedConfiguration publicConfig, BookmakerDetailsDTO dto)
        {
            ExceptionHandlingStrategy = publicConfig.ExceptionHandlingStrategy;
            AccessToken = publicConfig.AccessToken;
            AdjustAfterAge = publicConfig.AdjustAfterAge;
            ApiHost = publicConfig.ApiHost;
            UseApiSsl = publicConfig.UseApiSsl;
            BookmakerDetails = new BookmakerDetails(dto);
            DefaultLocale = publicConfig.DefaultLocale;
            Locales = publicConfig.Locales;
            DisabledProducers = publicConfig.DisabledProducers;
            Environment = publicConfig.Environment;
            MaxRecoveryTime = publicConfig.MaxRecoveryTime;
            InactivitySeconds = publicConfig.InactivitySeconds;
            HttpClientTimeout = publicConfig.HttpClientTimeout;
            RecoveryHttpClientTimeout = publicConfig.RecoveryHttpClientTimeout;
            Host = publicConfig.Host;
            UseSsl = publicConfig.UseSsl;
            Username = publicConfig.Username;
            Password = publicConfig.Password;
            MinIntervalBetweenRecoveryRequests = publicConfig.MinIntervalBetweenRecoveryRequests;
            NodeId = publicConfig.NodeId;
            Port = publicConfig.Port;
            VirtualHost = publicConfig.VirtualHost;
            StatisticsEnabled = true;
            StatisticsRecordLimit = 100000;
            StatisticsTimeout = (int)TimeSpan.FromHours(1).TotalSeconds;
        }

        public static BookmakerDetailsDTO GetBookmakerDetails()
        {
            return new BookmakerDetailsDTO(
                RestMessageBuilder.BuildBookmakerDetails(
                    TestData.BookmakerId,
                    DateTime.Now.AddDays(1),
                    response_code.OK,
                    TestData.VirtualHost),
                    TimeSpan.Zero);
        }

        public static TestConfigurationInternal GetConfig(IOddsFeedConfiguration publicConfig = null, BookmakerDetailsDTO dto = null)
        {
            var configBuilder = new TokenSetter(new TestSectionProvider(TestSection.DefaultSection))
                .SetAccessTokenFromConfigFile()
                .SelectIntegration()
                .LoadFromConfigFile()
                .SetInactivitySeconds(30)
                .SetSupportedLanguages(new[] { TestData.Culture });
            var config = configBuilder.Build();

            return new TestConfigurationInternal(publicConfig ?? config, dto ?? GetBookmakerDetails());
        }

        /// <inheritdoc />
        public string AccessToken { get; }

        /// <inheritdoc />
        public int InactivitySeconds { get; }

        /// <inheritdoc />
        public CultureInfo DefaultLocale { get; }

        /// <inheritdoc />
        public IEnumerable<CultureInfo> Locales { get; }

        /// <inheritdoc />
        public IEnumerable<int> DisabledProducers { get; }

        /// <inheritdoc />
        public int MaxRecoveryTime { get; }

        /// <inheritdoc />
        public int NodeId { get; }

        /// <inheritdoc />
        public SdkEnvironment Environment { get; internal set; }

        /// <inheritdoc />
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; internal set; }

        /// <inheritdoc />
        public string Host { get; private set; }

        /// <inheritdoc />
        public int Port { get; }

        /// <inheritdoc />
        public string Username { get; }

        /// <inheritdoc />
        public string Password { get; }

        /// <inheritdoc />
        public string VirtualHost { get; }

        /// <inheritdoc />
        public bool UseSsl { get; }

        /// <inheritdoc />
        public string ApiHost { get; }

        /// <inheritdoc />
        public bool UseApiSsl { get; }

        /// <inheritdoc />
        public bool AdjustAfterAge { get; }

        /// <inheritdoc />
        public int HttpClientTimeout { get; }

        /// <inheritdoc />
        public int RecoveryHttpClientTimeout { get; }

        /// <inheritdoc />
        public int MinIntervalBetweenRecoveryRequests { get; }

        /// <inheritdoc />
        public bool StatisticsEnabled { get; }

        /// <inheritdoc />
        public int StatisticsTimeout { get; }

        /// <inheritdoc />
        public int StatisticsRecordLimit { get; }

        /// <inheritdoc />
        public string ReplayApiHost => ApiHost + "/v1/replay";

        /// <inheritdoc />
        public string ReplayApiBaseUrl => UseApiSsl ? "https://" + ReplayApiHost : "http://" + ReplayApiHost;

        /// <inheritdoc />
        public string ApiBaseUri => UseApiSsl ? "https://" + ApiHost : "http://" + ApiHost;

        /// <inheritdoc />
        public void Load()
        {
        }

        /// <inheritdoc />
        public void EnableReplayServer()
        {
            Environment = SdkEnvironment.Replay;
            Host = EnvironmentManager.GetMqHost(SdkEnvironment.Replay);
        }

        /// <inheritdoc />
        public IBookmakerDetails BookmakerDetails { get; }
    }
}
