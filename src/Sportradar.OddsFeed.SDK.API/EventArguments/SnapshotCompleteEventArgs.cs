/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Text;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the <see cref="ISessionMessageManager.FeedMessageReceived"/> event
    /// </summary>
    public class SnapshotCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// A <see cref="IFeedMessageMapper"/> used to map feed message to the one dispatched to the user
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

        /// <summary>
        /// A <see cref="snapshot_complete"/> message received from the feed
        /// </summary>
        private readonly snapshot_complete _feedMessage;

        /// <summary>
        /// A <see cref="MessageInterest"/> of the session which received the message
        /// </summary>
        private readonly MessageInterest _interest;

        /// <summary>
        /// The raw message
        /// </summary>
        private readonly byte[] _rawMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotCompleteEventArgs"/> class
        /// </summary>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map feed message to the one dispatched to the user</param>
        /// <param name="feedMessage">A <see cref="snapshot_complete"/> message received from the feed</param>
        /// <param name="interest">A <see cref="MessageInterest"/> of the session which received the message</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        internal SnapshotCompleteEventArgs(IFeedMessageMapper messageMapper, snapshot_complete feedMessage, MessageInterest interest, byte[] rawMessage)
        {
            Guard.Argument(messageMapper).NotNull();
            Guard.Argument(feedMessage).NotNull();
            Guard.Argument(interest).NotNull();

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _interest = interest;
            _rawMessage = rawMessage;
        }

        /// <summary>
        /// Gets the <see cref="ISnapshotCompleted"/> implementation representing the received <see cref="snapshot_complete"/> message
        /// </summary>
        /// <returns>A <see cref="ISnapshotCompleted"/> representing the received <see cref="snapshot_complete"/> message</returns>
        public ISnapshotCompleted GetSnapshotCompleted()
        {
            return _messageMapper.MapSnapShotCompleted(_feedMessage);
        }

        /// <summary>
        /// Gets the <see cref="MessageInterest"/> from which session came the snapshot_complete message
        /// </summary>
        /// <returns>Returns the <see cref="MessageInterest"/> from which session came the snapshot_complete message</returns>
        internal MessageInterest GetMessageInterest()
        {
            return _interest;
        }

        /// <summary>
        /// Gets the raw xml message received from the feed
        /// </summary>
        /// <returns>Returns the raw xml message received from the feed</returns>
        [Obsolete("The message was moved to event")]
        public string GetRawMessage()
        {
            return _rawMessage == null ? null : Encoding.UTF8.GetString(_rawMessage);
        }
    }
}
