/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{CompetitorProfileDto}" />
    internal class CompetitorProfileMapper : ISingleTypeMapper<CompetitorProfileDto>
    {
        /// <summary>
        /// A <see cref="competitorProfileEndpoint"/> instance containing competitor profile data
        /// </summary>
        private readonly competitorProfileEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorProfileMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="competitorProfileEndpoint"/> instance containing competitor profile data</param>
        internal CompetitorProfileMapper(competitorProfileEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="CompetitorProfileDto"/> instance
        /// </summary>
        /// <returns>The created <see cref="CompetitorProfileDto"/> instance </returns>
        public CompetitorProfileDto Map()
        {
            return new CompetitorProfileDto(_data);
        }
    }
}
