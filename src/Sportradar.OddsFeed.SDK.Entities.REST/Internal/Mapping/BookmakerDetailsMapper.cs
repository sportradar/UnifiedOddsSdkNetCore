/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    internal class BookmakerDetailsMapper : ISingleTypeMapper<BookmakerDetailsDTO>
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
            Guard.Argument(data).NotNull();
            Guard.Argument(serverTimeDifference).Require(serverTimeDifference != null);

            _data = data;
            _serverTimeDifference = serverTimeDifference;
        }

        internal static ISingleTypeMapper<BookmakerDetailsDTO> Create(bookmaker_details data, TimeSpan serverTimeDifference)
        {
            Guard.Argument(data).NotNull();
            Guard.Argument(serverTimeDifference).Require(serverTimeDifference != null);

            return new BookmakerDetailsMapper(data, serverTimeDifference);
        }

        BookmakerDetailsDTO ISingleTypeMapper<BookmakerDetailsDTO>.Map()
        {
            return new BookmakerDetailsDTO(_data, _serverTimeDifference);
        }
    }
}
