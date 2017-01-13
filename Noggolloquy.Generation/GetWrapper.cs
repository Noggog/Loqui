using System;

namespace Noggolloquy.Generation
{
    public class GetWrapper : IDisposable
    {
        FileGeneration fg;

        public GetWrapper(FileGeneration fg)
        {
            this.fg = fg;
            fg.AppendLine("{");
            fg.Depth++;
            fg.AppendLine("get");
            fg.AppendLine("{");
            fg.Depth++;
        }

        public void Dispose()
        {
            fg.Depth--;
            fg.AppendLine("}");
            fg.Depth--;
            fg.AppendLine("}");
        }
    }
}
