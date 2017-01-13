using System;

namespace Noggolloquy.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field)
        {
            IDictType dictType = field as IDictType;
            LevType keyLevType = dictType.KeyTypeGen as LevType;
            LevType valueLevType = dictType.ValueTypeGen as LevType;
            string valueStr = $"{(valueLevType == null ? "T" : $"MaskItem<T, {valueLevType.RefGen.Obj.GetMaskString("T")}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{(keyLevType == null ? "T" : $"MaskItem<T, {keyLevType.RefGen.Obj.GetMaskString("T")}")}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            fg.AppendLine($"public MaskItem<T, Lazy<List<{itemStr}>>> {field.Name} = new MaskItem<T, Lazy<List<{itemStr}>>>(default(T), new Lazy<List<{itemStr}>>(() => new List<{itemStr}>()));");
        }
    }
}
