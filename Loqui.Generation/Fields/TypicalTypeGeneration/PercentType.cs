using Noggog;
using System;

namespace Loqui.Generation
{
    public class PercentType : DoubleType
    {
        public override Type Type => typeof(Percent);
    }
}
