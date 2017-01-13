using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class TypicalRangedTypeGeneration : TypicalTypeGeneration
    {
        public string Range;
        public bool RangeThrowException;
        public bool HasRange;

        public string RangeMemberName { get { return this.Name + "_Range"; } }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            HasRange = node.TryGetAttribute("range", out Range);
            RangeThrowException = node.GetAttribute<bool>("rangeThrowException", false);
        }

        protected override void GenerateNotifyingCtor(FileGeneration fg)
        {
            if (HasRange)
            {
                fg.AppendLine("protected INotifyingItem<" + TypeName + "> _" + this.Name + " = new NotifyingItemConvertWrapper<" + TypeName + ">(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine("(change) =>");
                    using (new BraceWrapper(fg) { AppendComma = true })
                    {
                        fg.AppendLine($"return TryGet<{this.TypeName}>.Success({this.Name}_Range.PutInRange(change.New));");
                    }
                    if (HasDefault)
                    {
                        fg.AppendLine($"({this.TypeName}){this.Name}_Range.PutInRange({GenerateDefaultValue()}, throwException: false),");
                        fg.AppendLine("markAsSet: false");
                    }
                    else
                    {
                        fg.AppendLine($"({this.TypeName}){this.Name}_Range.PutInRange(default({this.TypeName}), throwException: false),");
                        fg.AppendLine("markAsSet: false");
                    }
                }
                fg.AppendLine(");");
            }
            else
            {
                base.GenerateNotifyingCtor(fg);
            }
        }
    }
}
