// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using RabbitMQ.Client;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal static class ConfigLimit
    {
        public const int HttpClientTimeoutMin = 1;
        public const int HttpClientTimeoutDefault = 30;
        public const int HttpClientTimeoutMax = 60;
        public const int HttpClientRecoveryTimeoutMin = 1;
        public const int HttpClientRecoveryTimeoutDefault = 30;
        public const int HttpClientRecoveryTimeoutMax = 60;
        public const int HttpClientFastFailingTimeoutMin = 1;
        public const int HttpClientFastFailingTimeoutDefault = 5;
        public const int HttpClientFastFailingTimeoutMax = 30;
        public const int MaxConnectionsPerServerDefault = int.MaxValue;
        public const int RestConnectionFailureLimit = 5;
        public const int RestConnectionFailureTimeoutInSec = 15;

        public const int SportEventCacheTimeoutMin = 1;
        public const int SportEventCacheTimeoutDefault = 12;
        public const int SportEventCacheTimeoutMax = 48;
        public const int SportEventStatusCacheTimeoutMinutesMin = 1;
        public const int SportEventStatusCacheTimeoutMinutesDefault = 5;
        public const int SportEventStatusCacheTimeoutMinutesMax = 60;
        public const int ProfileCacheTimeoutMin = 1;
        public const int ProfileCacheTimeoutDefault = 24;
        public const int ProfileCacheTimeoutMax = 48;
        public const int SingleVariantMarketTimeoutMin = 1;
        public const int SingleVariantMarketTimeoutDefault = 3;
        public const int SingleVariantMarketTimeoutMax = 24;
        public const int IgnoreBetpalTimelineTimeoutMin = 1;
        public const int IgnoreBetpalTimelineTimeoutDefault = 3;
        public const int IgnoreBetpalTimelineTimeoutMax = 24;
        public const int MarketDescriptionMinFetchInterval = 30;

        public const int StatisticsIntervalMinutesMin = 0;
        public const int StatisticsIntervalMinutesDefault = 10;
        public const int StatisticsIntervalMinutesMax = 60 * 24;

        public const int InactivitySecondsMin = 10;
        public const int InactivitySecondsDefault = 20;
        public const int InactivitySecondsMax = 180;
        public const int InactivitySecondsPrematchMin = 10;
        public const int InactivitySecondsPrematchDefault = 20;
        public const int InactivitySecondsPrematchMax = 180;
        public const int MaxRecoveryTimeMin = 600;
        public const int MaxRecoveryTimeDefault = 1200;
        public const int MaxRecoveryTimeMax = 3600;
        public const int MinIntervalBetweenRecoveryRequestMin = 20;
        public const int MinIntervalBetweenRecoveryRequestDefault = 30;
        public const int MinIntervalBetweenRecoveryRequestMax = 180;

        public const int RabbitConnectionTimeoutMin = 10;
        public static readonly int RabbitConnectionTimeoutDefault = (int)ConnectionFactory.DefaultConnectionTimeout.TotalSeconds;
        public const int RabbitConnectionTimeoutMax = 120;
        public const int RabbitHeartbeatMin = 10;
        public static readonly int RabbitHeartbeatDefault = (int)ConnectionFactory.DefaultHeartbeat.TotalSeconds;
        public const int RabbitHeartbeatMax = 180;
        public const int RabbitMaxTimeBetweenMessages = 180;
    }
}
