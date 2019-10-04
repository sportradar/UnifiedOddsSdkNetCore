/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/


namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes providing basic tournament round information
    /// </summary>
    public interface IRoundV2 : IRoundV1
    {
        /// <summary>
        /// Gets the phase of the associated round
        /// </summary>
        string Phase { get; }
    }
}