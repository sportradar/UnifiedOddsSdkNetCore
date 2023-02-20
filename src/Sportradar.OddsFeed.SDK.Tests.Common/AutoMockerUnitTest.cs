using Moq.AutoMock;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public abstract class AutoMockerUnitTest
    {
        protected readonly AutoMocker Mocker = new AutoMocker();
    }
}
