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
    }
}
