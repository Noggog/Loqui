using System;

namespace Noggolloquy.Generation
{
    public class RegionWrapper : IDisposable
    {
        FileGeneration fg;
        int index;
        string name;
        public bool AppendExtraLine;

        public RegionWrapper(FileGeneration fg, string str, bool appendExtraLine = true)
        {
            this.fg = fg;
            this.index = fg.Strings.Count;
            this.name = str;
            this.AppendExtraLine = appendExtraLine;
        }

        public void Dispose()
        {
            if (index == fg.Strings.Count) return;
            fg.Strings.Insert(index - 1, fg.DepthStr + $"#region {name}");
            fg.AppendLine("#endregion");
            if (AppendExtraLine)
            {
                fg.AppendLine();
            }
        }
    }
}
