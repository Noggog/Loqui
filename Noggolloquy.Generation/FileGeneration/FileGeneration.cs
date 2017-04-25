using System;
using System.IO;
using System.Text;

namespace Noggolloquy.Generation
{
    public class FileGeneration
    {
        public int Depth;
        private StringBuilder sb = new StringBuilder();
        public bool AddCommasToLines;

        public void Append(string str)
        {
            if (str.Contains("item.RefSetter.CopyFieldsFrom"))
            {
                int wer = 23;
                wer++;
            }
            sb.Append((AddCommasToLines ? ", " : string.Empty) + str);
        }

        public void AppendLine()
        {
            sb.Append("\n");
        }

        public void AppendLine(string str, bool extraLine = false)
        {
            if (str.Contains("switch (copyMask?.Ref.Overall ?? CopyType.Reference)"))
            {
                int wer = 23;
                wer++;
            }
            using (new LineWrapper(this))
            {
                sb.Append(str);
            }

            if (extraLine)
            {
                AppendLine();
            }
        }

        public void Generate(FileInfo file, bool onlyIfChanged = true)
        {
            var str = sb.ToString();
            file.Refresh();
            if (onlyIfChanged && file.Exists)
            {
                var existStr = File.ReadAllText(file.FullName);
                if (str.Equals(existStr)) return;
            }
            file.Directory.Create();
            File.WriteAllText(file.FullName, str);
        }
    }
}
