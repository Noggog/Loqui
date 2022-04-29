namespace Loqui.Generation;

public class IfWrapper : IDisposable
{
    StructuredStringBuilder sb;
    bool first;
    public List<(string str, bool wrap)> Checks = new List<(string str, bool wrap)>();
    bool ands;
    public bool Empty => Checks.Count == 0;
    public Action<StructuredStringBuilder> Body;

    public IfWrapper(StructuredStringBuilder sb, bool ANDs, bool first = true)
    {
        ands = ANDs;
        this.first = first;
        this.sb = sb;
    }

    public void Add(string str, bool wrapInParens = false)
    {
        Checks.Add((str, wrapInParens));
    }

    private string Get(int index)
    {
        var item = Checks[index];
        if (!item.wrap || Checks.Count <= 1)
        {
            return item.str;
        }
        return $"({item.str})";
    }

    private void GenerateIf()
    {
        using (var line = new LineWrapper(sb))
        {
            if (!first)
            {
                sb.Append("else ");
            }
            sb.Append("if (");
            sb.Append(Get(0));
            if (Checks.Count == 1)
            {
                sb.Append(")");
                return;
            }
        }
        using (new DepthWrapper(sb))
        {
            for (int i = 1; i < Checks.Count; i++)
            {
                using (new LineWrapper(sb))
                {
                    if (ands)
                    {
                        sb.Append("&& ");
                    }
                    else
                    {
                        sb.Append("|| ");
                    }
                    sb.Append(Get(i));
                    if (i == Checks.Count - 1)
                    {
                        sb.Append(")");
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        if (Checks.Count == 0)
        {
            Body?.Invoke(sb);
            return;
        }
        GenerateIf();
        if (Body != null)
        {
            using (sb.CurlyBrace())
            {
                Body(sb);
            }
        }
    }
}