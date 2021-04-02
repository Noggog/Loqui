using System;
using Xunit;

namespace Loqui.Tests
{
    public class FileGeneration_Tests
    {
        [Fact]
        public void AppendWithEmbeddedNewLine()
        {
            var fg = new FileGeneration();
            fg.AppendLine($"A{Environment.NewLine}B");
            Assert.Equal(new string[] { "A", "B" }, fg);
        }

        [Fact]
        public void AppendNull()
        {
            var fg = new FileGeneration();
            fg.AppendLine(null);
            Assert.Equal(new string[] { "" }, fg);
        }

        [Fact]
        public void AppendEmpty()
        {
            var fg = new FileGeneration();
            fg.AppendLine(String.Empty);
            Assert.Equal(new string[] { "" }, fg);
        }
    }
}
