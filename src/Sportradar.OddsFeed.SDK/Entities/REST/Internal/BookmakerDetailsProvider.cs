/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{BookmakerDetailsDTO}"/> used to retrieve bookmaker details
    /// </summary>
    /// <seealso cref="DataProvider{bookmaker_details, BookmakerDetailsDTO}" />
    internal class BookmakerDetailsProvider : DataProvider<bookmaker_details, BookmakerDetailsDTO>
    {
        /// <summary>
        /// A <see cref="IDataFetcher"/> used to fetch the data
        /// </summary>
        private readonly IDataFetcher _fetcher;

        /// <summary>
        /// A <see cref="IDeserializer{T}"/> used to deserialize the fetch data
        /// </summary>
        private readonly IDeserializer<bookmaker_details> _deserializer;

        /// <summary>
        /// A <see cref="ISingleTypeMapperFactory{T, T1}"/> used to construct instances of <see cref="ISingleTypeMapper{T}"/>
        /// </summary>
        private readonly ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDTO> _mapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookmakerDetailsProvider"/> class
        /// </summary>
        /// <param name="bookmakerDetailsUriFormat">An address format used to retrieve sport event summary</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public BookmakerDetailsProvider(
            string bookmakerDetailsUriFormat,
            IDataFetcher fetcher,
            IDeserializer<bookmaker_details> deserializer,
            ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDTO> mapperFactory)
            : base(bookmakerDetailsUriFormat, fetcher, deserializer, mapperFactory)
        {
            Guard.Argument(bookmakerDetailsUriFormat, nameof(bookmakerDetailsUriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();

            _fetcher = fetcher;
            _deserializer = deserializer;
            _mapperFactory = mapperFactory;
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <code>languageCode</code>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public new virtual BookmakerDetailsDTO GetData(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            var stream = _fetcher.GetData(uri);
            var bookmakerDetailsDTO = _mapperFactory.CreateMapper(_deserializer.Deserialize(stream)).Map();

            bookmakerDetailsDTO.ServerTimeDifference = TimeSpan.Zero;

            if (_fetcher is HttpDataFetcher httpDataFetcher)
            {
                if (httpDataFetcher.ResponseHeaders != null && httpDataFetcher.ResponseHeaders.TryGetValue("Date", out var x))
                {
                    var date = SdkInfo.ParseDate(x.FirstOrDefault());
                    if (date != null)
                    {
                        bookmakerDetailsDTO.ServerTimeDifference = DateTime.Now - date.Value;
                    }
                }
            }

            return bookmakerDetailsDTO;
        }
    }
}
