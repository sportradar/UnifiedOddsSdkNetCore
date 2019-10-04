/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Common.Logging;
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
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static ILog _log;

        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            _log = LogManager.GetLogger(typeof(OddsFeedExample));

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
            Console.WriteLine(" Select which example you want to run:");
            Console.WriteLine(" 1 - Basic \t\t\t(single session, generic dispatcher, basic output)");
            Console.WriteLine(" 2 - Multi-Session \t\t(has Low and High priority sessions)");
            Console.WriteLine(" 3 - Use specific dispatchers \t(uses specific message dispatchers)");
            Console.WriteLine(" 4 - Display Market info \t(generates and displays market/outcomes names)");
            Console.WriteLine(" 5 - Display SportEvent info \t(displays info about sport events)");
            Console.WriteLine(" 6 - All together \t\t(includes all options above)");
            Console.WriteLine(" 7 - Extra: Market Mappings \t(displays market mappings related to sport events)");
            Console.WriteLine(" 8 - Extra: Replay Server \t(how to interact with xReplay Server)");
            Console.Write(" Enter number: ");
            var k = Console.ReadKey();

            var defaultLocale = Feed.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectIntegration().LoadFromConfigFile().Build().DefaultLocale;

            Console.WriteLine(string.Empty);
            Console.WriteLine(string.Empty);

            switch (k.KeyChar)
            {
                case '1':
                {
                    new Basic(_log).Run(MessageInterest.AllMessages);
                    break;
                }
                case '2':
                {
                    new MultiSession(_log).Run();
                    break;
                }
                case '3':
                {
                    new SpecificDispatchers(_log).Run(MessageInterest.AllMessages);
                    break;
                }
                case '4':
                {
                    new ShowMarketNames(_log).Run(MessageInterest.AllMessages, defaultLocale);
                    break;
                }
                case '5':
                {
                    new ShowEventInfo(_log).Run(MessageInterest.AllMessages, defaultLocale);
                    break;
                }
                case '6':
                {
                    new CompleteInfo(_log).Run(MessageInterest.AllMessages, defaultLocale);
                    break;
                }
                case '7':
                {
                    new ShowMarketMappings(_log).Run(MessageInterest.AllMessages, defaultLocale);
                    break;
                }
                case '8':
                {
                    new ReplayServer(_log).Run(MessageInterest.AllMessages);
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