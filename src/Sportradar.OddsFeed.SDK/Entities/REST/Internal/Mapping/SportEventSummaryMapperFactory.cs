/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SportEventSummaryDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class SportEventSummaryMapperFactory : ISingleTypeMapperFactory<RestMessage, SportEventSummaryDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{SportEventSummaryDto}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{SportEventSummaryDto}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{SportEventSummaryDto}" /> instance</returns>
        public ISingleTypeMapper<SportEventSummaryDto> CreateMapper(RestMessage data)
        {
            var match = data as matchSummaryEndpoint;
            if (match != null)
            {
                return new SportEventSummaryMapper(match);
            }
            var stage = data as stageSummaryEndpoint;
            if (stage != null)
            {
                return new SportEventSummaryMapper(stage);
            }
            var tour = data as tournamentInfoEndpoint;
            if (tour != null)
            {
                return new SportEventSummaryMapper(tour);
            }

            throw new ArgumentException($"Unknown data type. Type={data.GetType().Name}", nameof(data));
        }
    }
}
