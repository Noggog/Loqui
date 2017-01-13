using System;

namespace Noggolloquy.Generation
{
    public class Dimension2DType : TypicalGeneration
    {
        public override string TypeName
        {
            get { return "Dimension2D"; }
        }

        protected override string GenerateDefaultValue()
        {
            return "new Dimension2D(" + DefaultValue + ")";
        }
    }
}
