// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="resultChangesEndpoint"/> instances to <see cref="EntityList{ResultChangeDto}" /> instance
    /// </summary>
    internal class ResultChangesMapper : ISingleTypeMapper<EntityList<ResultChangeDto>>
    {
        /// <summary>
        /// A <see cref="resultChangesEndpoint"/> instance containing result changes
        /// </summary>
        private readonly resultChangesEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultChangesMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="resultChangesEndpoint"/> instance containing result changes</param>
        internal ResultChangesMapper(resultChangesEndpoint data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Maps it's data to <see cref="EntityList{ResultChangeDto}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="EntityList{ResultChangeDto}"/> instance</returns>
        public EntityList<ResultChangeDto> Map()
        {
            var resultChanges = new List<ResultChangeDto>();
            if (_data.result_change != null)
            {
                resultChanges = _data.result_change.Select(f => new ResultChangeDto(f, _data.generated_atSpecified ? _data.generated_at : (DateTime?)null)).ToList();
            }

            return new EntityList<ResultChangeDto>(resultChanges);
        }
    }
}
