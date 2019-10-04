/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming
namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Enumerates sdk exception handling strategies
    /// </summary>
    public enum ExceptionHandlingStrategy
    {
        /// <summary>
        /// Specifies a strategy in which none of the exceptions are thrown to caller
        /// </summary>
        THROW = 0,

        /// <summary>
        /// Specifies a strategy in which all exceptions are handled by the called instance
        /// </summary>
        CATCH = 1
    }
}
