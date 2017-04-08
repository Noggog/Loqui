using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalDoubleNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        string defaultFrom;
        string defaultTo;

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            if (!HasRange) return;
            string[] split = this.Range.Split('-');
            if (split.Length != 2)
            {
                throw new ArgumentException("Range field was not properly split with -");
            }

            defaultFrom = split[0];
            defaultTo = split[1];

            double min, max;
            if (string.IsNullOrWhiteSpace(defaultFrom) && string.IsNullOrWhiteSpace(defaultTo))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Range}");
            }

            if (string.IsNullOrWhiteSpace(defaultFrom))
            {
                min = double.MinValue;
                defaultFrom = "double.MinValue";
            }
            else if (!double.TryParse(defaultFrom, out min))
            {
                throw new ArgumentException($"Value was not convertable to double: {split[0]}");
            }

            if (string.IsNullOrWhiteSpace(defaultTo))
            {
                max = double.MaxValue;
                defaultTo = "double.MaxValue";
            }
            else if (!double.TryParse(defaultTo, out max))
            {
                throw new ArgumentException($"Value was not convertable to double: {split[1]}");
            }

            if (min > max)
            {
                throw new ArgumentException($"Min {min} was greater than max {max}");
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            base.GenerateForClass(fg);

            if (this.HasRange)
            {
                fg.AppendLine($"public static RangeDouble {RangeMemberName} = new RangeDouble({defaultFrom}, {defaultTo});");
            }
        }
    }
}
