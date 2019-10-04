/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Enumerates possible options for displaying outcome odds
    /// </summary>
    public enum OddsDisplayType
    {
        /// <summary>
        /// The decimal format
        /// </summary>
        Decimal,

        /// <summary>
        /// The American odds format
        /// </summary>
        // ReSharper disable once InconsistentNaming
        American
    }
}
