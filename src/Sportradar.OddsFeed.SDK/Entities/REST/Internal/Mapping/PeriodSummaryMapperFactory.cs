/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{PeriodSummaryDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class PeriodSummaryMapperFactory : ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="stagePeriodEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDTO"/> instances</returns>
        public ISingleTypeMapper<PeriodSummaryDTO> CreateMapper(stagePeriodEndpoint data)
        {
            return new PeriodSummaryMapper(data);
        }
    }
}
