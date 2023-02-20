/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Enumerates the results of object validation
    /// </summary>
    internal enum ValidationResult
    {
        /// <summary>
        /// The validation was successful, the validated object is valid
        /// </summary>
        Success,

        /// <summary>
        /// The validation detected some minor issues, but the validated object can still be used for further processing
        /// </summary>
        ProblemsDetected,

        /// <summary>
        /// The validation failed, the validated object is not valid
        /// </summary>
        Failure
    }
}
