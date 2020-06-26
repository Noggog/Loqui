using System;

namespace Loqui
{
    public class BraceWrapper : IDisposable
    {
        private readonly FileGeneration _fg;
        private readonly bool _doIt;

        public bool AppendParenthesis;
        public bool AppendSemicolon;
        public bool AppendComma;

        public BraceWrapper(FileGeneration fg, bool doIt = true)
        {
            this._fg = fg;
            this._doIt = doIt;
            if (doIt)
            {
                fg.AppendLine("{");
                fg.Depth++;
            }
        }

        public void Dispose()
        {
            if (_doIt)
            {
                _fg.Depth--;
                _fg.AppendLine("}"
                    + (this.AppendParenthesis ? ")" : string.Empty)
                    + (this.AppendSemicolon ? ";" : string.Empty)
                    + (this.AppendComma ? "," : string.Empty));
            }
        }
    }
}
