/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{PeriodSummaryDto}" />
    internal class PeriodSummaryMapper : ISingleTypeMapper<PeriodSummaryDto>
    {
        /// <summary>
        /// A <see cref="stagePeriodEndpoint"/> instance containing data
        /// </summary>
        private readonly stagePeriodEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodSummaryMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="stagePeriodEndpoint"/> instance containing fixture data</param>
        internal PeriodSummaryMapper(stagePeriodEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="PeriodSummaryDto"/> instance
        /// </summary>
        /// <returns>The created <see cref="PeriodSummaryDto"/> instance </returns>
        public PeriodSummaryDto Map()
        {
            return new PeriodSummaryDto(_data);
        }
    }
}
