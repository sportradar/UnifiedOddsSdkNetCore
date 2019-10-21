/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using System.Text.RegularExpressions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Factory used to build <see cref="INameExpression"/> instances
    /// </summary>
    internal class NameExpressionFactory : INameExpressionFactory
    {
        /// <summary>
        /// A regex pattern used to detect '{$competitor1} expression operands
        /// </summary>
        private const string SequencedCompetitorOperandRegexPatter = @"\Acompetitor[12]";

        /// <summary>
        /// A <see cref="IOperandFactory"/> used to build <see cref="IOperand"/> instances required by name expressions
        /// </summary>
        private readonly IOperandFactory _operandFactory;

        /// <summary>
        /// A <see cref="IProfileCache"/> used to fetch profiles
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameExpressionFactory"/> class
        /// </summary>
        /// <param name="operandFactory">A <see cref="IOperandFactory"/> used to build <see cref="IOperand"/> instances required by name expressions</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used to fetch profiles</param>
        public NameExpressionFactory(IOperandFactory operandFactory, IProfileCache profileCache)
        {
            Guard.Argument(operandFactory).NotNull();
            Guard.Argument(profileCache).NotNull();

            _operandFactory = operandFactory;
            _profileCache = profileCache;
        }

        /// <summary>
        /// Ensures that the provided <see cref="IReadOnlyDictionary{String, String}"/> is not a null reference or empty dictionary
        /// </summary>
        /// <param name="specifiers">The <see cref="IReadOnlyDictionary{String, String}"/> to be checked.</param>
        private static void EnsureSpecifiersNotNullOrEmpty(IReadOnlyDictionary<string, string> specifiers)
        {
            if (specifiers == null || !specifiers.Any())
            {
                throw new ArgumentException("value cannot be a null reference or an empty dictionary", nameof(specifiers));
            }
        }

        /// <summary>
        /// Builds and returns a <see cref="INameExpression"/> specified by the passed <code>operand</code>
        /// </summary>
        /// <param name="operand">The operand of the name expression</param>
        /// <param name="sportEvent">The <see cref="ISportEvent"/> instance associated with the target</param>
        /// <returns></returns>
        private INameExpression BuildEntityNameExpression(string operand, ISportEvent sportEvent)
        {
            Guard.Argument(operand).NotNull().NotEmpty();
            Guard.Argument(sportEvent).NotNull();

            // expression {$competitor(1-2)} indicates we need to get the name of the competitor from the sport event
            if (Regex.IsMatch(operand, SequencedCompetitorOperandRegexPatter))
            {
                return new EntityNameExpression(operand, sportEvent);
            }
            if(operand.Equals("event", StringComparison.InvariantCultureIgnoreCase))
            {
                return new EntityNameExpression(operand, sportEvent);
            }

            throw new ArgumentException($"operand:{operand} is not a valid operand for $ operator. Valid operators are: 'competitor1', 'competitor2', 'event'", nameof(operand));
        }

        /// <summary>
        /// Builds and returns a <see cref="INameExpression" /> instance which can be used to generate name from the provided expression
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent" /> instance representing associated sport @event</param>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String, String}" /> representing specifiers of the associated market</param>
        /// <param name="operator">A <see cref="string" /> specifying the operator for which to build the expression</param>
        /// <param name="operand">An operand for the built expression</param>
        /// <returns>The constructed <see cref="INameExpression" /> instance</returns>
        public INameExpression BuildExpression(ISportEvent sportEvent, IReadOnlyDictionary<string, string> specifiers, string @operator, string operand)
        {
            if (sportEvent == null)
            {
                throw new ArgumentNullException(nameof(sportEvent));
            }
            if (string.IsNullOrEmpty(operand))
            {
                throw new ArgumentNullException(nameof(operand));
            }

            if (@operator == null)
            {
                EnsureSpecifiersNotNullOrEmpty(specifiers);
                return new CardinalNameExpression(_operandFactory.BuildOperand(specifiers, operand));
            }

            switch (Array.IndexOf(NameExpressionHelper.DefinedOperators, @operator))
            {
                case 0: //+
                {
                    EnsureSpecifiersNotNullOrEmpty(specifiers);
                    return new PlusNameExpression(_operandFactory.BuildOperand(specifiers, operand));
                }
                case 1: //-
                {
                    EnsureSpecifiersNotNullOrEmpty(specifiers);
                    return new MinusNameExpression(_operandFactory.BuildOperand(specifiers, operand));
                }
                case 2: //$
                {
                    return BuildEntityNameExpression(operand, sportEvent);
                }
                case 3: //!
                {
                    EnsureSpecifiersNotNullOrEmpty(specifiers);
                    return new OrdinalNameExpression(_operandFactory.BuildOperand(specifiers, operand));
                }
                case 4: //%
                {
                    EnsureSpecifiersNotNullOrEmpty(specifiers);
                    return new PlayerProfileExpression(_profileCache, _operandFactory.BuildOperand(specifiers, operand));
                }
                default:
                {
                    throw new ArgumentException($"Operator {@operator} is not supported. Supported operators are: {string.Join(",", NameExpressionHelper.DefinedOperators)}", nameof(@operator));
                }
            }
        }
    }
}