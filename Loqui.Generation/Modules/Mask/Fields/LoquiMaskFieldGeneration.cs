using System;
using static Loqui.Generation.LoquiType;

namespace Loqui.Generation
{
    public class LoquiMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}> {field.Name} {{ get; set; }}");
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<Exception, {loqui.ErrorMaskItemString}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<Exception, {loqui.ErrorMaskItemString}>(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = (MaskItem<Exception, {loqui.ErrorMaskItemString}>)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            if (loqui.RefType == LoquiRefType.Direct)
            {
                if (loqui.SingletonType == SingletonLevel.Singleton)
                {
                    if (loqui.InterfaceType == LoquiInterfaceType.IGetter) return;
                    fg.AppendLine($"public MaskItem<bool, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
                }
            }
            else
            {
                if (loqui.ObjectGeneration == null)
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
