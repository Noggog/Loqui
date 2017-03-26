using System;

namespace Noggolloquy.Generation
{
    public abstract class MaskModuleField
    {
        public abstract void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr);
        public virtual void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            GenerateForField(fg, field, "Exception");
        }
        public abstract void GenerateSetException(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateSetMask(FileGeneration fg, TypeGeneration field);
    }
}
