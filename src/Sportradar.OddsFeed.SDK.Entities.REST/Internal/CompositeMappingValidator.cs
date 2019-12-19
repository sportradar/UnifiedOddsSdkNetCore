/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IMappingValidator"/> which wraps multiple market validators
    /// </summary>
    /// <seealso cref="IMappingValidator" />
    public class CompositeMappingValidator : IMappingValidator
    {
        /// <summary>
        /// The <see cref="IReadOnlyCollection{T}"/> containing actual validators
        /// </summary>
        private readonly IReadOnlyCollection<IMappingValidator> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeMappingValidator"/> class.
        /// </summary>
        /// <param name="validators">The <see cref="IReadOnlyCollection{T}"/> containing actual validators.</param>
        public CompositeMappingValidator(IEnumerable<IMappingValidator> validators)
        {
            Guard.Argument(validators).NotNull().NotEmpty();

            _validators = validators as IReadOnlyCollection<IMappingValidator> ?? new ReadOnlyCollection<IMappingValidator>(validators.ToList());
        }


        /// <summary>
        /// Validate the provided specifiers against current instance.
        /// </summary>
        /// <param name="specifiers">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing market specifiers.</param>
        /// <returns>True if the mapping associated with the current instance can be used to map associated market; False otherwise</returns>
        /// <exception cref="InvalidOperationException">Validation cannot be performed against the provided specifiers</exception>
        public bool Validate(IReadOnlyDictionary<string, string> specifiers)
        {
            return _validators.All(validator => validator.Validate(specifiers));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return _validators.Aggregate(string.Empty, (s, validator) => SdkInfo.SpecifiersDelimiter + validator);
        }
    }
}