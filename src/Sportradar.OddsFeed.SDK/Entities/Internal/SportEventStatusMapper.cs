// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Class SportEventStatusMapper.
    /// </summary>
    /// <seealso cref="ISportEventStatus" />
    /// <seealso cref="ISingleTypeMapper{ISportEventStatus}" />
    internal class SportEventStatusMapper : ISingleTypeMapper<SportEventStatusDto>
    {
        /// <summary>
        /// A <see cref="sportEventStatus" /> instance containing status data about the associated sport event
        /// </summary>
        private readonly sportEventStatus _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventStatusMapper" /> class.
        /// </summary>
        /// <param name="record">A <see cref="sportEventStatus" /> instance containing status data about the associated sport event</param>
        internal SportEventStatusMapper(sportEventStatus record)
        {
            Guard.Argument(record, nameof(record)).NotNull();

            _data = record;
        }

        /// <summary>
        /// Construct and returns a new instance of <see cref="ISingleTypeMapper{ISportEventStatus}"/> instance
        /// </summary>
        /// <param name="data">A <see cref="sportEventStatus"/> containing data used to build the <see cref="ISportEventStatus"/> instance</param>
        /// <returns>a new instance of <see cref="ISingleTypeMapper{ISportEventStatus}"/> instance</returns>
        internal static ISingleTypeMapper<SportEventStatusDto> Create(sportEventStatus data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            return new SportEventStatusMapper(data);
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="SportEventStatusDto"/>
        /// </summary>
        public SportEventStatusDto Map()
        {
            return new SportEventStatusDto(_data, null);
        }
    }
}
