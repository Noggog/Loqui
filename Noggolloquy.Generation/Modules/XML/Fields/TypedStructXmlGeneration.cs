using System;

namespace Noggolloquy.Generation
{
    public class TypedStructXmlGeneration<T> : StructTypeXmlGeneration
    {
        public TypedStructXmlGeneration()
            : base(typeof(T).GetName())
        {
        }
    }
}
