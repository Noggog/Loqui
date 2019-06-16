using System;

namespace Loqui.Generation
{
    public class RegionWrapper : IDisposable
    {
        FileGeneration fg;
        readonly int startingIndex;
        readonly string name;
        public bool AppendExtraLine;
        public bool SkipIfOnlyOneLine = false;

        public RegionWrapper(FileGeneration fg, string str, bool appendExtraLine = true)
        {
            this.fg = fg;
            this.startingIndex = fg.Strings.Count;
            this.name = str;
            this.AppendExtraLine = appendExtraLine;
        }

        public void Dispose()
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (startingIndex == fg.Strings.Count) return;
            if (SkipIfOnlyOneLine && startingIndex + 1 == fg.Strings.Count) return;
            fg.Strings.Insert(startingIndex - 1, fg.DepthStr + $"#region {name}");
            fg.AppendLine("#endregion");
            if (AppendExtraLine)
            {
                fg.AppendLine();
            }
        }
    }
}
