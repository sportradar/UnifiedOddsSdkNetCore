// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
{
    /// <summary>
    /// A <see cref="INameExpression"/> implementation supporting '$' (i.e. {$competitor1} ) name expressions
    /// </summary>
    /// <seealso cref="INameExpression" />
    internal class EntityNameExpression : INameExpression
    {
        /// <summary>
        /// The operand extracted from the provided expression
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// A <see cref="ICompetition"/> related to the @event associated with the current instance
        /// </summary>
        private readonly ISportEvent _sportEvent;

        private readonly IProfileCache _profileCache;

        /// <summary>
        /// Lists the supported operand names
        /// </summary>
        private static readonly string[] SupportedOperands =
        {
            "competitor1",
            "competitor2"
        };

        private readonly string _supportedOperandsString;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNameExpression"/> class
        /// </summary>
        /// <param name="propertyName">The name <see cref="ISportEvent"/> property</param>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> related to the entity associated with the current instance.</param>
        /// <param name="profileCache">A profile cache to get the competitor name</param>
        internal EntityNameExpression(string propertyName, ISportEvent sportEvent, IProfileCache profileCache)
        {
            Guard.Argument(propertyName, nameof(propertyName)).NotNull().NotEmpty();
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();

            _propertyName = propertyName;
            _sportEvent = sportEvent;
            _profileCache = profileCache;

            _supportedOperandsString = string.Join(", ", SupportedOperands);
        }

        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">The culture information.</param>
        /// <returns>A <see cref="Task{String}" /> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating name expression</exception>
        public async Task<string> BuildNameAsync(CultureInfo culture)
        {
            if (_propertyName.Equals("event"))
            {
                return await BuildEventNameAsync(culture).ConfigureAwait(false);
            }

            return await BuildOperandNameAsync(culture).ConfigureAwait(false);
        }

        private async Task<string> BuildEventNameAsync(CultureInfo culture)
        {
            if (_sportEvent is IMatch)
            {
                var homeCompetitor = await GetHomeCompetitor(culture).ConfigureAwait(false);
                var awayCompetitor = await GetAwayCompetitor(culture).ConfigureAwait(false);
                return $"{homeCompetitor} vs {awayCompetitor}";
            }

            return await _sportEvent.GetNameAsync(culture).ConfigureAwait(false);
        }

        private async Task<string> BuildOperandNameAsync(CultureInfo culture)
        {
            switch (Array.IndexOf(SupportedOperands, _propertyName))
            {
                case 0:
                    return await GetHomeCompetitor(culture).ConfigureAwait(false);
                case 1:
                    return await GetAwayCompetitor(culture).ConfigureAwait(false);
                default:
                    throw new NameExpressionException($"No valid operand found. Operand {_propertyName} is not supported [{_sportEvent.Id}]. Supported operands are: {_supportedOperandsString}", null);
            }
        }

        private async Task<string> GetHomeCompetitor(CultureInfo culture)
        {
            if (_sportEvent is Competition competition)
            {
                var competitorIds = await competition.GetCompetitorIdsAsync().ConfigureAwait(false);
                var listCompetitorIds = competitorIds.ToList();
                if (!listCompetitorIds.IsNullOrEmpty())
                {
                    var name = await _profileCache.GetCompetitorNameAsync(listCompetitorIds.First(), culture, false).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                    var competitors = await competition.GetCompetitorsAsync(culture).ConfigureAwait(false);
                    return competitors?.First().GetName(culture);
                }
            }

            throw new NameExpressionException($"No home competitor found. Operand {_propertyName} is not supported [{_sportEvent.Id}]. Supported operands are: {_supportedOperandsString}", null);
        }

        private async Task<string> GetAwayCompetitor(CultureInfo culture)
        {
            if (_sportEvent is Competition competition)
            {
                var competitorIds = await competition.GetCompetitorIdsAsync().ConfigureAwait(false);
                var listCompetitorIds = competitorIds.ToList();
                if (!listCompetitorIds.IsNullOrEmpty())
                {
                    var name = await _profileCache.GetCompetitorNameAsync(listCompetitorIds.Skip(1).First(), culture, false).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                    var competitors = await competition.GetCompetitorsAsync(culture).ConfigureAwait(false);
                    return competitors?.Skip(1).First().GetName(culture);
                }
            }

            throw new NameExpressionException($"No away competitor found. Operand {_propertyName} is not supported [{_sportEvent.Id}]. Supported operands are: {_supportedOperandsString}", null);
        }
    }
}
