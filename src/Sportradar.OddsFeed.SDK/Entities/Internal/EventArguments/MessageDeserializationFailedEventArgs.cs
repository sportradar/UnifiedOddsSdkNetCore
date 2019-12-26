/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments
{
    /// <summary>
    /// Event arguments for the <see cref="IMessageReceiver.FeedMessageDeserializationFailed"/> event
    /// </summary>
    public class MessageDeserializationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IEnumerable{Byte}"/> containing message unprocessed data
        /// </summary>
        public IEnumerable<byte> RawData { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDeserializationFailedEventArgs"/> class
        /// </summary>
        /// <param name="rawData">the name of the message which could not be deserialized, or a null reference if message name could not be retrieved</param>
        internal MessageDeserializationFailedEventArgs(IEnumerable<byte> rawData)
        {
            Guard.Argument(rawData, nameof(rawData)).NotNull().NotEmpty();

            RawData = rawData;
        }
    }
}
