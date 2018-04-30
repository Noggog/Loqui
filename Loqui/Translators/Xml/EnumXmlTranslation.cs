using Noggog;
using Noggog.Xml;
using System;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class EnumXmlTranslation<E> : PrimitiveXmlTranslation<E>
        where E : struct, IComparable, IConvertible
    {
        public readonly static EnumXmlTranslation<E> Instance = new EnumXmlTranslation<E>();
        public readonly static bool IsFlagsEnum = EnumExt<E>.IsFlagsEnum();

        protected override E ParseNonNullString(string str)
        {
            if (Enum.TryParse(str, out E enumType))
            {
                return enumType;
            }
            else if (int.TryParse(str, out var i)
                && EnumExt.TryParse<E>(i, out enumType))
            {
                return enumType;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }

        protected override E? ParseValue(XElement root)
        {
            if (!IsFlagsEnum)
            {
                return base.ParseValue(root);
            }
            if (root.TryGetAttribute<bool>("null", out var isNull))
            {
                if (isNull)
                {
                    return null;
                }
            }
            foreach (var child in root.Elements())
            {
            }
            throw new NotImplementedException();
        }

        protected override void WriteValue(XElement node, E? item)
        {
            if (!IsFlagsEnum)
            {
                base.WriteValue(node, item);
                return;
            }
            if (!item.HasValue)
            {
                node.SetAttributeValue("null", "true");
            }
            Enum e = item.Value as Enum;
            foreach (var eType in EnumExt<E>.Values)
            {
                if (e.HasFlag(eType as Enum))
                {
                    node.Add(new XElement(eType.ToStringFast_Enum_Only()));
                }
            }
        }

        protected override string GetItemStr(E item)
        {
            IConvertible cv = (IConvertible)item;
            var i = cv.ToInt32(CultureInfo.InvariantCulture);
            if (EnumExt.TryToStringFast_Enum_Only<E>(i, out var str))
            {
                return str;
            }
            else
            {
                return i.ToString();
            }
        }
    }
}
