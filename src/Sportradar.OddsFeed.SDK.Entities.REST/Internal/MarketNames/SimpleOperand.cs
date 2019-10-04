/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Represents a <see cref="IOperand"/> capable of handling simple operand - i.e. the operand is the name of a specifier
    /// </summary>
    /// <seealso cref="IOperand" />
    public class SimpleOperand : SpecifierBasedOperator, IOperand
    {
        /// <summary>
        /// A <see cref="IReadOnlyDictionary{String,String}"/> containing market specifiers.
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        /// <summary>
        /// The <see cref="string"/> representation of the operand - i.e. name of the specifier.
        /// </summary>
        private readonly string _operandString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleOperand"/> class.
        /// </summary>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String,String}"/> containing market specifiers.</param>
        /// <param name="operandString">The <see cref="string"/> representation of the operand - i.e. name of the specifier.</param>
        public SimpleOperand(IReadOnlyDictionary<string, string> specifiers, string operandString)
        {
            Contract.Requires(specifiers != null && specifiers.Any());
            Contract.Requires(!string.IsNullOrEmpty(operandString));

            _specifiers = specifiers;
            _operandString = operandString;
        }

        /// <summary>
        /// Specifies invariants as needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(_specifiers != null && _specifiers.Any());
            Contract.Invariant(!string.IsNullOrEmpty(_operandString));
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="int" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" />.</returns>
        public Task<int> GetIntValue()
        {
            int value;
            try
            {
                ParseSpecifier(_operandString, _specifiers, out value);
            }
            catch (InvalidOperationException ex)
            {
                throw new NameExpressionException("Error occurred while evaluating name expression", ex);
            }
            return Task.FromResult(value);
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="decimal" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" />.</returns>
        public Task<decimal> GetDecimalValue()
        {
            decimal value;
            try
            {
                ParseSpecifier(_operandString, _specifiers, out value);
            }
            catch (InvalidOperationException ex)
            {
                throw new NameExpressionException("Error occurred while evaluating name expression", ex);
            }
            return Task.FromResult(value);
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="string" />
        /// </summary>
        /// <returns>A <see cref="Task{String}" /> containing the value of the operand as a <see cref="string" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<string> GetStringValue()
        {
            string name;
            if (!_specifiers.TryGetValue(_operandString, out name))
            {
                throw new NameExpressionException($"Market specifiers do not contain a specifier with key={_operandString}", null);
            }
            return Task.FromResult(name);
        }
    }
}