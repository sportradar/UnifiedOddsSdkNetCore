/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A builder used to construct <see cref="IOddsFeedSession"/> instance
    /// </summary>
    internal class OddsFeedSessionBuilder : IOddsFeedSessionBuilder, ISessionBuilder
    {
        /// <summary>
        /// The <see cref="Feed"/> instance on which the build sessions will be constructed
        /// </summary>
        private readonly Feed _feed;

        /// <summary>
        /// A <see cref="MessageInterest"/> instance specifying which messages will be received by constructed <see cref="OddsFeedSessionBuilder"/> instance
        /// </summary>
        private MessageInterest _msgInterest;

        /// <summary>
        /// Value indicating whether the current instance has been already used to build a <see cref="IOddsFeedSession"/> instance
        /// </summary>
        private bool _hasBeenSessionBuild;

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsFeedSessionBuilder"/> class
        /// </summary>
        /// <param name="feed">The <see cref="Feed"/> instance on which the build sessions will be constructed</param>
        internal OddsFeedSessionBuilder(Feed feed)
        {
            Guard.Argument(feed).NotNull();

            _feed = feed;
        }

        /// <summary>
        /// Sets the <see cref="MessageInterest"/> specifying which messages will be received by the constructed <see cref="OddsFeedSessionBuilder"/> instance
        /// </summary>
        /// <param name="msgInterest">A <see cref="MessageInterest"/> instance specifying message interest</param>
        /// <returns>A <see cref="ISessionBuilder"/> instance used to perform further build tasks</returns>
        public ISessionBuilder SetMessageInterest(MessageInterest msgInterest)
        {
            _msgInterest = msgInterest;
            return this;
        }

        /// <summary>
        /// Builds and returns a <see cref="IOddsFeedSession"/> instance
        /// </summary>
        /// <returns>A <see cref="IOddsFeedSession"/> instance constructed with objects provided in previous steps of the builder</returns>
        public IOddsFeedSession Build()
        {
            if (_hasBeenSessionBuild)
            {
                throw new InvalidOperationException("The IOddsFeedSession instance has already been build by the current instance");
            }
            _hasBeenSessionBuild = true;
            return _feed.CreateSession(_msgInterest);
        }
    }
}
