/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST;
using Xunit;
using Xunit.Abstractions;
using MF = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactorySdk;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class EntityPrinterTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EntityPrinterTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            MF.SetOutputHelper(_outputHelper);
        }

        [Fact]
        public void PrintAssist()
        {
            var res = MF.GetAssist();
            PrintEntity(res);
        }

        [Fact]
        public void PrintBookmakerDetails()
        {
            var res = MF.GetBookmakerDetails();
            PrintEntity(res);
        }

        [Fact]
        public void PrintCategory()
        {
            var res = MF.GetCategory();
            PrintEntity(res);
        }

        [Fact]
        public void PrintCompetitor()
        {
            var competitor = MF.GetCompetitor();
            PrintEntity(competitor);
        }

        [Fact]
        public void PrintCoverageInfo()
        {
            var res = MF.GetCoverageInfo();
            PrintEntity(res);
        }

        [Fact]
        public void PrintEventClock()
        {
            var res = MF.GetEventClock();
            PrintEntity(res);
        }

        [Fact]
        public void PrintFixture()
        {
            var res = MF.GetFixture();
            PrintEntity(res);
        }

        [Fact]
        public void PrintGroup()
        {
            var res1 = MF.GetGroup();
            var res2 = MF.GetGroupWithCompetitors();
            PrintEntity(res1);
            PrintEntity(res2);
        }

        [Fact]
        public void PrintPeriodScore()
        {
            var res = MF.GetPeriodScore();
            PrintEntity(res);
        }

        [Fact]
        public void PrintPlayer()
        {
            var res = MF.GetPlayer();
            PrintEntity(res);
        }

        [Fact]
        public void PrintPlayerProfile()
        {
            var res = MF.GetPlayerProfile();
            PrintEntity(res);
        }

        [Fact]
        public void PrintProductInfo()
        {
            var res = MF.GetProductInfo();
            PrintEntity(res);
        }

        [Fact]
        public void PrintProductInfoLink()
        {
            var res = MF.GetProductInfoLink();
            PrintEntity(res);
        }

        [Fact]
        public void PrintReferee()
        {
            var res = MF.GetReferee();
            PrintEntity(res);
        }

        [Fact]
        public void PrintRoundSummary()
        {
            var res = MF.GetRoundSummary();
            PrintEntity(res);
        }

        [Fact]
        public void PrintSport()
        {
            var res = MF.GetSport(0, 5);
            PrintEntity(res);
        }

        [Fact]
        public void PrintSportEventConditions()
        {
            var res = MF.GetSportEventConditions();
            PrintEntity(res);
        }

        [Fact]
        public void PrintStreamingChannel()
        {
            var res = MF.GetStreamingChannel();
            PrintEntity(res);
        }

        [Fact]
        public void PrintTeamCompetitor()
        {
            var res = MF.GetTeamCompetitor();
            PrintEntity(res);
        }

        [Fact]
        public void PrintTvChannel()
        {
            var res = MF.GetTvChannel();
            PrintEntity(res);
        }

        [Fact]
        public void PrintVenue()
        {
            var res = MF.GetVenue();
            PrintEntity(res);
        }

        [Fact]
        public void PrintWeatherInfo()
        {
            var res = MF.GetWeatherInfo();
            PrintEntity(res);
        }

        private void PrintEntity(IEntityPrinter item)
        {
            Assert.NotNull(item);
            var type = item.GetType();
            _outputHelper.WriteLine($"Start printing info for {type.Name} ...");
            _outputHelper.WriteLine("0 => " + item.ToString());
            _outputHelper.WriteLine("I => " + item.ToString("i"));
            _outputHelper.WriteLine("C => " + item.ToString("c"));
            _outputHelper.WriteLine("F => " + item.ToString("f"));
            //_log.LogInformation("J => " + item.ToString("j"));
            _outputHelper.WriteLine($"Printing for {type.Name} finished.");
        }
    }
}
