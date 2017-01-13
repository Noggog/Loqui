using System;

namespace Noggolloquy.Generation
{
    public class Dimension2DNullType : Dimension2DType
    {
        public override string TypeName
        {
            get { return "Dimension2D?"; }
        }
    }
}
