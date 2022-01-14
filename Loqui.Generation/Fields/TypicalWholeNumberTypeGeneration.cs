using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypicalWholeNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            if (!HasRange) return;

            int min, max;
            if (string.IsNullOrWhiteSpace(this.Min) && string.IsNullOrWhiteSpace(this.Max))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Min}-{this.Max}");
            }

            if (string.IsNullOrWhiteSpace(this.Min))
            {
                min = int.MinValue;
                this.Min = $"{TypeName(getter: false)}.MinValue";
            }
            else if (!int.TryParse(this.Min, out min))
            {
                throw new ArgumentException($"Value was not convertable to int: {this.Min}");
            }

            if (string.IsNullOrWhiteSpace(this.Max))
            {
                max = int.MaxValue;
                this.Max = $"{TypeName(getter: false)}.MaxValue";
            }
            else if (!int.TryParse(this.Max, out max))
            {
                throw new ArgumentException($"Value was not convertable to int: {this.Max}");
            }

            if (min > max)
            {
                throw new ArgumentException($"Min {min} was greater than max {max}");
            }
        }
    }
}
