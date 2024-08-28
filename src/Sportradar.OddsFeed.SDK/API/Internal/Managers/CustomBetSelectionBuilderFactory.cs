// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Managers;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    internal class CustomBetSelectionBuilderFactory : ICustomBetSelectionBuilderFactory
    {
        public ICustomBetSelectionBuilder CreateBuilder()
        {
            return new CustomBetSelectionBuilder();
        }
    }
}
