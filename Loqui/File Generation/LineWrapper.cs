namespace Loqui;

public class LineWrapper : IDisposable
{
    readonly FileGeneration _fg; 

    public LineWrapper(FileGeneration fg)
    {
        _fg = fg;
        _fg.Append(_fg.DepthStr);
    }

    public void Dispose()
    {
        _fg.Append(Environment.NewLine);
    }
}