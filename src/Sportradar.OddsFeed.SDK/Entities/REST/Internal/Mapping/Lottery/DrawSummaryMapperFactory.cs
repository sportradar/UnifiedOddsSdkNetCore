/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{DrawDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class DrawSummaryMapperFactory : ISingleTypeMapperFactory<draw_summary, DrawDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{DrawDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{DrawDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{DrawDTO}" /> instance</returns>
        public ISingleTypeMapper<DrawDTO> CreateMapper(draw_summary data)
        {
            return new DrawSummaryMapper(data);
        }
    }
}
