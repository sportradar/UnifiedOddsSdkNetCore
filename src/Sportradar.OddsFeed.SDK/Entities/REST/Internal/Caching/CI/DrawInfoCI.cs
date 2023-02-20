/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for lottery draw info
    /// </summary>
    internal class DrawInfoCI
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
        /// Initializes a new instance of the <see cref="DrawInfoCI"/> class
        /// </summary>
        /// <param name="dto">A <see cref="DrawInfoDTO"/> instance containing information about the draw info</param>
        public DrawInfoCI(DrawInfoDTO dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            DrawType = dto.DrawType;
            TimeType = dto.TimeType;
            GameType = dto.GameType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawInfoCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableDrawInfoCI"/> instance containing information about the draw info</param>
        public DrawInfoCI(ExportableDrawInfoCI exportable)
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableDrawInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableDrawInfoCI
            {
                DrawType = DrawType,
                TimeType = TimeType,
                GameType = GameType
            });
        }
    }
}
