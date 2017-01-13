using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class RangeIntType : TypicalWholeNumberTypeGeneration
    {
        string defaultFrom, defaultTo;

        public override Type Type
        {
            get { return typeof(RangeInt); }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            if (!HasDefault) return;

            string[] split = this.DefaultValue.Split('-');
            if (split.Length != 2)
            {
                throw new ArgumentException("Range field was not properly split with -");
            }

            defaultFrom = split[0];
            defaultTo = split[1];
        }

        protected override string GenerateDefaultValue()
        {
            return "new RangeInt(" + this.defaultFrom + ", " + this.defaultTo + ")";
        }
    }
}
