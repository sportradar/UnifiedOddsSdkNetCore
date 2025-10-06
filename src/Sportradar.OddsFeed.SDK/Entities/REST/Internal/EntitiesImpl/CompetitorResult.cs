// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class CompetitorResult : ICompetitorResult
    {
        /// <summary>
        /// Get the type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        public string Value { get; }

        /// <summary>
        /// Gets the specifiers
        /// </summary>
        /// <value>The specifiers</value>
        public string Specifiers { get; }

        internal CompetitorResult(CompetitorResultDto result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Type = result.Type;
            Value = result.Value;
            Specifiers = result.Specifiers;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Type={Type}, Value={Value}, Specifiers={Specifiers}";
        }
    }
}
