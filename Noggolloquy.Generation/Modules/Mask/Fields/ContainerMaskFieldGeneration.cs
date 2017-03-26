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
                listStr = $"IEnumerable<{valueStr}>";
            }
            else
            {
                listStr = $"IEnumerable<{levType.RefGen.Obj.GetErrorMaskItemString()}>";
            }
            fg.AppendLine($"public MaskItem<{valueStr}, {listStr}> {field.Name};");
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
