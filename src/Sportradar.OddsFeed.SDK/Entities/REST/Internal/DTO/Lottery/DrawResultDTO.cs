/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for draw result
    /// </summary>
    internal class DrawResultDto
    {
        /// <summary>
        /// Gets the value of the draw
        /// </summary>
        public int? Value { get; }

        /// <summary>
        /// Gets the name (translatable)
        /// </summary>
        public string Name { get; }

        internal DrawResultDto(draw_resultDrawsDraw item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Value = item.valueSpecified
                ? item.value
                : (int?)null;
            Name = item.name;
        }
    }
}
