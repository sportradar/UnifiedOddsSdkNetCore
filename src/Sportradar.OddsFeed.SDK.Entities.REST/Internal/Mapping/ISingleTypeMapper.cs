/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Defines a contract implemented by classes capable of map data to instance of type specified by out parameter
    /// </summary>
    /// <typeparam name="T">Specifies the target type of the <see cref="ISingleTypeMapper{T}"/></typeparam>
    public interface ISingleTypeMapper<out T> where T : class
    {
        /// <summary>
        /// Maps it's data to instance of <typeparamref name="T"/>
        /// </summary>
        /// <returns>The created <typeparamref name="T"/> instance</returns>
        /// <exception cref="MappingException">The mapping of the entity failed</exception>
        T Map();
    }
}
