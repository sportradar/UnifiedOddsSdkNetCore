using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    internal class CompetitorResult : ICompetitorResult
    {
        /// <summary>
        /// Get the type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        public string Value { get; }

        /// <summary>
        /// Gets the specifiers
        /// </summary>
        /// <value>The specifiers</value>
        public string Specifiers { get; }

        internal CompetitorResult(CompetitorResultDTO result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Type = result.Type;
            Value = result.Value;
            Specifiers = result.Specifiers;
        }
    }
}
