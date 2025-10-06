// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{DrawDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class DrawSummaryMapperFactory : ISingleTypeMapperFactory<draw_summary, DrawDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{DrawDto}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{DrawDto}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{DrawDto}" /> instance</returns>
        public ISingleTypeMapper<DrawDto> CreateMapper(draw_summary data)
        {
            return new DrawSummaryMapper(data);
        }
    }
}
