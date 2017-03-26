using System;

namespace Noggolloquy.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            IDictType dictType = field as IDictType;
            NoggType keyNoggType = dictType.KeyTypeGen as NoggType;
            NoggType valueNoggType = dictType.ValueTypeGen as NoggType;
            string valueStr = $"{(valueNoggType == null ? typeStr : $"MaskItem<{typeStr}, {valueNoggType.RefGen.Obj.GetMaskString(typeStr)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{(keyNoggType == null ? typeStr : $"MaskItem<{typeStr}, {keyNoggType.RefGen.Obj.GetMaskString(typeStr)}")}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            fg.AppendLine($"public MaskItem<{typeStr}, Lazy<List<{itemStr}>>> {field.Name} = new MaskItem<{typeStr}, Lazy<List<{itemStr}>>>(default({typeStr}), new Lazy<List<{itemStr}>>(() => new List<{itemStr}>()));");
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
