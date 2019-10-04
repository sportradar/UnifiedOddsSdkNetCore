/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IMappingValidator"/> which checks the specified market specifier against specific value
    /// </summary>
    public class SpecificValueMappingValidator : IMappingValidator
    {
        /// <summary>
        /// The name of the specifier in the valid for attribute
        /// </summary>
        private readonly string _specifierName;

        /// <summary>
        /// The required value of the specifier
        /// </summary>
        private readonly string _specifierValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificValueMappingValidator"/> class.
        /// </summary>
        /// <param name="specifierName">The name of the specifier in the valid for attribute.</param>
        /// <param name="specifierValue">The required value of the specifier.</param>
        public SpecificValueMappingValidator(string specifierName, string specifierValue)
        {
            Contract.Requires(!string.IsNullOrEmpty(specifierName));
            Contract.Requires(!string.IsNullOrEmpty(specifierValue));

            this._specifierName = specifierName;
            this._specifierValue = specifierValue;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(_specifierValue));
            Contract.Invariant(!string.IsNullOrEmpty(_specifierName));
        }

        /// <summary>
        /// Validate the provided specifiers against current instance.
        /// </summary>
        /// <param name="specifiers">The <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing market specifiers.</param>
        /// <returns>True if the mapping associated with the current instance can be used to map associated market; False otherwise</returns>
        /// <exception cref="InvalidOperationException">Validation cannot be performed against the provided specifiers</exception>
        public bool Validate(IReadOnlyDictionary<string, string> specifiers)
        {
            string specifierValue;
            if (!specifiers.TryGetValue(_specifierName, out specifierValue))
            {
                throw new InvalidOperationException($"Required specifier[{_specifierName}] does not exist in the provided specifiers");
            }
            return _specifierValue == specifiers[_specifierName];
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{_specifierName}={_specifierValue}";
        }
    }
}