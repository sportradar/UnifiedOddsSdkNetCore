/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api
{
    /// <summary>
    /// Represents a first step when building a <see cref="IUofSession"/> instance
    /// </summary>
    public interface IUofSessionBuilder
    {
        /// <summary>
        /// Sets a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed
        /// </summary>
        /// <param name="msgInterest">a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed</param>
        /// <returns>A <see cref="ISessionBuilder"/> representing the second step when building a <see cref="IUofSession"/> instance</returns>
        ISessionBuilder SetMessageInterest(MessageInterest msgInterest);
    }

    /// <summary>
    /// Represents a second step when building a <see cref="IUofSession"/> instance
    /// </summary>
    public interface ISessionBuilder
    {
        /// <summary>
        /// Builds and returns a <see cref="IUofSession"/> instance
        /// </summary>
        /// <returns>the built <see cref="IUofSession"/> instance</returns>
        IUofSession Build();
    }
}
