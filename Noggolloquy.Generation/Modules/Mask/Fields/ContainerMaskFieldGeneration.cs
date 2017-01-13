using System;

namespace Noggolloquy.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public override void GenerateForField(FileGeneration fg, TypeGeneration field)
        {
            ContainerType listType = field as ContainerType;
            LevType levType = listType.SubTypeGeneration as LevType;
            string listStr;
            if (levType == null)
            {
                listStr = $"List<T>";
            }
            else
            {
                listStr = $"List<MaskItem<T, {levType.RefGen.Obj.GetMaskString("T")}>>";
            }
            fg.AppendLine($"public MaskItem<T, Lazy<{listStr}>> {field.Name} = new MaskItem<T, Lazy<{listStr}>>(default(T), new Lazy<{listStr}>(() => new {listStr}()));");
        }
    }
}
