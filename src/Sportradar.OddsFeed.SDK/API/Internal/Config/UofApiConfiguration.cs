using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofApiConfiguration : IUofApiConfiguration
    {
        public string Host { get; set; }
        public string BaseUrl => UseSsl ? "https://" + Host : "http://" + Host;
        public bool UseSsl { get; set; }
        public TimeSpan HttpClientTimeout { get; set; }
        public TimeSpan HttpClientRecoveryTimeout { get; set; }
        public TimeSpan HttpClientFastFailingTimeout { get; set; }
        public string ReplayHost => Host + "/v1/replay";
        public string ReplayBaseUrl => BaseUrl + "/v1/replay";
        public int MaxConnectionsPerServer { get; set; }

        public UofApiConfiguration()
        {
            UseSsl = true;
            HttpClientTimeout = TimeSpan.FromSeconds(ConfigLimit.HttpClientTimeoutDefault);
            HttpClientRecoveryTimeout = TimeSpan.FromSeconds(ConfigLimit.HttpClientRecoveryTimeoutDefault);
            HttpClientFastFailingTimeout = TimeSpan.FromSeconds(ConfigLimit.HttpClientFastFailingTimeoutDefault);
            MaxConnectionsPerServer = ConfigLimit.MaxConnectionsPerServerDefault;
        }

        public override string ToString()
        {
            var summaryValues = new Dictionary<string, string>
            {
                { "Host", Host },
                { "UseSsl", UseSsl.ToString() },
                { "HttpClientTimeout", HttpClientTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "HttpClientRecoveryTimeout", HttpClientRecoveryTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "HttpClientFastFailingTimeout", HttpClientFastFailingTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "MaxConnectionsPerServer", MaxConnectionsPerServer.ToString(CultureInfo.InvariantCulture) }
            };
            return "ApiConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
