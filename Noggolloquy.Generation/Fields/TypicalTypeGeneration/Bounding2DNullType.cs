using System;

namespace Noggolloquy.Generation
{
    public class Bounding2DNullType : Bounding2DType
    {
        public override string TypeName
        {
            get { return "Bounding2D?"; }
        }
    }
}
