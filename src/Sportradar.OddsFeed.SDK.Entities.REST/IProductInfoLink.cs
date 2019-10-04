/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing product info links
    /// </summary>
    public interface IProductInfoLink : IEntityPrinter
    {
        /// <summary>
        /// Gets the reference to the product info represented by the current instance
        /// </summary>
        string Reference { get; }

        /// <summary>
        /// Gets the name of the product link represented by the current instance
        /// </summary>
        string Name { get; }
    }
}
