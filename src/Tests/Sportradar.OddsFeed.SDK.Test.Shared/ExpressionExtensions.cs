/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq.Expressions;

namespace Sportradar.OddsFeed.SDK.Test.Shared
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
                default:
                    throw new InvalidOperationException($"Un-supported expression with NodeType:{expression.NodeType} encountered");
            }
        }
    }
}
