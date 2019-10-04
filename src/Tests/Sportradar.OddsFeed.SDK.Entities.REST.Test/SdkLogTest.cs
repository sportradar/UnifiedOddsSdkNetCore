/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using LoggingCommon = Common.Logging;
using SdkCommon = Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SdkLogTest
    {
        [TestInitialize]
        public void Init()
        {
            SdkLoggerFactory.Configure(new FileInfo("log4net.sdk.config"), SdkCommon.TestData.SdkTestLogRepositoryName);
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

        private static void LogPrint(LoggingCommon.ILog log)
        {
            log.Info("info message");
            log.Error(new InvalidDataException("just testing"));
            log.Debug("debug message");
            log.Warn("warn message");
            log.Error("error message");
            log.Fatal("fatal message");
        }
    }
}