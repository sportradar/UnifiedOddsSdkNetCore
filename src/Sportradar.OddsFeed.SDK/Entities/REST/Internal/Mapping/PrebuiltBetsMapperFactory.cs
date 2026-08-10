// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class PrebuiltBetsMapperFactory : ISingleTypeMapperFactory<PreBuiltBetsType, PrebuiltBetsDto>
    {
        public ISingleTypeMapper<PrebuiltBetsDto> CreateMapper(PreBuiltBetsType data)
        {
            return new PrebuiltBetsMapper(data);
        }
    }
}
