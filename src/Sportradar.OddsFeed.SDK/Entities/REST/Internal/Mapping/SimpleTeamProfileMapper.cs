// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{SimpleTeamProfileDto}" />
    internal class SimpleTeamProfileMapper : ISingleTypeMapper<SimpleTeamProfileDto>
    {
        /// <summary>
        /// A <see cref="simpleTeamProfileEndpoint"/> instance containing simple team profile data
        /// </summary>
        private readonly simpleTeamProfileEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTeamProfileMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="simpleTeamProfileEndpoint"/> instance containing simple team profile data</param>
        internal SimpleTeamProfileMapper(simpleTeamProfileEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="SimpleTeamProfileDto"/> instance
        /// </summary>
        /// <returns>The created <see cref="SimpleTeamProfileDto"/> instance </returns>
        public SimpleTeamProfileDto Map()
        {
            return new SimpleTeamProfileDto(_data);
        }
    }
}
