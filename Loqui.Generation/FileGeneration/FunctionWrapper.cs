using Noggog;
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
        public List<string> Wheres = new List<string>();
        private bool SemiColonAfterParenthesis => this.SemiColon && Wheres.Count == 0;

        public FunctionWrapper(
            FileGeneration fg,
            string initialLine)
        {
            this.fg = fg;
            this.initialLine = initialLine;
        }

        public void AddPassArg(string str)
        {
            Add($"{str}: {str}");
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
            if (gen.Empty) return;
            args.Add(gen.Strings.Take(gen.Strings.Count - 1).ToArray());
        }

        public void Dispose()
        {
            if (args.Count <= 1)
            {
                if (args.Count == 0)
                {
                    fg.AppendLine($"{initialLine}(){(this.SemiColonAfterParenthesis ? ";" : null)}");
                }
                else if (args[0].Length == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0][0]}){(this.SemiColonAfterParenthesis ? ";" : null)}");
                }
                else
                {
                    fg.AppendLine($"{initialLine}({(args.Count == 1 ? args[0] : null)}");
                    for (int i = 1; i < args[0].Length - 1; i++)
                    {
                        fg.AppendLine(args[0][i]);
                    }
                    fg.AppendLine($"{args[0][args[0].Length - 1]}){(this.SemiColonAfterParenthesis ? ";" : null)}");
                }
                this.fg.Depth++;
                foreach (var where in Wheres.IterateMarkLast())
                {
                    fg.AppendLine($"{where.Item}{(this.SemiColon && where.Last ? ";" : null)}");
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
                                fg.AppendLine($"{item}{(last ? $"){(SemiColonAfterParenthesis ? ";" : string.Empty)}" : string.Empty)}");
                            });
                    });
            }
            foreach (var where in Wheres.IterateMarkLast())
            {
                fg.AppendLine($"{where.Item}{(this.SemiColon && where.Last ? ";" : null)}");
            }
            this.fg.Depth--;
        }
    }
}
