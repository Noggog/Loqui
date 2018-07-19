using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class FunctionWrapper : IDisposable
    {
        FileGeneration fg;
        List<string[]> args = new List<string[]>();
        string initialLine;
        public bool SemiColon = false;
        string[] wheres;

        public FunctionWrapper(
            FileGeneration fg,
            string initialLine,
            params string[] wheres)
        {
            this.fg = fg;
            this.initialLine = initialLine;
            this.wheres = wheres ?? new string[0];
        }

        public void Add(params string[] lines)
        {
            foreach (var line in lines)
            {
                args.Add(new string[] { line });
            }
        }

        public void Add(string line)
        {
            args.Add(new string[] { line });
        }

        public void Add(Action<FileGeneration> generator)
        {
            var gen = new FileGeneration();
            generator(gen);
            args.Add(gen.Strings.ToArray());
        }

        public void Dispose()
        {
            if (args.Count <= 1)
            {
                if (args.Count == 0)
                {
                    fg.AppendLine($"{initialLine}(){(this.SemiColon ? ";" : null)}");
                }
                else if (args[0].Length == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0][0]}){(this.SemiColon ? ";" : null)}");
                }
                else
                {
                    fg.AppendLine($"{initialLine}({(args.Count == 1 ? args[0] : null)}");
                    for (int i = 1; i < args[0].Length - 1; i++)
                    {
                        fg.AppendLine(args[0][i]);
                    }
                    fg.AppendLine($"{args[0][args[0].Length - 1]}){(this.SemiColon ? ";" : null)}");
                }
                this.fg.Depth++;
                foreach (var where in wheres.IterateMarkLast())
                {
                    fg.AppendLine($"{where.item}{(this.SemiColon && where.Last ? ";" : null)}");
                }
                this.fg.Depth--;
                return;
            }

            fg.AppendLine($"{initialLine}(");
            this.fg.Depth++;
            if (args.Count != 0)
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
                                fg.AppendLine($"{item}{(last ? $"){(SemiColon ? ";" : string.Empty)}" : string.Empty)}");
                            });
                    });
            }
            foreach (var where in wheres.IterateMarkLast())
            {
                fg.AppendLine($"{where.item}{(this.SemiColon && where.Last ? ";" : null)}");
            }
            this.fg.Depth--;
        }
    }
}
