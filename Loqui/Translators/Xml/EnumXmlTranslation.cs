using Noggog;
using System;

namespace Loqui.Xml
{
    public class EnumXmlTranslation<E> : PrimitiveXmlTranslation<E>
        where E : struct, IComparable, IConvertible
    {
        public readonly static EnumXmlTranslation<E> Instance = new EnumXmlTranslation<E>();

        protected override E ParseNonNullString(string str)
        {
            if (Enum.TryParse(str, out E enumType))
            {
                return enumType;
            }
            throw new ArgumentException($"Could not convert to {NullableName}");
        }

        protected override string GetItemStr(E item)
        {
            return EnumExt.ToStringFast_Enum_Only(item);
        }
    }
}
