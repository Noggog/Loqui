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
            if (args.Count == 0)
            {
                fg.AppendLine($"{initialLine}()");
                this.fg.Depth++;
                foreach (var where in wheres)
                {
                    fg.AppendLine($"{where}");
                }
                this.fg.Depth--;
                return;
            }
            else if (args.Count == 1)
            {
                fg.AppendLine($"{initialLine}({args[0]})");
                this.fg.Depth++;
                foreach (var where in wheres)
                {
                    fg.AppendLine($"{where}");
                }
                this.fg.Depth--;
                return;
            }
            else
            {
                fg.AppendLine($"{initialLine}(");
            }
            this.fg.Depth++;
            if (args.Count != 0)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    fg.AppendLine(args[i] + ",");
                }
                fg.AppendLine($"{args[args.Count - 1]})");
            }
            foreach (var where in wheres)
            {
                fg.AppendLine($"{where}");
            }
            this.fg.Depth--;
        }
    }
}
