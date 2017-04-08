using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class MaskModule : GenerationModule
    {
        public Dictionary<Type, MaskModuleField> FieldMapping = new Dictionary<Type, MaskModuleField>();
        public TypicalMaskFieldGeneration TypicalField = new TypicalMaskFieldGeneration();

        public override string RegionString => "Mask";

        public MaskModule()
        {
            FieldMapping[typeof(NoggType)] = new NoggMaskFieldGeneration();
            FieldMapping[typeof(ListType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(Array2DType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(DictType)] = new DictMaskFieldGeneration();
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public class {obj.Name}_Mask<T> {(obj.HasBaseObject ? $" : {obj.BaseClass.GetMaskString("T")}" : string.Empty)}");
            using (new BraceWrapper(fg))
            {
                foreach (var field in obj.Fields)
                {
                    if (!FieldMapping.TryGetValue(field.GetType(), out MaskModuleField fieldGen))
                    {
                        fieldGen = TypicalField;
                    }
                    fieldGen.GenerateForField(fg, field, "T");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public class {obj.Name}_ErrorMask : {(obj.HasBaseObject ? $"{obj.BaseClass.Name}_ErrorMask" : "IErrorMask")}");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("public Exception Overall { get; set; }");
                fg.AppendLine("private List<string> _warnings;");
                fg.AppendLine("public List<string> Warnings");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("get");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (_warnings == null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine("_warnings = new List<string>();");
                        }
                        fg.AppendLine("return _warnings;");
                    }
                }
                foreach (var field in obj.Fields)
                {
                    if (!FieldMapping.TryGetValue(field.GetType(), out MaskModuleField fieldGen))
                    {
                        fieldGen = TypicalField;
                    }
                    fieldGen.GenerateForErrorMask(fg, field);
                }
                fg.AppendLine();

                fg.AppendLine("public void SetNthException(ushort index, Exception ex)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("switch (index)");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var item in obj.IterateFields())
                        {
                            fg.AppendLine($"case {item.Index}:");
                            using (new DepthWrapper(fg))
                            {
                                if (!FieldMapping.TryGetValue(item.Field.GetType(), out MaskModuleField fieldGen))
                                {
                                    fieldGen = TypicalField;
                                }
                                fieldGen.GenerateSetException(fg, item.Field);
                                fg.AppendLine("break;");
                            }
                        }

                        obj.GenerateStandardIndexDefault(fg, "SetNthException", "index", false);
                    }
                }
                fg.AppendLine();

                fg.AppendLine("public void SetNthMask(ushort index, object obj)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("switch (index)");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var item in obj.IterateFields())
                        {
                            fg.AppendLine($"case {item.Index}:");
                            using (new DepthWrapper(fg))
                            {
                                if (!FieldMapping.TryGetValue(item.Field.GetType(), out MaskModuleField fieldGen))
                                {
                                    fieldGen = TypicalField;
                                }
                                fieldGen.GenerateSetMask(fg, item.Field);
                                fg.AppendLine("break;");
                            }
                        }

                        obj.GenerateStandardIndexDefault(fg, "SetNthMask", "index", false);
                    }
                }
            }
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

        public override IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            yield break;
        }
    }
}
