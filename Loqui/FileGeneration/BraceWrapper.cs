using System;

namespace Loqui
{
    public class BraceWrapper : IDisposable
    {
        FileGeneration fg;
        private bool doIt;

        public BraceWrapper(FileGeneration fg, bool doIt = true)
        {
            this.fg = fg;
            this.doIt = doIt;
            if (doIt)
            {
                fg.AppendLine("{");
                fg.Depth++;
            }
        }

        public void Dispose()
        {
            if (doIt)
            {
                fg.Depth--;
                fg.AppendLine("}");
            }
        }
    }
}
