// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumerates sdk exception handling strategies
    /// </summary>
    public enum ExceptionHandlingStrategy
    {
        /// <summary>
        /// Specifies a strategy in which none of the exceptions are thrown to caller
        /// </summary>
        Throw = 0,

        /// <summary>
        /// Specifies a strategy in which all exceptions are handled by the called instance
        /// </summary>
        Catch = 1
    }
}
