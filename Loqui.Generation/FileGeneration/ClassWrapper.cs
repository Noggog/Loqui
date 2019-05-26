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
        public string BaseClass;
        public ObjectType Type = ObjectType.@class;
        public HashSet<string> Interfaces = new HashSet<string>();

        public ClassWrapper(FileGeneration fg, string name)
        {
            this.fg = fg;
            this.Name = name;
        }

        public void Dispose()
        {
            var classLine = $"{EnumExt.ToStringFast_Enum_Only<PermissionLevel>(Public)} {(this.Abstract ? "abstract " : null)}{(this.Partial ? "partial " : null)}{EnumExt.ToStringFast_Enum_Only<ObjectType>(Type)} {this.Name}";
            var toAdd = this.Interfaces.ToList();
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
        }
    }
}
