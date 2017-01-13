using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalFloatNumberTypeGeneration : TypicalRangedTypeGeneration
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

            float minFloat, maxFloat;
            if (string.IsNullOrWhiteSpace(defaultFrom) && string.IsNullOrWhiteSpace(defaultTo))
            {
                throw new ArgumentException("Value was not convertable to range: " + this.Range);
            }

            if (string.IsNullOrWhiteSpace(defaultFrom))
            {
                minFloat = float.MinValue;
                defaultFrom = "float.MinValue";
            }
            else if (!float.TryParse(defaultFrom, out minFloat))
            {
                throw new ArgumentException("Value was not convertable to float: " + split[0]);
            }

            if (string.IsNullOrWhiteSpace(defaultTo))
            {
                maxFloat = float.MaxValue;
                defaultTo = "float.MaxValue";
            }
            else if (!float.TryParse(defaultTo, out maxFloat))
            {
                throw new ArgumentException("Value was not convertable to float: " + split[1]);
            }

            if (minFloat > maxFloat)
            {
                throw new ArgumentException("Min " + minFloat + " was greater than max " + maxFloat);
            }

            if (!defaultFrom.EndsWith("f"))
            {
                defaultFrom += "f";
            }

            if (!defaultTo.EndsWith("f"))
            {
                defaultTo += "f";
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            base.GenerateForClass(fg);

            if (this.HasRange)
            {
                fg.AppendLine("public static RangeFloat " + RangeMemberName + " = new RangeFloat(" + defaultFrom + ", " + defaultTo + ");");
            }
        }
    }
}
