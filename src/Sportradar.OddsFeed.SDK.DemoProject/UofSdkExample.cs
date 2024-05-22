/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.DemoProject.Example;

namespace Sportradar.OddsFeed.SDK.DemoProject
{
    /// <summary>
    /// A simple example program used to demonstrate the usage of Odds Feed SDK
    /// </summary>
    internal class UofSdkExample
    {
        private static ILoggerFactory LoggerFactory;

        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddLog4Net("log4net.config"));
            var serviceProvider = services.BuildServiceProvider();
            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var log = LoggerFactory.CreateLogger(typeof(UofSdkExample));
            log.LogInformation("UofSdk example");

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
            Console.Write("Enter number: ");
            var k = Console.ReadLine();

            var defaultLocale = UofSdk.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectCustom().LoadFromConfigFile().Build().DefaultLanguage;

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Empty);

            switch (k)
            {
                case "1":
                    {
                        new Basic(LoggerFactory.CreateLogger<Basic>()).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "2":
                    {
                        new MultiSession(LoggerFactory.CreateLogger<MultiSession>()).Run();
                        break;
                    }
                case "3":
                    {
                        new SpecificDispatchers(LoggerFactory.CreateLogger<SpecificDispatchers>()).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "4":
                    {
                        new ShowMarketNames(LoggerFactory.CreateLogger<ShowMarketNames>(), defaultLocale).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "5":
                    {
                        new ShowEventInfo(LoggerFactory.CreateLogger<ShowEventInfo>(), defaultLocale).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "6":
                    {
                        new CompleteInfo(LoggerFactory.CreateLogger<CompleteInfo>(), defaultLocale).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "7":
                    {
                        new ShowMarketMappings(LoggerFactory.CreateLogger<ShowMarketMappings>(), defaultLocale).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "8":
                    {
                        new ReplayServer(LoggerFactory.CreateLogger<ReplayServer>()).Run(MessageInterest.AllMessages);
                        break;
                    }
                case "9":
                    {
                        new CacheExportImport(LoggerFactory.CreateLogger<CacheExportImport>()).Run(MessageInterest.AllMessages);
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
