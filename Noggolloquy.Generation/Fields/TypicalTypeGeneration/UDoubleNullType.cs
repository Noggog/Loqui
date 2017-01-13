using Noggog;
using System;

namespace Noggolloquy.Generation
{
    public class UDoubleNullType : TypicalDoubleNumberTypeGeneration
    {
        public override Type Type
        {
            get
            {
                return typeof(UDouble?);
            }
        }
    }
}
