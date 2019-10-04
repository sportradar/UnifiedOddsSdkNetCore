/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing streaming channels
    /// </summary>
    public interface IStreamingChannel : IEntityPrinter
    {
        /// <summary>
        /// Gets a value uniquely identifying the current streaming channel
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the name of the streaming channel represented by the current instance
        /// </summary>
        string Name { get; }
    }
}
