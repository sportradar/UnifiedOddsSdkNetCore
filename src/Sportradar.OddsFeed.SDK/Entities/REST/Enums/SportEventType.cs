/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Entities.REST.Enums
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