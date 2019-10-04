/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes capable of processing feed messages
    /// </summary>
    [ContractClass(typeof(FeedMessageProcessorContract))]
    public interface IFeedMessageProcessor
    {
        /// <summary>
        /// The processor identifier
        /// </summary>
        string ProcessorId { get; }

        /// <summary>
        /// Raised when a <see cref="FeedMessage"/> instance has been processed
        /// </summary>
        event EventHandler<FeedMessageReceivedEventArgs> MessageProcessed;

        /// <summary>
        /// Processes and dispatches the provided <see cref="FeedMessage"/> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance to be processed</param>
        /// <param name="interest">A <see cref="MessageInterest"/> specifying the interest of the associated session</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage);
    }
}
