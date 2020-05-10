using System;

namespace Loqui
{
    public class LineWrapper : IDisposable
    {
        readonly FileGeneration _fg; 

        public LineWrapper(FileGeneration fg)
        {
            this._fg = fg;
            this._fg.Append(this._fg.DepthStr);
        }

        public void Dispose()
        {
            this._fg.Append(Environment.NewLine);
        }
    }
}
