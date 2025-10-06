// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Telemetry
{
    internal static class UofSdkTelemetry
    {
        internal const string MetricsVersion = "v1";

        // .NET runtime libraries have metric names using '-' if a separator is needed.
        internal const string MetricNamePrefix = "uofsdk-";

        public const string ServiceName = "UofSdk-Net";

        public static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName);

        public static readonly Meter DefaultMeter = new Meter(ServiceName, SdkInfo.GetVersion());

        private const string MetricNameForSportDataCacheGetAll = MetricNamePrefix + "sportdatacache-getall";
        private const string MetricNameForSportEventStatusCacheGetSportEventStatus = MetricNamePrefix + "sporteventstatuscache-getsporteventstatus";
        private const string MetricNameForProfileCacheGetPlayerProfile = MetricNamePrefix + "profilecache-getplayerprofile";
        private const string MetricNameForProfileCacheGetCompetitorProfile = MetricNamePrefix + "profilecache-getcompetitorprofile";
        private const string MetricNameForSportEventCacheGetAll = MetricNamePrefix + "sporteventcache-getall";
        private const string MetricNameForCacheStoreEviction = MetricNamePrefix + "sdkcache-eviction";
        public static readonly Histogram<long> SportDataCacheGetAll = DefaultMeter.CreateHistogram<long>(MetricNameForSportDataCacheGetAll);
        public static readonly Histogram<long> SportEventCacheGetAll = DefaultMeter.CreateHistogram<long>(MetricNameForSportEventCacheGetAll);
        public static readonly Histogram<long> SportEventStatusCacheGetSportEventStatus = DefaultMeter.CreateHistogram<long>(MetricNameForSportEventStatusCacheGetSportEventStatus);
        public static readonly Histogram<long> ProfileCacheGetPlayerProfile = DefaultMeter.CreateHistogram<long>(MetricNameForProfileCacheGetPlayerProfile);
        public static readonly Histogram<long> ProfileCacheGetCompetitorProfile = DefaultMeter.CreateHistogram<long>(MetricNameForProfileCacheGetCompetitorProfile);
        public static readonly Histogram<long> CacheStoreEviction = DefaultMeter.CreateHistogram<long>(MetricNameForCacheStoreEviction);

        private const string MetricNameForFeedExtRawApiDataReceived = MetricNamePrefix + "extended-rawapidatareceived";
        private const string MetricNameForFeedExtRawFeedDataReceived = MetricNamePrefix + "extended-rawfeeddatareceived";
        public static readonly Histogram<long> FeedExtRawApiDataReceived = DefaultMeter.CreateHistogram<long>(MetricNameForFeedExtRawApiDataReceived);
        public static readonly Histogram<long> FeedExtRawFeedDataReceived = DefaultMeter.CreateHistogram<long>(MetricNameForFeedExtRawFeedDataReceived);

        private const string MetricNameForEventCacheManagerFixtureChanges = MetricNamePrefix + "eventcachemanager-fixturechanges";
        private const string MetricNameForEventCacheManagerResultChanges = MetricNamePrefix + "eventcachemanager-resultchanges";
        public static readonly Histogram<long> EventCacheManagerFixtureChanges = DefaultMeter.CreateHistogram<long>(MetricNameForEventCacheManagerFixtureChanges);
        public static readonly Histogram<long> EventCacheManagerResultChanges = DefaultMeter.CreateHistogram<long>(MetricNameForEventCacheManagerResultChanges);

        private const string MetricNameForSemaphorePoolAcquire = MetricNamePrefix + "semaphorepool-acquire";
        public const string MetricNameForSemaphorePoolAcquireSize = MetricNamePrefix + "semaphorepool-acquiresize";
        public static readonly Histogram<long> SemaphorePoolAcquire = DefaultMeter.CreateHistogram<long>(MetricNameForSemaphorePoolAcquire);

        private const string MetricNameForRabbitMessageReceiverMessageDeserializationTime = MetricNamePrefix + "rabbitmessagereceiver-messagedeserialization";
        private const string MetricNameForRabbitMessageReceiverRawMessageDispatched = MetricNamePrefix + "rabbitmessagereceiver-rawmessagedispatched";
        private const string MetricNameForRabbitMessageReceiverMessageReceived = MetricNamePrefix + "rabbitmessagereceiver-messagereceived";
        private const string MetricNameForRabbitMessageReceiverDeserializationException = MetricNamePrefix + "rabbitmessagereceiver-deserializationexception";
        private const string MetricNameForRabbitMessageReceiverConsumingException = MetricNamePrefix + "rabbitmessagereceiver-consumingexception";
        public static readonly Histogram<long> RabbitMessageReceiverMessageDeserializationTime = DefaultMeter.CreateHistogram<long>(MetricNameForRabbitMessageReceiverMessageDeserializationTime);
        public static readonly Histogram<long> RabbitMessageReceiverRawMessageDispatched = DefaultMeter.CreateHistogram<long>(MetricNameForRabbitMessageReceiverRawMessageDispatched);
        public static readonly Counter<long> RabbitMessageReceiverMessageReceived = DefaultMeter.CreateCounter<long>(MetricNameForRabbitMessageReceiverMessageReceived);
        public static readonly Counter<long> RabbitMessageReceiverDeserializationException = DefaultMeter.CreateCounter<long>(MetricNameForRabbitMessageReceiverDeserializationException);
        public static readonly Counter<long> RabbitMessageReceiverConsumingException = DefaultMeter.CreateCounter<long>(MetricNameForRabbitMessageReceiverConsumingException);

        private const string MetricNameForSportEventFetchMissingSummary = MetricNamePrefix + "sportevent-fetchmissingsummary";
        private const string MetricNameForSportEventFetchMissingFixtures = MetricNamePrefix + "sportevent-fetchmissingfixtures";
        public static readonly Histogram<long> SportEventFetchMissingSummary = DefaultMeter.CreateHistogram<long>(MetricNameForSportEventFetchMissingSummary);
        public static readonly Histogram<long> SportEventFetchMissingFixtures = DefaultMeter.CreateHistogram<long>(MetricNameForSportEventFetchMissingFixtures);

        private const string MetricNameForDataRouterManager = MetricNamePrefix + "dataroutermanager";
        private const string MetricNameForDataRouterManagerGetVariantMarketDescriptionDiff = MetricNamePrefix + "dataroutermanager-getvariantmarketdescriptiondiff";
        public static readonly Histogram<long> DataRouterManager = DefaultMeter.CreateHistogram<long>(MetricNameForDataRouterManager);
        public static readonly Counter<long> DataRouterManagerGetVariantMarketDescriptionDiff = DefaultMeter.CreateCounter<long>(MetricNameForDataRouterManagerGetVariantMarketDescriptionDiff);

        private const string MetricNameForDispatchFeedMessage = MetricNamePrefix + "dispatch-feedmessage";
        private const string MetricNameForDispatchSdkMessage = MetricNamePrefix + "dispatch-sdkmessage";
        public static readonly Histogram<long> DispatchFeedMessage = DefaultMeter.CreateHistogram<long>(MetricNameForDispatchFeedMessage);
        public static readonly Histogram<long> DispatchSdkMessage = DefaultMeter.CreateHistogram<long>(MetricNameForDispatchSdkMessage);

        private const string MetricNameForLocalizedNamedValueCache = MetricNamePrefix + "localizednamedvaluecache-getall";
        private const string MetricNameForNamedValueCache = MetricNamePrefix + "namedvaluecache-getall";
        public static readonly Histogram<long> LocalizedNamedValueCache = DefaultMeter.CreateHistogram<long>(MetricNameForLocalizedNamedValueCache);
        public static readonly Histogram<long> NamedValueCache = DefaultMeter.CreateHistogram<long>(MetricNameForNamedValueCache);

        private const string MetricNameForCacheDistribution = MetricNamePrefix + "cache-distribution";
        public static readonly Histogram<long> CacheDistribution = DefaultMeter.CreateHistogram<long>(MetricNameForCacheDistribution);

        public const string MetricNameForProducerStatus = MetricNamePrefix + "producer-status";
        public const string MetricDescForProducerStatus = "Producer status with optional information about down reason";
    }
}
