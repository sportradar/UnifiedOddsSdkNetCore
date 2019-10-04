/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by all classes representing messages received from the feed's REST interface
    /// </summary>
    public interface IRestMessage
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> instance specifying when the message represented by the current <see cref="IRestMessage"/>
        /// was generated, or a null reference if time of generation is not defined
        /// </summary>
        DateTime? GeneratedAt { get; }
    }
}
