﻿using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class EnumXmlTranslation<E> : TypicalXmlTranslation<E>
        where E : struct, IComparable, IConvertible
    {
        public readonly static EnumXmlTranslation<E> Instance = new EnumXmlTranslation<E>();

        protected override E ParseNonNullString(string str)
        {
            if (EnumExt.TryParse(str, out E enumType))
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
