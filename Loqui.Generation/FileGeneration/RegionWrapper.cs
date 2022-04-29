namespace Loqui.Generation;

public class RegionWrapper : IDisposable
{
    readonly StructuredStringBuilder _sb;
    readonly int _startingIndex;
    readonly string _name;
    public bool AppendExtraLine;
    public bool SkipIfOnlyOneLine = false;

    public RegionWrapper(StructuredStringBuilder sb, string str, bool appendExtraLine = true)
    {
        _sb = sb;
        _startingIndex = sb.Count;
        _name = str;
        AppendExtraLine = appendExtraLine;
    }

    public void Dispose()
    {
        if (string.IsNullOrWhiteSpace(_name)) return;
        if (_startingIndex == _sb.Count) return;
        if (SkipIfOnlyOneLine && _startingIndex + 1 == _sb.Count) return;
        _sb.Insert(Math.Max(0, _startingIndex), $"{_sb.DepthStr}#region {_name}");
        _sb.AppendLine("#endregion");
        if (AppendExtraLine)
        {
            _sb.AppendLine();
        }
    }
}