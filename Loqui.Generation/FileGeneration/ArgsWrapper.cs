using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;

namespace Loqui.Generation
{
    public class ArgsWrapper : IDisposable
    {
        FileGeneration fg;
        List<string[]> args = new List<string[]>();
        public bool SemiColon = true;
        string initialLine;
        string suffixLine;

        public ArgsWrapper(
            FileGeneration fg,
            string initialLine = null,
            string suffixLine = null,
            bool semiColon = true)
        {
            this.fg = fg;
            this.SemiColon = semiColon;
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

        public void Add(Action<FileGeneration> generator, bool removeSemicolon = true)
        {
            var gen = new FileGeneration();
            generator(gen);
            if (gen.Empty) return;
            if (removeSemicolon)
            {
                gen[^1] = gen[^1].TrimEnd(';');
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
            if (initialLine != null)
            {
                if (args.Count == 0)
                {
                    fg.AppendLine($"{initialLine}(){suffixLine}{(SemiColon ? ";" : string.Empty)}");
                    return;
                }
                else if (args.Count == 1
                    && args[0].Length == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0][0]}){suffixLine}{(SemiColon ? ";" : string.Empty)}");
                    return;
                }
                else
                {
                    fg.AppendLine($"{initialLine}(");
                }
            }
            this.fg.Depth++;
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
                            fg.AppendLine($"{item}{(last ? $"){suffixLine}{(SemiColon ? ";" : string.Empty)}" : string.Empty)}");
                        });
                });
            this.fg.Depth--;
        }
    }
}
