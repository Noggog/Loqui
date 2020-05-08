using Noggog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class EnumType : PrimitiveType
    {
        public string EnumName;
        public bool Nullable;

        public override string TypeName(bool getter, bool needsCovariance = false) => $"{EnumName}{(Nullable ? "?" : string.Empty)}";
        public string NoNullTypeName => $"{EnumName}";
        public override Type Type(bool getter) => throw new NotImplementedException();
        public override bool IsIEquatable => false;

        public EnumType()
        {
        }

        public EnumType(bool nullable)
        {
            this.Nullable = nullable;
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            EnumName = node.GetAttribute<string>(
                Constants.ENUM_NAME,
                throwException: true);
        }
    }
}
