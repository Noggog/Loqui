using Noggog;

namespace Loqui.Generation;

public class PropertyCtorWrapper : IDisposable
{
    FileGeneration fg;
    List<string[]> args = new List<string[]>();

    public PropertyCtorWrapper(FileGeneration fg)
    {
        this.fg = fg;
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

    public void Add(Action<FileGeneration> generator, bool removeSemicolon = true)
    {
        var gen = new FileGeneration();
        generator(gen);
        if (gen.Empty) return;
        if (removeSemicolon)
        {
            gen[gen.Count - 1] = gen[gen.Count - 1].TrimEnd(';');
        }
        args.Add(gen.ToArray());
    }

    public async Task Add(Func<FileGeneration, Task> generator)
    {
        var gen = new FileGeneration();
        await generator(gen);
        if (gen.Empty) return;
        args.Add(gen.ToArray());
    }

    public void Dispose()
    {
        using (new BraceWrapper(fg) { AppendSemicolon = true })
        {
            args.Last(
                each: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            fg.AppendLine($"{item}{(last ? "," : string.Empty)}");
                        });
                },
                last: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            fg.AppendLine($"{item}");
                        });
                });
        }
    }
}