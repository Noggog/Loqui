using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class ArgsWrapper : IDisposable
    {
        FileGeneration fg;
        List<string[]> args = new List<string[]>(); 
        public bool SemiColon = true;
        string initialLine;

        public ArgsWrapper(
            FileGeneration fg,
            string initialLine = null)
        {
            this.fg = fg;
            this.initialLine = initialLine;
        }

        public void Add(params string[] lines)
        {
            args.Add(lines);
        }

        public void Add(Action<FileGeneration> generator)
        {
            var gen = new FileGeneration();
            generator(gen);
            Add(gen.Strings.ToArray());
        }

        public void Dispose()
        {
            if (initialLine != null)
            {
                if (args.Count == 0)
                {
                    fg.AppendLine($"{initialLine}();");
                    return;
                }
                else if (args.Count == 1
                    && args[0].Length == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0][0]});");
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
                            fg.AppendLine($"{item}{(last ? $"){(SemiColon ? ";" : string.Empty)}" : string.Empty)}");
                        });
                });
            this.fg.Depth--;
        }
    }
}
