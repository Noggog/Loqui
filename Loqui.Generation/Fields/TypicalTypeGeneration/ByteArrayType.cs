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
        public override bool IsEnumerable => true;
        public override bool IsReference => true;

        public override void GenerateForClass(FileGeneration fg)
        {
            base.GenerateForClass(fg);
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}ByteExt.EqualsFast({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }

        public override string GetNewForNonNullable()
        {
            return $"new byte[{this.Length.Value}]";
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Length = node.GetAttribute<int?>("length", null);
            if (this.Length == null
                && this.Singleton != SingletonLevel.None)
            {
                throw new ArgumentException($"Cannot have a byte array with an undefined length that is not nullable. {this.ObjectGen.Name} {this.Name}");
            }

            if (this.Length != null
                && this.Singleton == SingletonLevel.None)
            {
                throw new ArgumentException($"Cannot have a byte array with a length that is nullable.  Doesn't apply. {this.ObjectGen.Name} {this.Name}");
            }
        }
    }
}
