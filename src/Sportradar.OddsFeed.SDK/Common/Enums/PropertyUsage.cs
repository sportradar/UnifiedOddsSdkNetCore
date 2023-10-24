/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumerates property usage requirements
    /// </summary>
    public enum PropertyUsage
    {
        /// <summary>
        /// The value of the property must not be specified
        /// </summary>
        Forbidden,

        /// <summary>
        /// The usage of the property is optional
        /// </summary>
        Optional,

        /// <summary>
        /// The usage of the property is required
        /// </summary>
        Required
    }
}
