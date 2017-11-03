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
        List<string> args = new List<string>();
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
            this.wheres = wheres;
        }

        public void Add(string line)
        {
            args.Add(line);
        }

        public void Dispose()
        {
            if (args.Count <= 1)
            {
                fg.AppendLine($"{initialLine}({(args.Count == 1 ? args[0] : null)}){(this.SemiColon ? ";" : null)}");
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
                for (int i = 0; i < args.Count - 1; i++)
                {
                    fg.AppendLine(args[i] + ",");
                }
                fg.AppendLine($"{args[args.Count - 1]}){(this.SemiColon && wheres.Length == 0 ? ";" : null)}");
            }
            foreach (var where in wheres.IterateMarkLast())
            {
                fg.AppendLine($"{where.item}{(this.SemiColon && where.Last ? ";" : null)}");
            }
            this.fg.Depth--;
        }
    }
}
