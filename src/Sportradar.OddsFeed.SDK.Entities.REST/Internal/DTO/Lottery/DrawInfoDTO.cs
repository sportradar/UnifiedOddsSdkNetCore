/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for draw info
    /// </summary>
    internal class DrawInfoDTO
    {
        public DrawType DrawType{ get; }

        public TimeType TimeType { get; }

        public string GameType { get; }

        internal DrawInfoDTO(lotteryDraw_info info)
        {
            Contract.Requires(info != null);

            DrawType = RestMapperHelper.MapDrawType(info.draw_type, info.draw_typeSpecified);
            TimeType = RestMapperHelper.MapTimeType(info.time_type, info.time_typeSpecified);
            GameType = info.game_type;
        }
    }
}
