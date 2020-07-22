using System;

namespace Loqui.Generation
{
    public class StringType : PrimitiveType
    {
        public override Type Type(bool getter) => typeof(string);
        public override bool IsReference => true;

        protected override string GenerateDefaultValue() => $"\"{this.DefaultValue}\"";

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}string.Equals({accessor.Access}, {rhsAccessor.Access})";
        }

        public override string GetDefault(bool getter)
        {
            if (this.Nullable)
            {
                return "default(string?)";
            }
            else
            {
                return "string.Empty";
            }
        }
    }
}
