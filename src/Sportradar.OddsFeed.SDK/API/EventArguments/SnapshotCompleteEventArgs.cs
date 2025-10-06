// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api.EventArguments
{
    /// <summary>
    /// Event arguments for the snapshot complete message received via session message manager
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
        [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Is kept for consistency between different messages event arguments")]
        // ReSharper disable once NotAccessedField.Local
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
            Guard.Argument(messageMapper, nameof(messageMapper)).NotNull();
            Guard.Argument(feedMessage, nameof(feedMessage)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();

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
    }
}
