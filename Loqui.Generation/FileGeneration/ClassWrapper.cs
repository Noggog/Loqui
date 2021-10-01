using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui.Generation
{
    public class ClassWrapper : IDisposable
    {
        public enum ObjectType
        {
            @class,
            @struct,
            @interface
        }

        private FileGeneration fg;
        public string Name { get; }
        public PermissionLevel Public = PermissionLevel.@public;
        public bool Partial;
        public bool Abstract;
        public bool Static;
        public string BaseClass;
        public bool New;
        public ObjectType Type = ObjectType.@class;
        public HashSet<string> Interfaces = new();
        public List<string> Wheres = new();
        public List<string> Attributes = new();

        public ClassWrapper(FileGeneration fg, string name)
        {
            this.fg = fg;
            this.Name = name;
        }

        public void Dispose()
        {
            foreach (var attr in Attributes)
            {
                fg.AppendLine(attr);
            }
            var classLine = $"{EnumExt.ToStringFast_Enum_Only<PermissionLevel>(Public)} {(this.Static ? "static " : null)}{(this.New ? "new " : null)}{(this.Abstract ? "abstract " : null)}{(this.Partial ? "partial " : null)}{EnumExt.ToStringFast_Enum_Only<ObjectType>(Type)} {this.Name}";
            var toAdd = this.Interfaces.OrderBy(x => x).ToList();
            if (!string.IsNullOrWhiteSpace(this.BaseClass))
            {
                toAdd.Insert(0, this.BaseClass);
            }
            if (toAdd.Count > 1)
            {
                this.fg.AppendLine($"{classLine} :");
                this.fg.Depth++;
                toAdd.Last(
                    each: (item, last) =>
                    {
                        fg.AppendLine($"{item}{(last ? string.Empty : ",")}");
                    });
                this.fg.Depth--;
            }
            else if (toAdd.Count == 1)
            {
                this.fg.AppendLine($"{classLine} : {toAdd.First()}");
            }
            else
            {
                this.fg.AppendLine(classLine);
            }
            if (Wheres.Count > 0)
            {
                using (new DepthWrapper(fg))
                {
                    foreach (var where in this.Wheres)
                    {
                        fg.AppendLine(where);
                    }
                }
            }
        }
    }
}
