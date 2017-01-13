using System;

namespace Noggolloquy.Generation
{
    public class DoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type
        {
            get { return typeof(double); }
        }
    }
}
