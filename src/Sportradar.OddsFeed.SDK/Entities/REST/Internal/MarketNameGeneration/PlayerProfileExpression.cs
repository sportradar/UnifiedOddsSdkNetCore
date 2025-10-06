// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
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
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(operand, nameof(operand)).NotNull();

            _profileCache = profileCache;
            _operand = operand;
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
            var urn = Urn.Parse(urnString);
            string name = null;
            if (urn.Type == "player")
            {
                name = await _profileCache.GetPlayerNameAsync(urn, culture, true).ConfigureAwait(false);
            }
            else if (urn.Type == "competitor")
            {
                name = await _profileCache.GetCompetitorNameAsync(urn, culture, true).ConfigureAwait(false);
            }
            return name;
        }
    }
}
