// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Internal.Authentication
{
    internal interface IJsonWebTokenFactory
    {
        /// <summary>
        /// Creates a JSON Web Token (JWT) using the provided authentication configuration.
        /// </summary>
        /// <returns>JSON Web Token (JWT) as a string</returns>
        string CreateJsonWebToken();
    }
}
