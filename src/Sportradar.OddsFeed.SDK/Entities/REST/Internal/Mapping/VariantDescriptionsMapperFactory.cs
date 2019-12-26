/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Class VariantDescriptionsMapperFactory
    /// </summary>
    public class VariantDescriptionsMapperFactory : ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDTO>>
    {
        /// <summary>
        /// Creates and returns an instance of Mapper for mapping <see cref="variant_descriptions"/>
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="VariantDescriptionsMapper"/> will map</param>
        /// <returns>New <see cref="VariantDescriptionsMapper" /> instance</returns>
        public ISingleTypeMapper<EntityList<VariantDescriptionDTO>> CreateMapper(variant_descriptions data)
        {
            return new VariantDescriptionsMapper(data);
        }
    }
}
