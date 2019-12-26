/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="competitorProfileEndpoint"/> instances to <see cref="CompetitorProfileDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{CompetitorProfileDTO}" />
    internal class CompetitorProfileMapper : ISingleTypeMapper<CompetitorProfileDTO>
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
        /// Maps it's data to <see cref="CompetitorProfileDTO"/> instance
        /// </summary>
        /// <returns>The created <see cref="CompetitorProfileDTO"/> instance </returns>
        public CompetitorProfileDTO Map()
        {
            return new CompetitorProfileDTO(_data);
        }
    }
}