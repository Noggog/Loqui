using Noggog;

namespace Loqui.Generation;

public class ArgsWrapper : IDisposable
{
    StructuredStringBuilder sb;
    List<string[]> args = new List<string[]>();
    public bool SemiColon = true;
    string initialLine;
    string suffixLine;

    public ArgsWrapper(
        StructuredStringBuilder sb,
        string initialLine = null,
        string suffixLine = null,
        bool semiColon = true)
    {
        this.sb = sb;
        SemiColon = semiColon;
        this.initialLine = initialLine;
        this.suffixLine = suffixLine;
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
        if (removeSemicolon && gen.Count != 0)
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
        if (initialLine != null)
        {
            if (args.Count == 0)
            {
                sb.AppendLine($"{initialLine}(){suffixLine}{(SemiColon ? ";" : string.Empty)}");
                return;
            }
            else if (args.Count == 1
                     && args[0].Length == 1)
            {
                sb.AppendLine($"{initialLine}({args[0][0]}){suffixLine}{(SemiColon ? ";" : string.Empty)}");
                return;
            }
            else
            {
                sb.AppendLine($"{initialLine}(");
            }
        }
        sb.Depth++;
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
                        sb.AppendLine($"{item}{(last ? $"){suffixLine}{(SemiColon ? ";" : string.Empty)}" : string.Empty)}");
                    });
            });
        sb.Depth--;
    }
}