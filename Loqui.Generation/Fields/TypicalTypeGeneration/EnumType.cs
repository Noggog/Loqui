using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class EnumType : PrimitiveType
    {
        public string EnumName;
        public bool Nullable;

        public override string TypeName => $"{EnumName}{(Nullable ? "?" : string.Empty)}";
        public string NoNullTypeName => $"{EnumName}";
        public override Type Type => throw new NotImplementedException();

        public EnumType()
        {
        }

        public EnumType(bool nullable)
        {
            this.Nullable = nullable;
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            EnumName = node.GetAttribute<string>(
                "enumName",
                throwException: true);
        }
    }
}
