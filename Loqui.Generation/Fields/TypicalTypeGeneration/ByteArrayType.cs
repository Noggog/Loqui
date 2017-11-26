using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class ByteArrayType : ClassType
    {
        public int? Length;
        public override Type Type => typeof(byte[]);

        public override void GenerateForClass(FileGeneration fg)
        {
            base.GenerateForClass(fg);
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"if (!{this.Name}.EqualsFast({rhsAccessor}.{this.Name})) return false;");
        }
        
        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{retAccessor} = {accessor}.EqualsFast({rhsAccessor});");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor}.Equals({rhsAccessor}, (l, r) => l.EqualsFast(r));");
            }
        }

        public override string GetNewForNonNullable()
        {
            return $"new byte[{this.Length.Value}]";
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            this.Length = node.GetAttribute<int?>("length", null);
            if (this.Length == null
                && !this.Nullable)
            {
                throw new ArgumentException("Cannot have a byte array with an undefined length that is not nullable.");
            }

            if (this.Length != null
                & this.Nullable)
            {
                throw new ArgumentException("Cannot have a byte array with a length that is nullable.  Doesn't apply.");
            }
        }
    }
}
