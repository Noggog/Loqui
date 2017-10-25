using System;

namespace Loqui
{
    public class DepthWrapper : IDisposable
    {
        FileGeneration fg;
        bool doIt;

        public DepthWrapper(
            FileGeneration fg,
            bool doIt = true)
        {
            this.fg = fg;
            this.doIt = doIt;
            if (doIt)
            {
                this.fg.Depth++;
            }
        }

        public void Dispose()
        {
            if (doIt)
            {
                this.fg.Depth--;
            }
        }
    }
}
