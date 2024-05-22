// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Represents a division of associated competitor
    /// </summary>
    public interface IDivision
    {
        /// <summary>
        /// Id of the division
        /// </summary>
        int? Id { get; }

        /// <summary>
        /// Name of the division
        /// </summary>
        string Name { get; }
    }
}
