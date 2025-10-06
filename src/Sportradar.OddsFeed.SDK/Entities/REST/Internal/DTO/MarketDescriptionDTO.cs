// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data transfer object for market description
    /// </summary>
    internal class MarketDescriptionDto
    {
        internal long Id { get; private set; }

        internal string Name { get; }

        internal string Description { get; }

        internal string Variant { get; }

        internal string OutcomeType { get; }

        internal ICollection<OutcomeDescriptionDto> Outcomes { get; }

        internal ICollection<SpecifierDto> Specifiers { get; }

        internal ICollection<MarketMappingDto> Mappings { get; }

        internal ICollection<MarketAttributeDto> Attributes { get; }

        internal ICollection<string> Groups { get; private set; }

        internal MarketDescriptionDto(desc_market apiMarketDescription)
        {
            Guard.Argument(apiMarketDescription, nameof(apiMarketDescription)).NotNull();

            Id = apiMarketDescription.id;
            Name = apiMarketDescription.name;
            Description = apiMarketDescription.description;
            Outcomes = apiMarketDescription.outcomes?.Select(o => new OutcomeDescriptionDto(o)).ToList();
            Specifiers = apiMarketDescription.specifiers?.Select(s => new SpecifierDto(s)).ToList();
            Mappings = apiMarketDescription.mappings?.Select(m => new MarketMappingDto(m)).ToList();
            Attributes = apiMarketDescription.attributes?.Select(a => new MarketAttributeDto(a)).ToList();
            Variant = apiMarketDescription.variant;
            OutcomeType = MapOutcomeType(apiMarketDescription.outcome_type, apiMarketDescription.includes_outcomes_of_type);
            Groups = apiMarketDescription.groups?.Split(new[] { SdkInfo.MarketGroupsDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string MapOutcomeType(string outcomeType, string includesOutcomesOfType)
        {
            if (outcomeType != null)
            {
                return outcomeType;
            }

            if (includesOutcomesOfType == null)
            {
                return null;
            }

            if (includesOutcomesOfType == SdkInfo.OutcomeTextVariantValue)
            {
                return SdkInfo.FreeTextVariantValue;
            }

            if (includesOutcomesOfType.StartsWith("sr:", StringComparison.InvariantCultureIgnoreCase))
            {
                return includesOutcomesOfType.Substring(3);
            }

            return null;
        }

        /// <summary>
        /// Overrides the identifier.
        /// </summary>
        /// <param name="newId">The new identifier.</param>
        /// <remarks>Used when returned variant market description has different id then the requesting one</remarks>
        internal void OverrideId(long newId)
        {
            if (newId > 0)
            {
                Id = newId;
            }
        }

        /// <summary>
        /// Overrides the group list
        /// </summary>
        /// <param name="groups">The new groups</param>
        internal void OverrideGroups(IReadOnlyCollection<string> groups)
        {
            Groups = groups?.ToList();
        }
    }
}
