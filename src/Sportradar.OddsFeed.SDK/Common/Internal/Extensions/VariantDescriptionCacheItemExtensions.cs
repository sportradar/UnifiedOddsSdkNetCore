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
    internal static class VariantDescriptionCacheItemExtensions
    {
        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        internal static void HandleMarketMergeResult(this VariantDescriptionCacheItem vdCacheItem, ILogger logger, MarketMergeResult mergeResult, VariantDescriptionDto vdDto, CultureInfo culture)
        {
            if (mergeResult.IsAllMerged())
            {
                return;
            }

            mergeResult.LogMergeOutcomeProblems(logger, vdDto, culture);

            mergeResult.LogMergeMappingProblems(logger, vdDto, culture);

            vdCacheItem.ComparePrint(logger, vdDto, culture);
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void LogMergeOutcomeProblems(this MarketMergeResult mergeResult, ILogger logger, VariantDescriptionDto vdDto, CultureInfo culture)
        {
            if (mergeResult.GetOutcomeProblem().IsNullOrEmpty())
            {
                return;
            }

            foreach (var outcomeProblem in mergeResult.GetOutcomeProblem())
            {
                logger.LogWarning(
                    "Could not merge outcome[Id={MdOutcomeId}] for lang={Language} on marketDescription[Id={MdId}] because the specified outcome does not exist on stored market description",
                    outcomeProblem,
                    culture.TwoLetterISOLanguageName,
                    vdDto.Id);
            }
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void LogMergeMappingProblems(this MarketMergeResult mergeResult, ILogger logger, VariantDescriptionDto vdDto, CultureInfo culture)
        {
            if (mergeResult.GetMappingProblem().IsNullOrEmpty())
            {
                return;
            }

            foreach (var mappingProblem in mergeResult.GetMappingProblem())
            {
                logger.LogWarning(
                    "Could not merge mapping[MarketId={MdMappingType}] for lang={Language} on marketDescription[Id={MdId}] because the specified mapping does not exist on stored market description",
                    mappingProblem,
                    culture.TwoLetterISOLanguageName,
                    vdDto.Id);
            }
        }

        [SuppressMessage("ReSharper", "TooManyArguments", Justification = "Needed here")]
        private static void ComparePrint(this VariantDescriptionCacheItem vdCacheItem, ILogger logger, VariantDescriptionDto vdDto, CultureInfo culture)
        {
            var outcomes = vdCacheItem.Outcomes == null ? null : string.Join(",", vdCacheItem.Outcomes.Select(s => s.Id));
            var maps = vdCacheItem.Mappings == null ? null : string.Join(",", vdCacheItem.Mappings.Select(s => s.MarketTypeId));
            logger.LogDebug("Original Id={MdId}, Outcomes=[{MdOutcomes}], Mappings=[{MdMappings}]", vdCacheItem.Id, outcomes, maps);

            var outcomesNew = vdDto.Outcomes == null ? null : string.Join(",", vdDto.Outcomes.Select(s => s.Id));
            var mapsNew = vdDto.Mappings == null ? null : string.Join(",", vdDto.Mappings.Select(s => s.MarketTypeId));
            logger.LogDebug("New Id={MdId}, Lang={Language}, Outcomes=[{MdOutcomes}], Mappings=[{MdMappings}]", vdDto.Id, culture.TwoLetterISOLanguageName, outcomesNew, mapsNew);
        }
    }
}
