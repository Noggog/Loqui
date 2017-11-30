using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypicalDoubleNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            if (!HasRange) return;

            double min, max;
            if (string.IsNullOrWhiteSpace(Min) && string.IsNullOrWhiteSpace(Max))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Min}-{this.Max}");
            }

            if (string.IsNullOrWhiteSpace(Min))
            {
                min = double.MinValue;
                Min = "double.MinValue";
            }
            else if (!double.TryParse(Min, out min))
            {
                throw new ArgumentException($"Value was not convertable to double: {this.Min}");
            }

            if (string.IsNullOrWhiteSpace(Max))
            {
                max = double.MaxValue;
                Max = "double.MaxValue";
            }
            else if (!double.TryParse(Max, out max))
            {
                throw new ArgumentException($"Value was not convertable to double: {this.Max}");
            }

            if (min > max)
            {
                throw new ArgumentException($"Min {min} was greater than max {max}");
            }

            if (!Min.EndsWith("d"))
            {
                Min += "d";
            }

            if (!Max.EndsWith("d"))
            {
                Max += "d";
            }
        }
    }
}
