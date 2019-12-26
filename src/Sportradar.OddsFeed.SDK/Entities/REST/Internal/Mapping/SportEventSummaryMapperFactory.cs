/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{SportEventSummaryDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class SportEventSummaryMapperFactory : ISingleTypeMapperFactory<RestMessage, SportEventSummaryDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{SportEventSummaryDTO}" /> instance</returns>
        public ISingleTypeMapper<SportEventSummaryDTO> CreateMapper(RestMessage data)
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
