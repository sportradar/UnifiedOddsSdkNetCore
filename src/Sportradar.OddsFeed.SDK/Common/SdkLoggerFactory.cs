/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Provides methods to get logger for specific <see cref="LoggerType"/>
    /// </summary>
    public static class SdkLoggerFactory
    {
        /// <summary>
        /// Default repository name for the SDK
        /// </summary>
        internal const string SdkLogRepositoryName = "Sportradar.OddsFeed.SDK";

        private static ILoggerFactory Factory;

        /// <summary>
        /// The <see cref="ILoggerFactory"/> used with SDK to create <see cref="ILogger"/>
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                return Factory ??= new NullLoggerFactory();
            }
        }

        /// <summary>
        /// Set the <see cref="ILoggerFactory"/>
        /// </summary>
        /// <param name="factory">An <see cref="ILoggerFactory"/> to be used</param>
        public static void SetLoggerFactory(ILoggerFactory factory)
        {
            Factory = factory;
        }

        /// <summary>
        /// Method for getting log4net.ILog for feed traffic
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForFeedTraffic(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.FeedTraffic);
        }

        /// <summary>
        /// Method for getting log4net.ILog for rest traffic
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForRestTraffic(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.RestTraffic);
        }

        /// <summary>
        /// Method for getting log4net.ILog for client iteration
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForClientInteraction(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.ClientInteraction);
        }

        /// <summary>
        /// Method for getting log4net.ILog for cache
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForCache(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.Cache);
        }

        /// <summary>
        /// Method for getting log4net.ILog for statistics
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForStats(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.Stats);
        }

        /// <summary>
        /// Method for getting log4net.ILog for execution
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILogger"/> with specified settings</returns>
        public static ILogger GetLoggerForExecution(Type type, string repositoryName = SdkLogRepositoryName)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            return GetLogger(type, repositoryName, LoggerType.Execution);
        }

        /// <summary>
        /// Method for getting correct <see cref="ILogger"/> for specific logger type
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <param name="loggerType">A value of <see cref="LoggerType"/> to be used to get <see cref="ILogger"/></param>
        /// <returns>Returns <see cref="ILogger"/> with specific settings for this <see cref="LoggerType"/></returns>
        public static ILogger GetLogger(Type type, string repositoryName = SdkLogRepositoryName, LoggerType loggerType = LoggerType.Execution)
        {
            if (loggerType == LoggerType.Execution)
            {
                return LoggerFactory.CreateLogger(type);
            }
            var key = repositoryName + "." + Enum.GetName(typeof(LoggerType), loggerType);
            return LoggerFactory.CreateLogger(key);
        }

        /// <summary>
        /// Get the logger level
        /// </summary>
        /// <param name="logger">The logger to check</param>
        /// <returns>The log level supported by the logger</returns>
        public static LogLevel GetLoggerLogLevel(ILogger logger)
        {
            if (logger == null)
            {
                return LogLevel.None;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                return LogLevel.Trace;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                return LogLevel.Debug;
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                return LogLevel.Information;
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                return LogLevel.Warning;
            }

            if (logger.IsEnabled(LogLevel.Error))
            {
                return LogLevel.Error;
            }

            if (logger.IsEnabled(LogLevel.Critical))
            {
                return LogLevel.Critical;
            }

            return LogLevel.None;
        }

        /// <summary>
        /// Get write logger level
        /// </summary>
        /// <param name="logger">The logger to check</param>
        /// <param name="minLevel">The minimum log level</param>
        /// <returns>The log level supported by the logger which is not lower then specified minimum level</returns>
        public static LogLevel GetWriteLogLevel(ILogger logger, LogLevel minLevel)
        {
            if (logger == null)
            {
                return minLevel;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                return minLevel;
            }

            if (logger.IsEnabled(LogLevel.Debug) && LogLevel.Debug >= minLevel)
            {
                return LogLevel.Debug;
            }

            if (logger.IsEnabled(LogLevel.Information) && LogLevel.Information >= minLevel)
            {
                return LogLevel.Information;
            }

            if (logger.IsEnabled(LogLevel.Warning) && LogLevel.Warning >= minLevel)
            {
                return LogLevel.Warning;
            }

            if (logger.IsEnabled(LogLevel.Error) && LogLevel.Error >= minLevel)
            {
                return LogLevel.Error;
            }

            if (logger.IsEnabled(LogLevel.Critical) && LogLevel.Critical >= minLevel)
            {
                return LogLevel.Critical;
            }

            return minLevel;
        }
    }
}
