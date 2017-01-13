using System;

namespace Noggolloquy.Generation
{
    public abstract class MaskModuleField
    {
        public abstract void GenerateForField(FileGeneration fg, TypeGeneration field);
    }
}
