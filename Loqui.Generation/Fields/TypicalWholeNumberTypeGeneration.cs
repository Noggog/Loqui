using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypicalWholeNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        string defaultFrom, defaultTo;

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            if (!HasRange) return;

            defaultFrom = this.Min;
            defaultTo = this.Max;

            int min, max;
            if (string.IsNullOrWhiteSpace(defaultFrom) && string.IsNullOrWhiteSpace(defaultTo))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Min}-{this.Max}");
            }

            if (string.IsNullOrWhiteSpace(defaultFrom))
            {
                min = int.MinValue;
                defaultFrom = "int.MinValue";
            }
            else if (!int.TryParse(defaultFrom, out min))
            {
                throw new ArgumentException($"Value was not convertable to int: {this.Min}");
            }

            if (string.IsNullOrWhiteSpace(defaultTo))
            {
                max = int.MinValue;
                defaultTo = "int.MaxValue";
            }
            else if (!int.TryParse(defaultTo, out max))
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
