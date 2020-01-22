using Loqui.Internal;
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
        where E : struct, Enum
    {
        public readonly static EnumXmlTranslation<E> Instance = new EnumXmlTranslation<E>();
        public readonly static bool IsFlagsEnum = EnumExt<E>.IsFlagsEnum();
        public readonly static string UnknownString = "Unknown";

        private bool TryParseToEnum(string str, out E value)
        {
            if (Enum.TryParse(str, out E enumType))
            {
                value = enumType;
                return true;
            }
            else if (int.TryParse(str, out var i)
                && EnumExt.TryParse<E>(i, out enumType))
            {
                value = enumType;
                return true;
            }
            value = default;
            return false;
        }

        protected override bool Parse(string str, out E value, ErrorMaskBuilder? errorMask)
        {
            if (TryParseToEnum(str, out value)) return true;
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"Could not convert to {ElementName}"));
            value = default;
            return false;
        }

        public override bool Parse(XElement root, out E value, ErrorMaskBuilder? errorMask)
        {
            if (!IsFlagsEnum)
            {
                return base.Parse(root, out value, errorMask);
            }
            if (root.TryGetAttribute<bool>("null", out var isNull)
                && isNull)
            {
                value = default;
                return true;
            }
            value = default(E);
            foreach (var item in root.Elements())
            {
                if (TryParseToEnum(item.Name.LocalName, out E subEnum))
                {
                    value = value.Or(subEnum);
                }
                else if (item.Name.LocalName.StartsWith(UnknownString)
                    && int.TryParse(item.Name.LocalName.Substring(UnknownString.Length), out var num))
                {
                    int i = Convert.ToInt32(value);
                    i += 1 << num;
                    value = (E)(object)i;
                }
                else
                {
                    errorMask.ReportExceptionOrThrow(
                        new ArgumentException($"Could not convert to {ElementName}"));
                }
            }
            return true;
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
                return;
            }
            // Write normal values
            Enum e = item.Value as Enum;
            int intVal = Convert.ToInt32(e);
            foreach (var eType in EnumExt<E>.Values)
            {
                if (e.HasFlag(eType))
                {
                    node.Add(new XElement(eType.ToStringFast_Enum_Only()));
                    int intRhs = Convert.ToInt32(eType);
                    intVal -= intRhs;
                }
            }
            // Write unexpected values
            if (intVal == 0) return;
            for (int i = 0; i < 32; i++)
            {
                var masked = intVal & 1;
                if (masked == 1)
                {
                    node.Add(new XElement(UnknownString + i));
                }
                intVal = intVal >> 1;
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
