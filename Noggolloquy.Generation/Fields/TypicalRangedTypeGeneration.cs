using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalRangedTypeGeneration : TypicalTypeGeneration
    {
        public string Range;
        public bool RangeThrowException;
        public bool HasRange;

        public string RangeMemberName => $"{this.Name}_Range";

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            HasRange = node.TryGetAttribute("range", out Range);
            RangeThrowException = node.GetAttribute<bool>("rangeThrowException", false);
        }
    }
}
