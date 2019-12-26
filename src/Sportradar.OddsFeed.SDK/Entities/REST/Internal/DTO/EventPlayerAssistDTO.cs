/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    public class EventPlayerAssistDTO : SportEntityDTO
    {
        public string Type { get; }

        internal EventPlayerAssistDTO(string id, string name, string type)
            : base(id, name)
        {
            Type = type;
        }

        internal EventPlayerAssistDTO(eventPlayerAssist eventPlayerAssist)
            : base(eventPlayerAssist.id, eventPlayerAssist.name)
        {
            Type = eventPlayerAssist.type;
        }
    }
}
