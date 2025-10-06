// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    internal static class MarketDescriptionCacheItemExtensions
    {
        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        internal static void HandleMarketMergeResult(this MarketDescriptionCacheItem mdCacheItem, ILogger logger, MarketMergeResult mergeResult, MarketDescriptionDto mdDto, CultureInfo culture)
        {
            if (mergeResult.IsAllMerged())
            {
                return;
            }

            mergeResult.LogMergeOutcomeProblems(logger, mdDto, culture);

            mergeResult.LogMergeMappingProblems(logger, mdDto, culture);

            mdCacheItem.ComparePrint(logger, mdDto, culture);
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void LogMergeOutcomeProblems(this MarketMergeResult mergeResult, ILogger logger, MarketDescriptionDto mdDto, CultureInfo culture)
        {
            if (mergeResult.GetOutcomeProblem().IsNullOrEmpty())
            {
                return;
            }

            foreach (var outcomeProblem in mergeResult.GetOutcomeProblem())
            {
                logger.LogWarning("Could not merge outcome[Id={MdOutcomeId}] for lang={Language} on marketDescription[Id={MdId}] because the specified outcome does not exist on stored market description",
                                  outcomeProblem,
                                  culture.TwoLetterISOLanguageName,
                                  mdDto.Id);
            }
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void LogMergeMappingProblems(this MarketMergeResult mergeResult, ILogger logger, MarketDescriptionDto mdDto, CultureInfo culture)
        {
            if (mergeResult.GetMappingProblem().IsNullOrEmpty())
            {
                return;
            }

            foreach (var mappingProblem in mergeResult.GetMappingProblem())
            {
                logger.LogWarning("Could not merge mapping[MarketId={MdMappingType}] for lang={Language} on marketDescription[Id={MdId}] because the specified mapping does not exist on stored market description",
                                  mappingProblem,
                                  culture.TwoLetterISOLanguageName,
                                  mdDto.Id);
            }
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void ComparePrint(this MarketDescriptionCacheItem mdCacheItem, ILogger logger, MarketDescriptionDto mdDto, CultureInfo culture)
        {
            var names = mdCacheItem.Names.Aggregate(string.Empty, (current, name) => current + $", {name.Key.TwoLetterISOLanguageName}-{name.Value}").Substring(2);
            var desc = mdCacheItem.Descriptions.Aggregate(string.Empty, (current, d) => current + $", {d.Key.TwoLetterISOLanguageName}-{d.Value}");
            var specs = mdCacheItem.Specifiers == null ? null : string.Join(", ", mdCacheItem.Specifiers.Select(s => s.Name));
            var outcomes = mdCacheItem.Outcomes == null ? null : string.Join(",", mdCacheItem.Outcomes.Select(s => s.Id));
            var maps = mdCacheItem.Mappings == null ? null : string.Join(",", mdCacheItem.Mappings.Select(s => s.MarketTypeId));
            logger.LogDebug("Original Id={MdId}, Names=[{MdNames}], Descriptions=[{MdDescription}], Variant=[{MdVariant}], Specifiers=[{MdSpecifiers}], Outcomes=[{MdOutcomes}], Mappings=[{MdMappings}]",
                            mdCacheItem.Id,
                            names,
                            desc,
                            mdCacheItem.Variant,
                            specs,
                            outcomes,
                            maps);

            var specsNew = mdDto.Specifiers == null ? null : string.Join(", ", mdDto.Specifiers.Select(s => s.Name));
            var outcomesNew = mdDto.Outcomes == null ? null : string.Join(",", mdDto.Outcomes.Select(s => s.Id));
            var mapsNew = mdDto.Mappings == null ? null : string.Join(",", mdDto.Mappings.Select(s => s.MarketTypeId));
            logger.LogDebug("New Id={MdId}, Name=[{Language}-{MdName}], Descriptions=[{MdDescription}], Variant=[{MdVariant}], Specifiers=[{MdSpecifiers}], Outcomes=[{MdOutcomes}], Mappings=[{MdMappings}]",
                            mdDto.Id,
                            culture.TwoLetterISOLanguageName,
                            mdDto.Name,
                            mdDto.Description,
                            mdDto.Variant,
                            specsNew,
                            outcomesNew,
                            mapsNew);
        }
    }
}
