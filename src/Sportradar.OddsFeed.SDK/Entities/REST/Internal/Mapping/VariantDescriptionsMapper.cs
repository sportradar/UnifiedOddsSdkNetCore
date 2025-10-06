// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class VariantDescriptionsMapper : ISingleTypeMapper<EntityList<VariantDescriptionDto>>
    {
        /// <summary>
        /// A <see cref="variant_descriptions"/> instance containing data used to construct <see cref="EntityList{VariantDescriptionDto}"/> instance
        /// </summary>
        private readonly variant_descriptions _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantDescriptionsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="variant_descriptions"/> instance containing data used to construct <see cref="EntityList{VariantDescriptionDto}"/> instance</param>
        internal VariantDescriptionsMapper(variant_descriptions data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{VariantDescriptionDto}"/> instance
        /// </summary>
        /// <returns>The created<see cref="EntityList{VariantDescriptionDto}"/> instance</returns>
        EntityList<VariantDescriptionDto> ISingleTypeMapper<EntityList<VariantDescriptionDto>>.Map()
        {
            var descriptions = _data.variant.Select(m => new VariantDescriptionDto(m)).ToList();
            return new EntityList<VariantDescriptionDto>(descriptions);
        }
    }
}
