/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using log4net;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using SdkCommon = Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SdkLogTest
    {
        [TestInitialize]
        public void Init()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddLog4Net("log4net.sdk.config"));
            var servicesProvider = services.BuildServiceProvider();
            var loggerFactory = servicesProvider.GetService<ILoggerFactory>();
            var _ = new SdkLoggerFactory(loggerFactory);
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

            var logDefault = SdkLoggerFactory.GetLogger(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);
            var logCache = SdkLoggerFactory.GetLoggerForCache(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);
            var logClientIteration = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);
            var logFeedTraffic = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);
            var logRestTraffic = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);
            var logStatsTraffic = SdkLoggerFactory.GetLoggerForStats(typeof(SdkLogTest), SdkCommon.TestData.SdkTestLogRepositoryName);

            LogPrint(logDefault);
            LogPrint(logCache);
            LogPrint(logClientIteration);
            LogPrint(logFeedTraffic);
            LogPrint(logRestTraffic);
            LogPrint(logStatsTraffic);

            //for (int i = 0; i < 10000; i++)
            //{
            //    LogPrint(logRestTraffic);
            //}
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

        [TestMethod]
        public void SdkLoggerFactoryTest()
        {
            PrintLogManagerStatus();

            Assert.IsTrue(LogManager.GetAllRepositories().Length > 0);

            foreach (var loggerRepository in LogManager.GetAllRepositories())
            {
                Assert.IsNotNull(loggerRepository);
                Assert.IsTrue(loggerRepository.GetCurrentLoggers().Length > 0);
                foreach (var currentLogger in loggerRepository.GetCurrentLoggers())
                {
                    Assert.IsNotNull(currentLogger);
                    currentLogger.Log(typeof(SdkLogTest), Level.Info, "some message", null);
                }
            }
        }
    }
}