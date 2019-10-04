/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A factory used to build <see cref="IMappingValidator"/> from their string definition
    /// </summary>
    /// <seealso cref="IMappingValidatorFactory" />
    public class MappingValidatorFactory : IMappingValidatorFactory
    {
        /// <summary>
        /// A regex pattern used to match specifiers requiring specific decimal value (e.g. x.5)
        /// </summary>
        private const string DecimalPattern = @"\A\*\.\d{1,2}\z";

        /// <summary>
        /// Builds a single <see cref="IMappingValidator"/>
        /// </summary>
        /// <param name="name">The name of the specifier</param>
        /// <param name="value">The value defining required value(s) of the specifier</param>
        /// <returns>IMappingValidator</returns>
        private static IMappingValidator BuildSingle(string name, string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(!string.IsNullOrEmpty(value));

            return Regex.IsMatch(value, DecimalPattern)
                ? new DecimalValueMappingValidator(name, decimal.Parse(value.Replace("*", "0"), NumberStyles.Any, CultureInfo.InvariantCulture))
                : (IMappingValidator) new SpecificValueMappingValidator(name, value);
        }

        /// <summary>
        /// Builds and returns a <see cref="IMappingValidator"/> from the provided string
        /// </summary>
        /// <param name="value">A value defining the <see cref="IMappingValidator"/> to be constructed</param>
        /// <returns>A <see cref="IMappingValidator"/> build from the provided string</returns>
        public IMappingValidator Build(string value)
        {
            IReadOnlyDictionary<string, string> specifiers;
            try
            {
                specifiers = FeedMapperHelper.GetValidForAttributes(value);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is FormatException)
                {
                    throw new ArgumentException("Value is not correct", nameof(value), ex);
                }
                throw;
            }

            if (specifiers.Count == 1)
            {
                return BuildSingle(specifiers.Keys.First(), specifiers.Values.First());
            }

            var validators = new List<IMappingValidator>(specifiers.Count);
            validators.AddRange(specifiers.Keys.Select(specifier => BuildSingle(specifier, specifiers[specifier])));
            return new CompositeMappingValidator(validators);
        }
    }
}