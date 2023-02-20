/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Linq.Expressions;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    /// <summary>
    /// Contains extension methods for <see cref="Expression"/> class
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the inner (next in chain) <see cref="Expression"/>
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> from which to retrieve the inner expression</param>
        /// <returns></returns>
        public static Expression InnerExpression(this Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Expression;
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Object;
                case ExpressionType.Convert:
                    return ((UnaryExpression)expression).Operand;
                case ExpressionType.Add:

                case ExpressionType.AddChecked:

                case ExpressionType.And:

                case ExpressionType.AndAlso:

                case ExpressionType.ArrayLength:

                case ExpressionType.ArrayIndex:

                case ExpressionType.Coalesce:

                case ExpressionType.Conditional:

                case ExpressionType.Constant:

                case ExpressionType.ConvertChecked:

                case ExpressionType.Divide:

                case ExpressionType.Equal:

                case ExpressionType.ExclusiveOr:

                case ExpressionType.GreaterThan:

                case ExpressionType.GreaterThanOrEqual:

                case ExpressionType.Invoke:

                case ExpressionType.Lambda:

                case ExpressionType.LeftShift:

                case ExpressionType.LessThan:

                case ExpressionType.LessThanOrEqual:

                case ExpressionType.ListInit:

                case ExpressionType.MemberInit:

                case ExpressionType.Modulo:

                case ExpressionType.Multiply:

                case ExpressionType.MultiplyChecked:

                case ExpressionType.Negate:

                case ExpressionType.UnaryPlus:

                case ExpressionType.NegateChecked:

                case ExpressionType.New:

                case ExpressionType.NewArrayInit:

                case ExpressionType.NewArrayBounds:

                case ExpressionType.Not:

                case ExpressionType.NotEqual:

                case ExpressionType.Or:

                case ExpressionType.OrElse:

                case ExpressionType.Parameter:

                case ExpressionType.Power:

                case ExpressionType.Quote:

                case ExpressionType.RightShift:

                case ExpressionType.Subtract:

                case ExpressionType.SubtractChecked:

                case ExpressionType.TypeAs:

                case ExpressionType.TypeIs:

                case ExpressionType.Assign:

                case ExpressionType.Block:

                case ExpressionType.DebugInfo:

                case ExpressionType.Decrement:

                case ExpressionType.Dynamic:

                case ExpressionType.Default:

                case ExpressionType.Extension:

                case ExpressionType.Goto:

                case ExpressionType.Increment:

                case ExpressionType.Index:

                case ExpressionType.Label:

                case ExpressionType.RuntimeVariables:

                case ExpressionType.Loop:

                case ExpressionType.Switch:

                case ExpressionType.Throw:

                case ExpressionType.Try:

                case ExpressionType.Unbox:

                case ExpressionType.AddAssign:

                case ExpressionType.AndAssign:

                case ExpressionType.DivideAssign:

                case ExpressionType.ExclusiveOrAssign:

                case ExpressionType.LeftShiftAssign:

                case ExpressionType.ModuloAssign:

                case ExpressionType.MultiplyAssign:

                case ExpressionType.OrAssign:

                case ExpressionType.PowerAssign:

                case ExpressionType.RightShiftAssign:

                case ExpressionType.SubtractAssign:

                case ExpressionType.AddAssignChecked:

                case ExpressionType.MultiplyAssignChecked:

                case ExpressionType.SubtractAssignChecked:

                case ExpressionType.PreIncrementAssign:

                case ExpressionType.PreDecrementAssign:

                case ExpressionType.PostIncrementAssign:

                case ExpressionType.PostDecrementAssign:

                case ExpressionType.TypeEqual:

                case ExpressionType.OnesComplement:

                case ExpressionType.IsTrue:

                case ExpressionType.IsFalse:

                default:
                    throw new InvalidOperationException($"Un-supported expression with NodeType:{expression.NodeType} encountered");
            }
        }
    }
}
