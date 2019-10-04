/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a base class for classes representing messages received from the RESS API
    /// </summary>
    /// <seealso cref="IRestMessage" />
    internal abstract class RestMessage : IRestMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestMessage"/> class
        /// </summary>
        /// <param name="generatedAt">a <see cref="DateTime" /> instance specifying when the message represented by the current <see cref="IRestMessage" /> was generated</param>
        protected RestMessage(DateTime? generatedAt)
        {
            GeneratedAt = generatedAt;
        }

        /// <summary>
        /// Gets a <see cref="DateTime" /> instance specifying when the message represented by the current <see cref="IRestMessage" />
        /// was generated, or a null reference if time of generation is not defined
        /// </summary>
        /// <value>The generated at.</value>
        public DateTime? GeneratedAt { get; }
    }
}
