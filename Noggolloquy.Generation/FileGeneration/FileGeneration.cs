using System;
using System.IO;
using System.Text;

namespace Noggolloquy.Generation
{
    public abstract class FileGeneration
    {
        public int Depth;

        public abstract void Append(string str);

        public void AppendLine()
        {
            Append("\n");
        }

        public void AppendLine(string str, bool extraLine = false)
        {
            if (str.Equals("if (copyMask?.RefDict.Overall ?? true)"))
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

        public abstract string GetString();
    }
}
