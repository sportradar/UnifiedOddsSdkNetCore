﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Text;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class MessageDataExtractorTests
{
    private readonly IMessageDataExtractor _dataExtractor = new MessageDataExtractor();

    [Fact]
    public void EmptyOrNullDataDoesNotCauseException()
    {
        var result = _dataExtractor.GetBasicMessageData(null);
        Assert.NotNull(result);

        var data = Array.Empty<byte>();
        result = _dataExtractor.GetBasicMessageData(data);
        Assert.NotNull(result);
    }

    [Fact]
    public void MessageTypeIsExtractedFromShortMessageStart()
    {
        var message = "<alive 'rest of the message";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
        Assert.Equal(MessageType.Alive, result.MessageType);
    }

    [Fact]
    public void MessageTypeIsExtractedFromLongMessageStart()
    {
        var message = "'xml header here' <bet_settlement> 'rest of the message here";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
        Assert.Equal(MessageType.BetSettlement, result.MessageType);
    }

    [Fact]
    public void MessageTypeIsExtractedFromMessageEnd()
    {
        var message = "'start of the message'</product_down>";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));
        Assert.Equal(MessageType.Unknown, result.MessageType);
    }

    [Fact]
    public void AttributeValueIsCorrectWhenStartQuoteIsMissing()
    {
        var message = "<alive product=1";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

        Assert.Null(result.ProducerId);
    }

    [Fact]
    public void AttributeValueIsCorrectWhenEndQuoteIsMissing()
    {
        var message = "<alive product=\"1";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

        Assert.Null(result.ProducerId);
    }

    [Fact]
    public void DataExtractedFromValidMessageIsCorrect()
    {
        var message = "<bet_stop product=\"1\" event_id=\"sr:match:9578495\" 'remaining message' />";
        var result = _dataExtractor.GetBasicMessageData(Encoding.UTF8.GetBytes(message));

        Assert.NotNull(result);
        Assert.Equal(MessageType.BetStop, result.MessageType);
        Assert.Equal("1", result.ProducerId);
        Assert.Equal("sr:match:9578495", result.EventId);
    }
}
