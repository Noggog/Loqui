namespace Loqui;

public class LineWrapper : IDisposable
{
    readonly StructuredStringBuilder _sb; 

    public LineWrapper(StructuredStringBuilder sb)
    {
        _sb = sb;
        _sb.Append(_sb.DepthStr);
    }

    public void Dispose()
    {
        _sb.Append(Environment.NewLine);
    }
}