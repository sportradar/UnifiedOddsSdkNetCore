﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
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

        internal IEnumerable<OutcomeDescriptionDto> Outcomes { get; }

        internal IEnumerable<SpecifierDto> Specifiers { get; }

        internal IEnumerable<MarketMappingDto> Mappings { get; }

        internal IEnumerable<MarketAttributeDto> Attributes { get; }

        internal IEnumerable<string> Groups { get; private set; }

        internal MarketDescriptionDto(desc_market description)
        {
            Guard.Argument(description, nameof(description)).NotNull();
            Guard.Argument(description.name, nameof(description.name)).NotNull().NotEmpty();

            Id = description.id;
            Name = description.name;
            Description = description.description;
            Outcomes = description.outcomes?.Select(o => new OutcomeDescriptionDto(o)).ToList();
            Specifiers = description.specifiers?.Select(s => new SpecifierDto(s)).ToList();
            Mappings = description.mappings?.Select(m => new MarketMappingDto(m)).ToList();
            Attributes = description.attributes?.Select(a => new MarketAttributeDto(a)).ToList();
            Variant = description.variant;
            OutcomeType = MapOutcomeType(description.outcome_type, description.includes_outcomes_of_type);
            Groups = description.groups?.Split(new[] { SdkInfo.MarketGroupsDelimiter }, StringSplitOptions.RemoveEmptyEntries);
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
            Groups = groups;
        }
    }
}
