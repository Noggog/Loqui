using System;

namespace Noggolloquy.Generation
{
    public class NoggMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"public MaskItem<{typeStr}, {nogg.GenerateMaskString(typeStr)}> {field.Name} {{ get; set; }}");
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"public MaskItem<Exception, {nogg.GenerateErrorMaskItemString()}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<Exception, {nogg.GenerateErrorMaskItemString()}>(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"this.{field.Name} = (MaskItem<Exception, {nogg.GenerateErrorMaskItemString()}>)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"public MaskItem<{nameof(CopyType)}, {nogg.ObjectGen.CopyMask}> {field.Name};");
        }
    }
}
