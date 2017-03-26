using System;

namespace Noggolloquy
{
    public class NoggolloquyTypeRegister
    {
        public readonly ObjectKey ObjectKey;
        public readonly Type Class;
        public readonly string FullName;
        public readonly byte GenericCount;

        public NoggolloquyTypeRegister(
            ObjectKey objectKey,
            Type classType,
            string fullName,
            byte genericCount)
        {
            this.ObjectKey = objectKey;
            this.Class = classType;
            this.FullName = fullName;
            this.GenericCount = genericCount;
        }
    }
}
