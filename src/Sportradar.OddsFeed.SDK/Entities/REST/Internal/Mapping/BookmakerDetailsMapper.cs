// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class BookmakerDetailsMapper : ISingleTypeMapper<BookmakerDetailsDto>
    {
        /// <summary>
        /// A <see cref="bookmaker_details"/> instance containing bookmaker details data
        /// </summary>
        private readonly bookmaker_details _data;

        /// <summary>
        /// The server time difference
        /// </summary>
        private readonly TimeSpan _serverTimeDifference;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetailsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="bookmaker_details"/> instance containing bookmaker details data</param>
        /// <param name="serverTimeDifference">The server time difference</param>
        public BookmakerDetailsMapper(bookmaker_details data, TimeSpan serverTimeDifference)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
            _serverTimeDifference = serverTimeDifference;
        }

        internal static ISingleTypeMapper<BookmakerDetailsDto> Create(bookmaker_details data, TimeSpan serverTimeDifference)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            return new BookmakerDetailsMapper(data, serverTimeDifference);
        }

        BookmakerDetailsDto ISingleTypeMapper<BookmakerDetailsDto>.Map()
        {
            return new BookmakerDetailsDto(_data, _serverTimeDifference);
        }
    }
}
