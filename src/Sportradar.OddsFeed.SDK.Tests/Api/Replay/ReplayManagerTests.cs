// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Replay;

public class ReplayManagerTests
{
    private readonly Mock<IDataRestful> _mockDataRestful;
    private readonly ReplayManager _replayManager;

    public ReplayManagerTests()
    {
        _mockDataRestful = new Mock<IDataRestful>();
        _replayManager = new ReplayManager(TestConfiguration.GetConfig(), _mockDataRestful.Object);
    }

    [Fact]
    public void Constructor_Normal()
    {
        Assert.NotNull(_replayManager);
    }

    [Fact]
    public void Constructor_MissingConfiguration_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplayManager(null, _mockDataRestful.Object));
    }

    [Fact]
    public void Constructor_MissingDataRestful_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplayManager(TestConfiguration.GetConfig(), null));
    }

    [Fact]
    public void AddMessagesToReplayQueue_EventIdNoStartTime()
    {
        _mockDataRestful.Setup(x => x.PutDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.AddMessagesToReplayQueue(TestData.EventMatchId);

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains(TestData.EventMatchId.ToString())), null), Times.Once());
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("?start_time=")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    [Fact]
    public void AddMessagesToReplayQueue_EventIdNoStartTime_ErrorMessage()
    {
        _mockDataRestful.Setup(x => x.PutDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueErrorResponse(HttpStatusCode.BadRequest, TestData.EventMatchId)));
        var response = _replayManager.AddMessagesToReplayQueue(TestData.EventMatchId);

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains(TestData.EventMatchId.ToString())), null), Times.Once());
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("?start_time=")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PutDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains("We do not have data for given event anymore.", response.Message);
        Assert.Contains("Request was not successfully completed.", response.ErrorMessage);
        Assert.False(response.Success);
    }

    [Fact]
    public void RemoveEventFromReplayQueue_EventId()
    {
        _mockDataRestful.Setup(x => x.DeleteDataAsync(It.IsAny<Uri>())).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.RemoveEventFromReplayQueue(TestData.EventMatchId);

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.DeleteDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains(TestData.EventMatchId.ToString()))), Times.Once());
        _mockDataRestful.Verify(m => m.DeleteDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id="))), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    [Fact]
    public void RemoveEventFromReplayQueue_EventId_ErrorMessage()
    {
        _mockDataRestful.Setup(x => x.DeleteDataAsync(It.IsAny<Uri>())).Returns(Task.FromResult(CreateAddQueueErrorResponse(HttpStatusCode.BadRequest, TestData.EventMatchId)));
        var response = _replayManager.RemoveEventFromReplayQueue(TestData.EventMatchId);

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.DeleteDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains(TestData.EventMatchId.ToString()))), Times.Once());
        _mockDataRestful.Verify(m => m.DeleteDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id="))), Times.Once());
        Assert.Contains("We do not have data for given event anymore.", response.Message);
        Assert.Contains("Request was not successfully completed.", response.ErrorMessage);
        Assert.False(response.Success);
    }

    [Fact]
    public void GetReplayEventsInQueue_GetIds()
    {
        _mockDataRestful.Setup(x => x.GetDataAsync(It.IsAny<Uri>())).Returns(Task.FromResult(CreateQueueListSuccessResponse()));
        var response = _replayManager.GetEventsInQueue();

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.GetDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains("/replay"))), Times.Once());
        _mockDataRestful.Verify(m => m.GetDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id="))), Times.Once());
        Assert.Equal(5, response.Count());
    }

    [Fact]
    public void GetReplayEventsInQueue_GetReplayEvents()
    {
        _mockDataRestful.Setup(x => x.GetDataAsync(It.IsAny<Uri>())).Returns(Task.FromResult(CreateQueueListSuccessResponse()));
        var response = _replayManager.GetReplayEventsInQueue();

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.GetDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains("/replay"))), Times.Once());
        _mockDataRestful.Verify(m => m.GetDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id="))), Times.Once());
        Assert.Equal(5, response.Count());
    }

    [Fact]
    public void StartReplay_Base()
    {
        _mockDataRestful.Setup(x => x.PostDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.StartReplay();

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => i.PathAndQuery.Contains("/play?speed=")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    [Fact]
    public void StartReplayScenario_Base()
    {
        _mockDataRestful.Setup(x => x.PostDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.StartReplayScenario(1);

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains("/scenario/play/")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    [Fact]
    public void StopReplay_Base()
    {
        _mockDataRestful.Setup(x => x.PostDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.StopReplay();

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains("/stop")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    [Fact]
    public void StopAndClearReplay_Base()
    {
        _mockDataRestful.Setup(x => x.PostDataAsync(It.IsAny<Uri>(), null)).Returns(Task.FromResult(CreateAddQueueSuccessResponse(TestData.EventMatchId)));
        var response = _replayManager.StopAndClearReplay();

        Assert.NotNull(response);
        Assert.Single(_mockDataRestful.Invocations);
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => i.AbsolutePath.Contains("/reset")), null), Times.Once());
        _mockDataRestful.Verify(m => m.PostDataAsync(It.Is<Uri>(i => !i.AbsolutePath.Contains("node_id=")), null), Times.Once());
        Assert.Contains($"Event: {TestData.EventMatchId} was added to the playlist set.", response.Message);
        Assert.Null(response.ErrorMessage);
        Assert.True(response.Success);
    }

    private HttpResponseMessage CreateAddQueueErrorResponse(HttpStatusCode statusCode, Urn eventId)
    {
        var responseContent =
            $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n  <response>\n    <action>Adding event {eventId} to playlist set failed.</action>\n    <message>We do not have data for given event anymore.</message>\n  </response>";

        return new HttpResponseMessage(statusCode) { Content = new StringContent(responseContent) };
    }

    private HttpResponseMessage CreateAddQueueSuccessResponse(Urn eventId)
    {
        var responseContent = $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n  <response>\n    <action>OK</action>\n    <message>Event: {eventId} was added to the playlist set.</message>\n  </response>";

        return new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent(responseContent) };
    }

    private Stream CreateQueueListSuccessResponse()
    {
        var responseContent =
            $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n  <replay_set_content>\n    <event id=\"sr:match:10841234\" position=\"1\" />\n    <event id=\"sr:match:10982340\" position=\"2\" />\n    <event id=\"sr:match:10982346\" position=\"3\" />\n    <event id=\"sr:match:11365261\" position=\"4\" />\n    <event id=\"sr:match:11427861\" position=\"5\" />\n  </replay_set_content>";

        var byteArray = Encoding.UTF8.GetBytes(responseContent);
        return new MemoryStream(byteArray);
    }
}
