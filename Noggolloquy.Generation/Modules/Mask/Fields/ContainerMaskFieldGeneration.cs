using System;

namespace Noggolloquy.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            ContainerType listType = field as ContainerType;
            LevType levType = listType.SubTypeGeneration as LevType;
            string listStr;
            if (levType == null)
            {
                listStr = $"List<{valueStr}>";
            }
            else
            {
                listStr = $"List<{levType.RefGen.Obj.GetErrorMaskItemString()}>";
            }
            fg.AppendLine($"public MaskItem<{valueStr}, Lazy<{listStr}>> {field.Name} = new MaskItem<{valueStr}, Lazy<{listStr}>>(default({valueStr}), new Lazy<{listStr}>(() => new {listStr}()));");
        }
    }
}
