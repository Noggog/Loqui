using System;

namespace Noggolloquy.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        private string GetMaskString(IDictType dictType, string typeStr)
        {
            NoggType keyNoggType = dictType.KeyTypeGen as NoggType;
            NoggType valueNoggType = dictType.ValueTypeGen as NoggType;
            string valueStr = $"{(valueNoggType == null ? typeStr : $"MaskItem<{typeStr}, {valueNoggType.RefGen.Obj.GetMaskString(typeStr)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{(keyNoggType == null ? typeStr : $"MaskItem<{typeStr}, {keyNoggType.RefGen.Obj.GetMaskString(typeStr)}>")}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return $"MaskItem<{typeStr}, IEnumerable<{itemStr}>>";
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            fg.AppendLine($"public {GetMaskString(field as IDictType, typeStr)} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as IDictType, "Exception")}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetMaskString(field as IDictType, "Exception")})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            DictType dictType = field as DictType;
            NoggType keyNoggType = dictType.KeyTypeGen as NoggType;
            NoggType valueNoggType = dictType.ValueTypeGen as NoggType;

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (keyNoggType == null && valueNoggType == null)
                    {
                        fg.AppendLine($"public bool {field.Name};");
                    }
                    else if (keyNoggType != null && valueNoggType != null)
                    {
                        fg.AppendLine($"public MaskItem<bool, KeyValuePair<({nameof(RefCopyType)} Type, {keyNoggType.RefGen.Obj.CopyMask} Mask), ({nameof(RefCopyType)} Type, {valueNoggType.RefGen.Obj.CopyMask} Mask)>> {field.Name};");
                    }
                    else
                    {
                        NoggType nogg = keyNoggType ?? valueNoggType;
                        fg.AppendLine($"public MaskItem<bool, ({nameof(RefCopyType)} Type, {nogg.RefGen.Obj.CopyMask} Mask)> {field.Name};");
                    }
                    break;
                case DictMode.KeyedValue:
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {valueNoggType.RefGen.Obj.CopyMask}> {field.Name};");
                    break;
                default:
                    break;
            }
        }
    }
}
