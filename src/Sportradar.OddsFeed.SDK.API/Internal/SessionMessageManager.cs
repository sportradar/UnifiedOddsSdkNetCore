/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Stashes or releases stateful messages based on direction from FeedRecoveryManager
    /// </summary>
    /// <seealso cref="ISessionMessageManager" />
    internal class SessionMessageManager : MessageProcessorBase, IFeedMessageProcessor, ISessionMessageManager
    {
        /// <summary>
        /// The <see cref="ILog"/> used for execution logging
        /// </summary>
        private readonly ILog _log = SdkLoggerFactory.GetLogger(typeof(SessionMessageManager));

        /// <summary>
        /// The <see cref="List{T}"/> containing currently stashed(stored) items
        /// </summary>
        private readonly List<StashedItem> _stashedItems;

        /// <summary>
        /// The <see cref="IFeedMessageMapper"/> used to map feed messages
        /// </summary>
        private readonly IFeedMessageMapper _feedMessageMapper;

        /// <summary>
        /// Raised when a alive message is received from the feed
        /// </summary>
        public event EventHandler<AliveEventArgs> AliveReceived;

        /// <summary>
        /// Raised when a snapshot_complete message is received from the feed
        /// </summary>
        public event EventHandler<SnapshotCompleteEventArgs> SnapshotCompleteReceived;

        /// <summary>
        /// Raised when other message is received from the feed
        /// </summary>
        public event EventHandler<FeedMessageReceivedEventArgs> FeedMessageReceived;

        /// <summary>
        /// The processor identifier
        /// </summary>
        /// <value>The processor identifier</value>
        public string ProcessorId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionMessageManager"/> class
        /// </summary>
        public SessionMessageManager(IFeedMessageMapper messageMapper)
        {
            Contract.Requires(messageMapper != null);

            ProcessorId = Guid.NewGuid().ToString().Substring(0, 4);

            _stashedItems = new List<StashedItem>();
            _feedMessageMapper = messageMapper;
        }

        private async Task ReleaseMessagesTask(IProducer producer, long requestId)
        {
            StashedItem stashedItem;
            lock (_stashedItems)
            {
                stashedItem = GetStashedItem(producer.Id);
                if (stashedItem == null || stashedItem.RequestId != requestId)
                {
                    _log.Debug("StashedItem missing. Nothing to release.");
                    return;
                }
                _stashedItems.Remove(stashedItem);
            }

            await Task.Run(() =>
            {
                ReleaseMessagesFromStashedItem(stashedItem);
            }).ConfigureAwait(false);
        }

        private void ReleaseMessagesFromStashedItem(StashedItem stashedItem)
        {
            while (stashedItem.StatefulFeedMessagesQueue.Count > 0)
            {
                var item = stashedItem.StatefulFeedMessagesQueue.Dequeue();
                RaiseOnMessageProcessedEvent(new FeedMessageReceivedEventArgs(item.Message, item.Interest, item.RawMsg));
            }
        }

        private static void EnqueueMessage(StashedItem stashedItem, StashedMessage stashedMessage)
        {
            stashedItem.StatefulFeedMessagesQueue.Enqueue(stashedMessage);
        }

        private StashedItem GetStashedItem(int producerId)
        {
            return _stashedItems.Find(s => s.ProducerId == producerId);
        }

        /// <summary>
        /// Processes and dispatches the provided <see cref="FeedMessage"/> instance
        /// </summary>
        /// <param name="message">A <see cref="FeedMessage"/> instance to be processed</param>
        /// <param name="interest">A <see cref="MessageInterest"/> specifying the interest of the associated session</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        public void ProcessMessage(FeedMessage message, MessageInterest interest, byte[] rawMessage)
        {
            var alive = message as alive;
            if (alive != null)
            {
                AliveReceived?.Invoke(this, new AliveEventArgs(_feedMessageMapper, alive, rawMessage));
            }

            var snap = message as snapshot_complete;
            if (snap != null)
            {
                SnapshotCompleteReceived?.Invoke(this, new SnapshotCompleteEventArgs(_feedMessageMapper, snap, interest, rawMessage));
            }

            FeedMessageReceived?.Invoke(this, new FeedMessageReceivedEventArgs(message, interest, rawMessage));

            RaiseOnMessageProcessedEvent(new FeedMessageReceivedEventArgs(message, interest, rawMessage));
        }

        /// <summary>
        /// Stashes the messages for specified request id and <see cref="IProducer" />
        /// </summary>
        /// <param name="producer">The <see cref="IProducer" /> for which we want to stash messages</param>
        /// <param name="requestId">The request identifier</param>
        public void StashMessages(IProducer producer, long requestId)
        {
            var stashedItem = GetStashedItem(producer.Id);
            if (stashedItem != null)
            {
                ReleaseMessagesTask(producer, stashedItem.RequestId).ConfigureAwait(false);
            }

            _stashedItems.Add(new StashedItem(producer.Id, requestId));
        }

        /// <summary>
        /// Releases messages for specified request id and <see cref="IProducer" />
        /// </summary>
        /// <param name="producer">The <see cref="IProducer" /> for which we want to release messages</param>
        /// <param name="requestId">The request identifier</param>
        public void ReleaseMessages(IProducer producer, long requestId)
        {
            ReleaseMessagesTask(producer, requestId).ConfigureAwait(false);
        }

        private class StashedItem
        {
            public int ProducerId { get; }

            public long RequestId { get; }

            public Queue<StashedMessage> StatefulFeedMessagesQueue { get; }

            public StashedItem(int producerId, long requestId)
            {
                ProducerId = producerId;
                RequestId = requestId;
                StatefulFeedMessagesQueue = new Queue<StashedMessage>();
            }
        }

        private class StashedMessage
        {
            public FeedMessage Message { get; }

            public MessageInterest Interest { get; }

            public byte[] RawMsg { get; }

            public StashedMessage(FeedMessage message, MessageInterest messageInterest, byte[] rawMessage)
            {
                Message = message;
                Interest = messageInterest;
                RawMsg = rawMessage;
            }
        }
    }
}
