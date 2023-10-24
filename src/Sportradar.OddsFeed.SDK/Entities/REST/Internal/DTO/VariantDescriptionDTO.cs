/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data transfer object for variant description
    /// </summary>
    internal class VariantDescriptionDto
    {
        internal string Id { get; }

        internal IEnumerable<OutcomeDescriptionDto> Outcomes { get; }

        internal IEnumerable<MarketMappingDto> Mappings { get; }

        internal VariantDescriptionDto(desc_variant description)
        {
            Guard.Argument(description, nameof(description)).NotNull();

            Id = description.id;
            Outcomes = description.outcomes?.Select(o => new OutcomeDescriptionDto(o)).ToList();
            Mappings = description.mappings?.Select(m => new MarketMappingDto(m)).ToList();
        }
    }
}
