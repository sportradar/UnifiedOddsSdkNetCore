using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofCacheConfiguration : IUofCacheConfiguration
    {
        public TimeSpan SportEventCacheTimeout { get; set; }
        public TimeSpan SportEventStatusCacheTimeout { get; set; }
        public TimeSpan ProfileCacheTimeout { get; set; }
        public TimeSpan VariantMarketDescriptionCacheTimeout { get; set; }
        public TimeSpan IgnoreBetPalTimelineSportEventStatusCacheTimeout { get; set; }
        public bool IgnoreBetPalTimelineSportEventStatus { get; set; }

        public UofCacheConfiguration()
        {
            SportEventCacheTimeout = TimeSpan.FromHours(ConfigLimit.SportEventCacheTimeoutDefault);
            SportEventStatusCacheTimeout = TimeSpan.FromMinutes(ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault);
            ProfileCacheTimeout = TimeSpan.FromHours(ConfigLimit.ProfileCacheTimeoutDefault);
            VariantMarketDescriptionCacheTimeout = TimeSpan.FromHours(ConfigLimit.SingleVariantMarketTimeoutDefault);
            IgnoreBetPalTimelineSportEventStatusCacheTimeout = TimeSpan.FromHours(ConfigLimit.IgnoreBetpalTimelineTimeoutDefault);
            IgnoreBetPalTimelineSportEventStatus = false;
        }

        public override string ToString()
        {
            var summaryValues = new Dictionary<string, string>
            {
                { "SportEventCacheTimeout", SportEventCacheTimeout.TotalHours.ToString(CultureInfo.InvariantCulture) },
                { "SportEventStatusCacheTimeout", SportEventStatusCacheTimeout.TotalMinutes.ToString(CultureInfo.InvariantCulture) },
                { "ProfileCacheTimeout", ProfileCacheTimeout.TotalHours.ToString(CultureInfo.InvariantCulture) },
                { "VariantMarketDescriptionCacheTimeout", VariantMarketDescriptionCacheTimeout.TotalHours.ToString() },
                { "IgnoreBetPalTimelineSportEventStatusCacheTimeout", IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours.ToString(CultureInfo.InvariantCulture) },
                { "IgnoreBetPalTimelineSportEventStatus", IgnoreBetPalTimelineSportEventStatus.ToString() }
            };
            return "CacheConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
