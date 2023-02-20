/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.IO;
using log4net;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SdkLogTests
    {
        public SdkLogTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddLog4Net("log4net.sdk.config"));
            var servicesProvider = services.BuildServiceProvider();
            var loggerFactory = servicesProvider.GetService<ILoggerFactory>();
            SdkLoggerFactory.SetLoggerFactory(loggerFactory);
        }

        private static void PrintLogManagerStatus()
        {
            Console.WriteLine($"Number of repositories: {LogManager.GetAllRepositories().Length}");
            foreach (var r in LogManager.GetAllRepositories())
            {
                Console.WriteLine($"\tRepository: {r.Name}");
                Console.WriteLine($"Number of loggers: {LogManager.GetCurrentLoggers(r.Name).Length}");
                foreach (var l in LogManager.GetCurrentLoggers(r.Name))
                {
                    Console.WriteLine($"\tLogger: {l.Logger.Name}");
                    foreach (var a in l.Logger.Repository.GetAppenders())
                    {
                        Console.WriteLine($"\t\t Appender: {a.Name}");
                    }
                }
            }

            Console.WriteLine(Environment.NewLine);

            var logDefault = SdkLoggerFactory.GetLogger(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
            var logCache = SdkLoggerFactory.GetLoggerForCache(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
            var logClientIteration = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
            var logFeedTraffic = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
            var logRestTraffic = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);
            var logStatsTraffic = SdkLoggerFactory.GetLoggerForStats(typeof(SdkLogTests), TestData.SdkTestLogRepositoryName);

            LogPrint(logDefault);
            LogPrint(logCache);
            LogPrint(logClientIteration);
            LogPrint(logFeedTraffic);
            LogPrint(logRestTraffic);
            LogPrint(logStatsTraffic);
        }

        private static void LogPrint(Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation("info message");
            log.LogError(new InvalidDataException("just testing").Message);
            log.LogDebug("debug message");
            log.LogWarning("warn message");
            log.LogError("error message");
            log.LogCritical("fatal message");
        }

        [Fact]
        public void SdkLoggerFactoryTest()
        {
            PrintLogManagerStatus();

            Assert.True(LogManager.GetAllRepositories().Length > 0);

            foreach (var loggerRepository in LogManager.GetAllRepositories())
            {
                Assert.NotNull(loggerRepository);
                Assert.True(loggerRepository.GetCurrentLoggers().Length > 0);
                foreach (var currentLogger in loggerRepository.GetCurrentLoggers())
                {
                    Assert.NotNull(currentLogger);
                    currentLogger.Log(typeof(SdkLogTests), Level.Info, "some message", null);
                }
            }
        }
    }
}
