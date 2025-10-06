// Copyright (C) Sportradar AG.See LICENSE for full license governing this code
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
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
        /// Returns the Lugas id for this instance if provided among reference ids, null otherwise
        /// </summary>
        /// <returns>The Lugas id for this instance if provided among reference ids, null otherwise</returns>
        public string LugasId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> class
        /// </summary>
        /// <param name="referenceCacheItem">The reference cache item</param>
        public Reference(ReferenceIdCacheItem referenceCacheItem)
        {
            if (referenceCacheItem == null)
            {
                return;
            }

            References = referenceCacheItem.ReferenceIds;
            BetradarId = referenceCacheItem.BetradarId;
            BetfairId = referenceCacheItem.BetfairId;
            RotationNumber = referenceCacheItem.RotationNumber;
            AamsId = referenceCacheItem.AamsId;
            LugasId = referenceCacheItem.LugasId;
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
            if (References.IsNullOrEmpty())
            {
                return "no references";
            }
            return string.Join(", ", References.Select(s => $"{s.Key}={s.Value}"));
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
