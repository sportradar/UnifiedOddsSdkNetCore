/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums
{
    /// <summary>
    /// The enumeration of types of data-transfer-object obtained via REST requests
    /// </summary>
    internal enum DtoType
    {
        /// <summary>
        /// The booking status
        /// </summary>
        BookingStatus,
        /// <summary>
        /// The category
        /// </summary>
        Category,
        /// <summary>
        /// The competitor
        /// </summary>
        Competitor,
        /// <summary>
        /// The competitor profile
        /// </summary>
        CompetitorProfile,
        /// <summary>
        /// The fixture
        /// </summary>
        Fixture,
        /// <summary>
        /// The lottery
        /// </summary>
        Lottery,
        /// <summary>
        /// The lottery draw
        /// </summary>
        LotteryDraw,
        /// <summary>
        /// The lottery list
        /// </summary>
        LotteryList,
        /// <summary>
        /// The market description (used for variant market description provider)
        /// </summary>
        MarketDescription,
        /// <summary>
        /// The market descriptions list
        /// </summary>
        MarketDescriptionList,
        /// <summary>
        /// The match summary
        /// </summary>
        MatchSummary,
        /// <summary>
        /// The match timeline
        /// </summary>
        MatchTimeline,
        /// <summary>
        /// The player profile
        /// </summary>
        PlayerProfile,
        /// <summary>
        /// The race summary
        /// </summary>
        RaceSummary, // also covers Stages, but REST object is called race_summary
        /// <summary>
        /// The sport
        /// </summary>
        Sport,
        /// <summary>
        /// The sport list
        /// </summary>
        SportList,
        /// <summary>
        /// The sport event status
        /// </summary>
        SportEventStatus,
        /// <summary>
        /// The sport event summary
        /// </summary>
        SportEventSummary,
        /// <summary>
        /// The sport event summary list
        /// </summary>
        SportEventSummaryList,
        /// <summary>
        /// The tournament
        /// </summary>
        Tournament,
        /// <summary>
        /// The tournament information
        /// </summary>
        TournamentInfo,
        /// <summary>
        /// The tournament seasons
        /// </summary>
        TournamentSeasons,
        /// <summary>
        /// The variant description
        /// </summary>
        VariantDescription,
        /// <summary>
        /// The variant description list
        /// </summary>
        VariantDescriptionList,
        /// <summary>
        /// The sport categories
        /// </summary>
        SportCategories,
        /// <summary>
        /// The simple team profile
        /// </summary>
        SimpleTeamProfile,
        /// <summary>
        /// The available selections for the event
        /// </summary>
        AvailableSelections,
        /// <summary>
        /// The tournament list
        /// </summary>
        TournamentInfoList
    }
}