// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofRabbitConfiguration : IUofRabbitConfiguration
    {
        public bool UseSsl { get; internal set; }
        public string Host { get; internal set; }
        public int Port { get; internal set; }
        public string Username { get; internal set; }
        public string Password { get; internal set; }
        public string VirtualHost { get; internal set; }
        public TimeSpan ConnectionTimeout { get; internal set; }
        public TimeSpan Heartbeat { get; internal set; }

        public UofRabbitConfiguration()
        {
            UseSsl = true;
            ConnectionTimeout = TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault);
            Heartbeat = TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault);
        }

        public override string ToString()
        {
            var sanitizedUsername = SdkInfo.ClearSensitiveData(Username);
            var sanitizedPassword = SdkInfo.ClearSensitiveData(Password);

            var summaryValues = new Dictionary<string, string>
            {
                { "Host", Host },
                { "Port", Port.ToString(CultureInfo.InvariantCulture) },
                { "UseSsl", UseSsl.ToString(CultureInfo.InvariantCulture) },
                { "VirtualHost", VirtualHost },
                { "Username", sanitizedUsername },
                { "Password", sanitizedPassword },
                { "ConnectionTimeout", ConnectionTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "Heartbeat", Heartbeat.TotalSeconds.ToString(CultureInfo.InvariantCulture) }
            };
            return "RabbitConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
