/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    internal class VariantDescriptionsMapper : ISingleTypeMapper<EntityList<VariantDescriptionDTO>>
    {
        /// <summary>
        /// A <see cref="variant_descriptions"/> instance containing data used to construct <see cref="EntityList{VariantDescriptionDTO}"/> instance
        /// </summary>
        private readonly variant_descriptions _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariantDescriptionsMapper"/> class
        /// </summary>
        /// <param name="data">A <see cref="variant_descriptions"/> instance containing data used to construct <see cref="EntityList{VariantDescriptionDTO}"/> instance</param>
        internal VariantDescriptionsMapper(variant_descriptions data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{VariantDescriptionDTO}"/> instance
        /// </summary>
        /// <returns>The created<see cref="EntityList{VariantDescriptionDTO}"/> instance</returns>
        EntityList<VariantDescriptionDTO> ISingleTypeMapper<EntityList<VariantDescriptionDTO>>.Map()
        {
            var descriptions = _data.variant.Select(m => new VariantDescriptionDTO(m)).ToList();
            return new EntityList<VariantDescriptionDTO>(descriptions);
        }
    }
}
