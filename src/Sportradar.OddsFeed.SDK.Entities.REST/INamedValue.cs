/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Specifies a contract implemented by classes representing values with names / descriptions
    /// </summary>
    public interface INamedValue
    {
        /// <summary>
        /// Gets the value associated with the current instance
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the description associated with the current instance
        /// </summary>
        string Description { get; }
    }
}
