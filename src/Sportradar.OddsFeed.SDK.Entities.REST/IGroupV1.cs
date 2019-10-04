/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a competition group
    /// </summary>
    public interface IGroupV1 : IGroup
    {
        /// <summary>
        /// Gets the id of the group represented by the current <see cref="IGroup"/> instance
        /// </summary>

        string Id { get; }
    }
}
