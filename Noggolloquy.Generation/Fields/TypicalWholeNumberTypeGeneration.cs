using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalWholeNumberTypeGeneration : TypicalRangedTypeGeneration
    {
        string defaultFrom, defaultTo;

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

            int min, max;
            if (string.IsNullOrWhiteSpace(defaultFrom) && string.IsNullOrWhiteSpace(defaultTo))
            {
                throw new ArgumentException($"Value was not convertable to range: {this.Range}");
            }

            if (string.IsNullOrWhiteSpace(defaultFrom))
            {
                min = int.MinValue;
                defaultFrom = "int.MinValue";
            }
            else if (!int.TryParse(defaultFrom, out min))
            {
                throw new ArgumentException($"Value was not convertable to int: {split[0]}");
            }

            if (string.IsNullOrWhiteSpace(defaultTo))
            {
                max = int.MinValue;
                defaultTo = "int.MaxValue";
            }
            else if (!int.TryParse(defaultTo, out max))
            {
                throw new ArgumentException($"Value was not convertable to int: {split[1]}");
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
                string[] split = this.Range.Split('-');
                fg.AppendLine($"public static RangeInt {RangeMemberName} = new RangeInt({split[0]}, {split[1]});");
            }
        }
    }
}
