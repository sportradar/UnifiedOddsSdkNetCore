/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Contracts
{
    [ContractClassFor(typeof(IFeedMessageProcessor))]
    internal abstract class FeedMessageProcessorContract : IFeedMessageProcessor
    {
        [Pure]
        public event EventHandler<FeedMessageReceivedEventArgs> MessageProcessed;

        public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            Contract.Requires(message != null);
            Contract.Requires(interest != null);
        }

        public string ProcessorId
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                return Contract.Result<string>();
            }
        }
    }
}
