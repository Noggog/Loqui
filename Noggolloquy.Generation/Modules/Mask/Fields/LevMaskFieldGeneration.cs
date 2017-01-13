using System;

namespace Noggolloquy.Generation
{
    public class LevMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field)
        {
            LevType lev = field as LevType;
            fg.AppendLine($"public MaskItem<T, {lev.GenerateMaskString("T")}> {field.Name} {{ get; set; }}");
        }
    }
}
