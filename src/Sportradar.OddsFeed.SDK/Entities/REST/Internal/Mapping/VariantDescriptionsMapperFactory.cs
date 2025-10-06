// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Class VariantDescriptionsMapperFactory
    /// </summary>
    internal class VariantDescriptionsMapperFactory : ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDto>>
    {
        /// <summary>
        /// Creates and returns an instance of Mapper for mapping <see cref="variant_descriptions"/>
        /// </summary>
        /// <param name="data">A input instance which the created <see cref="VariantDescriptionsMapper"/> will map</param>
        /// <returns>New <see cref="VariantDescriptionsMapper" /> instance</returns>
        public ISingleTypeMapper<EntityList<VariantDescriptionDto>> CreateMapper(variant_descriptions data)
        {
            return new VariantDescriptionsMapper(data);
        }
    }
}
