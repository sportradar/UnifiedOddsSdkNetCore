/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Runtime.Serialization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a competing team
    /// </summary>
    public interface ITeamCompetitor : ICompetitor
    {
        /// <summary>
        /// Gets a qualifier additionally describing the competitor (e.g. home, away, ...)
        /// </summary>
        [DataMember]
        string Qualifier { get; }
    }
}
