// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofUsageConfiguration : IUofUsageConfiguration
    {
        public bool IsExportEnabled { get; internal set; } = true;
        public int ExportIntervalInSec { get; } = 300;
        public int ExportTimeoutInSec { get; } = 20;
        public string Host { get; internal set; } = string.Empty;

        public override string ToString()
        {
            var summaryValues = new Dictionary<string, string>
            {
                { "IsExportEnabled", IsExportEnabled.ToString() },
                { "ExportIntervalInSec", ExportIntervalInSec.ToString() },
                { "ExportTimeoutInSec", ExportTimeoutInSec.ToString() },
                { "Host", Host }
            };
            return "UsageConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
