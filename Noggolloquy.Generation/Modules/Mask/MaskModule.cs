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

            fg.AppendLine($"public class {obj.ErrorMask} : {(obj.HasBaseObject ? $"{obj.BaseClass.ErrorMask}" : "IErrorMask")}");
            using (new BraceWrapper(fg))
            {
                if (!obj.HasBaseObject)
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

                fg.AppendLine($"public{obj.FunctionOverride}void SetNthException(ushort index, Exception ex)");
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

                        GenerateStandardDefault(fg, obj, "SetNthException", "index", false, "ex");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void SetNthMask(ushort index, object obj)");
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

                        GenerateStandardDefault(fg, obj, "SetNthMask", "index", false, "obj");
                    }
                }
            }

            fg.AppendLine($"public class {obj.CopyMask}{(obj.HasBaseObject ? $" : {obj.BaseClass.CopyMask}" : string.Empty)}");
            using (new BraceWrapper(fg))
            {
                foreach (var field in obj.Fields)
                {
                    if (!FieldMapping.TryGetValue(field.GetType(), out MaskModuleField fieldGen))
                    {
                        fieldGen = TypicalField;
                    }
                    fieldGen.GenerateForCopyMask(fg, field);
                }
                fg.AppendLine();
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

        public void GenerateStandardDefault(FileGeneration fg, ObjectGeneration obj, string functionName, string indexAccessor, bool ret, params string[] otherParameters)
        {
            fg.AppendLine("default:");
            using (new DepthWrapper(fg))
            {
                if (obj.HasBaseObject)
                {
                    fg.AppendLine($"{(ret ? "return " : string.Empty)}base.{functionName}({string.Join(", ", indexAccessor.And(otherParameters))});");
                    if (!ret)
                    {
                        fg.AppendLine("break;");
                    }
                }
                else
                {
                    obj.GenerateIndexOutOfRangeEx(fg, indexAccessor);
                }
            }
        }
    }
}
