/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide mappers used to map instances of one
    /// type to instances of another type
    /// </summary>
    /// <typeparam name="TIn">Specifies the type of data with which the created <see cref="ISingleTypeMapper{T}"/> instances work with</typeparam>
    /// <typeparam name="TOut">Specifies the type returned by created <see cref="ISingleTypeMapper{T}"/> instances </typeparam>
    public interface ISingleTypeMapperFactory<in TIn, out TOut>
        where TIn : class
        where TOut : class
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="ISingleTypeMapper{T}"/> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        ISingleTypeMapper<TOut> CreateMapper(TIn data);
    }
}
