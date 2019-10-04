/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for draw result
    /// </summary>
    internal class DrawResultDTO
    {
        /// <summary>
        /// Gets the value of the draw
        /// </summary>
        public int? Value { get; }

        /// <summary>
        /// Gets the name (translatable)
        /// </summary>
        public string Name { get; }

        internal DrawResultDTO(draw_resultDrawsDraw item)
        {
            Contract.Requires(item != null);

            Value = item.valueSpecified
                ? item.value
                : (int?) null;
            Name = item.name;
        }
    }
}
