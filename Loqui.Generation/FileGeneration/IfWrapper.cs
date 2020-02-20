using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class IfWrapper : IDisposable
    {
        FileGeneration fg;
        bool first;
        public List<(string str, bool wrap)> Checks = new List<(string str, bool wrap)>();
        bool ands;
        public bool Empty => Checks.Count == 0;

        public IfWrapper(FileGeneration fg, bool ANDs, bool first = true)
        {
            this.ands = ANDs;
            this.first = first;
            this.fg = fg;
        }

        public void Add(string str, bool wrapInParens = false)
        {
            this.Checks.Add((str, wrapInParens));
        }

        private string Get(int index)
        {
            var item = Checks[index];
            if (!item.wrap || Checks.Count <= 1)
            {
                return item.str;
            }
            return $"({item.str})";
        }

        public void Dispose()
        {
            if (Checks.Count == 0) return;
            using (var line = new LineWrapper(fg))
            {
                if (!first)
                {
                    fg.Append("else ");
                }
                fg.Append("if (");
                fg.Append(Get(0));
                if (Checks.Count == 1)
                {
                    fg.Append(")");
                    return;
                }
            }
            using (new DepthWrapper(fg))
            {
                for (int i = 1; i < Checks.Count; i++)
                {
                    using (new LineWrapper(fg))
                    {
                        if (this.ands)
                        {
                            fg.Append("&& ");
                        }
                        else
                        {
                            fg.Append("|| ");
                        }
                        fg.Append(Get(i));
                        if (i == Checks.Count - 1)
                        {
                            fg.Append(")");
                        }
                    }
                }
            }
        }
    }
}
