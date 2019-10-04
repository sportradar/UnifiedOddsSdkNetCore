/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="draw_summary" /> instances to <see cref="DrawDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{DrawDTO}" />
    internal class DrawSummaryMapper : ISingleTypeMapper<DrawDTO>
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
            Contract.Requires(drawSummary != null);

            _drawSummary = drawSummary;
        }

        public DrawDTO Map()
        {
            return new DrawDTO(_drawSummary);
        }
    }
}
