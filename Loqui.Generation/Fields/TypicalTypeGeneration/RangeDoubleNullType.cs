using Noggog;
using System;

namespace Loqui.Generation
{
    public class RangeDoubleNullType : RangeDoubleType
    {
        public override Type Type(bool getter) => typeof(RangeDouble?);
    }
}
