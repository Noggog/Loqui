using Noggog;

namespace Loqui.Generation;

public class PropertyCtorWrapper : IDisposable
{
    StructuredStringBuilder sb;
    List<string[]> args = new List<string[]>();

    public PropertyCtorWrapper(StructuredStringBuilder sb)
    {
        this.sb = sb;
    }

    public void Add(params string[] lines)
    {
        foreach (var line in lines)
        {
            args.Add(new string[] { line });
        }
    }

    public void AddPassArg(string str)
    {
        Add($"{str}: {str}");
    }

    public void Add(Action<StructuredStringBuilder> generator, bool removeSemicolon = true)
    {
        var gen = new StructuredStringBuilder();
        generator(gen);
        if (gen.Empty) return;
        if (removeSemicolon)
        {
            gen[gen.Count - 1] = gen[gen.Count - 1].TrimEnd(';');
        }
        args.Add(gen.ToArray());
    }

    public async Task Add(Func<StructuredStringBuilder, Task> generator)
    {
        var gen = new StructuredStringBuilder();
        await generator(gen);
        if (gen.Empty) return;
        args.Add(gen.ToArray());
    }

    public void Dispose()
    {
        using (sb.CurlyBrace(appendSemiColon: true))
        {
            args.Last(
                each: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            sb.AppendLine($"{item}{(last ? "," : string.Empty)}");
                        });
                },
                last: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            sb.AppendLine($"{item}");
                        });
                });
        }
    }
}