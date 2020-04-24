using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public class CommaWrapper : IDisposable
    {
        FileGeneration fg;
        string delimiter = ",";
        List<string> items = new List<string>();

        public CommaWrapper(FileGeneration fg, string delimiter = ",")
        {
            this.fg = fg;
            this.delimiter = delimiter;
        }

        public void Add(string item)
        {
            this.items.Add(item);
        }

        public void Add(params string[] items)
        {
            this.items.AddRange(items);
        }

        public void Add(Action<FileGeneration> generator)
        {
            var gen = new FileGeneration();
            generator(gen);
            if (gen.Empty) return;
            Add(gen.ToArray());
        }

        public void Dispose()
        {
            foreach (var item in this.items.IterateMarkLast())
            {
                if (item.Last)
                {
                    fg.AppendLine(item.Item);
                }
                else
                {
                    using (new LineWrapper(fg))
                    {
                        fg.Append(item.Item);
                        fg.Append(delimiter);
                    }
                }
            }
        }
    }
}
