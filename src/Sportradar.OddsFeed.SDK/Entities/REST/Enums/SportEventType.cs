/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Enums
{
    /// <summary>
    /// Enumerates available types of sport event types
    /// </summary>
    public enum SportEventType
    {
        /// <summary>
        /// Indicates a parent sport event type (multi-stage race event, ...)
        /// </summary>
        Parent,

        /// <summary>
        /// Indicates a child sport event type(a specific stage in multi-stage race event, ...)
        /// </summary>
        Child
    }
}
