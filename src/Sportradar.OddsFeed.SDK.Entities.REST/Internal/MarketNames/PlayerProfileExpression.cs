/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="INameExpression"/> implementation supporting ! based expressions (e.g. {!reply_nr})
    /// </summary>
    internal class PlayerProfileExpression : INameExpression
    {
        /// <summary>
        /// A <see cref="IProfileCache"/> instance used to fetch player profiles
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// A <see cref="IOperand"/> representing part of the name expression
        /// </summary>
        private readonly IOperand _operand;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalNameExpression"/> class.
        /// </summary>
        /// <param name="profileCache">A <see cref="IProfileCache"/> instance used to fetch player profiles</param>
        /// <param name="operand">A <see cref="IOperand"/> representing part of the name expression</param>
        internal PlayerProfileExpression(IProfileCache profileCache, IOperand operand)
        {
            Contract.Requires(profileCache != null);
            Contract.Requires(operand != null);

            _profileCache = profileCache;
            _operand = operand;
        }

        /// <summary>
        /// Defines object invariants as required by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_profileCache != null);
            Contract.Invariant(_operand != null);
        }

        /// <summary>
        /// Asynchronously builds a name of the associated instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the constructed name</param>
        /// <returns>A <see cref="Task{String}" /> representing the asynchronous operation</returns>
        /// <exception cref="NameExpressionException">Error occurred while evaluating name expression</exception>
        public async Task<string> BuildNameAsync(CultureInfo culture)
        {
            var urnString = await _operand.GetStringValue().ConfigureAwait(false);
            var urn = URN.Parse(urnString);
            string name = null;
            if (urn.Type == "player")
            {
                var profile = await _profileCache.GetPlayerProfileAsync(urn, new[] {culture}).ConfigureAwait(false);
                name = profile?.GetName(culture);
            }
            else if (urn.Type == "competitor")
            {
                var profile = await _profileCache.GetCompetitorProfileAsync(urn, new[] {culture}).ConfigureAwait(false);
                name = profile?.GetName(culture);
            }
            return name;
        }
    }
}
