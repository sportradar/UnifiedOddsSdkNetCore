/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Enumerates the results of object validation
    /// </summary>
    public enum ValidationResult
    {
        /// <summary>
        /// The validation was successful, the validated object is valid
        /// </summary>
        SUCCESS,

        /// <summary>
        /// The validation detected some minor issues, but the validated object can still be used for further processing
        /// </summary>
        PROBLEMS_DETECTED,

        /// <summary>
        /// The validation failed, the validated object is not valid
        /// </summary>
        FAILURE
    }
}