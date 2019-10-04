/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for streaming channel
    /// </summary>
    internal class StreamingChannelDTO
    {
        internal int Id { get; }

        internal string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChannelDTO"/> class.
        /// </summary>
        /// <param name="channel">The <see cref="streamingChannel"/> used for creating instance</param>
        internal StreamingChannelDTO(streamingChannel channel)
        {
            Contract.Requires(channel != null);
            Contract.Requires(!string.IsNullOrEmpty(channel.name));

            Id = channel.id;
            Name = channel.name;
        }
    }
}