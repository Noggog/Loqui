using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Noggolloquy.Generation
{
    public class FileGeneration
    {
        public int Depth;
        public List<string> Strings = new List<string>();
        public string DepthStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < this.Depth; i++)
                {
                    sb.Append("    ");
                }
                return sb.ToString();
            }
        }

        public void Append(string str)
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

        public void AppendLine()
        {
            Append("\n");
        }

        public void AppendLine(string str, bool extraLine = false)
        {
            if (str.Equals("errorMask().SetNthException(52, ex);"))
            {
                int wer = 23;
                wer++;
            }

            using (new LineWrapper(this))
            {
                Append(str);
            }

            if (extraLine)
            {
                AppendLine();
            }
        }

        public void Generate(FileInfo file, bool onlyIfChanged = true)
        {
            var str = GetString();
            file.Refresh();
            if (onlyIfChanged && file.Exists)
            {
                var existStr = File.ReadAllText(file.FullName);
                if (str.Equals(existStr)) return;
            }
            file.Directory.Create();
            File.WriteAllText(file.FullName, str);
        }
        
        public string GetString()
        {
            return string.Join("\n", Strings);
        }
    }
}
