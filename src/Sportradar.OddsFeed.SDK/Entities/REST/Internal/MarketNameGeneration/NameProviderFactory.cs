// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
{
    /// <summary>
    /// A factory used to construct <see cref="INameProvider"/> instances
    /// </summary>
    internal class NameProviderFactory : INameProviderFactory
    {
        /// <summary>
        /// A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors
        /// </summary>
        private readonly IMarketCacheProvider _marketCacheProvider;

        /// <summary>
        /// A <see cref="IProfileCache"/> instance used to retrieve player and competitor profiles
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// A <see cref="INameExpressionFactory"/> instance used to built <see cref="INameExpression"/> instances
        /// </summary>
        private readonly INameExpressionFactory _expressionFactory;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> describing the mode in which the SDK is running
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameProviderFactory"/> class.
        /// </summary>
        /// <param name="marketCacheProvider">A <see cref="IMarketCacheProvider"/> instance used to retrieve market descriptors</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> instance used to retrieve player and competitor profiles</param>
        /// <param name="expressionFactory">A <see cref="INameExpressionFactory"/> instance used to built <see cref="INameExpression"/> instances</param>
        /// <param name="exceptionHandlingStrategy">A <see cref="ExceptionHandlingStrategy"/> to specifying how to handle potential exceptions thrown to the user code</param>
        /// <param name="loggerFactory">A logger factory to create logs when creating <see cref="INameProvider"/></param>
        public NameProviderFactory(IMarketCacheProvider marketCacheProvider, IProfileCache profileCache, INameExpressionFactory expressionFactory, ExceptionHandlingStrategy exceptionHandlingStrategy, ILoggerFactory loggerFactory)
        {
            Guard.Argument(marketCacheProvider, nameof(marketCacheProvider)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(expressionFactory, nameof(expressionFactory)).NotNull();
            Guard.Argument(loggerFactory, nameof(loggerFactory)).NotNull();

            _marketCacheProvider = marketCacheProvider;
            _profileCache = profileCache;
            _expressionFactory = expressionFactory;
            _exceptionStrategy = exceptionHandlingStrategy;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="INameProvider" />
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent" /> instance representing associated sport @event</param>
        /// <param name="marketId">A market identifier, identifying the market associated with the returned instance</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}" /> representing specifiers of the associated market</param>
        /// <returns>INameProvider</returns>
        public INameProvider BuildNameProvider(ISportEvent sportEvent, int marketId, IReadOnlyDictionary<string, string> specifiers)
        {
            return new NameProvider(_marketCacheProvider, _profileCache, _expressionFactory, sportEvent, marketId, specifiers, _exceptionStrategy, _loggerFactory.CreateLogger<NameProvider>());
        }
    }
}
