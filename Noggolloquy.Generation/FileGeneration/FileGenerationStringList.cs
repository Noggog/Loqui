using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class FileGenerationStringList : FileGeneration
    {
        public List<string> Strings = new List<string>();

        public override void Append(string str)
        {
            if (str.Length == 1 && str[0] == '\n')
            {
                Strings.Add("");
                return;
            }
            string[] split = str.Split('\n');
            split.First(
                (s, first) =>
                {
                    if (Strings.Count == 0)
                    {
                        Strings.Add(s);
                    }
                    else
                    {
                        if (first)
                        {
                            Strings[Strings.Count - 1] = Strings[Strings.Count - 1] + s;
                        }
                        else
                        {
                            Strings.Add(s);
                        }
                    }
                });
        }

        public override string GetString()
        {
            return string.Join("\n", Strings);
        }
    }
}
