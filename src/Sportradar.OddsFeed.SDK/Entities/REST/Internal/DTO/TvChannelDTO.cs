/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for tv channel
    /// </summary>
    internal class TvChannelDto
    {
        internal string Name { get; }

        internal DateTime? StartTime { get; }

        internal string StreamUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TvChannelDto"/> class
        /// </summary>
        /// <param name="tvChannel">The <see cref="tvChannel"/> used for creating instance</param>
        internal TvChannelDto(tvChannel tvChannel)
        {
            Guard.Argument(tvChannel, nameof(tvChannel)).NotNull();
            Guard.Argument(tvChannel.name, nameof(tvChannel.name)).NotNull().NotEmpty();

            Name = tvChannel.name;
            StartTime = tvChannel.start_timeSpecified
                ? (DateTime?)tvChannel.start_time
                : null;
            StreamUrl = tvChannel.stream_url;
        }
    }
}
