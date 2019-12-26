/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class DrawInfo
    /// </summary>
    /// <seealso cref="IDrawInfo" />
    internal class DrawInfo : EntityPrinter, IDrawInfo
    {
        /// <summary>
        /// Gets the type of the draw
        /// </summary>
        /// <value>The type of the draw</value>
        public DrawType DrawType { get; }
        /// <summary>
        /// Gets the type of the time
        /// </summary>
        /// <value>The type of the time</value>
        public TimeType TimeType { get; }
        /// <summary>
        /// Gets the type of the game
        /// </summary>
        /// <value>The type of the game</value>
        public string GameType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfo"/> class
        /// </summary>
        /// <param name="item">The item</param>
        public DrawInfo(DrawInfoCI item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            DrawType = item.DrawType;
            TimeType = item.TimeType;
            GameType = item.GameType;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"DrawType={DrawType}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"DrawType={DrawType}, TimeType={TimeType}, GameType={GameType}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
