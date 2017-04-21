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
        List<string> args = new List<string>();
        public bool SemiColon = true;
        string initialLine;

        public ArgsWrapper(
            FileGeneration fg,
            string initialLine = null)
        {
            this.fg = fg;
            this.initialLine = initialLine;
        }

        public void Add(string line)
        {
            args.Add(line);
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
                else if (args.Count == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0]});");
                    return;
                }
                else
                {
                    fg.AppendLine($"{initialLine}(");
                }
            }
            this.fg.Depth++;
            if (args.Count != 0)
            {
                for (int i = 0; i < args.Count - 1; i++)
                {
                    fg.AppendLine(args[i] + ",");
                }
                fg.AppendLine($"{args[args.Count - 1]}){(SemiColon ? ";" : string.Empty)}");
            }
            this.fg.Depth--;
        }
    }
}
