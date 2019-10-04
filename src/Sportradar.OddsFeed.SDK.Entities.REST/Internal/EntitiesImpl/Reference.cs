/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// A implementation of <see cref="IReference" /> used to return results to user
    /// </summary>
    internal class Reference : EntityPrinter, IReferenceV1
    {
        /// <summary>
        /// Gets the Betradar id for this instance if provided amount reference ids
        /// </summary>
        public int BetradarId { get; }

        /// <summary>
        /// Gets the Betfair id for this instance if provided amount reference ids
        /// </summary>
        public int BetfairId { get; }

        /// <summary>
        /// Gets the rotation number if provided among reference ids
        /// </summary>
        /// <value>If exists among reference ids, result is greater then 0</value>
        /// <remarks>This id only exists for US leagues</remarks>
        public int RotationNumber { get; }

        /// <summary>
        /// Gets all the reference ids associated with this instance
        /// </summary>
        public IReadOnlyDictionary<string, string> References { get; }

        /// <summary>
        /// Returns the AAMS id for this instance if provided among reference ids, null otherwise
        /// </summary>
        /// <returns>The AAMS id for this instance if provided among reference ids, null otherwise</returns>
        public int? AamsId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> class
        /// </summary>
        /// <param name="referenceCI">The reference ci</param>
        public Reference(ReferenceIdCI referenceCI)
        {
            if (referenceCI == null)
            {
                return;
            }

            References = referenceCI.ReferenceIds;
            BetradarId = referenceCI.BetradarId;
            BetfairId = referenceCI.BetfairId;
            RotationNumber = referenceCI.RotationNumber;
            AamsId = referenceCI.AamsId;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"BetradarId={BetradarId}, BetfairId={BetfairId}, RotationNumber={RotationNumber}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return PrintI();
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return References == null || !References.Any()
                ? "no references"
                : References.Aggregate(string.Empty, (current, item) => current = $"{current}, {item.Key}={item.Value}").Substring(2);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
