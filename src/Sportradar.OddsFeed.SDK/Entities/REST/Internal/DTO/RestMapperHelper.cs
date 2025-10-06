// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A helper for mapping of rest data to sdk data-transfer-objects
    /// </summary>
    internal static class RestMapperHelper
    {
        /// <summary>
        /// Maps the <see cref="sportEvent"/> instance to the one of the derived types of <see cref="SportEventSummaryDto"/>
        /// </summary>
        /// <param name="item">The item to be mapped</param>
        /// <returns>A <see cref="SportEventSummaryDto"/> derived instance</returns>
        /// <exception cref="ArgumentException">id</exception>
        public static SportEventSummaryDto MapSportEvent(sportEvent item)
        {
            if (item == null)
            {
                return null;
            }
            var id = Urn.Parse(item.id);

            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.Match:
                    {
                        return new MatchDto(item);
                    }
                case ResourceTypeGroup.Stage:
                    {
                        return new StageDto(item);
                    }
                case ResourceTypeGroup.BasicTournament:
                    {
                        return new BasicTournamentDto(item);
                    }
                case ResourceTypeGroup.Tournament:
                case ResourceTypeGroup.Season:
                    {
                        return new TournamentInfoDto(item);
                    }
                case ResourceTypeGroup.Unknown:
                    {
                        return new SportEventSummaryDto(item);
                    }

                case ResourceTypeGroup.Other:
                case ResourceTypeGroup.Draw:
                case ResourceTypeGroup.Lottery:
                default:
                    throw new ArgumentException($"ResourceTypeGroup: {id.TypeGroup} is not supported", nameof(item));
            }
        }

        /// <summary>
        /// Maps the <see cref="tournamentExtended"/> instance to the one of the derived types of <see cref="SportEventSummaryDto"/>
        /// </summary>
        /// <param name="item">The item to be mapped</param>
        /// <returns>A <see cref="SportEventSummaryDto"/> derived instance</returns>
        /// <exception cref="ArgumentException">id</exception>
        public static SportEventSummaryDto MapSportEvent(tournamentExtended item)
        {
            if (item == null)
            {
                return null;
            }
            var id = Urn.Parse(item.id);

            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.BasicTournament:
                    {
                        return new BasicTournamentDto(item);
                    }
                case ResourceTypeGroup.Tournament:
                case ResourceTypeGroup.Season:
                    {
                        return new TournamentInfoDto(item);
                    }

                case ResourceTypeGroup.Match:
                case ResourceTypeGroup.Stage:
                case ResourceTypeGroup.Other:
                case ResourceTypeGroup.Unknown:
                case ResourceTypeGroup.Draw:
                case ResourceTypeGroup.Lottery:
                default:
                    throw new ArgumentException($"ResourceTypeGroup: {id.TypeGroup} is not supported", nameof(item));
            }
        }

        /// <summary>
        /// It checks if there are exactly 2 competitors and one is home and seconds away. If so, it returns dictionary indicating which of team is home and which away; else null
        /// </summary>
        /// <param name="competitors">The competitors to be checked</param>
        /// <returns>The dictionary of home and away competitor ids</returns>
        public static IDictionary<HomeAway, Urn> FillHomeAwayCompetitors(IReadOnlyCollection<teamCompetitor> competitors)
        {
            var homeAwayCompetitors = new Dictionary<HomeAway, Urn>();
            if (competitors == null || competitors.Count != 2)
            {
                return homeAwayCompetitors;
            }

            var team = competitors.FirstOrDefault(f => f.qualifier == "home");
            if (team == null)
            {
                return homeAwayCompetitors;
            }
            homeAwayCompetitors.Add(HomeAway.Home, Urn.Parse(team.id));
            team = competitors.FirstOrDefault(f => f.qualifier == "away");
            if (team == null)
            {
                homeAwayCompetitors.Clear();
                return homeAwayCompetitors;
            }
            homeAwayCompetitors.Add(HomeAway.Away, Urn.Parse(team.id));
            return homeAwayCompetitors;
        }

        /// <summary>
        /// Maps the type of the bonus drum
        /// </summary>
        /// <param name="bonusDrumType">Type of the bonus drum</param>
        /// <param name="restTypeSpecified">if set to <c>true</c> [rest type specified]</param>
        /// <returns>System.Nullable&lt;BonusDrumType&gt;</returns>
        public static BonusDrumType? MapBonusDrumType(bonusDrumType bonusDrumType, bool restTypeSpecified)
        {
            if (!restTypeSpecified)
            {
                return null;
            }
            if (bonusDrumType == bonusDrumType.same)
            {
                return BonusDrumType.Same;
            }
            return BonusDrumType.Additional;
        }

        /// <summary>
        /// Maps the type of the draw
        /// </summary>
        /// <param name="drawType">Type of the draw</param>
        /// <param name="restTypeSpecified">if set to <c>true</c> [rest type specified]</param>
        /// <returns>DrawType</returns>
        public static DrawType MapDrawType(drawType drawType, bool restTypeSpecified)
        {
            if (!restTypeSpecified)
            {
                return DrawType.Unknown;
            }
            if (drawType == drawType.drum)
            {
                return DrawType.Drum;
            }
            return DrawType.Rng;
        }

        /// <summary>
        /// Maps the draw status
        /// </summary>
        /// <param name="drawStatus">The draw status</param>
        /// <param name="restTypeSpecified">if set to <c>true</c> [rest type specified]</param>
        /// <returns>DrawStatus</returns>
        public static DrawStatus MapDrawStatus(drawStatus drawStatus, bool restTypeSpecified)
        {
            if (!restTypeSpecified)
            {
                return DrawStatus.Unknown;
            }
            if (drawStatus == drawStatus.open)
            {
                return DrawStatus.Open;
            }
            if (drawStatus == drawStatus.canceled)
            {
                return DrawStatus.Cancelled;
            }
            if (drawStatus == drawStatus.closed)
            {
                return DrawStatus.Closed;
            }
            if (drawStatus == drawStatus.finished)
            {
                return DrawStatus.Finished;
            }
            return DrawStatus.Unknown;
        }

        /// <summary>
        /// Maps the type of the time
        /// </summary>
        /// <param name="timeType">Type of the time</param>
        /// <param name="restTypeSpecified">if set to <c>true</c> [rest type specified]</param>
        /// <returns>TimeType</returns>
        public static TimeType MapTimeType(timeType timeType, bool restTypeSpecified)
        {
            if (!restTypeSpecified)
            {
                return TimeType.Unknown;
            }
            if (timeType == timeType.interval)
            {
                return TimeType.Interval;
            }
            return TimeType.Fixed;
        }

        /// <summary>
        /// Maps the response code
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="itemSpecified">if set to <c>true</c> [item specified]</param>
        /// <returns>System.Nullable&lt;HttpStatusCode&gt;</returns>
        public static HttpStatusCode? MapResponseCode(response_code item, bool itemSpecified)
        {
            if (!itemSpecified)
            {
                return null;
            }
            HttpStatusCode code;
            switch (item)
            {
                case response_code.OK:
                    code = HttpStatusCode.OK;
                    break;
                case response_code.CREATED:
                    code = HttpStatusCode.Created;
                    break;
                case response_code.ACCEPTED:
                    code = HttpStatusCode.Accepted;
                    break;
                case response_code.FORBIDDEN:
                    code = HttpStatusCode.Forbidden;
                    break;
                case response_code.NOT_FOUND:
                    code = HttpStatusCode.NotFound;
                    break;
                case response_code.CONFLICT:
                    code = HttpStatusCode.Conflict;
                    break;
                case response_code.SERVICE_UNAVAILABLE:
                    code = HttpStatusCode.ServiceUnavailable;
                    break;
                case response_code.NOT_IMPLEMENTED:
                    code = HttpStatusCode.NotImplemented;
                    break;
                case response_code.MOVED_PERMANENTLY:
                    code = HttpStatusCode.MovedPermanently;
                    break;
                case response_code.BAD_REQUEST:
                    code = HttpStatusCode.BadRequest;
                    break;
                default:
                    code = HttpStatusCode.SeeOther;
                    break;
            }
            return code;
        }

        /// <summary>
        /// Tries to map the provided <see cref="string"/> to <see cref="BookingStatus"/> enum member
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of the <see cref="BookingStatus"/>, or a null reference. Note that mapping of a null
        /// reference is allowed and result of mapping is a null reference</param>
        /// <param name="result">When the call is completed it contains <see cref="BookingStatus"/> which was obtained by mapping or a null reference if a null reference
        /// was provided or mapping failed</param>
        /// <returns>True if the provided <see cref="string"/> was successfully mapped to <see cref="BookingStatus"/>; False otherwise</returns>
        public static bool TryGetBookingStatus(string value, out BookingStatus? result)
        {
            switch (value)
            {
                case "not_available":
                    result = BookingStatus.Unavailable;
                    return true;
                case "booked":
                    result = BookingStatus.Booked;
                    return true;
                case "not_booked":
                case "bookable":
                    result = BookingStatus.Bookable;
                    return true;
                case "buyable":
                    result = BookingStatus.Buyable;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Tries to map the provided <see cref="string"/> to <see cref="SportEventType"/> enum member
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of the <see cref="SportEventType"/></param>
        /// <param name="result">When invocation completes contains a mapped value if method returned true. Undefined otherwise</param>
        /// <returns>A <see cref="SportEventType"/> member obtained by mapping. A null reference is mapped to null reference</returns>
        public static bool TryGetSportEventType(string value, out SportEventType? result)
        {
            switch (value)
            {
                case "parent":
                    result = SportEventType.Parent;
                    return true;
                case "child":
                    result = SportEventType.Child;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Tries to map the provided <see cref="string"/> to <see cref="SportEventType"/> enum member
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of the <see cref="SportEventType"/></param>
        /// <param name="result">When invocation completes contains a mapped value if method returned true. Undefined otherwise</param>
        /// <returns>A <see cref="SportEventType"/> member obtained by mapping. A null reference is mapped to null reference</returns>
        // ReSharper disable once MethodTooLong
        public static bool TryGetStageType(string value, out StageType? result)
        {
            switch (value)
            {
                case "parent":
                    result = StageType.Parent;
                    return true;
                case "child":
                    result = StageType.Child;
                    return true;
                case "event":
                    result = StageType.Event;
                    return true;
                case "season":
                    result = StageType.Season;
                    return true;
                case "round":
                    result = StageType.Round;
                    return true;
                case "competition_group":
                    result = StageType.CompetitionGroup;
                    return true;
                case "discipline":
                case "dicipline":
                    result = StageType.Discipline;
                    return true;
                case "race":
                    result = StageType.Race;
                    return true;
                case "stage":
                    result = StageType.Stage;
                    return true;
                case "practice":
                    result = StageType.Practice;
                    return true;
                case "qualifying":
                    result = StageType.Qualifying;
                    return true;
                case "qualifying_part":
                    result = StageType.QualifyingPart;
                    return true;
                case "lap":
                    result = StageType.Lap;
                    return true;
                case "prologue":
                    result = StageType.Prologue;
                    return true;
                case "run":
                    result = StageType.Run;
                    return true;
                case "sprint_race":
                    result = StageType.SprintRace;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Tries to map the provided <see cref="string"/> to <see cref="CoveredFrom"/> enum member
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of the <see cref="CoveredFrom"/>, or a null reference. Note that mapping of a null
        /// reference is allowed and result of mapping is a null reference</param>
        /// <param name="result">When the call is completed it contains <see cref="CoveredFrom"/> which was obtained by mapping or a null reference if a null reference
        /// was provided or mapping failed</param>
        /// <returns>True if the provided <see cref="string"/> was successfully mapped to <see cref="CoveredFrom"/>; False otherwise</returns>
        public static bool TryGetCoveredFrom(string value, out CoveredFrom? result)
        {
            switch (value)
            {
                case "tv":
                    result = CoveredFrom.Tv;
                    return true;
                case "venue":
                    result = CoveredFrom.Venue;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
