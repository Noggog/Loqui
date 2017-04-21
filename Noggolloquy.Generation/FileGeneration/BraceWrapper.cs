using System;

namespace Noggolloquy.Generation
{
    public class BraceWrapper : IDisposable
    {
        FileGeneration fg;
        public bool AppendParenthesis;
        public bool AppendSemicolon;
        public bool AppendComma;
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
                fg.AppendLine("}"
                    + (this.AppendParenthesis ? ")" : string.Empty)
                    + (this.AppendSemicolon ? ";" : string.Empty)
                    + (this.AppendComma ? "," : string.Empty));
            }
        }
    }
}
