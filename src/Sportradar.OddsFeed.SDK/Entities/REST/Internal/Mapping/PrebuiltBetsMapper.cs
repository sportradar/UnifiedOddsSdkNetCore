// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    internal class PrebuiltBetsMapper : ISingleTypeMapper<PrebuiltBetsDto>
    {
        private readonly PreBuiltBetsType _prebuiltBets;

        public PrebuiltBetsMapper(PreBuiltBetsType prebuiltBets)
        {
            _prebuiltBets = prebuiltBets;
        }
        public PrebuiltBetsDto Map()
        {
            return new PrebuiltBetsDto(_prebuiltBets);
        }
    }
}
