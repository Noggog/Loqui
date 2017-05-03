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
