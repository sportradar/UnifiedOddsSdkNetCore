/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;

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

        /// <summary>
        /// Method for getting log4net.ILog for feed traffic
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForFeedTraffic(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.FeedTraffic);
        }

        /// <summary>
        /// Method for getting log4net.ILog for rest traffic
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForRestTraffic(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.RestTraffic);
        }

        /// <summary>
        /// Method for getting log4net.ILog for client iteration
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForClientInteraction(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.ClientInteraction);
        }

        /// <summary>
        /// Method for getting log4net.ILog for cache
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForCache(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.Cache);
        }

        /// <summary>
        /// Method for getting log4net.ILog for statistics
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForStats(Type type, string repositoryName = SdkLogRepositoryName)
        {
            return GetLogger(type, repositoryName, LoggerType.Stats);
        }

        /// <summary>
        /// Method for getting log4net.ILog for execution
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <returns>Returns default <see cref="ILog"/> with specified settings</returns>
        public static ILog GetLoggerForExecution(Type type, string repositoryName = SdkLogRepositoryName)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            return GetLogger(type, repositoryName, LoggerType.Execution);
        }

        /// <summary>
        /// Method for getting correct <see cref="ILog"/> for specific logger type
        /// </summary>
        /// <param name="type">A type to be used for creating new ILog</param>
        /// <param name="repositoryName">Repository containing the logger</param>
        /// <param name="loggerType">A value of <see cref="LoggerType"/> to be used to get <see cref="ILog"/></param>
        /// <returns>Returns <see cref="ILog"/> with specific settings for this <see cref="LoggerType"/></returns>
        public static ILog GetLogger(Type type, string repositoryName = SdkLogRepositoryName, LoggerType loggerType = LoggerType.Execution)
        {
            if (loggerType == LoggerType.Execution)
            {
                return LogManager.GetLogger(type);
            }
            var key = SdkLogRepositoryName + "." + Enum.GetName(typeof(LoggerType), loggerType);
            return LogManager.GetLogger(key);
        }

        /// <summary>
        /// Configures the LogManager for SDK
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <param name="repositoryName">Repository name to be created containing the SDK loggers</param>
        [Obsolete("This method is no longer required since the SDK uses Common.Logging facade")]
        public static void Configure(FileInfo configFile, string repositoryName = SdkLogRepositoryName)
        {
        }

        /// <summary>
        /// Method checks if all loggers are created for each <see cref="LoggerType"/>
        /// </summary>
        /// <param name="repositoryName">Repository containing the loggers</param>
        /// <returns>Returns value indicating if all sdk defined loggers exists</returns>
        [Obsolete("This method is no longer required since the SDK uses Common.Logging facade")]
        public static bool CheckAllLoggersExists(string repositoryName = SdkLogRepositoryName)
        {
            return true;
        }
    }
}
