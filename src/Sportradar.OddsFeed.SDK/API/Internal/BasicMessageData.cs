/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A class used to hold basic information about the received message
    /// </summary>
    internal class BasicMessageData
    {
        /// <summary>
        /// A <see cref="MessageType"/> specifying the type of the message
        /// </summary>
        internal MessageType MessageType { get; }

        /// <summary>
        /// A <see cref="string"/> representation of the producer id
        /// </summary>
        internal string ProducerId { get; }

        /// <summary>
        /// A <see cref="string"/> representation of the event id
        /// </summary>
        internal string EventId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicMessageData"/> class.
        /// </summary>
        /// <param name="messageType">A <see cref="MessageType"/> specifying the type of the message.</param>
        /// <param name="producerId">A <see cref="string"/> representation of the producer id.</param>
        /// <param name="eventId">A <see cref="string"/> representation of the event id.</param>
        internal BasicMessageData(MessageType messageType, string producerId, string eventId)
        {
            MessageType = messageType;
            EventId = eventId;
            ProducerId = producerId;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var producerId = string.IsNullOrEmpty(ProducerId)
                ? "[null]"
                : ProducerId;
            var eventId = string.IsNullOrEmpty(EventId)
                ? "[null]"
                : EventId;

            return $"MessageType={Enum.GetName(typeof(MessageType), MessageType)} ProducerId={producerId} EventId={eventId}";
        }
    }
}