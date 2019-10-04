/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Represents a first step when building a <see cref="IOddsFeedSession"/> instance
    /// </summary>
    [ContractClass(typeof(OddsFeedSessionBuilderContract))]
    public interface IOddsFeedSessionBuilder
    {
        /// <summary>
        /// Sets a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed
        /// </summary>
        /// <param name="msgInterest">a <see cref="MessageInterest"/> specifying which type of messages should be received from the feed</param>
        /// <returns>A <see cref="ISessionBuilder"/> representing the second step when building a <see cref="IOddsFeedSession"/> instance</returns>
        ISessionBuilder SetMessageInterest(MessageInterest msgInterest);
    }

    /// <summary>
    /// Represents a second step when building a <see cref="IOddsFeedSession"/> instance
    /// </summary>
    [ContractClass(typeof(SessionBuilderContract))]
    public interface ISessionBuilder
    {
        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedSession"/> instance
        /// </summary>
        /// <returns>the built <see cref="IOddsFeedSession"/> instance</returns>
        IOddsFeedSession Build();
    }
}