using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class PrimitiveType : TypicalTypeGeneration
    {
        public override bool IsNullable() => this.TypeName(getter: false).EndsWith("?");
        public override bool IsEnumerable => false;
        public override bool IsClass => false;

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{accessor.DirectAccess} {(negate ? "!" : "=")}= {rhsAccessor.DirectAccess}";
        }

        public override string GetDefault()
        {
            if (this.HasBeenSet)
            {
                return $"default({this.TypeName(getter: true)}?)";
            }
            else
            {
                return "default";
            }
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, string rhsAccessorPrefix, string copyMaskAccessor, bool protectedMembers, bool deepCopy)
        {
            fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)};");
        }
    }
}
