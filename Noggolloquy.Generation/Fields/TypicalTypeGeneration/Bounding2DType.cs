using System;

namespace Noggolloquy.Generation
{
    public class Bounding2DType : TypicalGeneration
    {
        public override string TypeName
        {
            get { return "Bounding2D"; }
        }

        protected override string GenerateDefaultValue()
        {
            return "new Bounding2D(" + DefaultValue + ")";
        }
    }
}
