/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data transfer object for variant description
    /// </summary>
    public class VariantDescriptionDTO
    {
        internal string Id { get; }

        internal IEnumerable<OutcomeDescriptionDTO> Outcomes { get; }

        internal IEnumerable<MarketMappingDTO> Mappings { get; }

        internal VariantDescriptionDTO(desc_variant description)
        {
            Guard.Argument(description, nameof(description)).NotNull();

            Id = description.id;
            Outcomes = description.outcomes?.Select(o => new OutcomeDescriptionDTO(o)).ToList();
            Mappings = description.mappings?.Select(m => new MarketMappingDTO(m)).ToList();
        }
    }
}
