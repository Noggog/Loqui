using System;

namespace Loqui.Generation
{
    public class StringType : TypicalTypeGeneration
    {
        public override Type Type => typeof(string);

        protected override string GenerateDefaultValue() => $"\"{this.DefaultValue}\"";
    }
}
