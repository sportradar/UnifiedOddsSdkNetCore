// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Class BookmakerDetailsMapperFactory
    /// </summary>
    internal class BookmakerDetailsMapperFactory : ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDto>
    {
        /// <summary>
        /// Creates and returns an instance of Mapper for mapping <see cref="bookmaker_details" />
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="BookmakerDetailsMapper" /> will map</param>
        /// <returns>New <see cref="BookmakerDetailsMapper" /> instance</returns>
        public ISingleTypeMapper<BookmakerDetailsDto> CreateMapper(bookmaker_details data)
        {
            return BookmakerDetailsMapper.Create(data, TimeSpan.Zero);
        }
    }
}
