/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="stagePeriodEndpoint"/> instances to <see cref="PeriodSummaryDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{PeriodSummaryDTO}" />
    internal class PeriodSummaryMapper : ISingleTypeMapper<PeriodSummaryDTO>
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
        /// Maps it's data to <see cref="PeriodSummaryDTO"/> instance
        /// </summary>
        /// <returns>The created <see cref="PeriodSummaryDTO"/> instance </returns>
        public PeriodSummaryDTO Map()
        {
            return new PeriodSummaryDTO(_data);
        }
    }
}
