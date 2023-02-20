/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="resultChangesEndpoint"/> instances to <see cref="IEnumerable{ResultChangeDTO}" /> instance
    /// </summary>
    internal class ResultChangesMapper : ISingleTypeMapper<IEnumerable<ResultChangeDTO>>
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
        /// Maps it's data to <see cref="IEnumerable{ResultChangeDTO}"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="IEnumerable{ResultChangeDTO}"/> instance</returns>
        public IEnumerable<ResultChangeDTO> Map()
        {
            if (_data.result_change == null)
            {
                return new List<ResultChangeDTO>();
            }

            return _data.result_change.Select(f => new ResultChangeDTO(f, _data.generated_atSpecified ? _data.generated_at : (DateTime?)null)).ToList();
        }
    }
}
