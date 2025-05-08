// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// A contract for classes implementing array a+of references
    /// </summary>
    public interface IReferenceV1 : IReference
    {
        /// <summary>
        /// Returns the Lugas id for this instance if provided among reference ids, null otherwise
        /// </summary>
        /// <returns>The Lugas id for this instance if provided among reference ids, null otherwise</returns>
        string LugasId { get; }
    }
}
