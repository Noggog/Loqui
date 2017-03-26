using System;

namespace Noggolloquy.Generation
{
    public class LevMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            LevType lev = field as LevType;
            fg.AppendLine($"public MaskItem<{typeStr}, {lev.GenerateMaskString(typeStr)}> {field.Name} {{ get; set; }}");
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            LevType lev = field as LevType;
            fg.AppendLine($"public MaskItem<Exception, {lev.GenerateErrorMaskItemString()}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            throw new NotImplementedException();
        }
    }
}
