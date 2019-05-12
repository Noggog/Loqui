using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loqui.Internal;

namespace Loqui.Generation
{
    public class MaskModule : GenerationModule
    {
        public const string ErrMaskNickname = "ErrMask";
        public const string CopyMaskNickname = "CopyMask";
        public const string TranslationMaskNickname = "TranslMask";
        private Dictionary<Type, MaskModuleField> _fieldMapping = new Dictionary<Type, MaskModuleField>();
        public static readonly TypicalMaskFieldGeneration TypicalField = new TypicalMaskFieldGeneration();

        public override string RegionString => "Mask";

        public MaskModule()
        {
            _fieldMapping[typeof(LoquiType)] = new LoquiMaskFieldGeneration();
            _fieldMapping[typeof(ListType)] = new ContainerMaskFieldGeneration();
            _fieldMapping[typeof(DictType)] = new DictMaskFieldGeneration();
            _fieldMapping[typeof(UnsafeType)] = new UnsafeMaskFieldGeneration();
            _fieldMapping[typeof(WildcardType)] = new UnsafeMaskFieldGeneration();
        }

        public static string MaskNickname(MaskType type)
        {
            switch (type)
            {
                case MaskType.Error:
                    return ErrMaskNickname;
                case MaskType.Copy:
                    return CopyMaskNickname;
                case MaskType.Translation:
                    return TranslationMaskNickname;
                case MaskType.Normal:
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddTypeAssociation<T>(MaskModuleField gen)
            where T : TypeGeneration
        {
            _fieldMapping[typeof(T)] = gen;
        }

        public void GenerateForErrorMaskToStringForField(FileGeneration fg, ObjectGeneration obj, TypeGeneration field)
        {
            if (field.IntegrateField)
            {
                fg.AppendLine($"if ({GetMaskModule(field.GetType()).GenerateBoolMaskCheck(field, "printMask")})");
            }
            using (new BraceWrapper(fg, doIt: field.IntegrateField))
            {
                GetMaskModule(field.GetType()).GenerateForErrorMaskToString(fg, field, field.Name, true);
            }
        }

        public void GenerateSetExceptionForField(FileGeneration fg, TypeGeneration field)
        {
            if (field.IntegrateField)
            {
                fg.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
                using (new DepthWrapper(fg))
                {
                    GetMaskModule(field.GetType()).GenerateSetException(fg, field);
                    fg.AppendLine("break;");
                }
            }
            else
            {
                GetMaskModule(field.GetType()).GenerateSetException(fg, field);
            }
        }

        public void GenerateGetMaskForField(FileGeneration fg, TypeGeneration field)
        {
            if (field.IntegrateField)
            {
                fg.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"return {field.Name};");
                }
            }
        }

        public void GenerateSetSetNthMaskForField(FileGeneration fg, TypeGeneration field)
        {
            if (field.IntegrateField)
            {
                fg.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
                using (new DepthWrapper(fg))
                {
                    GetMaskModule(field.GetType()).GenerateSetMask(fg, field);
                    fg.AppendLine("break;");
                }
            }
            else
            {
                GetMaskModule(field.GetType()).GenerateSetMask(fg, field);
            }
        }

        public override async Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            foreach (var item in _fieldMapping.Values)
            {
                item.Module = this;
            }

            await GenerateNormalMask(obj, fg);
            await GenerateErrorMask(obj, fg);
            GenerateCopyMask(obj, fg);
            await GenerateTranslationMask(obj, fg);
        }

        private void GenerateCopyMask(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public class {obj.Mask(MaskType.Copy)}{(obj.HasLoquiBaseObject ? $" : {obj.BaseClass.Mask(MaskType.Copy)}" : string.Empty)}");
            using (new DepthWrapper(fg))
            {
                fg.AppendLines(obj.GenericTypeMaskWheres(MaskType.Copy));
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public {obj.Mask_BasicName(MaskType.Copy)}()");
                using (new BraceWrapper(fg))
                {
                }
                fg.AppendLine();

                fg.AppendLine($"public {obj.Mask_BasicName(MaskType.Copy)}(bool defaultOn, CopyOption deepCopyOption = CopyOption.Reference)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForCopyMaskCtor(fg, field, basicValueStr: "defaultOn", deepCopyStr: "deepCopyOption");
                    }
                }
                fg.AppendLine();

                using (new RegionWrapper(fg, "Members"))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForCopyMask(fg, field);
                    }
                }
            }
            fg.AppendLine();
        }

        private async Task GenerateErrorMask(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public class {obj.Mask(MaskType.Error)} : {(obj.HasLoquiBaseObject ? $"{obj.BaseClass.Mask(MaskType.Error)}" : "IErrorMask")}, IErrorMask<{obj.Mask(MaskType.Error)}>");
            using (new DepthWrapper(fg))
            {
                fg.AppendLines(obj.GenericTypeMaskWheres(MaskType.Error));
            }
            using (new BraceWrapper(fg))
            {
                using (new RegionWrapper(fg, "Members"))
                {
                    if (!obj.HasLoquiBaseObject)
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
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForErrorMask(fg, field);
                    }
                }

                using (new RegionWrapper(fg, "IErrorMask"))
                {
                    fg.AppendLine($"public{await obj.FunctionOverride()}object GetNthMask(int index)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                        fg.AppendLine("switch (enu)");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                GenerateGetMaskForField(fg, item);
                            }

                            GenerateStandardDefault(fg, obj, "GetNthMask", "index", ret: true);
                        }
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public{await obj.FunctionOverride()}void SetNthException(int index, Exception ex)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                        fg.AppendLine("switch (enu)");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                GenerateSetExceptionForField(fg, item);
                            }

                            GenerateStandardDefault(fg, obj, "SetNthException", "index", false, "ex");
                        }
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public{await obj.FunctionOverride()}void SetNthMask(int index, object obj)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                        fg.AppendLine("switch (enu)");
                        using (new BraceWrapper(fg))
                        {
                            foreach (var item in obj.IterateFields())
                            {
                                GenerateSetSetNthMaskForField(fg, item);
                            }

                            GenerateStandardDefault(fg, obj, "SetNthMask", "index", false, "obj");
                        }
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public{await obj.FunctionOverride()}bool IsInError()");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (Overall != null) return true;");
                        foreach (var item in obj.IterateFields())
                        {
                            fg.AppendLine($"if ({item.Name} != null) return true;");
                        }
                        fg.AppendLine("return false;");
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

                    fg.AppendLine($"public{await obj.FunctionOverride()}void ToString({nameof(FileGeneration)} fg)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.AppendLine(\"{obj.Mask_BasicName(MaskType.Error)} =>\");");
                        fg.AppendLine($"fg.AppendLine(\"[\");");
                        fg.AppendLine($"using (new DepthWrapper(fg))");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"if (this.Overall != null)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"fg.AppendLine(\"Overall =>\");");
                                fg.AppendLine($"fg.AppendLine(\"[\");");
                                fg.AppendLine($"using (new DepthWrapper(fg))");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("fg.AppendLine($\"{this.Overall}\");");
                                }
                                fg.AppendLine($"fg.AppendLine(\"]\");");
                            }
                            fg.AppendLine($"ToString_FillInternal(fg);");
                        }
                        fg.AppendLine($"fg.AppendLine(\"]\");");
                    }

                    fg.AppendLine($"protected{await obj.FunctionOverride()}void ToString_FillInternal({nameof(FileGeneration)} fg)");
                    using (new BraceWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine("base.ToString_FillInternal(fg);");
                        }
                        foreach (var item in obj.IterateFields())
                        {
                            GetMaskModule(item.GetType()).GenerateForErrorMaskToString(fg, item, item.Name, true);
                        }
                    }
                }

                using (new RegionWrapper(fg, "Combine"))
                {
                    fg.AppendLine($"public {obj.Mask(MaskType.Error)} Combine({obj.Mask(MaskType.Error)} rhs)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var ret = new {obj.Mask(MaskType.Error)}();");
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForErrorMaskCombine(fg, field, $"this.{field.Name}", $"ret.{field.Name}", $"rhs.{field.Name}");
                        }
                        fg.AppendLine("return ret;");
                    }

                    fg.AppendLine($"public static {obj.Mask(MaskType.Error)} Combine({obj.Mask(MaskType.Error)} lhs, {obj.Mask(MaskType.Error)} rhs)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if (lhs != null && rhs != null) return lhs.Combine(rhs);");
                        fg.AppendLine($"return lhs ?? rhs;");
                    }
                }

                using (new RegionWrapper(fg, "Factory"))
                {
                    fg.AppendLine($"public static {obj.Mask(MaskType.Error)} Factory(ErrorMaskBuilder errorMask)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (errorMask?.Empty ?? true) return null;");
                        fg.AppendLine($"return new {obj.Mask(MaskType.Error)}();");
                    }
                }
            }
        }

        private async Task GenerateNormalMask(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public class {obj.Name}_Mask<T> : {(obj.HasLoquiBaseObject ? $"{obj.BaseClass.GetMaskString("T")}, " : string.Empty)}IMask<T>, IEquatable<{obj.Name}_Mask<T>>");
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
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForCtor(fg, field, typeStr: "T", valueStr: "initialValue");
                        }
                    }
                }

                using (new RegionWrapper(fg, "Members"))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForField(fg, field, "T");
                    }
                }

                using (new RegionWrapper(fg, "Equals"))
                {
                    fg.AppendLine("public override bool Equals(object obj)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if (!(obj is {obj.Name}_Mask<T> rhs)) return false;");
                        fg.AppendLine($"return Equals(rhs);");
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public bool Equals({obj.Name}_Mask<T> rhs)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (rhs == null) return false;");
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine($"if (!base.Equals(rhs)) return false;");
                        }
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForEqual(fg, field, $"rhs.{field.Name}");
                        }
                        fg.AppendLine("return true;");
                    }

                    fg.AppendLine("public override int GetHashCode()");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("int ret = 0;");
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForHashCode(fg, field, $"rhs.{field.Name}");
                        }
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine($"ret = ret.CombineHashCode(base.GetHashCode());");
                        }
                        fg.AppendLine("return ret;");
                    }
                    fg.AppendLine();
                }

                using (new RegionWrapper(fg, "All Equal"))
                {
                    fg.AppendLine($"public{await obj.FunctionOverride()}bool AllEqual(Func<T, bool> eval)");
                    using (new BraceWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine($"if (!base.AllEqual(eval)) return false;");
                        }
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForAllEqual(fg, field, new Accessor(field, "this."), nullCheck: true, indexed: false);
                        }
                        fg.AppendLine("return true;");
                    }
                }

                using (new RegionWrapper(fg, "Translate"))
                {
                    fg.AppendLine($"public{obj.NewOverride()}{obj.Name}_Mask<R> Translate<R>(Func<T, R> eval)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var ret = new {obj.GetMaskString("R")}();");
                        fg.AppendLine($"this.Translate_InternalFill(ret, eval);");
                        fg.AppendLine("return ret;");
                    }
                    fg.AppendLine();

                    fg.AppendLine($"protected void Translate_InternalFill<R>({obj.Name}_Mask<R> obj, Func<T, R> eval)");
                    using (new BraceWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine($"base.Translate_InternalFill(obj, eval);");
                        }
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForTranslate(fg, field, $"obj.{field.Name}", $"this.{field.Name}", indexed: false);
                        }
                    }
                }

                using (new RegionWrapper(fg, "Clear Enumerables"))
                {
                    fg.AppendLine($"public{await obj.FunctionOverride()}void ClearEnumerables()");
                    using (new BraceWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine($"base.ClearEnumerables();");
                        }
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForClearEnumerable(fg, field);
                        }
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
                                this.GenerateForErrorMaskToStringForField(fg, obj, item);
                            }
                        }
                        fg.AppendLine($"fg.AppendLine(\"]\");");
                    }
                }
            }
            fg.AppendLine();
        }

        private async Task GenerateTranslationMask(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public class {obj.Mask(MaskType.Translation)} : {(obj.HasLoquiBaseObject ? $"{obj.BaseClass.Mask(MaskType.Translation)}" : $"{nameof(ITranslationMask)}")}");
            using (new DepthWrapper(fg))
            {
                fg.AppendLines(obj.GenericTypeMaskWheres(MaskType.Translation));
            }
            using (new BraceWrapper(fg))
            {
                using (new RegionWrapper(fg, "Members"))
                {
                    if (!obj.HasLoquiBaseObject)
                    {
                        fg.AppendLine("private TranslationCrystal _crystal;");
                    }

                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForTranslationMask(fg, field);
                    }
                }

                using (new RegionWrapper(fg, "Ctors"))
                {
                    fg.AppendLine($"public {obj.Mask_BasicName(MaskType.Translation)}()");
                    using (new DepthWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine(": base()");
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                    }
                    fg.AppendLine();

                    fg.AppendLine($"public {obj.Mask_BasicName(MaskType.Translation)}(bool defaultOn)");
                    using (new DepthWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine(": base(defaultOn)");
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForTranslationMaskSet(fg, field, new Accessor(field, "this."), "defaultOn");
                        }

                    }
                    fg.AppendLine();
                }

                if (!obj.HasLoquiBaseObject)
                {
                    fg.AppendLine("public TranslationCrystal GetCrystal()");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (_crystal != null) return _crystal;");
                        fg.AppendLine("List<(bool On, TranslationCrystal SubCrystal)> ret = new List<(bool On, TranslationCrystal SubCrystal)>();");
                        fg.AppendLine($"GetCrystal(ret);");
                        fg.AppendLine($"_crystal = new TranslationCrystal()");
                        using (new BraceWrapper(fg) { AppendSemicolon = true })
                        {
                            fg.AppendLine($"Crystal = ret.ToArray()");
                        }
                        fg.AppendLine("return _crystal;");
                    }
                    fg.AppendLine();
                }

                if (!obj.HasLoquiBaseObject || obj.IterateFields().Any())
                {
                    fg.AppendLine($"protected{await obj.FunctionOverride()}void GetCrystal(List<(bool On, TranslationCrystal SubCrystal)> ret)");
                    using (new BraceWrapper(fg))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            fg.AppendLine("base.GetCrystal(ret);");
                        }
                        foreach (var field in obj.IterateFields())
                        {
                            fg.AppendLine($"ret.Add({GetMaskModule(field.GetType()).GenerateForTranslationMaskCrystalization(field)});");
                        }
                    }
                }
            }
        }

        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override async Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg, bool internalInterface)
        {
        }

        public override async Task<IEnumerable<string>> Interfaces(ObjectGeneration obj)
        {
            return Enumerable.Empty<string>();
        }

        public override async Task Modify(LoquiGenerator gen)
        {
        }

        public override async Task<IEnumerable<string>> RequiredUsingStatements(ObjectGeneration obj)
        {
            return Enumerable.Empty<string>();
        }

        public override async Task<IEnumerable<string>> GetWriterInterfaces(ObjectGeneration obj)
        {
            return Enumerable.Empty<string>();
        }

        public override async Task<IEnumerable<string>> GetReaderInterfaces(ObjectGeneration obj)
        {
            return Enumerable.Empty<string>();
        }

        public void GenerateStandardDefault(FileGeneration fg, ObjectGeneration obj, string functionName, string indexAccessor, bool ret, params string[] otherParameters)
        {
            fg.AppendLine("default:");
            using (new DepthWrapper(fg))
            {
                if (obj.HasLoquiBaseObject)
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

        public override async Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public MaskModuleField GetMaskModule(Type t)
        {
            if (!this._fieldMapping.TryGetValue(t, out var fieldGen))
            {
                foreach (var kv in _fieldMapping.ToList())
                {
                    if (t.InheritsFrom(kv.Key))
                    {
                        _fieldMapping[t] = kv.Value;
                        return kv.Value;
                    }
                }
                _fieldMapping[t] = TypicalField;
                return TypicalField;
            }
            return fieldGen;
        }

        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
        }
    }
}
