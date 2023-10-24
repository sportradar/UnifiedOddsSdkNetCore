/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{SportEventSummaryDto}"/> used to retrieve sport event summary
    /// </summary>
    /// <seealso cref="DataProvider{RestMessage, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    internal class SportEventSummaryProvider : DataProvider<RestMessage, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// An address format used to retrieve sport event summary
        /// </summary>
        private readonly string _sportEventSummaryUriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventSummaryProvider"/> class
        /// </summary>
        /// <param name="sportEventSummaryUriFormat">An address format used to retrieve sport event summary</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public SportEventSummaryProvider(
            string sportEventSummaryUriFormat,
            IDataFetcher fetcher,
            IDeserializer<RestMessage> deserializer,
            ISingleTypeMapperFactory<RestMessage, EntityList<SportEventSummaryDto>> mapperFactory)
            : base(sportEventSummaryUriFormat, fetcher, deserializer, mapperFactory)
        {
            Guard.Argument(sportEventSummaryUriFormat, nameof(sportEventSummaryUriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();

            _sportEventSummaryUriFormat = sportEventSummaryUriFormat;
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <c>id</c>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <c>identifiers</c></returns>
        protected override Uri GetRequestUri(params object[] identifiers)
        {
            return identifiers.Length == 1
                ? new Uri(string.Format(_sportEventSummaryUriFormat, identifiers))
                : base.GetRequestUri(identifiers);
        }
    }
}
