using System;

namespace Loqui
{
    public class DepthWrapper : IDisposable
    {
        FileGeneration fg;

        public DepthWrapper(FileGeneration fg)
        {
            this.fg = fg;
            this.fg.Depth++;
        }

        public void Dispose()
        {
            this.fg.Depth--;
        }
    }
}
