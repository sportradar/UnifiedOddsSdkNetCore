/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Log
{
    /// <summary>
    /// Factory for the <see cref="LogInterceptor" />
    /// </summary>
    internal static class LogInterceptorFactory
    {
        /// <summary>
        /// Creates the specified arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments</param>
        /// <param name="filter">The filter</param>
        /// <param name="loggerType">Type of the logger</param>
        /// <param name="canOverrideLoggerType">if set to <c>true</c> [can override logger type]</param>
        /// <returns>T</returns>
        public static T Create<T>(object[] args, Predicate<MethodInfo> filter = null, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true) where T : class
        {
            var interceptor = new LogInterceptor(loggerType, canOverrideLoggerType, filter);
            var proxy = (T)new ProxyGenerator().CreateClassProxy(typeof(T), ProxyGenerationOptions.Default, args, interceptor);
            return proxy;
        }

        /// <summary>
        /// Creates the specified arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">The filter</param>
        /// <param name="loggerType">Type of the logger</param>
        /// <param name="canOverrideLoggerType">if set to <c>true</c> [can override logger type]</param>
        /// <returns>T</returns>
        public static T Create<T>(Predicate<MethodInfo> filter = null, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true) where T : class
        {
            var interceptor = new LogInterceptor(loggerType, canOverrideLoggerType, filter);
            var proxy = new ProxyGenerator().CreateClassProxy<T>(interceptor);
            return proxy;
        }
    }
}
