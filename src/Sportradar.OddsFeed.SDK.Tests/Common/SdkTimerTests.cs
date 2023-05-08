using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class SdkTimerTests
    {
        private readonly ITimer _sdkTimer;
        private readonly IList<string> _timerMsgs;

        public SdkTimerTests()
        {
            ThreadPool.SetMinThreads(100, 100);
            _timerMsgs = new List<string>();
            _sdkTimer = new SdkTimer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void SdkTimerOnElapsed(object sender, EventArgs e)
        {
            _timerMsgs.Add($"{_timerMsgs.Count + 1}. message");
        }

        [Fact]
        public void TimerNormalInitialization()
        {
            Assert.NotNull(_sdkTimer);
            Assert.Empty(_timerMsgs);
        }

        [Fact]
        public void TimerWrongDueTime()
        {
            Action action = () => SdkTimer.Create(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
            action.Should().Throw<ArgumentException>();
        }

        //[Fact]
        //public void TimerWrongPeriodTime()
        //{
        //    Action action = () => SdkTimer.Create(TimeSpan.FromSeconds(1), TimeSpan.Zero);
        //    action.Should().Throw<ArgumentException>();
        //}

        [Fact]
        public void TimerStartWrongDueTime()
        {
            Action action = () => _sdkTimer.Start(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(10));
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TimerStartWrongPeriodTime()
        {
            Action action = () => _sdkTimer.Start(TimeSpan.FromSeconds(1), TimeSpan.Zero);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task TimerNormalOperation()
        {
            _sdkTimer.Elapsed += SdkTimerOnElapsed;
            _sdkTimer.Start();
            await Task.Delay(5200).ConfigureAwait(false);
            Assert.NotNull(_sdkTimer);
            Assert.NotEmpty(_timerMsgs);
            Assert.True(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
        }

        [Fact]
        public async Task TimerFailedOnTickWillNotBreak()
        {
            _sdkTimer.Elapsed += (sender, args) =>
                                 {
                                     _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
                                     throw new InvalidOperationException("Some error");
                                 };
            _sdkTimer.Start();
            await Task.Delay(1200).ConfigureAwait(false);
            Assert.NotNull(_sdkTimer);
            Assert.NotEmpty(_timerMsgs);
            Assert.Equal(1, _timerMsgs.Count);
        }

        [Fact]
        public async Task TimerFailedOnTickWillContinueOnPeriod()
        {
            _sdkTimer.Elapsed += (sender, args) =>
                                 {
                                     _timerMsgs.Add($"{_timerMsgs.Count + 1}. message with error");
                                     throw new InvalidOperationException("Some error");
                                 };
            _sdkTimer.Start();
            await Task.Delay(5200).ConfigureAwait(false);
            Assert.NotNull(_sdkTimer);
            Assert.NotEmpty(_timerMsgs);
            Assert.True(5 <= _timerMsgs.Count, $"Expected 5, actual {_timerMsgs.Count}");
        }

        [Fact]
        public async Task TimerFireOnce()
        {
            var sdkTimer = new SdkTimer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
            sdkTimer.Elapsed += SdkTimerOnElapsed;
            sdkTimer.FireOnce(TimeSpan.Zero);
            await Task.Delay(3200).ConfigureAwait(false);
            Assert.NotNull(_sdkTimer);
            Assert.NotEmpty(_timerMsgs);
            Assert.Equal(1, _timerMsgs.Count);
        }
    }
}
