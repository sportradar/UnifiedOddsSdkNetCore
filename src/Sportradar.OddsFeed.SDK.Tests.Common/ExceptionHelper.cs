// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class ExceptionHelper
{
    public static MappingException GetMappingException()
    {
        return new MappingException("Mapping exception", "some-property", "some-property-value", "any-target-name", null);
    }

    public static DeserializationException GetDeserializationException()
    {
        return new DeserializationException("Deserialization exception", null);
    }

    public static InvalidOperationException GetInvalidException()
    {
        return new InvalidOperationException("InvalidOperationException exception");
    }
}
