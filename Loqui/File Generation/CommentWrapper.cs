using System;
using System.Collections.Generic;
using System.Text;

namespace Loqui
{
    public class CommentWrapper : IDisposable
    {
        private readonly FileGeneration _fg;
        public readonly FileGeneration Summary = new FileGeneration();
        public readonly Dictionary<string, FileGeneration> Parameters = new Dictionary<string, FileGeneration>();
        public readonly FileGeneration Return = new FileGeneration();

        public CommentWrapper(FileGeneration fg)
        {
            this._fg = fg;
        }

        public void AddParameter(string name, string comment)
        {
            var fg = new FileGeneration();
            fg.AppendLine(comment);
            Parameters[name] = fg;
        }

        public void Apply(FileGeneration fg)
        {
            if (fg == null) return;
            if (Summary.Count > 0)
            {
                fg.AppendLine("/// <summary>");
                foreach (var line in Summary)
                {
                    fg.AppendLine($"/// {line}");
                }
                fg.AppendLine("/// </summary>");
            }
            foreach (var param in Parameters)
            {
                if (param.Value.Count > 1)
                {
                    fg.AppendLine("/// <param name=\"{param.Key}\">");
                    foreach (var line in param.Value)
                    {
                        fg.AppendLine($"/// {line}");
                    }
                    fg.AppendLine("/// </param>");
                }
                else
                {
                    fg.AppendLine($"/// <param name=\"{param.Key}\">{param.Value[0]}</param>");
                }
            }
            if (Return.Count == 1)
            {
                fg.AppendLine($"/// <returns>{Return[0]}</returns>");
            }
            else if (Return.Count > 0)
            {
                fg.AppendLine("/// <returns>");
                foreach (var line in Return)
                {
                    fg.AppendLine($"/// {line}");
                }
                fg.AppendLine("/// </returns>");
            }
        }

        public void Dispose()
        {
            Apply(_fg);
        }
    }
}
