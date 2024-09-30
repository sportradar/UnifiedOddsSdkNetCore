// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration
{
    /// <summary>
    /// A <see cref="IOperand"/> implementation which can handle simple expression based operators
    /// i.e. specifier +/- static value
    /// </summary>
    /// <seealso cref="SpecifierBasedOperator" />
    /// <seealso cref="IOperand" />
    internal class ExpressionOperand : SpecifierBasedOperator, IOperand
    {
        /// <summary>
        /// A <see cref="IReadOnlyDictionary{TKey,TValue}"/> containing market specifiers
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _specifiers;

        /// <summary>
        /// The <see cref="string"/> representation of the operand - i.e. name of the specifier
        /// </summary>
        private readonly string _operandString;

        /// <summary>
        /// A <see cref="int"/> static value of the expression
        /// </summary>
        private readonly int _staticValue;

        /// <summary>
        /// Indicates if the specifier is first in the operandString
        /// </summary>
        private readonly bool _specifierIsFirst;

        /// <summary>
        /// A <see cref="SimpleMathOperation"/> specifying the operation between static value and the value of the specifier
        /// </summary>
        private readonly SimpleMathOperation _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleOperand"/> class.
        /// </summary>
        /// <param name="specifiers">A <see cref="IReadOnlyDictionary{String,String}"/> containing market specifiers</param>
        /// <param name="operandString">The <see cref="string"/> representation of the operand - i.e. name of the specifier</param>
        /// <param name="operation">A <see cref="SimpleMathOperation"/> specifying the operation</param>
        /// <param name="staticValue">The value to be added to the value of the specifier</param>
        /// <param name="specifierIsFirst">Indicates if the specifier is first is operandString</param>
        public ExpressionOperand(IReadOnlyDictionary<string, string> specifiers, string operandString, SimpleMathOperation operation, int staticValue, bool specifierIsFirst = true)
        {
            Guard.Argument(specifiers, nameof(specifiers)).NotNull().NotEmpty();
            Guard.Argument(operandString, nameof(operandString)).NotNull().NotEmpty();
            Guard.Argument(operation, nameof(operation)).Require(Enum.IsDefined(typeof(SimpleMathOperation), operation));

            _specifiers = specifiers;
            _operandString = operandString;
            _operation = operation;
            _staticValue = staticValue;
            _specifierIsFirst = specifierIsFirst;
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="int" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" /></returns>
        /// <exception cref="InvalidOperationException">
        /// Static int value was not provided to the constructor
        /// or
        /// </exception>
        public Task<int> GetIntValue()
        {
            int specifierValue;
            try
            {
                ParseSpecifier(_operandString, _specifiers, out specifierValue);
            }
            catch (InvalidOperationException ex)
            {
                throw new NameExpressionException("Error occurred while evaluating name expression", ex);
            }

            switch (_operation)
            {
                case SimpleMathOperation.Add:
                    return Task.FromResult(specifierValue + _staticValue);
                case SimpleMathOperation.Subtract:
                    return _specifierIsFirst
                               ? Task.FromResult(specifierValue - _staticValue)
                               : Task.FromResult(_staticValue - specifierValue);
                default:
                    throw new InvalidOperationException($"Operation {Enum.GetName(typeof(SimpleMathOperation), _operation)} is not supported");
            }
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="decimal" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" /></returns>
        /// <exception cref="InvalidOperationException">
        /// Static decimal value was not provided to the constructor
        /// or
        /// </exception>
        public Task<decimal> GetDecimalValue()
        {
            decimal specifierValue;
            try
            {
                ParseSpecifier(_operandString, _specifiers, out specifierValue);
            }
            catch (InvalidOperationException ex)
            {
                throw new NameExpressionException("Error occurred while evaluating name expression", ex);
            }

            switch (_operation)
            {
                case SimpleMathOperation.Add:
                    return Task.FromResult(specifierValue + _staticValue);
                case SimpleMathOperation.Subtract:
                    return _specifierIsFirst
                               ? Task.FromResult(specifierValue - _staticValue)
                               : Task.FromResult(_staticValue - specifierValue);
                default:
                    throw new InvalidOperationException($"Operation {Enum.GetName(typeof(SimpleMathOperation), _operation)} is not supported");
            }
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="string" />
        /// </summary>
        /// <returns>A <see cref="Task{String}" /> containing the integer value of the operand as a <see cref="string" /></returns>
        public async Task<string> GetStringValue()
        {
            var newValue = await GetIntValue();
            return newValue.ToString();
        }
    }
}
