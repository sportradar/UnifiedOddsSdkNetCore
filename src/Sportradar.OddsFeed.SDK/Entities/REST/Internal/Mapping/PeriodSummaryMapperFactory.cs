/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{PeriodSummaryDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class PeriodSummaryMapperFactory : ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="stagePeriodEndpoint" /> instance containing fixture data</param>
        /// <returns>a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDto"/> instances</returns>
        public ISingleTypeMapper<PeriodSummaryDto> CreateMapper(stagePeriodEndpoint data)
        {
            return new PeriodSummaryMapper(data);
        }
    }
}
