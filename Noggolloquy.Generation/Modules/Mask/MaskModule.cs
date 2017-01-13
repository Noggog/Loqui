using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class MaskModule : GenerationModule
    {
        public Dictionary<Type, MaskModuleField> FieldMapping = new Dictionary<Type, MaskModuleField>();
        public TypicalMaskFieldGeneration TypicalField = new TypicalMaskFieldGeneration();

        public override string RegionString
        {
            get
            {
                return "Mask";
            }
        }

        public MaskModule()
        {
            FieldMapping[typeof(LevType)] = new LevMaskFieldGeneration();
            FieldMapping[typeof(ListType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(Array2DType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(DictType)] = new DictMaskFieldGeneration();
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine("public class " + obj.Name + "_Mask<T> " + (obj.HasBaseObject ? " : " + obj.BaseClass.GetMaskString("T") : string.Empty));
            using (new BraceWrapper(fg))
            {
                foreach (var field in obj.Fields)
                {
                    MaskModuleField fieldGen;
                    if (!FieldMapping.TryGetValue(field.GetType(), out fieldGen))
                    {
                        fieldGen = TypicalField;
                    }
                    fieldGen.GenerateForField(fg, field);
                }
            }
            fg.AppendLine();
        }

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override void Modify(NoggolloquyGenerator gen)
        {
        }

        public override void Modify(ObjectGeneration obj)
        {
        }

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield break;
        }
    }
}
