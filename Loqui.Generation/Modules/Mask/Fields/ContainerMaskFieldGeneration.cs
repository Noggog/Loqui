using System;

namespace Loqui.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public static string GetListString(ContainerType listType, string valueStr)
        {
            LoquiType loquiType = listType.SubTypeGeneration as LoquiType;
            string listStr;
            if (loquiType == null)
            {
                listStr = $"IEnumerable<{valueStr}>";
            }
            else
            {
                listStr = $"IEnumerable<{loquiType.RefGen.Obj.ErrorMask}>";
            }
            return listStr;
        }

        public static string GetMaskString(ContainerType listType, string valueStr)
        {
            return $"MaskItem<{valueStr}, {GetListString(listType, valueStr)}>";
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            fg.AppendLine($"public {GetMaskString(field as ContainerType, valueStr)} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as ContainerType, "Exception")}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetMaskString(field as ContainerType, "Exception")})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui)
            {
                fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
            }
        }
    }
}
