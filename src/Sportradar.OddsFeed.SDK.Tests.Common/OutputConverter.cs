using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class OutputConverter : TextWriter
    {
        private readonly ITestOutputHelper _output;

        public OutputConverter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding
        {
            get => Encoding.Unicode;
        }

        public override void WriteLine(string value)
        {
            _output.WriteLine(value);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            _output.WriteLine(format, arg);
        }

        public override void Write(char value)
        {
            _output.WriteLine(value.ToString());
        }
    }
}
