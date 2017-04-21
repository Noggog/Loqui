using System;

namespace Noggolloquy.Generation
{
    public class LineWrapper : IDisposable
    {
        FileGeneration fg; 

        public LineWrapper(FileGeneration fg)
        {
            this.fg = fg;
            for (int i = 0; i < fg.Depth; i++)
            {
                fg.Append("    ");
            }
        }

        public void Dispose()
        {
            fg.Append("\n");
        }
    }
}
