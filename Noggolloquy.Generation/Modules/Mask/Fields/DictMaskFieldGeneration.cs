using System;

namespace Noggolloquy.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            IDictType dictType = field as IDictType;
            LevType keyLevType = dictType.KeyTypeGen as LevType;
            LevType valueLevType = dictType.ValueTypeGen as LevType;
            string valueStr = $"{(valueLevType == null ? typeStr : $"MaskItem<{typeStr}, {valueLevType.RefGen.Obj.GetMaskString(typeStr)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{(keyLevType == null ? typeStr : $"MaskItem<{typeStr}, {keyLevType.RefGen.Obj.GetMaskString(typeStr)}")}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            fg.AppendLine($"public MaskItem<{typeStr}, Lazy<List<{itemStr}>>> {field.Name} = new MaskItem<{typeStr}, Lazy<List<{itemStr}>>>(default({typeStr}), new Lazy<List<{itemStr}>>(() => new List<{itemStr}>()));");
        }
    }
}
