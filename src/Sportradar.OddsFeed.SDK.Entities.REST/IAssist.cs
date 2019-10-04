/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing assists on a sport event
    /// </summary>
    public interface IAssist : IPlayer
    {
        /// <summary>
        /// Gets a <see cref="string"/> specifying the type of the assist
        /// </summary>
        string Type { get; }
    }
}
