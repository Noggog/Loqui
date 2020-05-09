using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class PrimitiveType : TypicalTypeGeneration
    {
        public override bool IsEnumerable => false;
        public override bool IsClass => false;

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{accessor.DirectAccess} {(negate ? "!" : "=")}= {rhsAccessor.DirectAccess}";
        }

        public override string GetDefault(bool getter)
        {
            if (this.HasBeenSet)
            {
                return $"default({this.TypeName(getter: getter)}?)";
            }
            else
            {
                return "default";
            }
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
        {
            fg.AppendLine($"if ({(deepCopy ? this.GetTranslationIfAccessor(copyMaskAccessor) : this.SkipCheck(copyMaskAccessor, deepCopy))})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{accessor.DirectAccess} = {rhs};");
            }
        }

        public override string GetDuplicate(Accessor accessor)
        {
            return $"{accessor}";
        }
    }
}
