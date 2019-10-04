/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    public static class ExampleReplayEvents
    {
        public static IReadOnlyList<SampleEvent> SampleEvents => new List<SampleEvent>
        {
            new SampleEvent("Soccer Match - English Premier League 2017 (Watford vs Westham)", "sr:match:11830662"),
            new SampleEvent("Soccer Match w Overtime - Primavera Cup", "sr:match:12865222"),
            new SampleEvent(
                "Soccer Match w Overtime & Penalty Shootout - KNVB beker 17/18 - FC Twente Enschede vs Ajax Amsterdam",
                "sr:match:12873164"),
            new SampleEvent("Soccer Match with Rollback Betsettlement from Prematch Producer", "sr:match:11958226"),
            new SampleEvent(
                "Soccer Match aborted mid-game - new match played later (first match considered cancelled according to betting rules)",
                "sr:match:11971876"),
            new SampleEvent("Soccer Match w PlayerProps (prematch odds only)", "sr:match:12055466"),
            new SampleEvent("Tennis Match - ATP Paris Final 2017", "sr:match:12927908"),
            new SampleEvent("Tennis Match where one of the players retired", "sr:match:12675240"),
            new SampleEvent("Tennis Match with bet_cancel adjustments using rollback_bet_cancel", "sr:match:13616027"),
            new SampleEvent(
                "Tennis Match w voided markets due to temporary loss of coverage - no ability to verify results",
                "sr:match:13600533"),
            new SampleEvent("Basketball Match - NBA Final 2017 - (Golden State Warriors vs Cleveland Cavaliers)",
                "sr:match:11733773"),
            new SampleEvent("Basketball Match w voided DrawNoBet (2nd half draw)", "sr:match:12953638"),
            new SampleEvent("Basketball Match w PlayerProps", "sr:match:12233896"),
            new SampleEvent("Icehockey Match - NHL Final 2017 (6th match - (Nashville Predators vs Pittsburg Penguins)",
                "sr:match:11784628"),
            new SampleEvent("Icehockey Match with Rollback BetCancel", "sr:match:11878140"),
            new SampleEvent("Icehockey Match with overtime + rollback_bet_cancel + match_status=\"aet\"",
                "sr:match:11878386"),
            new SampleEvent("American Football Game - NFL 2018/2018 (Chicago Bears vs Atlanta Falcons)",
                "sr:match:11538563"),
            new SampleEvent("American Football Game w PlayerProps", "sr:match:13552497"),
            new SampleEvent("Handball Match - DHB Pokal 17/18 (SG Flensburg-Handewitt vs Fuchse Berlin)",
                "sr:match:12362564"),
            new SampleEvent("Baseball Game - MLB 2017 (Final Los Angeles Dodgers vs Houston Astros)",
                "sr:match:12906380"),
            new SampleEvent("Badminton Game - Indonesia Masters 2018", "sr:match:13600687"),
            new SampleEvent("Snooker - International Championship 2017 (Final Best-of-19 frames)", "sr:match:12927314"),
            new SampleEvent("Darts - PDC World Championship 17/18 - (Final)", "sr:match:13451765"),
            new SampleEvent("CS:GO (ESL Pro League 2018)", "sr:match:13497893"),
            new SampleEvent("Dota2 (The International 2017 - Final)", "sr:match:12209528"),
            new SampleEvent("League of Legends Match (LCK Spring 2018)", "sr:match:13516251"),
            new SampleEvent("Cricket Match [Premium Cricket] - The Ashes 2017 (Australia vs England)",
                "sr:match:11836360"),
            new SampleEvent(
                "Cricket Match (rain affected) [Premium Cricket] - ODI Series New Zealand vs. Pakistan 2018",
                "sr:match:13073610"),
            new SampleEvent("Volleyball Match (includes bet_cancels)", "sr:match:12716714"),
            new SampleEvent("Volleyball match where Betradar loses coverage mid-match - no ability to verify results",
                "sr:match:13582831"),
            new SampleEvent("Aussie Rules Match (AFL 2017 Final)", "sr:match:12587650"),
            new SampleEvent("Table Tennis Match (World Cup 2017 Final", "sr:match:12820410"),
            new SampleEvent("Squash Match (Qatar Classic 2017)", "sr:match:12841530"),
            new SampleEvent("Beach Volleyball", "sr:match:13682571"),
            new SampleEvent("Badminton", "sr:match:13600687"),
            new SampleEvent("Bowls", "sr:match:13530237"),
            new SampleEvent("Rugby League", "sr:match:12979908"),
            new SampleEvent("Rugby Union", "sr:match:12420636"),
            new SampleEvent("Rugby Union 7s", "sr:match:13673067"),
            new SampleEvent("Handball", "sr:match:12362564"),
            new SampleEvent("Futsal", "sr:match:12363102"),
            new SampleEvent("Golf Winner Events + Three Balls - South African Open (Winner events + Three balls)",
                "sr:simple_tournament:66820"),
            new SampleEvent("Season Outrights (Long-term Outrights) - NFL 2017/18", "sr:season:40175"),
            new SampleEvent("Race Outrights (Short-term Outrights) - Cycling Tour Down Under 2018", "sr:stage:329361")
        }.AsReadOnly();

        public class SampleEvent
        {
            public SampleEvent(string Description, string EventId)
            {
                this.Description = Description;
                this.EventId = URN.Parse(EventId);
            }

            public string Description { get; }
            public URN EventId { get; }

            public override string ToString()
            {
                return $"EventId: '{EventId}', Event: '{Description}'";
            }
        }
    }
}
