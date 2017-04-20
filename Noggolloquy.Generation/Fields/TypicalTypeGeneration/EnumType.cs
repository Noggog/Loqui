using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class EnumType : TypicalGeneration
    {
        public string EnumName;
        public string NameSpace;

        public override string TypeName => EnumName; 

        protected override void GenerateNotifyingCtor(FileGeneration fg, bool notifying)
        {
            fg.AppendLine($"protected readonly {(notifying ? "INotifyingItem" : "IHasBeenSetItem")}<{TypeName}> _{this.Name} = new {(notifying ? "NotifyingItem" : "HasBeenSetItem")}<{TypeName}>(");
            using (new DepthWrapper(fg))
            {
                if (!string.IsNullOrWhiteSpace(this.DefaultValue))
                {
                    using (new LineWrapper(fg))
                    {
                        if (!string.IsNullOrWhiteSpace(NameSpace))
                        {
                            fg.Append($"{NameSpace}.");
                        }
                        fg.Append($"{EnumName}.{this.DefaultValue}");
                    }
                }
            }
            fg.AppendLine(");");
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            EnumName = node.GetAttribute<string>(
                "enumName",
                throwException: true);

            int periodIndex = EnumName.LastIndexOf('.');
            if (periodIndex != -1)
            {
                NameSpace = EnumName.Substring(0, periodIndex);
                EnumName = EnumName.Substring(periodIndex + 1);
            }
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            if (!string.IsNullOrWhiteSpace(NameSpace))
            {
                yield return NameSpace;
            }
        }
    }
}
