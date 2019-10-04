/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Messages.Feed
{
    /// <summary>
    /// Enumerates property usage requirements
    /// </summary>
    public enum PropertyUsage
    {
        /// <summary>
        /// The value of the property must not be specified
        /// </summary>
        FORBBIDEN,

        /// <summary>
        /// The usage of the property is optional
        /// </summary>
        OPTIONAL,

        /// <summary>
        /// The usage of the property is required
        /// </summary>
        REQUIRED
    }
}