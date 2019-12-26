/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    internal class StreamingChannelCI
    {
        /// <summary>
        /// The <see cref="Id"/> property backing field
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// The <see cref="Name"/> property backing field
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Gets a value uniquely identifying the current streaming channel
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Gets the name of the streaming channel represented by the current instance
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingChannelCI"/> class.
        /// </summary>
        /// <param name="dto">a value uniquely identifying the current streaming channel</param>
        internal StreamingChannelCI(StreamingChannelDTO dto)
        {
            _id = dto.Id;
            _name = dto.Name;
        }
    }
}
