/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for streaming channel
    /// </summary>
    internal class StreamingChannelDto
    {
        internal int Id { get; }

        internal string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChannelDto"/> class.
        /// </summary>
        /// <param name="channel">The <see cref="streamingChannel"/> used for creating instance</param>
        internal StreamingChannelDto(streamingChannel channel)
        {
            Guard.Argument(channel, nameof(channel)).NotNull();
            Guard.Argument(channel.name, nameof(channel.name)).NotNull().NotEmpty();

            Id = channel.id;
            Name = channel.name;
        }
    }
}
