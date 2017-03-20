using System;

namespace Noggolloquy.Generation
{
    public class TypicalMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            fg.AppendLine($"public {typeStr} {field.Name} {{ get; set; }}");
        }
    }
}
