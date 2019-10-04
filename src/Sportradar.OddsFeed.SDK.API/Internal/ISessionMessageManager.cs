/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    ///  Defines a contract implemented by classes capable of processing feed messages and stashing them. Only 1 per session should be used.
    /// </summary>
    [ContractClass(typeof(SessionMessageManagerContract))]
    public interface ISessionMessageManager
    {
        /// <summary>
        /// Stashes the messages for specified request id and <see cref="IProducer"/>
        /// </summary>
        /// <param name="producer">The <see cref="IProducer"/> for which we want to stash messages</param>
        /// <param name="requestId">The request identifier</param>
        void StashMessages(IProducer producer, long requestId);

        /// <summary>
        /// Releases messages for specified request id and <see cref="IProducer"/>
        /// </summary>
        /// <param name="producer">The <see cref="IProducer"/> for which we want to release messages</param>
        /// <param name="requestId">The request identifier</param>
        void ReleaseMessages(IProducer producer, long requestId);

        ///// <summary>
        ///// Raised when a alive message is received from the feed
        ///// </summary>
        //event EventHandler<AliveEventArgs> AliveReceived;

        ///// <summary>
        ///// Raised when a snapshot_complete message is received from the feed
        ///// </summary>
        //event EventHandler<SnapshotCompleteEventArgs> SnapshotCompleteReceived;

        ///// <summary>
        ///// Raised when a product_down message is received from the feed
        ///// </summary>
        //event EventHandler<ProducerStatusChangeEventArgs> ProducerDownReceived;

        /// <summary>
        /// Raised when other message is received from the feed
        /// </summary>
        event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;
    }
}
