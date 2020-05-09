using System;

namespace Loqui.Generation
{
    public class RegionWrapper : IDisposable
    {
        readonly FileGeneration _fg;
        readonly int _startingIndex;
        readonly string _name;
        public bool AppendExtraLine;
        public bool SkipIfOnlyOneLine = false;

        public RegionWrapper(FileGeneration fg, string str, bool appendExtraLine = true)
        {
            this._fg = fg;
            this._startingIndex = fg.Count;
            this._name = str;
            this.AppendExtraLine = appendExtraLine;
        }

        public void Dispose()
        {
            if (string.IsNullOrWhiteSpace(_name)) return;
            if (_startingIndex == _fg.Count) return;
            if (SkipIfOnlyOneLine && _startingIndex + 1 == _fg.Count) return;
            _fg.Insert(Math.Max(0, _startingIndex), $"{_fg.DepthStr}#region {_name}");
            _fg.AppendLine("#endregion");
            if (AppendExtraLine)
            {
                _fg.AppendLine();
            }
        }
    }
}
