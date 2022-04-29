using Noggog;

namespace Loqui;

public class CommaWrapper : IDisposable
{
    StructuredStringBuilder sb;
    string delimiter = ",";
    List<string> items = new List<string>();

    public CommaWrapper(StructuredStringBuilder sb, string delimiter = ",")
    {
        this.sb = sb;
        this.delimiter = delimiter;
    }

    public void Add(string item)
    {
        items.Add(item);
    }

    public void Add(params string[] items)
    {
        this.items.AddRange(items);
    }

    public void Add(Action<StructuredStringBuilder> generator)
    {
        var gen = new StructuredStringBuilder();
        generator(gen);
        if (gen.Empty) return;
        Add(gen.ToArray());
    }

    public void Dispose()
    {
        foreach (var item in items.IterateMarkLast())
        {
            if (item.Last)
            {
                sb.AppendLine(item.Item);
            }
            else
            {
                using (new LineWrapper(sb))
                {
                    sb.Append(item.Item);
                    sb.Append(delimiter);
                }
            }
        }
    }
}