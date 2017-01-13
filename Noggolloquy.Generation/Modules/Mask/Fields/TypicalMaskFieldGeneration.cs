using System;

namespace Noggolloquy.Generation
{
    public class TypicalMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine("public T " + field.Name + " { get; set; }");
        }
    }
}
