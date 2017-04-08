using System;

namespace Noggolloquy.Generation
{
    public class RegionWrapper : IDisposable
    {
        FileGeneration fg;

        public RegionWrapper(FileGeneration fg, string str)
        {
            this.fg = fg;
            fg.AppendLine($"#region {str}");
        }

        public void Dispose()
        {
            fg.AppendLine("#endregion");
        }
    }
}
