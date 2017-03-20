using Noggog;
using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class RangeDoubleType : TypicalDoubleNumberTypeGeneration
    {
        string defaultFrom, defaultTo;

        public override Type Type
        {
            get { return typeof(RangeDouble); }
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
            return "new RangeDouble(" + this.defaultFrom + ", " + this.defaultTo + ")";
        }
    }
}
