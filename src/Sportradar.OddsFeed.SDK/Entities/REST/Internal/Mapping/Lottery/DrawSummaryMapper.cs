// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="draw_summary" /> instances to <see cref="DrawDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{DrawDto}" />
    internal class DrawSummaryMapper : ISingleTypeMapper<DrawDto>
    {
        /// <summary>
        /// A <see cref="draw_summary"/> containing rest data
        /// </summary>
        private readonly draw_summary _drawSummary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawSummaryMapper"/> class
        /// </summary>
        /// <param name="drawSummary">A <see cref="draw_summary"/> containing lottery draw data</param>
        internal DrawSummaryMapper(draw_summary drawSummary)
        {
            Guard.Argument(drawSummary, nameof(drawSummary)).NotNull();

            _drawSummary = drawSummary;
        }

        public DrawDto Map()
        {
            return new DrawDto(_drawSummary);
        }
    }
}
