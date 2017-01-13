using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class UDoubleType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type
        {
            get
            {
                return typeof(UDouble);
            }
        }
    }
}
