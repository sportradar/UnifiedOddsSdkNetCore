/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data transfer object for market description
    /// </summary>
    public class MarketDescriptionDTO
    {
        internal long Id { get; }

        internal string Name { get; }

        internal string Description { get; }

        internal string Variant { get; }

        internal string OutcomeType { get; }

        internal IEnumerable<OutcomeDescriptionDTO> Outcomes { get; }

        internal IEnumerable<SpecifierDTO> Specifiers { get; }

        internal IEnumerable<MarketMappingDTO> Mappings { get; }

        internal IEnumerable<MarketAttributeDTO> Attributes { get; }

        internal IEnumerable<string> Groups { get; }

        internal MarketDescriptionDTO(desc_market description)
        {
            Contract.Requires(description != null);
            Contract.Requires(!string.IsNullOrEmpty(description.name));

            Id = description.id;
            Name = description.name;
            Description = description.description;
            Outcomes = description.outcomes?.Select(o => new OutcomeDescriptionDTO(o)).ToList();
            Specifiers = description.specifiers?.Select(s => new SpecifierDTO(s)).ToList();
            Mappings = description.mappings?.Select(m => new MarketMappingDTO(m)).ToList();
            Attributes = description.attributes?.Select(a => new MarketAttributeDTO(a)).ToList();
            Variant = description.variant;
            OutcomeType = mapOutcomeType(description.outcome_type, description.includes_outcomes_of_type);
            Groups = description.groups?.Split(new[] {SdkInfo.MarketGroupsDelimiter}, StringSplitOptions.RemoveEmptyEntries);
        }

        private string mapOutcomeType(string outcomeType, string includesOutcomesOfType)
        {
            if (outcomeType != null)
                return outcomeType;

            if (includesOutcomesOfType == null)
                return null;

            if (includesOutcomesOfType == SdkInfo.OutcometextVariantValue)
                return SdkInfo.FreeTextVariantValue;

            if (includesOutcomesOfType.StartsWith("sr:"))
                return includesOutcomesOfType.Substring(3);

            return null;
        }
    }
}
