/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a goal scorer in a sport event
    /// </summary>
    /// <seealso cref="IPlayer" />
    /// <seealso cref="IEventPlayer" />
    internal class GoalScorer : BaseEntity, IGoalScorer
    {
        /// <summary>
        /// Gets the method value
        /// </summary>
        /// <value>The method value</value>
        /// <remarks>The attribute can assume values such as 'penalty' and 'own goal'. In case the attribute is not inserted, then the goal is not own goal neither penalty.</remarks>
        public string Method { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IGoalScorer"/> class
        /// </summary>
        /// <param name="data">The <see cref="EventPlayerCI"/> data</param>
        public GoalScorer(EventPlayerCI data)
            : base(data.Id, data.Name as IReadOnlyDictionary<CultureInfo, string>)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            Method = data.Method;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"{base.PrintC()}, Method={Method}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return $"{base.PrintF()}, Method={Method}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
