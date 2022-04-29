namespace Loqui;

public class DepthWrapper : IDisposable
{
    StructuredStringBuilder sb;
    bool doIt;

    public DepthWrapper(
        StructuredStringBuilder sb,
        bool doIt = true)
    {
        this.sb = sb;
        this.doIt = doIt;
        if (doIt)
        {
            this.sb.Depth++;
        }
    }

    public void Dispose()
    {
        if (doIt)
        {
            sb.Depth--;
        }
    }
}