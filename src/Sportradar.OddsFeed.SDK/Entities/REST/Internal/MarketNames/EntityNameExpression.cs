﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
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

        /// <summary>
        /// Lists the supported operand names
        /// </summary>
        private static readonly string[] SupportedOperands =
        {
            "competitor1",
            "competitor2"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNameExpression"/> class
        /// </summary>
        /// <param name="propertyName">The name <see cref="ISportEvent"/> property</param>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> related to the entity associated with the current instance.</param>
        internal EntityNameExpression(string propertyName, ISportEvent sportEvent)
        {
            Guard.Argument(propertyName, nameof(propertyName)).NotNull().NotEmpty();
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();

            _propertyName = propertyName;
            _sportEvent = sportEvent;
        }

        /// <summary>
        /// Asynchronous invokes the specified method and wraps potential exception into <see cref="NameExpressionException"/>.
        /// </summary>
        /// <typeparam name="T">The type returned by provided async method</typeparam>
        /// <param name="method">A <see cref="Func{TResult}"/> representing async method to invoke.</param>
        /// <returns>A <see cref="Task{T}"/> representing the async method.</returns>
        private static async Task<T> InvokeAndWrapAsync<T>(Func<Task<T>> method)
        {
            try
            {
                return await method.Invoke().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is CommunicationException || ex is DeserializationException || ex is MappingException)
                {
                    throw new NameExpressionException("Error occurred while evaluating name expression", ex);
                }
                throw;
            }
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
                if (_sportEvent is IMatch)
                {
                    var homeCompetitor = await GetHomeCompetitor(culture).ConfigureAwait(false);
                    var awayCompetitor = await GetAwayCompetitor(culture).ConfigureAwait(false);
                    return $"{homeCompetitor} vs {awayCompetitor}";
                }

                return await _sportEvent.GetNameAsync(culture).ConfigureAwait(false);
            }

            switch (Array.IndexOf(SupportedOperands, _propertyName))
            {
                case 0:
                    return await GetHomeCompetitor(culture).ConfigureAwait(false);
                case 1:
                    return await GetAwayCompetitor(culture).ConfigureAwait(false);
                default:
                    throw new NameExpressionException($"Operand {_propertyName} is not supported. Supported operands are: {string.Join(", ", SupportedOperands)}", null);
            }
        }

        private async Task<string> GetHomeCompetitor(CultureInfo culture)
        {
            if (_sportEvent is IMatch match)
            {
                var competitor = await InvokeAndWrapAsync(match.GetHomeCompetitorAsync).ConfigureAwait(false);
                await competitor.EnsureProfileLoaded();
                return competitor?.GetName(culture);
            }

            if (_sportEvent is IStage stage)
            {
                var competitors = await InvokeAndWrapAsync(stage.GetCompetitorsAsync).ConfigureAwait(false);
                var competitor = competitors?.First();
                if (competitor != null)
                {
                    await competitor.EnsureProfileLoaded();
                    return competitor.GetName(culture);
                }

                return null;
            }

            throw new NameExpressionException($"Operand {_propertyName} is not supported. Supported operands are: {string.Join(",", SupportedOperands)}", null);
        }

        private async Task<string> GetAwayCompetitor(CultureInfo culture)
        {
            if (_sportEvent is IMatch match)
            {
                var competitor = await InvokeAndWrapAsync(match.GetAwayCompetitorAsync).ConfigureAwait(false);
                await competitor.EnsureProfileLoaded();
                return competitor?.GetName(culture);
            }

            if (_sportEvent is IStage stage)
            {
                var competitors = await InvokeAndWrapAsync(stage.GetCompetitorsAsync).ConfigureAwait(false);
                var competitor = competitors?.Skip(1).First();
                if (competitor != null)
                {
                    await competitor.EnsureProfileLoaded();
                    return competitor.GetName(culture);
                }

                return null;
            }

            throw new NameExpressionException($"Operand {_propertyName} is not supported. Supported operands are: {string.Join(",", SupportedOperands)}", null);
        }
    }
}
