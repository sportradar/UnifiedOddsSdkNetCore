/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Reflection;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Log
{
    /// <summary>
    /// Factory for the <see cref="LogProxy{T}" />
    /// </summary>
    public static class LogProxyFactory
    {
        /// <summary>
        /// Creates the specified arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments</param>
        /// <param name="loggerType">Type of the logger</param>
        /// <param name="canOverrideLoggerType">if set to <c>true</c> [can override logger type]</param>
        /// <returns>T</returns>
        public static T Create<T>(object[] args, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true)
        {
            var tmp = (T)Activator.CreateInstance(typeof(T), args);
            //var logProxy = new LogProxy<T>(tmp, loggerType, canOverrideLoggerType);
            //return LogProxy<T>.Create(tmp, null, loggerType, canOverrideLoggerType);
            var logProxy = new LogProxy<T>(tmp, loggerType, canOverrideLoggerType);
            //return null;
            //return (T)logProxy;
            return tmp;
        }

        /// <summary>
        /// Creates the specified arguments
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments</param>
        /// <param name="filter">The filter</param>
        /// <param name="loggerType">Type of the logger</param>
        /// <param name="canOverrideLoggerType">if set to <c>true</c> [can override logger type]</param>
        /// <returns>T</returns>
        public static T Create<T>(object[] args, Predicate<MethodInfo> filter, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true)
        {
            var tmp = (T)Activator.CreateInstance(typeof(T), args);
            //return LogProxy<T>.Create(tmp, filter, loggerType, canOverrideLoggerType);
            return tmp;
        }

        /// <summary>
        /// Creates the specified filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">The filter</param>
        /// <param name="loggerType">Type of the logger</param>
        /// <param name="canOverrideLoggerType">if set to <c>true</c> [can override logger type]</param>
        /// <param name="args">The arguments</param>
        /// <returns>T</returns>
        public static T Create<T>(Predicate<MethodInfo> filter, LoggerType loggerType, bool canOverrideLoggerType, params object[] args)
        {
            var tmp = (T)Activator.CreateInstance(typeof(T), args);
            //return LogProxy<T>.Create(tmp, filter, loggerType, canOverrideLoggerType);
            return tmp;
        }
    }
}