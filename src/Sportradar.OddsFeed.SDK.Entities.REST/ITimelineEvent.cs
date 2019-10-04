/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing timeline event
    /// </summary>
    public interface ITimelineEvent
    {
        /// <summary>
        /// Gets the timeline event identifier
        /// </summary>
        /// <value>The id of timeline event</value>
        int Id { get; }
        /// <summary>
        /// Gets the home score
        /// </summary>
        /// <value>The home score</value>
        decimal? HomeScore { get; }
        /// <summary>
        /// Gets the away score
        /// </summary>
        /// <value>The away score</value>
        decimal? AwayScore { get; }
        /// <summary>
        /// Gets the match time
        /// </summary>
        /// <value>The match time</value>
        int? MatchTime { get; }
        /// <summary>
        /// Gets the period
        /// </summary>
        /// <value>The period</value>
        string Period { get; }
        /// <summary>
        /// Gets the name of the period
        /// </summary>
        /// <value>The name of the period</value>
        string PeriodName { get; }
        /// <summary>
        /// Gets the points
        /// </summary>
        /// <value>The points</value>
        string Points { get; }
        /// <summary>
        /// Gets the stoppage time
        /// </summary>
        /// <value>The stoppage time</value>
        string StoppageTime { get; }
        /// <summary>
        /// Gets the team
        /// </summary>
        /// <value>The team</value>
        HomeAway? Team { get; }
        /// <summary>
        /// Gets the type
        /// </summary>
        /// <value>The type</value>
        string Type { get; }
        /// <summary>
        /// Gets the value
        /// </summary>
        /// <value>The value</value>
        string Value { get; }
        /// <summary>
        /// Gets the x
        /// </summary>
        /// <value>The x</value>
        int X { get; }
        /// <summary>
        /// Gets the y
        /// </summary>
        /// <value>The y</value>
        int Y { get; }
        /// <summary>
        /// Gets the time
        /// </summary>
        /// <value>The time</value>
        DateTime Time { get; }
        /// <summary>
        /// Gets the list of assists
        /// </summary>
        /// <value>The assists</value>
        IEnumerable<IAssist> Assists { get; }
        /// <summary>
        /// Gets the goal scorer
        /// </summary>
        /// <value>The goal scorer</value>
        IGoalScorer GoalScorer { get; }
        /// <summary>
        /// Gets the player
        /// </summary>
        /// <value>The player</value>
        IPlayer Player { get; }
    }
}
