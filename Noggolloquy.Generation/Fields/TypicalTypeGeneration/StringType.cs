using System;

namespace Noggolloquy.Generation
{
    public class StringType : TypicalTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(string); }
        }

        protected override string GenerateDefaultValue()
        {
            return $"\"{this.DefaultValue}\"";
        }
    }
}
