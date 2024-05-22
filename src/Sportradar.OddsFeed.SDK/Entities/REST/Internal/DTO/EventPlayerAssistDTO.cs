// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class EventPlayerAssistDto : SportEntityDto
    {
        public string Type { get; }

        internal EventPlayerAssistDto(string id, string name, string type)
            : base(id, name)
        {
            Type = type;
        }

        internal EventPlayerAssistDto(eventPlayerAssist eventPlayerAssist)
            : base(eventPlayerAssist.id, eventPlayerAssist.name)
        {
            Type = eventPlayerAssist.type;
        }
    }
}
