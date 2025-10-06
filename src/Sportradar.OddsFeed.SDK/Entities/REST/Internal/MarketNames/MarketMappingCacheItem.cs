// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketMappingCacheItem
    {
        internal IEnumerable<int> ProducerIds { get; }

        public Urn SportId { get; }

        public string OrgMarketId { get; private set; }

        public int MarketTypeId { get; }

        public int? MarketSubTypeId { get; }

        public string SovTemplate { get; }

        public IMappingValidator Validator { get; }

        public string ValidFor { get; }

        public IList<OutcomeMappingCacheItem> OutcomeMappings { get; }

        private MarketMappingCacheItem(MarketMappingDto dto, IMappingValidator validator, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            ProducerIds = dto.ProducerIds;
            SportId = dto.SportId;
            OrgMarketId = dto.OrgMarketId;
            MarketTypeId = dto.MarketTypeId;
            MarketSubTypeId = dto.MarketSubTypeId;
            SovTemplate = dto.SovTemplate;
            ValidFor = dto.ValidFor;
            Validator = validator;

            OutcomeMappings = new List<OutcomeMappingCacheItem>();
            if (dto.OutcomeMappings != null)
            {
                OutcomeMappings = dto.OutcomeMappings.Select(o => new OutcomeMappingCacheItem(o, culture)).ToList();
            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="MarketMappingCacheItem"/> from the provided Dto
        /// </summary>
        /// <param name="dto">The <see cref="MarketMappingDto"/> containing mapping data</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> used to construct mapping validator</param>
        /// <param name="culture">A <see cref="CultureInfo"/> </param>
        /// <returns>The constructed <see cref="MarketMappingCacheItem"/></returns>
        /// <exception cref="InvalidOperationException">The format of <see cref="MarketMappingDto.ValidFor"/> is not correct</exception>
        public static MarketMappingCacheItem Build(MarketMappingDto dto, IMappingValidatorFactory factory, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(factory, nameof(factory)).NotNull();

            return string.IsNullOrEmpty(dto.ValidFor)
                       ? new MarketMappingCacheItem(dto, null, culture)
                       : new MarketMappingCacheItem(dto, factory.Build(dto.ValidFor), culture);
        }

        internal void Merge(MarketMappingDto dto, CultureInfo culture)
        {
            if (dto.OutcomeMappings.IsNullOrEmpty())
            {
                return;
            }

            if (string.IsNullOrEmpty(dto.OrgMarketId))
            {
                OrgMarketId = dto.OrgMarketId;
            }

            if (!OutcomeMappings.IsNullOrEmpty())
            {
                MergeMappings(dto, culture);
            }
        }

        private void MergeMappings(MarketMappingDto dto, CultureInfo culture)
        {
            foreach (var outcomeMappingDto in dto.OutcomeMappings)
            {
                var mapping = OutcomeMappings.FirstOrDefault(f => f.OutcomeId.Equals(outcomeMappingDto.OutcomeId, StringComparison.InvariantCultureIgnoreCase));
                if (mapping == null)
                {
                    //investigate
                }
                if (mapping != null)
                {
                    mapping.Merge(outcomeMappingDto, culture);
                }
                else
                {
                    OutcomeMappings.Add(new OutcomeMappingCacheItem(outcomeMappingDto, culture));
                }
            }
        }
    }
}
