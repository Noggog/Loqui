using System;

namespace Noggolloquy.Generation
{
    public class RegionWrapper : IDisposable
    {
        FileGeneration fg;
        int index;
        string name;

        public RegionWrapper(FileGeneration fg, string str)
        {
            this.fg = fg;
            this.index = fg.Strings.Count;
            this.name = str;
        }

        public void Dispose()
        {
            if (index == fg.Strings.Count) return;
            fg.Strings.Insert(index - 1, fg.DepthStr + $"#region {name}");
            fg.AppendLine("#endregion");
        }
    }
}
