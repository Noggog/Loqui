using System;
using static Noggolloquy.Generation.NoggType;

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
            fg.AppendLine($"public MaskItem<Exception, {nogg.ErrorMaskItemString}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<Exception, {nogg.ErrorMaskItemString}>(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            fg.AppendLine($"this.{field.Name} = (MaskItem<Exception, {nogg.ErrorMaskItemString}>)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            NoggType nogg = field as NoggType;
            if (nogg.RefType == NoggRefType.Direct)
            {
                if (nogg.SingletonType == SingletonLevel.Singleton)
                {
                    if (nogg.InterfaceType == NoggInterfaceType.IGetter) return;
                    fg.AppendLine($"public MaskItem<bool, {nogg.RefGen.Obj.CopyMask}> {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {nogg.RefGen.Obj.CopyMask}> {field.Name};");
                }
            }
            else
            {
                if (nogg.ObjectGeneration == null)
                {
                    fg.AppendLine($"public {nameof(GetterCopyOption)} {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
                }
            }
        }
    }
}
