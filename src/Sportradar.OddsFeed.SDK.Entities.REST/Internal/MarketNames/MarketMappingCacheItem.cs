/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal class MarketMappingCacheItem
    {
        [Obsolete("Changed with ProducerIds property")]
        public int ProducerId { get; }

        internal IEnumerable<int> ProducerIds { get; }

        public URN SportId { get; }

        public string OrgMarketId { get; private set; }

        public int MarketTypeId { get; }

        public  int? MarketSubTypeId { get; }

        public string SovTemplate { get; }

        public IMappingValidator Validator { get; }

        public string ValidFor { get; }

        public IList<OutcomeMappingCacheItem> OutcomeMappings { get; }

        protected MarketMappingCacheItem(MarketMappingDTO dto, IMappingValidator validator, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();

            ProducerId = dto.ProducerId;
            ProducerIds = dto.ProducerIds;
            SportId = dto.SportId;
            OrgMarketId = dto.OrgMarketId;
            MarketTypeId = dto.MarketTypeId;
            MarketSubTypeId = dto.MarketSubTypeId;
            SovTemplate = dto.SovTemplate;
            ValidFor = dto.ValidFor;
            Validator = validator;

            if (dto.OutcomeMappings != null)
            {
                OutcomeMappings = dto.OutcomeMappings.Select(o => new OutcomeMappingCacheItem(o, culture)).ToList();
            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="MarketMappingCacheItem"/> from the provided DTO
        /// </summary>
        /// <param name="dto">The <see cref="MarketMappingDTO"/> containing mapping data</param>
        /// <param name="factory">The <see cref="IMappingValidatorFactory"/> used to construct mapping validator</param>
        /// <param name="culture">A <see cref="CultureInfo"/> </param>
        /// <returns>The constructed <see cref="MarketMappingCacheItem"/></returns>
        /// <exception cref="InvalidOperationException">The format of <see cref="MarketMappingDTO.ValidFor"/> is not correct</exception>
        public static MarketMappingCacheItem Build(MarketMappingDTO dto, IMappingValidatorFactory factory, CultureInfo culture)
        {
            Guard.Argument(dto).NotNull();
            Guard.Argument(factory).NotNull();

            return string.IsNullOrEmpty(dto.ValidFor)
                ? new MarketMappingCacheItem(dto, null, culture)
                : new MarketMappingCacheItem(dto, factory.Build(dto.ValidFor), culture);
        }

        internal void Merge(MarketMappingDTO dto, CultureInfo culture)
        {
            if (dto.OutcomeMappings == null)
            {
                return;
            }

            var shouldHave = OutcomeMappings.Count > 0;

            if (string.IsNullOrEmpty(dto.OrgMarketId))
            {
                OrgMarketId = dto.OrgMarketId;
            }

            foreach (var outcomeMappingDTO in dto.OutcomeMappings)
            {
                var mapping = OutcomeMappings.FirstOrDefault(f=>f.OutcomeId.Equals(outcomeMappingDTO.OutcomeId, StringComparison.InvariantCultureIgnoreCase));
                if (mapping == null && shouldHave)
                {
                    //investigate
                }
                if (mapping != null)
                {
                    mapping.Merge(outcomeMappingDTO, culture);
                }
                else
                {
                    OutcomeMappings.Add(new OutcomeMappingCacheItem(outcomeMappingDTO, culture));
                }
            }
        }
    }
}
