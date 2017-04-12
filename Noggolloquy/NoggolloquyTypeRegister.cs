using System;

namespace Noggolloquy
{
    public class NoggolloquyTypeRegister
    {
        public readonly ObjectKey ObjectKey;
        public readonly Type Class;
        public readonly Type ErrorMask;
        public readonly string FullName;
        public readonly byte GenericCount;

        public NoggolloquyTypeRegister(
            ObjectKey objectKey,
            Type classType,
            Type errorMask,
            string fullName,
            byte genericCount)
        {
            this.ObjectKey = objectKey;
            this.Class = classType;
            this.ErrorMask = errorMask;
            this.FullName = fullName;
            this.GenericCount = genericCount;
        }
    }
}
