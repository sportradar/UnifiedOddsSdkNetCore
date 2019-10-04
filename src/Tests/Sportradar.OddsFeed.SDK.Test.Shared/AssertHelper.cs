/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class AssertHelper
    {
        private const string NotEqualMessagePattern = "Value of property {0} is not correct. Expected={1}, Actual={2}";

        private const string IsNullPattern = "Value of property {0} cannot be a null reference";

        private const string IsNotNullPattern = "Value of property {0} must be a null reference";

        private static readonly NullFormat ToStringFormatter = new NullFormat();

        private readonly PathManager _pathManager;

        public AssertHelper(object instance)
        {
            _pathManager = new PathManager(instance);
        }

        public T StepInto<T>(Expression<Func<T>> propertyExpression, bool assertNotNull = true)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            _pathManager.Add(ExpressionPath.Create(propertyExpression.Body));
            var result = (T)_pathManager.Result;

            if (assertNotNull && EqualityComparer<T>.Default.Equals(result, default(T)))
            {
                Assert.Fail(IsNullPattern, _pathManager.GetPath(null));
            }
            return result;
        }

        public void StepOut()
        {
            _pathManager.RemoveLast();
        }

        public T Next<T>(Expression<Func<T>> propertyExpression)
        {
            StepOut();
            return StepInto(propertyExpression);
        }

        public void AreEqual<T>(Expression<Func<T>> propertyExpression, T expectedValue)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var expressionPath = ExpressionPath.Create(propertyExpression.Body);
            var actualValue = (T) (expressionPath.Result);
            if (!EqualityComparer<T>.Default.Equals(actualValue, expectedValue))
            {
                var message = string.Format(ToStringFormatter, NotEqualMessagePattern, _pathManager.GetPath(expressionPath), expectedValue, actualValue);
                Assert.Fail(message);
            }
        }

        public void IsNull<T>(Expression<Func<T>> propertyExpression) where T : class
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var expressionPath = ExpressionPath.Create(propertyExpression.Body);
            var actualValue = (T)(expressionPath.Result);
            if (actualValue != null)
            {
                var message = string.Format(ToStringFormatter, IsNotNullPattern, _pathManager.GetPath(expressionPath));
                Assert.Fail(message);
            }
        }

        public void IsNotNull<T>(Expression<Func<T>> propertyExpression) where T : class
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var expressionPath = ExpressionPath.Create(propertyExpression.Body);
            var actualValue = (T)(expressionPath.Result);
            if (actualValue == null)
            {
                var message = string.Format(ToStringFormatter, IsNullPattern, _pathManager.GetPath(expressionPath));
                Assert.Fail(message);
            }
        }

        public class PathManager
        {
            /// <summary>
            /// The root instance
            /// </summary>
            private readonly object _instance;

            /// <summary>
            /// A <see cref="List{T}"/> containing <see cref="ExpressionPath"/> instance representing expression paths
            /// </summary>
            private readonly List<ExpressionPath> _paths;

            /// <summary>
            /// Gets an <see cref="object"/> obtained by invoking <see cref="ExpressionPath"/> instances added to current <see cref="PathManager"/>
            /// </summary>
            public object Result => _paths.Any()
                ? _paths.Last().Result
                : _instance;

            /// <summary>
            /// Initializes a new instance of the <see cref="PathManager"/> class
            /// </summary>
            /// <param name="instance">An <see cref="object"/> representing root instance for the constructed instance</param>
            public PathManager(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }
                _instance = instance;
                _paths = new List<ExpressionPath>();
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="ExpressionPath.Instance"/> of the passed <see cref="ExpressionPath"/> as the object
            /// obtained by executing the <see cref="ExpressionPath"/> instances currently added to current <see cref="ExpressionPath"/>
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool IsAligned(ExpressionPath path)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }
                return Result == path.Instance;
            }

            /// <summary>
            /// Adds the passed <see cref="ExpressionPath"/> to the current <see cref="PathManager"/> instance
            /// </summary>
            /// <param name="path">A <see cref="ExpressionPath"/> instance to be added</param>
            public void Add(ExpressionPath path)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }
                if (!IsAligned(path))
                {
                    throw new InvalidOperationException("The provided ExpressionPath is not aligned with the last ExpressionPath added to PathManager");
                }
                _paths.Insert(_paths.Count, path);
            }

            /// <summary>
            /// Removes the <see cref="ExpressionPath"/> instance which was last added to current <see cref="PathManager"/> instance.
            /// </summary>
            public void RemoveLast()
            {
                if (!_paths.Any())
                {
                    throw new InvalidOperationException("Ne ExpressionPath instances are currently added");
                }

                _paths.Remove(_paths.Last());
            }

            /// <summary>
            /// Gets a <see cref="string"/> representing <see cref="ExpressionPath"/> instances added to current <see cref="PathManager"/> extended by the provided <see cref="ExpressionPath"/>
            /// </summary>
            /// <param name="additionalPath">A null reference or a <see cref="ExpressionPath"/> representing an additional path to be added to returned string</param>
            /// <returns>A constructed <see cref="string"/></returns>
            public string GetPath(ExpressionPath additionalPath)
            {
                var builder = new StringBuilder(_instance.GetType().Name);
                foreach (var path in _paths)
                {
                    if (path.HasName && !path.StartsWithIndexer)
                    {
                        builder.Append(".");
                    }
                    if (path.HasName)
                    {
                        builder.Append(path);
                    }
                }
                if (additionalPath == null || !additionalPath.HasName)
                {
                    return builder.ToString();
                }

                if (!additionalPath.StartsWithIndexer)
                {
                    builder.Append(".");
                }
                builder.Append(additionalPath);
                return builder.ToString();
            }
        }

        public class NullFormat : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type service)
            {
                return service == typeof(ICustomFormatter)
                    ? this
                    : null;
            }

            public string Format(string format, object arg, IFormatProvider provider)
            {
                if (arg == null)
                {
                    return "<null>";
                }
                var formattable = arg as IFormattable;
                if (formattable != null)
                {
                    return formattable.ToString(format, provider);
                }
                return arg.ToString();
            }
        }

        public class ExpressionPath
        {
            /// <summary>
            /// A <see cref="List{T}"/> of <see cref="ExpressionDetail"/> instances representing a nested <see cref="Expression"/>
            /// </summary>
            private readonly IList<ExpressionDetail> _items;

            /// <summary>
            /// Gets a <see cref="object"/> representing an instance on which the associated <see cref="Expression"/> was invoked
            /// </summary>
            public object Instance => _items.First().Instance;

            /// <summary>
            /// Gets a <see cref="object"/> representing a result of the invocation of associated <see cref="Expression"/>
            /// </summary>
            public object Result => _items.Last().Result;

            /// <summary>
            /// Gets a value indicating whether the current <see cref="ExpressionPath"/> has a displayable name
            /// </summary>
            public bool HasName
            {
                get { return _items.Any(i => i.HasName); }
            }

            /// <summary>
            /// Gets a value indicating whether the first expression in the current <see cref="ExpressionPath"/> instance is an indexer
            /// </summary>
            public bool StartsWithIndexer
            {
                get
                {
                    var firstWithName = _items.FirstOrDefault(i => i.HasName);
                    return firstWithName != null && firstWithName.IsIndexer;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionPath"/> class
            /// </summary>
            /// <param name="items"> A <see cref="List{T}"/> of <see cref="ExpressionDetail"/> instances representing a nested <see cref="Expression"/></param>
            public ExpressionPath(IEnumerable<ExpressionDetail> items)
            {
                var expressionDetails = items == null
                    ? null
                    : items as IList<ExpressionDetail> ?? items.ToList();

                if (expressionDetails == null || !expressionDetails.Any())
                {
                    throw new ArgumentException("Value cannot be a null reference or an empty string", nameof(items));
                }
                _items = expressionDetails;
            }

            /// <summary>
            /// Creates a new <see cref="ExpressionPath"/> from provided <see cref="Expression"/> instance
            /// </summary>
            /// <param name="expression">The <see cref="Expression"/> instance</param>
            /// <returns>The <see cref="ExpressionPath"/> instance created from the provided <see cref="Expression"/> instance.</returns>
            public static ExpressionPath Create(Expression expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }

                var pathItems = new List<ExpressionDetail>();

                var current = expression;
                while (true)
                {
                    var inner = current.InnerExpression();

                    if (inner.NodeType == ExpressionType.Constant)
                    {
                        break;
                    }

                    pathItems.Add(new ExpressionDetail(current));
                    current = inner;
                }

                if (!pathItems.Any())
                {
                    throw new ArgumentException("The expression.Body must be either a MemberExpression or a MethodCallExpression", nameof(expression));
                }
                pathItems.Reverse();
                return new ExpressionPath(pathItems);
            }

            /// <summary>
            /// Constructs and returns a <see cref="string"/> representation of the current <see cref="ExpressionPath"/> instance
            /// </summary>
            /// <returns>a <see cref="string"/> representation of the current <see cref="ExpressionPath"/> instance</returns>
            public override string ToString()
            {
                var builder = new StringBuilder();
                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (i != 0 && item.HasName && !item.IsIndexer)
                    {
                        builder.Append(".");
                    }
                    if (item.HasName)
                    {
                        builder.Append(item);
                    }
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Contains basic information about a <see cref="Expression"/>
        /// </summary>
        public class ExpressionDetail
        {
            /// <summary>
            /// A name of the index getter method
            /// </summary>
            private const string IndexerName = "get_Item";

            /// <summary>
            /// An array containing <see cref="ExpressionType"/> instance supported by <see cref="ExpressionDetail"/> class
            /// </summary>
            private static readonly ExpressionType[] SupportedTypes =
            {
                ExpressionType.MemberAccess,
                ExpressionType.Call,
                ExpressionType.Convert
            };

            /// <summary>
            /// An <see cref="Expression"/> associated with the current <see cref="ExpressionDetail"/> instance
            /// </summary>
            private readonly Expression _expression;

            /// <summary>
            /// A value indicating whether an attempt to set the <see cref="TargetName"/> property from the <code>_expression</code> was already made.
            /// </summary>
            private bool _targetNameRetrieved;

            /// <summary>
            /// A <see cref="TargetName"/> property backing field
            /// </summary>
            private string _targetName;

            /// <summary>
            /// A <see cref="Instance"/> property backing field
            /// </summary>
            private object _instance;

            /// <summary>
            /// A <see cref="Arguments"/> property backing field
            /// </summary>
            private List<object> _arguments;

            /// <summary>
            /// A <see cref="Result"/> property backing field
            /// </summary>
            private object _result;

            /// <summary>
            /// A value indicating whether an attempt to set the <see cref="Result"/> property from the <code>_expression</code> was already made.
            /// </summary>
            private bool _resultRetrieved;

            /// <summary>
            /// Gets a <see cref="ExpressionType"/> specifying the type of the associated <see cref="Expression"/>
            /// </summary>
            public ExpressionType NodeType => _expression.NodeType;

            /// <summary>
            /// A value indicating whether the current <see cref="ExpressionDetail"/> instance represents an indexer
            /// </summary>
            public bool IsIndexer => TargetName == IndexerName;

            /// <summary>
            /// Gets a value indicating whether the name property is available for current <see cref="ExpressionDetail"/> instance
            /// </summary>
            public bool HasName => NodeType == ExpressionType.MemberAccess || NodeType == ExpressionType.Call;

            /// <summary>
            /// Gets a <see cref="string"/> specifying the name of the operation (name of the property, method, ...) associated with the <see cref="Expression"/>
            /// </summary>
            public string TargetName
            {
                get
                {
                    if (!_targetNameRetrieved)
                    {
                        SetTargetName();
                    }
                    return _targetName;
                }
            }

            /// <summary>
            /// Gets a <see cref="object"/> representing an instance on which the represented <see cref="Expression"/> operates
            /// </summary>
            public object Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        SetInstance();
                    }
                    return _instance;
                }
            }

            /// <summary>
            /// Gets an <see cref="IEnumerable{T}"/> containing arguments of the operation associated with the <see cref="Expression"/>
            /// </summary>
            public IEnumerable<object> Arguments
            {
                get
                {
                    if (_arguments == null)
                    {
                        SetArguments();
                    }
                    return _arguments;
                }
            }

            /// <summary>
            /// Gets an <see cref="object"/> representing the result of the operation
            /// </summary>
            public object Result
            {
                get
                {
                    if (!_resultRetrieved)
                    {
                        SetResult();
                    }
                    return _result;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionDetail"/> class
            /// </summary>
            /// <param name="expression">A <see cref="Expression"/> instance</param>
            /// <param name="previous">A <see cref="ExpressionDetail"/>instance representing previous (inner) expression</param>
            public ExpressionDetail(Expression expression, ExpressionDetail previous)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }
                if (!SupportedTypes.Contains(expression.NodeType))
                {
                    throw new ArgumentException($"The ExpressionType:{expression.NodeType}, is not supported");
                }
                if (previous == null)
                {
                    throw new ArgumentNullException(nameof(previous));
                }
                _instance = previous.Result;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionDetail"/> class
            /// </summary>
            /// <param name="expression">A <see cref="Expression"/> instance</param>
            public ExpressionDetail(Expression expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }
                if (!SupportedTypes.Contains(expression.NodeType))
                {
                    throw new ArgumentException($"The ExpressionType:{expression.NodeType}, is not supported");
                }
                _expression = expression;
            }

            /// <summary>
            /// Compiles and invokes the passed <see cref="Expression"/> instance
            /// </summary>
            /// <param name="expression">The <see cref="Expression"/> instance to be invoked</param>
            /// <returns>The result of the <see cref="Expression"/> invocation</returns>
            private static object InvokeExpression(Expression expression)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException(nameof(expression));
                }

                var targetOnly = Expression.Lambda(expression, null);
                var compiled = targetOnly.Compile();
                return compiled.DynamicInvoke(null);
            }

            /// <summary>
            /// Sets the <see cref="TargetName"/> property to value retrieved from <see cref="Expression"/> instance associated with current <see cref="ExpressionDetail"/> instance
            /// </summary>
            private void SetTargetName()
            {
                if (_targetNameRetrieved)
                {
                    throw new InvalidOperationException("TargetName property was already set");
                }

                _targetNameRetrieved = true;
                switch (NodeType)
                {
                    case(ExpressionType.Call):
                        _targetName = ((MethodCallExpression) _expression).Method.Name;
                        break;
                    case(ExpressionType.MemberAccess):
                        _targetName = ((MemberExpression) _expression).Member.Name;
                        break;
                }
            }

            /// <summary>
            /// Sets the <see cref="Instance"/> property by invoking the inner expression of the <see cref="Expression"/> instance associated by current <see cref="ExpressionDetail"/> instance
            /// </summary>
            private void SetInstance()
            {
                if (_instance != null)
                {
                    throw new InvalidOperationException("The Instance property was already set");
                }

                var innerExpression = _expression.InnerExpression();
                _instance = InvokeExpression(innerExpression);
            }


            /// <summary>
            /// Sets the <see cref="Arguments"/> property by invoking argument expressions of the <see cref="Expression"/> associated with current <see cref="ExpressionDetail"/> instance
            /// </summary>
            private void SetArguments()
            {
                if (_arguments != null)
                {
                    throw new InvalidOperationException("Arguments property was already set");
                }

                var methodCallExpression = _expression as MethodCallExpression;
                _arguments = methodCallExpression == null
                    ? new List<object>()
                    : methodCallExpression.Arguments.Select(InvokeExpression).ToList();
            }

            /// <summary>
            /// Sets the <see cref="Result"/> property by invoking the <see cref="Expression"/> associated with current <see cref="ExpressionDetail"/> instance
            /// </summary>
            private void SetResult()
            {
                if (_resultRetrieved)
                {
                    throw new InvalidOperationException("Result property was already set");
                }

                _resultRetrieved = true;
                _result = InvokeExpression(_expression);
            }

            /// <summary>
            /// Returns a <see cref="string"/> constructed by joining the passed values
            /// </summary>
            /// <param name="arguments">A <see cref="List{T}"/> of instances whose string representations are to be joined </param>
            /// <returns>A <see cref="string"/> representation of the provided values</returns>
            private static string GetArgumentsString(IEnumerable<object> arguments)
            {
                var argumentsList = arguments == null
                    ? null
                    : arguments as IList<object> ?? arguments.ToList();

                return argumentsList == null || !argumentsList.Any()
                    ? ""
                    : string.Join(",", argumentsList);
            }

            /// <summary>
            /// Constructs and returns a <see cref="string"/> representation of the current <see cref="ExpressionDetail"/> instance
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (!HasName)
                {
                    return string.Empty;
                }

                return IsIndexer
                    ? $"[{GetArgumentsString(Arguments)}]"
                    : NodeType == ExpressionType.Call
                        ? $"{TargetName}({GetArgumentsString(Arguments)})"
                        : TargetName;
            }
        }
    }
}
