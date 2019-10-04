/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to dispatch entities.
    /// </summary>
    [ContractClass(typeof(EntityDispatcherInternalContract))]
    internal interface IEntityDispatcherInternal : IOpenable
    {
        /// <summary>
        /// Dispatches the provided <see cref="FeedMessage"/> to the user
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> to dispatch.</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        void Dispatch(FeedMessage message, byte[] rawMessage);
    }
}
