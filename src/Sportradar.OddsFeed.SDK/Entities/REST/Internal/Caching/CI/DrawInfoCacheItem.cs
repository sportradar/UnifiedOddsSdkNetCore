// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for lottery draw info
    /// </summary>
    internal class DrawInfoCacheItem
    {
        /// <summary>
        /// Gets the type of the draw
        /// </summary>
        /// <value>The type of the draw</value>
        public DrawType DrawType { get; }

        /// <summary>
        /// Gets the type of the time
        /// </summary>
        /// <value>The type of the time</value>
        public TimeType TimeType { get; }

        /// <summary>
        /// Gets the type of the game
        /// </summary>
        /// <value>The type of the game</value>
        public string GameType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfoCacheItem"/> class
        /// </summary>
        /// <param name="dto">A <see cref="DrawInfoDto"/> instance containing information about the draw info</param>
        public DrawInfoCacheItem(DrawInfoDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            DrawType = dto.DrawType;
            TimeType = dto.TimeType;
            GameType = dto.GameType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfoCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableDrawInfo"/> instance containing information about the draw info</param>
        public DrawInfoCacheItem(ExportableDrawInfo exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            DrawType = exportable.DrawType;
            TimeType = exportable.TimeType;
            GameType = exportable.GameType;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableDrawInfo> ExportAsync()
        {
            return Task.FromResult(new ExportableDrawInfo
            {
                DrawType = DrawType,
                TimeType = TimeType,
                GameType = GameType
            });
        }
    }
}
