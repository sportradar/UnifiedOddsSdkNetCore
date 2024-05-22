// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal interface IBookmakerDetailsProvider
    {
        /// <summary>
        /// Loads the current config object with bookmaker details data retrieved from the Sports API
        /// </summary>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        void LoadBookmakerDetails(UofConfiguration config);
    }
}
