// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// A builder used to construct <see cref="IUofSession"/> instance
    /// </summary>
    internal class UofSessionBuilder : IUofSessionBuilder, ISessionBuilder
    {
        /// <summary>
        /// The <see cref="UofSdk"/> instance on which the build sessions will be constructed
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        private readonly IList<IOpenable> _sessions;

        private readonly int _nodeId;

        /// <summary>
        /// A <see cref="MessageInterest"/> instance specifying which messages will be received by constructed <see cref="UofSessionBuilder"/> instance
        /// </summary>
        private MessageInterest _msgInterest;

        /// <summary>
        /// Value indicating whether the current instance has been already used to build a <see cref="IUofSession"/> instance
        /// </summary>
        private bool _isBuild;

        /// <summary>
        /// Initializes a new instance of the <see cref="UofSessionBuilder"/> class
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance with which the build sessions will be constructed</param>
        /// <param name="sessions">The list of current sessions</param>
        /// <param name="nodeId">The node id</param>
        internal UofSessionBuilder(IServiceProvider serviceProvider, IList<IOpenable> sessions, int nodeId)
        {
            Guard.Argument(serviceProvider, nameof(serviceProvider)).NotNull();

            _serviceProvider = serviceProvider;
            _sessions = sessions;
            _nodeId = nodeId;
        }

        /// <summary>
        /// Sets the <see cref="MessageInterest"/> specifying which messages will be received by the constructed <see cref="UofSessionBuilder"/> instance
        /// </summary>
        /// <param name="msgInterest">A <see cref="MessageInterest"/> instance specifying message interest</param>
        /// <returns>A <see cref="ISessionBuilder"/> instance used to perform further build tasks</returns>
        public ISessionBuilder SetMessageInterest(MessageInterest msgInterest)
        {
            _msgInterest = msgInterest;
            return this;
        }

        /// <summary>
        /// Builds and returns a <see cref="IUofSession"/> instance
        /// </summary>
        /// <returns>A <see cref="IUofSession"/> instance constructed with objects provided in previous steps of the builder</returns>
        public IUofSession Build()
        {
            if (_isBuild)
            {
                throw new InvalidOperationException("The IUofSession instance has already been build by the current instance");
            }
            _isBuild = true;

            var newScope = _serviceProvider.CreateScope();
            Func<UofSession, IEnumerable<string>> func = GetSessionRoutingKeys;
            var newSession = new UofSession(newScope.ServiceProvider.GetService<IMessageReceiver>(),
                                            newScope.ServiceProvider.GetService<CompositeMessageProcessor>(),
                                            newScope.ServiceProvider.GetService<IFeedMessageMapper>(),
                                            newScope.ServiceProvider.GetService<IFeedMessageValidator>(),
                                            newScope.ServiceProvider.GetService<IMessageDataExtractor>(),
                                            newScope.ServiceProvider.GetService<IDispatcherStore>(),
                                            _msgInterest,
                                            newScope.ServiceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                            func);
            _sessions.Add(newSession);
            return newSession;
        }

        /// <summary>
        /// Constructs and returns a <see cref="IEnumerable{String}"/> containing routing keys for the specified session
        /// </summary>
        /// <param name="session">The <see cref="UofSession"/> for which to get the routing keys</param>
        /// <returns>The <see cref="IEnumerable{String}"/> containing routing keys for the specified session</returns>
        private IEnumerable<string> GetSessionRoutingKeys(UofSession session)
        {
            var interests = _sessions.Select(s => ((UofSession)s).MessageInterest);
            var keys = FeedRoutingKeyBuilder.GenerateKeys(interests, _nodeId).ToList();
            return keys[_sessions.IndexOf(session)];
        }
    }
}
