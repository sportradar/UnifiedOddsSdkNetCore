/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using MF = Sportradar.OddsFeed.SDK.Test.Shared.MessageFactorySdk;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class EntityPrinterTest
    {
        private ILog _log;

        [TestInitialize]
        public void Init()
        {
            _log = SdkLoggerFactory.GetLogger(typeof(EntityPrinterTest));
        }

        [TestMethod]
        public void PrintAssist()
        {
            var res = MF.GetAssist();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintBookmakerDetails()
        {
            var res = MF.GetBookmakerDetails();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintCategory()
        {
            var res = MF.GetCategory();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintCompetitor()
        {
            var competitor = MF.GetCompetitor();
            PrintEntity(competitor);
        }

        [TestMethod]
        public void PrintCoverageInfo()
        {
            var res = MF.GetCoverageInfo();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintEventClock()
        {
            var res = MF.GetEventClock();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintFixture()
        {
            var res = MF.GetFixture();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintGroup()
        {
            var res1 = MF.GetGroup();
            var res2 = MF.GetGroupWithCompetitors();
            PrintEntity(res1);
            PrintEntity(res2);
        }

        [TestMethod]
        public void PrintPeriodScore()
        {
            var res = MF.GetPeriodScore();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintPlayer()
        {
            var res = MF.GetPlayer();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintPlayerProfile()
        {
            var res = MF.GetPlayerProfile();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintProductInfo()
        {
            var res = MF.GetProductInfo();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintProductInfoLink()
        {
            var res = MF.GetProductInfoLink();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintReferee()
        {
            var res = MF.GetReferee();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintRoundSummary()
        {
            var res = MF.GetRoundSummary();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintSport()
        {
            var res = MF.GetSport();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintSportEventConditions()
        {
            var res = MF.GetSportEventConditions();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintStreamingChannel()
        {
            var res = MF.GetStreamingChannel();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintTeamCompetitor()
        {
            var res = MF.GetTeamCompetitor();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintTvChannel()
        {
            var res = MF.GetTvChannel();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintVenue()
        {
            var res = MF.GetVenue();
            PrintEntity(res);
        }

        [TestMethod]
        public void PrintWeatherInfo()
        {
            var res = MF.GetWeatherInfo();
            PrintEntity(res);
        }

        private void PrintEntity(IEntityPrinter item)
        {
            var type = item.GetType();
            _log.Info($"Start printing info for {type.Name} ...");
            _log.Info("0 => " + item.ToString());
            _log.Info("I => " + item.ToString("i"));
            _log.Info("C => " + item.ToString("c"));
            _log.Info("F => " + item.ToString("f"));
            //_log.Info("J => " + item.ToString("j"));
            _log.Info($"Printing for {type.Name} finished.");
        }
    }
}
