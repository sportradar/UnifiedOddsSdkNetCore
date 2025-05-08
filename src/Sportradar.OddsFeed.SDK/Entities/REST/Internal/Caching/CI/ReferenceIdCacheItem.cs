﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A ReferenceId representation for cache
    /// </summary>
    internal class ReferenceIdCacheItem
    {
        /// <summary>
        /// A <see cref="BetradarId"/> property backing field
        /// </summary>
        private int _betradarId;

        /// <summary>
        /// A <see cref="BetfairId"/> property backing field
        /// </summary>
        private int _betfairId;

        /// <summary>
        /// The rotation number
        /// </summary>
        private int _rotationNumber;

        /// <summary>
        /// The AAMS id for this instance if provided among reference ids, null otherwise
        /// </summary>
        private int? _aamsId;

        /// <summary>
        /// The Lugas id for this instance if provided among reference ids, null otherwise
        /// </summary>
        private string _lugasId;

        /// <summary>
        /// Gets the Betradar id for this instance if provided among reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        public int BetradarId => _betradarId;

        /// <summary>
        /// Gets the Betfair id for this instance if provided among reference ids
        /// </summary>
        /// <returns>If exists among reference ids, result is greater then 0</returns>
        public int BetfairId => _betfairId;

        /// <summary>
        /// Gets the rotation number if provided among reference ids
        /// </summary>
        /// <value>If exists among reference ids, result is greater then 0</value>
        /// <remarks>This id only exists for US leagues</remarks>
        public int RotationNumber => _rotationNumber;

        /// <summary>
        /// Gets all the reference ids
        /// </summary>
        public IReadOnlyDictionary<string, string> ReferenceIds { get; private set; }

        /// <summary>
        /// Returns the AAMS id for this instance if provided among reference ids, null otherwise
        /// </summary>
        /// <returns>The AAMS id for this instance if provided among reference ids, null otherwise</returns>
        public int? AamsId => _aamsId;

        /// <summary>
        /// Returns the Lugas id for this instance if provided among reference ids, null otherwise
        /// </summary>
        /// <returns>The Lugas id for this instance if provided among reference ids, null otherwise</returns>
        public string LugasId => _lugasId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceIdCacheItem"/> class
        /// </summary>
        public ReferenceIdCacheItem(IDictionary<string, string> referenceIds)
        {
            _betradarId = 0;
            _betfairId = 0;
            _rotationNumber = 0;
            _aamsId = null;
            _lugasId = null;
            if (referenceIds == null)
            {
                return;
            }

            ReferenceIds = new ReadOnlyDictionary<string, string>(referenceIds);

            Merge(referenceIds, false);
        }

        /// <summary>
        /// Merges the specified reference ids
        /// </summary>
        /// <param name="referenceIds">The reference ids</param>
        /// <param name="checkDic">Check the dictionary for new values</param>
        public void Merge(IDictionary<string, string> referenceIds, bool checkDic)
        {
            if (referenceIds == null)
            {
                return;
            }

            if (referenceIds.TryGetValue("betradar", out var val))
            {
                int.TryParse(val, out _betradarId);
            }
            if (referenceIds.TryGetValue("BetradarCtrl", out val))
            {
                int.TryParse(val, out _betradarId);
            }
            if (referenceIds.TryGetValue("betfair", out val))
            {
                int.TryParse(val, out _betfairId);
            }
            if (referenceIds.TryGetValue("rotation_number", out val))
            {
                int.TryParse(val, out _rotationNumber);
            }
            if (referenceIds.TryGetValue("aams", out val))
            {
                _aamsId = int.TryParse(val, out var i)
                              ? (int?)i
                              : null;
            }
            referenceIds.TryGetValue("lugas", out _lugasId);

            if (!checkDic)
            {
                return;
            }

            var refIds = ReferenceIds == null
                             ? new Dictionary<string, string>()
                             : new Dictionary<string, string>(referenceIds);
            foreach (var id in referenceIds)
            {
                refIds[id.Key] = id.Value;
            }
            ReferenceIds = new ReadOnlyDictionary<string, string>(refIds);
        }
    }
}
