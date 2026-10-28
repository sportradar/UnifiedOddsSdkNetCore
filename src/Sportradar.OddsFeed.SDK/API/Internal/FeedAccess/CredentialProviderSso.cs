// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess
{
    internal class CredentialProviderSso : ICredentialsProvider
    {
        public void Refresh()
        {
            // No-op
        }

        public string Name
        {
            get;
        }
        public string UserName
        {
            get;
        }
        public string Password
        {
            get;
        }
        public TimeSpan? ValidUntil
        {
            get;
        }

        public CredentialProviderSso(IUofConfiguration config)
        {
            _ = Guard.Argument(config, nameof(config)).NotNull();

            Name = "Sso";
            UserName = config.Rabbit.Username;
            Password = config.Rabbit.Password ?? string.Empty;
            ValidUntil = config.BookmakerDetails == null
                             ? (TimeSpan?)null
                             : config.BookmakerDetails.ExpireAt - DateTime.Now;
        }
    }
}
