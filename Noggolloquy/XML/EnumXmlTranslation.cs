using Noggog;
using System;

namespace Noggolloquy
{
    public class EnumXmlTranslation<E> : TypicalXmlTranslation<E>
        where E : struct, IComparable, IConvertible
    {
        public readonly static EnumXmlTranslation<E> Instance = new EnumXmlTranslation<E>();

        public override TryGet<E?> ParseNonNullString(string str)
        {
            E enumType;
            if (EnumExt.TryParse(str, out enumType))
            {
                return TryGet<E?>.Success(enumType);
            }
            return TryGet<E?>.Failure($"Could not convert to {ElementName}");
        }

        public override string GetItemStr(E? item)
        {
            if (!item.HasValue) return null;

            return EnumExt.ToStringFast_Enum_Only(item.Value);
        }
    }
}
