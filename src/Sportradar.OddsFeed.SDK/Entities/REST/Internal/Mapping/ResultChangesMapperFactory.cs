/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Creates mapper for result changes
    /// </summary>
    internal class ResultChangesMapperFactory : ISingleTypeMapperFactory<resultChangesEndpoint, IEnumerable<ResultChangeDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance for result changes
        /// </summary>
        /// <param name="data">A <see cref="resultChangesEndpoint" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<IEnumerable<ResultChangeDTO>> CreateMapper(resultChangesEndpoint data)
        {
            return new ResultChangesMapper(data);
        }
    }
}
