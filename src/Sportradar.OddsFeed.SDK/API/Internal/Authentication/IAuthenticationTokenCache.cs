// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal interface IAuthenticationTokenCache
    {
        Task<string> GetTokenForApi();

        Task<string> GetTokenForFeed();
    }
}
