/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    internal static class FlexMarketHelper
    {
        /// <summary>
        /// The name of the specifier required by the flex score markets
        /// </summary>
        private const string SpecifierName = "score";

        /// <summary>
        /// Gets the value of the specified specifier
        /// </summary>
        /// <param name="specifierName">Name of the specifier to parse</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}"/> containing market specifiers</param>
        /// <exception cref="InvalidOperationException">The specified specifier does not exist</exception>
        private static string GetSpecifier(string specifierName, IReadOnlyDictionary<string, string> specifiers)
        {
            Contract.Requires(!string.IsNullOrEmpty(specifierName));
            Contract.Requires(specifiers != null && specifiers.Any());

            string specifierValueString;
            if (!specifiers.TryGetValue(specifierName, out specifierValueString))
            {
                throw new InvalidOperationException($"Specifier with name {specifierName} does not exist");
            }
            return specifierValueString;
        }

        /// <summary>
        /// Gets the name of the flex score market outcome
        /// </summary>
        /// <param name="nameDescription">The descriptor of the outcome name</param>
        /// <param name="specifiers">The market specifiers</param>
        /// <returns>The name of the outcome</returns>
        /// <exception cref="NameExpressionException">The generation of the name failed due to missing specifier or due to wrong format of the name descriptor </exception>
        public static string GetName(string nameDescription, IReadOnlyDictionary<string, string> specifiers)
        {
            if (string.IsNullOrEmpty(nameDescription))
            {
                return nameDescription;
            }

            string specifierValue;
            try
            {
                specifierValue = GetSpecifier(SpecifierName, specifiers);
            }
            catch (InvalidOperationException ex)
            {
                throw new NameExpressionException("The required specifier score could not be found", ex);
            }

            Score specifierScore;
            try
            {
                specifierScore = Score.Parse(specifierValue);
            }
            catch (FormatException ex)
            {
                throw new NameExpressionException($"The value of the specifier 'score'= {specifierValue} is not a valid representation of a score", ex);
            }

            Score outcomeScore;
            try
            {
                outcomeScore = Score.Parse(nameDescription);
            }
            catch (FormatException ex)
            {
                throw new NameExpressionException($"The value {nameDescription} is not a valid representation of a score", ex);
            }
            return (outcomeScore + specifierScore).ToString();
        }
    }
}
