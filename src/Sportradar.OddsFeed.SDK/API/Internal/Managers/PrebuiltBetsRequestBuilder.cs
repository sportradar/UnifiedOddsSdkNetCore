// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    internal sealed class PrebuiltBetsRequestBuilder : IPrebuiltBetsRequestBuilder
    {
        private Urn _eventId;
        private int _subBookmakerId;
        private int? _count;
        private int? _length;
        private string _user;

        public PrebuiltBetsRequestBuilder(int bookmakerId)
        {
            _subBookmakerId = bookmakerId;
        }

        public IPrebuiltBetsRequestBuilder WithCount(int count)
        {
            _count = count;
            return this;
        }

        public IPrebuiltBetsRequestBuilder WithLength(int length)
        {
            _length = length;
            return this;
        }

        public IPrebuiltBetsRequestBuilder WithUser(string user)
        {
            _user = user;
            return this;
        }

        public IPrebuiltBetsRequestBuilder WithEvent(Urn eventId)
        {
            _eventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            return this;
        }

        public IPrebuiltBetsRequestBuilder WithSubBookmakerId(int subBookmakerId)
        {
            _subBookmakerId = subBookmakerId;
            return this;
        }

        public IPrebuiltBetsRequest Build()
        {
            return new PrebuiltBetsRequest(_eventId, _subBookmakerId, _count, _length, _user);
        }
    }
}
