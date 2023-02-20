/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.EventArguments
{
    /// <summary>
    /// Event arguments for the alive event
    /// </summary>
    internal class AliveEventArgs : EventArgs
    {
        /// <summary>
        /// A <see cref="IFeedMessageMapper"/> used to map feed message to the one dispatched to the user
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

        /// <summary>
        /// A <see cref="alive"/> message received from the feed
        /// </summary>
        private readonly alive _feedMessage;

        /// <summary>
        /// The raw message
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Is kept for consistency between different messages event arguments")]
        // ReSharper disable once NotAccessedField.Local
        private readonly byte[] _rawMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AliveEventArgs"/> class
        /// </summary>
        /// <param name="messageMapper">A <see cref="IFeedMessageMapper"/> used to map feed message to the one dispatched to the user</param>
        /// <param name="feedMessage">A <see cref="alive"/> message received from the feed</param>
        /// <param name="rawMessage">A raw message received from the feed</param>
        internal AliveEventArgs(IFeedMessageMapper messageMapper, alive feedMessage, byte[] rawMessage)
        {
            Guard.Argument(messageMapper, nameof(messageMapper)).NotNull();
            Guard.Argument(feedMessage, nameof(feedMessage)).NotNull();

            _messageMapper = messageMapper;
            _feedMessage = feedMessage;
            _rawMessage = rawMessage;
        }

        /// <summary>
        /// Gets the <see cref="IAlive"/> implementation representing the received <see cref="alive"/> message
        /// </summary>
        /// <returns>A <see cref="IAlive"/> representing the received <see cref="alive"/> message</returns>
        public IAlive GetAlive()
        {
            return _messageMapper.MapAlive(_feedMessage);
        }
    }
}
