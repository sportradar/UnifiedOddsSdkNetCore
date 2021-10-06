/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.DemoProject.Example;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.DemoProject
{
    /// <summary>
    /// A simple example program used to demonstrate the usage of Odds Feed SDK
    /// </summary>
    internal class OddsFeedExample
    {
        private static ILoggerFactory _loggerFactory;

        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddLog4Net("log4net.config"));
            var serviceProvider = services.BuildServiceProvider();
            _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var log = _loggerFactory.CreateLogger(typeof(OddsFeedExample));
            log.LogInformation("OddsFeed example");

            var key = 'y';
            while (key.Equals('y'))
            {
                DoExampleSelection();

                Console.WriteLine(string.Empty);
                Console.Write(" Want to run another example? (y|n): ");
                key = Console.ReadKey().KeyChar;
                Console.WriteLine(string.Empty);
            }
        }

        private static void DoExampleSelection()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("Select which example you want to run:");
            Console.WriteLine("  1 - Basic \t\t\t(single session, generic dispatcher, basic output)");
            Console.WriteLine("  2 - Multi-Session \t\t(has Low and High priority sessions)");
            Console.WriteLine("  3 - Use specific dispatchers \t(uses specific message dispatchers)");
            Console.WriteLine("  4 - Display Market info \t(generates and displays market/outcomes names)");
            Console.WriteLine("  5 - Display SportEvent info \t(displays info about sport events)");
            Console.WriteLine("  6 - All together \t\t(includes all options above)");
            Console.WriteLine("  7 - Extra: Market Mappings \t(displays market mappings related to sport events)");
            Console.WriteLine("  8 - Extra: Replay Server \t(how to interact with xReplay Server)");
            Console.WriteLine("  9 - Extra: Export/import \t(how to export/import current cache state)");
            Console.WriteLine(" 10 - Extra: Advanced setup \t(single session with multi-threaded message parsing)");
            Console.Write("Enter number: ");
            var k = Console.ReadLine();

            var defaultLocale = Feed.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectIntegration().LoadFromConfigFile().Build().DefaultLocale;

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Empty);

            switch (k)
            {
                case "1":
                    {
                        new Basic(_loggerFactory).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "2":
                    {
                        new MultiSession(_loggerFactory).Run();
                        break;
                    }
                case "3":
                    {
                        new SpecificDispatchers(_loggerFactory).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "4":
                    {
                        new ShowMarketNames(_loggerFactory).Run(MessageInterest.AllMessages, defaultLocale);
                        break;
                    }
                case "5":
                    {
                        new ShowEventInfo(_loggerFactory).Run(MessageInterest.AllMessages, defaultLocale);
                        break;
                    }
                case "6":
                    {
                        new CompleteInfo(_loggerFactory).Run(MessageInterest.AllMessages, defaultLocale);
                        break;
                    }
                case "7":
                    {
                        new ShowMarketMappings(_loggerFactory).Run(MessageInterest.AllMessages, defaultLocale);
                        break;
                    }
                case "8":
                    {
                        new ReplayServer(_loggerFactory).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "9":
                    {
                        new CacheExportImport(_loggerFactory).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "10":
                    {
                        new MultiThreaded(_loggerFactory).Run(MessageInterest.AllMessages);
                        break;
                    }
                default:
                    {
                        DoExampleSelection();
                        break;
                    }
            }
        }
    }
}