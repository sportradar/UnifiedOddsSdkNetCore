/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// A <see cref="IOperand"/> implementation which can handle simple expression based operators
    /// i.e. specifier +/- static value
    /// </summary>
    /// <seealso cref="SpecifierBasedOperator" />
    /// <seealso cref="IOperand" />
    public class ExpressionOperand : SpecifierBasedOperator, IOperand
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
        public ExpressionOperand(IReadOnlyDictionary<string, string> specifiers, string operandString, SimpleMathOperation operation, int staticValue)
        {
            Guard.Argument(specifiers).NotNull().NotEmpty();
            Guard.Argument(operandString).NotNull().NotEmpty();
            Guard.Argument(operation).Require(System.Enum.IsDefined(typeof(SimpleMathOperation), operation));

            _specifiers = specifiers;
            _operandString = operandString;
            _operation = operation;
            _staticValue = staticValue;
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="int" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" /></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Static int value was not provided to the constructor
        /// or
        /// </exception>
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

            switch (_operation)
            {
                case SimpleMathOperation.ADD:
                    return Task.FromResult(value + _staticValue);
                case SimpleMathOperation.SUBTRACT:
                    return Task.FromResult(value - _staticValue);
                default:
                    throw new InvalidOperationException($"Operation {Enum.GetName(typeof(SimpleMathOperation), _operation)} is not supported");
            }

        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="decimal" />
        /// </summary>
        /// <returns>A <see cref="Task{Int32}" /> containing the value of the operand as a <see cref="int" /></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Static decimal value was not provided to the constructor
        /// or
        /// </exception>
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

            switch (_operation)
            {
                case SimpleMathOperation.ADD:
                    return Task.FromResult(value + _staticValue);
                case SimpleMathOperation.SUBTRACT:
                    return Task.FromResult(value - _staticValue);
                default:
                    throw new InvalidOperationException($"Operation {Enum.GetName(typeof(SimpleMathOperation), _operation)} is not supported");
            }
        }

        /// <summary>
        /// Gets the value of the operand as a <see cref="string" />
        /// </summary>
        /// <returns>A <see cref="Task{String}" /> containing the value of the operand as a <see cref="string" /></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public Task<string> GetStringValue()
        {
            throw new NotSupportedException();
        }
    }
}