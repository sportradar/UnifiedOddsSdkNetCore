/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Contracts
{
    [ContractClassFor(typeof(IRabbitMqChannel))]
    internal abstract class RabbitMqChannelContract : IRabbitMqChannel
    {
        public bool IsOpened => Contract.Result<bool>();

        public void Open(IEnumerable<string> routingKeys)
        {
            Contract.Requires(routingKeys != null);
            Contract.Requires(routingKeys.Any());
            Contract.Ensures(IsOpened);
        }

        public void Close()
        {
            Contract.Ensures(!IsOpened);
        }

        [Pure]
        public event EventHandler<BasicDeliverEventArgs> Received;
    }
}