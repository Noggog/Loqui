using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Text;

namespace Loqui
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
                sb.Append(' ', this.Depth * 4);
                return sb.ToString();
            }
        }
        public bool Empty
        {
            get
            {
                if (this.Strings.Count > 1) return false;
                if (this.Strings.Count == 0) return true;
                return string.IsNullOrWhiteSpace(this.Strings[0]);
            }
        }

        // Debug inspection members
        private static readonly Subject<string> _LineAppended = new Subject<string>();
        public static IObservable<string> LineAppended => _LineAppended;

        public FileGeneration()
        {
            this.AppendLine();
        }

        public void Append(string str)
        {
#if DEBUG
            _LineAppended.OnNext(str);
#endif
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

        public void AppendLines(IEnumerable<string> strs)
        {
            foreach (var str in strs)
            {
                AppendLine(str);
            }
        }

        public void AppendLines(IEnumerable<string> strs, string delimeter)
        {
            foreach (var str in strs.IterateMarkLast())
            {
                if (str.Last)
                {
                    AppendLine(str.Item);
                }
                else
                {
                    Append(str.Item);
                    Append(delimeter);
                }
            }
        }

        public void AppendLine(string str, bool extraLine = false)
        {
            using (new LineWrapper(this))
            {
                Append(str);
            }

            if (extraLine)
            {
                AppendLine();
            }
        }

        public void Generate(string path, bool onlyIfChanged = true)
        {
            Generate(
                new FileInfo(path),
                onlyIfChanged);
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

        public override string ToString()
        {
            return GetString();
        }
    }
}
