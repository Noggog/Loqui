using System;
using System.Collections.Generic;

namespace Loqui.Generation
{
    public class MaskModule : GenerationModule
    {
        public Dictionary<Type, MaskModuleField> FieldMapping = new Dictionary<Type, MaskModuleField>();
        public TypicalMaskFieldGeneration TypicalField = new TypicalMaskFieldGeneration();

        public override string RegionString => "Mask";

        public MaskModule()
        {
            FieldMapping[typeof(LoquiType)] = new LoquiMaskFieldGeneration();
            FieldMapping[typeof(ListType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(Array2DType)] = new ContainerMaskFieldGeneration();
            FieldMapping[typeof(DictType)] = new DictMaskFieldGeneration();
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
            foreach (var item in FieldMapping.Values)
            {
                item.Module = this;
            }

            fg.AppendLine($"public class {obj.Name}_Mask<T> : {(obj.HasBaseObject ? $"{obj.BaseClass.GetMaskString("T")}, " : string.Empty)}IMask<T>");
            using (new BraceWrapper(fg))
            {
                using (new RegionWrapper(fg, "Ctors"))
                {
                    fg.AppendLine($"public {obj.Name}_Mask()");
                    using (new BraceWrapper(fg))
                    {
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public {obj.Name}_Mask(T initialValue)");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var field in obj.Fields)
                        {
                            GetMaskModule(field.GetType()).GenerateForCtor(fg, field, "initialValue");
                        }
                    }
                }

                using (new RegionWrapper(fg, "Members"))
                {
                    foreach (var field in obj.Fields)
                    {
                        GetMaskModule(field.GetType()).GenerateForField(fg, field, "T");
                    }
                }

                using (new RegionWrapper(fg, "All Equal"))
                {
                    fg.AppendLine("public bool AllEqual(Func<T, bool> eval)");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var field in obj.Fields)
                        {
                            GetMaskModule(field.GetType()).GenerateForAllEqual(fg, field);
                        }
                        fg.AppendLine("return true;");
                    }
                }

                using (new RegionWrapper(fg, "Translate"))
                {
                    fg.AppendLine($"public {obj.Name}_Mask<R> Translate<R>(Func<T, R> eval)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var ret = new {obj.GetMaskString("R")}();");
                        foreach (var field in obj.Fields)
                        {
                            GetMaskModule(field.GetType()).GenerateForTranslate(fg, field, $"ret.{field.Name}", $"this.{field.Name}");
                        }
                        fg.AppendLine("return ret;");
                    }
                }

                using (new RegionWrapper(fg, "To String"))
                {
                    fg.AppendLine($"public override string ToString()");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"return ToString(printMask: null);");
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public string ToString({obj.GetMaskString("bool")} printMask = null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var fg = new {nameof(FileGeneration)}();");
                        fg.AppendLine($"ToString(fg, printMask);");
                        fg.AppendLine("return fg.ToString();");
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public void ToString({nameof(FileGeneration)} fg, {obj.GetMaskString("bool")} printMask = null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.AppendLine($\"{{nameof({obj.GetMaskString("T")})}} =>\");");
                        fg.AppendLine($"fg.AppendLine(\"[\");");
                        fg.AppendLine($"using (new DepthWrapper(fg))");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                fg.AppendLine($"if ({GetMaskModule(item.Field.GetType()).GenerateBoolMaskCheck(item.Field, "printMask")})");
                                using (new BraceWrapper(fg))
                                {
                                    GetMaskModule(item.Field.GetType()).GenerateForErrorMaskToString(fg, item.Field, item.Field.Name, true);
                                }
                            }
                        }
                        fg.AppendLine($"fg.AppendLine(\"]\");");
                    }
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public class {obj.ErrorMask} : {(obj.HasBaseObject ? $"{obj.BaseClass.ErrorMask}" : "IErrorMask")}");
            using (new BraceWrapper(fg))
            {
                using (new RegionWrapper(fg, "Members"))
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
                        GetMaskModule(field.GetType()).GenerateForErrorMask(fg, field);
                    }
                }

                using (new RegionWrapper(fg, "IErrorMask"))
                {
                    fg.AppendLine($"public{obj.FunctionOverride}void SetNthException(ushort index, Exception ex)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{obj.EnumName} enu = ({obj.EnumName})index;");
                        fg.AppendLine("switch (enu)");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                fg.AppendLine($"case {obj.EnumName}.{item.Field.Name}:");
                                using (new DepthWrapper(fg))
                                {
                                    GetMaskModule(item.Field.GetType()).GenerateSetException(fg, item.Field);
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
                        fg.AppendLine($"{obj.EnumName} enu = ({obj.EnumName})index;");
                        fg.AppendLine("switch (enu)");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                fg.AppendLine($"case {obj.EnumName}.{item.Field.Name}:");
                                using (new DepthWrapper(fg))
                                {
                                    GetMaskModule(item.Field.GetType()).GenerateSetMask(fg, item.Field);
                                    fg.AppendLine("break;");
                                }
                            }

                            GenerateStandardDefault(fg, obj, "SetNthMask", "index", false, "obj");
                        }
                    }
                }

                using (new RegionWrapper(fg, "To String"))
                {
                    fg.AppendLine($"public override string ToString()");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var fg = new {nameof(FileGeneration)}();");
                        fg.AppendLine($"ToString(fg);");
                        fg.AppendLine("return fg.ToString();");
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public void ToString({nameof(FileGeneration)} fg)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.AppendLine(\"{obj.ErrorMask} =>\");");
                        fg.AppendLine($"fg.AppendLine(\"[\");");
                        fg.AppendLine($"using (new DepthWrapper(fg))");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                fg.AppendLine($"if ({item.Field.Name} != null)");
                                using (new BraceWrapper(fg))
                                {
                                    GetMaskModule(item.Field.GetType()).GenerateForErrorMaskToString(fg, item.Field, item.Field.Name, true);
                                }
                            }
                        }
                        fg.AppendLine($"fg.AppendLine(\"]\");");
                    }
                }

                using (new RegionWrapper(fg, "Combine"))
                {
                    fg.AppendLine($"public {obj.ErrorMask} Combine({obj.ErrorMask} rhs)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var ret = new {obj.ErrorMask}();");
                        foreach (var field in obj.Fields)
                        {
                            GetMaskModule(field.GetType()).GenerateForErrorMaskCombine(fg, field, $"this.{field.Name}", $"ret.{field.Name}", $"rhs.{field.Name}");
                        }
                        fg.AppendLine("return ret;");
                    }

                    fg.AppendLine($"public static {obj.ErrorMask} Combine({obj.ErrorMask} lhs, {obj.ErrorMask} rhs)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if (lhs != null && rhs != null) return lhs.Combine(rhs);");
                        fg.AppendLine($"return lhs ?? rhs;");
                    }
                }
            }

            fg.AppendLine($"public class {obj.CopyMask}{(obj.HasBaseObject ? $" : {obj.BaseClass.CopyMask}" : string.Empty)}");
            using (new BraceWrapper(fg))
            {
                using (new RegionWrapper(fg, "Members"))
                {
                    foreach (var field in obj.Fields)
                    {
                        GetMaskModule(field.GetType()).GenerateForCopyMask(fg, field);
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

        public override void Modify(LoquiGenerator gen)
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

        public override void GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public MaskModuleField GetMaskModule(Type t)
        {
            if (!this.FieldMapping.TryGetValue(t, out var fieldGen))
            {
                fieldGen = this.TypicalField;
            }
            return fieldGen;
        }
    }
}
