using Noggog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
#if NETSTANDARD2_0
#else
using System.Reactive.Subjects;
#endif
using System.Text;

namespace Loqui
{
    public class FileGeneration : IEnumerable<string>
    {
        private static readonly string[] NewLineArr = new[] { Environment.NewLine };
        
        public int Depth;
        private readonly List<string> _strings = new List<string>();
        public string DepthStr
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(' ', this.Depth * _depthCount);
                return sb.ToString();
            }
        }
        public bool Empty
        {
            get
            {
                if (this._strings.Count > 1) return false;
                if (this._strings.Count == 0) return true;
                return string.IsNullOrWhiteSpace(this._strings[0]);
            }
        }

#if NETSTANDARD2_0
#else
        // Debug inspection members
        private static readonly Subject<string> _LineAppended = new Subject<string>();
        public static IObservable<string> LineAppended => _LineAppended;
#endif
        private readonly int _depthCount;

        public int Count => this._strings.Count - 1;

        public string this[int index]
        {
            get
            {
                CheckIndex(index);
                return this._strings[index];
            }
            set
            {
                CheckIndex(index);
                if (index == this._strings.Count - 1)
                {
                    this.AppendLine(value);
                }
                else
                {
                    this._strings[index] = value;
                }
            }
        }

        public FileGeneration()
            : this(depthCount: 4)
        {
        }

        public FileGeneration(int depthCount)
        {
            _depthCount = depthCount;
            this.AppendLine();
        }

        public void Insert(int index, string str)
        {
            CheckIndex(index);
            this._strings.Insert(index, str);
        }

        private void CheckIndex(int index)
        {
            if (index >= this._strings.Count - 1 || index < 0)
            {
                throw new IndexOutOfRangeException();
            }
        }

        public void Append(string? str)
        {
            if (str == null) str = string.Empty;
#if NETSTANDARD2_0
#else
            _LineAppended.OnNext(str);
#endif
            if (str.StartsWith(Environment.NewLine))
            {
                this._strings.Add("");
                return;
            }
            string[] split = str.Split(NewLineArr, StringSplitOptions.None);
            split.First(
                (s, first) =>
                {
                    if (this._strings.Count == 0)
                    {
                        this._strings.Add(s);
                    }
                    else
                    {
                        if (first)
                        {
                            this._strings[this._strings.Count - 1] = this._strings[this._strings.Count - 1] + s;
                        }
                        else
                        {
                            this._strings.Add(s);
                        }
                    }
                });
        }

        public void AppendLine()
        {
            Append(Environment.NewLine);
        }

        public void AppendLines(IEnumerable<string?> strs)
        {
            foreach (var str in strs)
            {
                AppendLine(str);
            }
        }

        public void AppendLines(IEnumerable<string?> strs, string delimeter)
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

        public void AppendLine(string? str, bool extraLine = false)
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

        public void AppendItem<T>([MaybeNull] T item, string? name = null)
        {
            if (item == null) return;
            if (name == null)
            {
                AppendLine(item.ToString());
            }
            else
            {
                AppendLine($"{name} => {item}");
            }
        }

        public void AppendItem(IPrintable? item, string? name = null)
        {
            if (item == null) return;
            item.ToString(this, name);
        }

        public void RemoveAt(int index)
        {
            CheckIndex(index);
            this._strings.RemoveAt(index);
        }

        public void Generate(FilePath file, bool onlyIfChanged = true, IFileSystem? fileSystem = null)
        {
            fileSystem ??= IFileSystemExt.DefaultFilesystem;
            var str = GetString();
            if (onlyIfChanged && fileSystem.File.Exists(file))
            {
                var existStr = fileSystem.File.ReadAllText(file.Path);
                if (str.Equals(existStr)) return;
            }

            fileSystem.Directory.CreateDirectory(file.Directory);
            fileSystem.File.WriteAllText(file.Path, str);
        }

        public string GetString()
        {
            return string.Join(Environment.NewLine, this._strings);
        }

        public override string ToString()
        {
            return GetString();
        }

        public IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < this._strings.Count - 1; i++)
            {
                yield return this._strings[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
