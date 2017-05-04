using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalFloatNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            if (!HasRange) return;

            float minFloat, maxFloat;
            if (string.IsNullOrWhiteSpace(Min) && string.IsNullOrWhiteSpace(Max))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Min}-{this.Max}");
            }

            if (string.IsNullOrWhiteSpace(Min))
            {
                minFloat = float.MinValue;
                Min = "float.MinValue";
            }
            else if (!float.TryParse(Min, out minFloat))
            {
                throw new ArgumentException($"Value was not convertable to float: {this.Min}");
            }

            if (string.IsNullOrWhiteSpace(Max))
            {
                maxFloat = float.MaxValue;
                Max = "float.MaxValue";
            }
            else if (!float.TryParse(Max, out maxFloat))
            {
                throw new ArgumentException($"Value was not convertable to float: {this.Max}");
            }

            if (minFloat > maxFloat)
            {
                throw new ArgumentException($"Min {minFloat} was greater than max {maxFloat}");
            }

            if (!Min.EndsWith("f"))
            {
                Min += "f";
            }

            if (!Max.EndsWith("f"))
            {
                Max += "f";
            }
        }
    }
}
