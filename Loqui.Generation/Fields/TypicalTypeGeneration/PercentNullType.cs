using Noggog;
using System;

namespace Loqui.Generation
{
    public class PercentNullType : DoubleType
    {
        public override Type Type => typeof(Percent?);
    }
}
