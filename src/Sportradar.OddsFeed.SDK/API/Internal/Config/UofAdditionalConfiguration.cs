// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofAdditionalConfiguration : IUofAdditionalConfiguration
    {
        public TimeSpan StatisticsInterval { get; set; }

        public bool OmitMarketMappings { get; set; }

        public UofAdditionalConfiguration()
        {
            StatisticsInterval = TimeSpan.FromMinutes(ConfigLimit.StatisticsIntervalMinutesDefault);
            OmitMarketMappings = false;
        }

        public override string ToString()
        {
            var summaryValues = new Dictionary<string, string>
                                    {
                                        { "OmitMarketMappings", OmitMarketMappings.ToString() },
                                        { "StatisticsInterval", StatisticsInterval.TotalMinutes.ToString(CultureInfo.InvariantCulture) }
                                    };
            return "AdditionalConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
