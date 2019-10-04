/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="simpleTeamProfileEndpoint"/> instances to <see cref="SimpleTeamProfileDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{SimpleTeamProfileDTO}" />
    internal class SimpleTeamProfileMapper : ISingleTypeMapper<SimpleTeamProfileDTO>
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
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="SimpleTeamProfileDTO"/> instance
        /// </summary>
        /// <returns>The created <see cref="SimpleTeamProfileDTO"/> instance </returns>
        public SimpleTeamProfileDTO Map()
        {
            return new SimpleTeamProfileDTO(_data);
        }
    }
}