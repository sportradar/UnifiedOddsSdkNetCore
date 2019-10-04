/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable
{
    /// <summary>
    /// Class used to export/import pitcher cache item properties
    /// </summary>
    public class ExportableStageCI : ExportableCompetitionCI
    {
        /// <summary>
        /// A <see cref="string"/> representing the category id
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// A <see cref="ExportableStageCI"/> representing the parent stage
        /// </summary>
        public ExportableStageCI ParentStage { get; set; }

        /// <summary>
        /// A <see cref="IEnumerable{T}"/> representing the child stages
        /// </summary>
        public IEnumerable<ExportableStageCI> ChildStages { get; set; }

        /// <summary>
        /// A <see cref="StageType"/> representing the stage type
        /// </summary>
        public StageType StageType { get; set; }
    }
}
