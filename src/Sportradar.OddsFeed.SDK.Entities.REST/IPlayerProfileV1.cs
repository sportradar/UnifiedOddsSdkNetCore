/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Runtime.Serialization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a player profile
    /// </summary>
    public interface IPlayerProfileV1 : IPlayerProfile
    {
        /// <summary>
        /// Gets the gender
        /// </summary>
        [DataMember]
        string Gender { get; }
    }
}