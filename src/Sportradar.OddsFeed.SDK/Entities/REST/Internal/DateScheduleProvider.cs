// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ISportEventsSchedule}"/> used to retrieve sport events scheduled for a specified date
    /// or currently live sport events
    /// </summary>
    /// <seealso cref="DataProvider{scheduleType, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    internal class DateScheduleProvider : DataProviderNamed<scheduleEndpoint, EntityList<SportEventSummaryDto>>
    {
        /// <summary>
        /// An address format used to retrieve live sport events
        /// </summary>
        private readonly string _liveScheduleUriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateScheduleProvider"/> class.
        /// </summary>
        /// <param name="name">Name for the date schedule provider</param>
        /// <param name="liveScheduleUriFormat">An address format used to retrieve live sport events</param>
        /// <param name="dateScheduleUriFormat">An address format used to retrieve sport events for a specified date</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{scheduleType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{scheduleType,EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ISportEventsSchedule}" /></param>
        public DateScheduleProvider(string name,
            string liveScheduleUriFormat,
            string dateScheduleUriFormat,
            IDataFetcher fetcher,
            IDeserializer<scheduleEndpoint> deserializer,
            ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>> mapperFactory)
            : base(name, dateScheduleUriFormat, fetcher, deserializer, mapperFactory)
        {
            Guard.Argument(liveScheduleUriFormat, nameof(liveScheduleUriFormat)).NotNull().NotEmpty();
            Guard.Argument(dateScheduleUriFormat, nameof(dateScheduleUriFormat)).NotNull().NotEmpty();

            _liveScheduleUriFormat = liveScheduleUriFormat;
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <c>id</c>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <c>identifiers</c></returns>
        protected override Uri GetRequestUri(params string[] identifiers)
        {
            // ReSharper disable once CoVariantArrayConversion
            return identifiers.Length == 1
                       ? new Uri(string.Format(CultureInfo.InvariantCulture, _liveScheduleUriFormat, identifiers))
                       : base.GetRequestUri(identifiers);
        }
    }
}
