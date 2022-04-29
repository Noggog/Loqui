namespace Loqui;

public interface IPrintable
{
    void ToString(StructuredStringBuilder sb, string? name = null);
}