// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames
{
    internal class MarketMergeResult
    {
        private IList<string> _outcomeProblem;

        private IList<string> _mappingProblem;

        public IList<string> GetOutcomeProblem() { return _outcomeProblem; }

        public IList<string> GetMappingProblem() { return _mappingProblem; }

        public void AddOutcomeProblem(string outcomeId)
        {
            if (_outcomeProblem == null)
            {
                _outcomeProblem = new List<string>();
            }
            _outcomeProblem.Add(outcomeId);
        }

        public void AddMappingProblem(string mappingMarketId)
        {
            if (_mappingProblem == null)
            {
                _mappingProblem = new List<string>();
            }
            _mappingProblem.Add(mappingMarketId);
        }

        public bool IsAllMerged()
        {
            return _outcomeProblem == null && _mappingProblem == null;
        }
    }
}
