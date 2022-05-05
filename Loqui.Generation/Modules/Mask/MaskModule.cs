using Noggog;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class MaskModule : GenerationModule
{
    public const string GenItem = "TItem";
    public const string ErrMaskNickname = "ErrMask";
    public const string CopyMaskNickname = "CopyMask";
    public const string TranslationMaskNickname = "TranslMask";
    private Dictionary<Type, MaskModuleField> _fieldMapping = new();
    public static readonly TypicalMaskFieldGeneration TypicalField = new();

    public override string RegionString => "Mask";

    public MaskModule()
    {
        _fieldMapping[typeof(LoquiType)] = new LoquiMaskFieldGeneration();
        _fieldMapping[typeof(ListType)] = new ContainerMaskFieldGeneration();
        _fieldMapping[typeof(Array2dType)] = new Array2dMaskFieldGeneration();
        _fieldMapping[typeof(DictType)] = new DictMaskFieldGeneration();
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
        lock (_fieldMapping)
        {
            _fieldMapping[typeof(T)] = gen;
        }
    }

    public void GenerateSetExceptionForField(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (field.IntegrateField && field.Enabled)
        {
            sb.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
            using (sb.IncreaseDepth())
            {
                GetMaskModule(field.GetType()).GenerateSetException(sb, field);
                sb.AppendLine("break;");
            }
        }
        else
        {
            GetMaskModule(field.GetType()).GenerateSetException(sb, field);
        }
    }

    public void GenerateGetMaskForField(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField || !field.Enabled) return;
        sb.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
        using (sb.IncreaseDepth())
        {
            sb.AppendLine($"return {field.Name};");
        }
    }

    public void GenerateSetSetNthMaskForField(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (field.IntegrateField && field.Enabled)
        {
            sb.AppendLine($"case {field.ObjectGen.FieldIndexName}.{field.Name}:");
            using (sb.IncreaseDepth())
            {
                GetMaskModule(field.GetType()).GenerateSetMask(sb, field);
                sb.AppendLine("break;");
            }
        }
        else
        {
            GetMaskModule(field.GetType()).GenerateSetMask(sb, field);
        }
    }

    private void GenerateCopyMask(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = sb.Class(obj.Mask(MaskType.Copy, addClassName: false)))
        {
            args.BaseClass = obj.HasLoquiBaseObject ? obj.BaseClass.Mask(MaskType.Copy, addClassName: true) : string.Empty;
            args.New = obj.HasLoquiBaseObject;
            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: MaskType.Copy));
        }
        using (sb.IncreaseDepth())
        {
            sb.AppendLines(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: MaskType.Copy));
        }
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"public {obj.Mask_BasicName(MaskType.Copy)}()");
            using (sb.CurlyBrace())
            {
            }
            sb.AppendLine();

            sb.AppendLine($"public {obj.Mask_BasicName(MaskType.Copy)}(bool defaultOn, CopyOption deepCopyOption = CopyOption.Reference)");
            using (sb.CurlyBrace())
            {
                foreach (var field in obj.IterateFields())
                {
                    GetMaskModule(field.GetType()).GenerateForCopyMaskCtor(sb, field, basicValueStr: "defaultOn", deepCopyStr: "deepCopyOption");
                }
            }
            sb.AppendLine();

            using (sb.Region("Members"))
            {
                foreach (var field in obj.IterateFields())
                {
                    GetMaskModule(field.GetType()).GenerateForCopyMask(sb, field);
                }
            }
        }
        sb.AppendLine();
    }

    private async Task GenerateErrorMask(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = sb.Class(obj.Mask(MaskType.Error, addClassName: false)))
        {
            args.BaseClass = obj.HasLoquiBaseObject ? $"{obj.BaseClass.Mask(MaskType.Error, addClassName: true)}" : string.Empty;
            if (!obj.HasLoquiBaseObject)
            {
                args.Interfaces.Add("IErrorMask");
            }
            args.Interfaces.Add($"IErrorMask<{obj.Mask(MaskType.Error, addClassName: false)}>");
            args.New = obj.HasLoquiBaseObject;
            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: MaskType.Error));
        }
        using (sb.CurlyBrace())
        {
            using (sb.Region("Members"))
            {
                if (!obj.HasLoquiBaseObject)
                {
                    sb.AppendLine("public Exception? Overall { get; set; }");
                    sb.AppendLine("private List<string>? _warnings;");
                    sb.AppendLine("public List<string> Warnings");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine("get");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine("if (_warnings == null)");
                            using (sb.CurlyBrace())
                            {
                                sb.AppendLine("_warnings = new List<string>();");
                            }
                            sb.AppendLine("return _warnings;");
                        }
                    }
                }
                foreach (var field in obj.IterateFields())
                {
                    GetMaskModule(field.GetType()).GenerateForErrorMask(sb, field);
                }
            }

            using (sb.Region("IErrorMask"))
            {
                sb.AppendLine($"public{obj.FunctionOverride()}object? GetNthMask(int index)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                    sb.AppendLine("switch (enu)");
                    using (sb.CurlyBrace())
                    {
                        foreach (var item in obj.IterateFields())
                        {
                            GenerateGetMaskForField(sb, item);
                        }

                        GenerateStandardDefault(sb, obj, "GetNthMask", "index", ret: true);
                    }
                }
                sb.AppendLine();

                sb.AppendLine($"public{obj.FunctionOverride()}void SetNthException(int index, Exception ex)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                    sb.AppendLine("switch (enu)");
                    using (sb.CurlyBrace())
                    {
                        foreach (var item in obj.IterateFields())
                        {
                            GenerateSetExceptionForField(sb, item);
                        }

                        GenerateStandardDefault(sb, obj, "SetNthException", "index", false, "ex");
                    }
                }
                sb.AppendLine();

                sb.AppendLine($"public{obj.FunctionOverride()}void SetNthMask(int index, object obj)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"{obj.FieldIndexName} enu = ({obj.FieldIndexName})index;");
                    sb.AppendLine("switch (enu)");
                    using (sb.CurlyBrace())
                    {
                        foreach (var item in obj.IterateFields())
                        {
                            GenerateSetSetNthMaskForField(sb, item);
                        }

                        GenerateStandardDefault(sb, obj, "SetNthMask", "index", false, "obj");
                    }
                }
                sb.AppendLine();

                sb.AppendLine($"public{obj.FunctionOverride()}bool IsInError()");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine("if (Overall != null) return true;");
                    foreach (var item in obj.IterateFields())
                    {
                        sb.AppendLine($"if ({item.Name} != null) return true;");
                    }
                    sb.AppendLine("return false;");
                }
            }

            using (sb.Region("To String"))
            {
                sb.AppendLine($"public override string ToString() => this.Print();");
                sb.AppendLine();

                sb.AppendLine($"public{obj.FunctionOverride()}void Print({nameof(StructuredStringBuilder)} sb, string? name = null)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"sb.AppendLine($\"{{(name ?? \"{obj.Mask_BasicName(MaskType.Error)}\")}} =>\");");
                    sb.AppendLine($"using (sb.Brace())");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"if (this.Overall != null)");
                        using (sb.CurlyBrace())
                        {
                            sb.AppendLine($"sb.AppendLine(\"Overall =>\");");
                            sb.AppendLine($"using (sb.Brace())");
                            using (sb.CurlyBrace())
                            {
                                sb.AppendLine("sb.AppendLine($\"{this.Overall}\");");
                            }
                        }
                        sb.AppendLine($"PrintFillInternal(sb);");
                    }
                }

                sb.AppendLine($"protected{obj.FunctionOverride()}void PrintFillInternal({nameof(StructuredStringBuilder)} sb)");
                using (sb.CurlyBrace())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine("base.PrintFillInternal(sb);");
                    }
                    foreach (var item in obj.IterateFields())
                    {
                        GetMaskModule(item.GetType()).GenerateMaskToString(sb, item, item.Name, topLevel: true, printMask: false);
                    }
                }
            }

            using (sb.Region("Combine"))
            {
                sb.AppendLine($"public {obj.Mask(MaskType.Error, addClassName: false)} Combine({obj.Mask(MaskType.Error, addClassName: false)}? rhs)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine("if (rhs == null) return this;");
                    sb.AppendLine($"var ret = new {obj.Mask(MaskType.Error, addClassName: false)}();");
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForErrorMaskCombine(sb, field, $"this.{field.Name}", $"ret.{field.Name}", $"rhs.{field.Name}");
                    }
                    sb.AppendLine("return ret;");
                }

                sb.AppendLine($"public static {obj.Mask(MaskType.Error, addClassName: false)}? Combine({obj.Mask(MaskType.Error, addClassName: false)}? lhs, {obj.Mask(MaskType.Error, addClassName: false)}? rhs)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"if (lhs != null && rhs != null) return lhs.Combine(rhs);");
                    sb.AppendLine($"return lhs ?? rhs;");
                }
            }

            using (sb.Region("Factory"))
            {
                sb.AppendLine($"public static{obj.NewOverride()}{obj.Mask(MaskType.Error, addClassName: false)} Factory(ErrorMaskBuilder errorMask)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"return new {obj.Mask(MaskType.Error, addClassName: false)}();");
                }
            }
        }
    }

    private async Task GenerateNormalMask(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = sb.Class($"Mask<{GenItem}>"))
        {
            args.BaseClass = obj.HasLoquiBaseObject ? $"{obj.BaseClass.GetMaskString(GenItem)}" : string.Empty;
            args.Interfaces.Add($"IMask<{GenItem}>");
            args.Interfaces.Add($"IEquatable<Mask<{GenItem}>>");
            args.New = obj.HasLoquiBaseObject;
        }
        using (sb.CurlyBrace())
        {
            using (sb.Region("Ctors"))
            {
                if (obj.IterateFields(includeBaseClass: true).CountGreaterThan(1) || !obj.IterateFields(includeBaseClass: true).Any())
                {
                    using (var args = new Function(sb,
                               $"public Mask"))
                    {
                        args.Add($"{GenItem} initialValue");
                    }
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($": base(initialValue)");
                    }
                    using (sb.CurlyBrace())
                    {
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForCtor(sb, field, typeStr: GenItem, valueStr: "initialValue");
                        }
                    }
                    sb.AppendLine();
                }

                if (obj.IterateFields(includeBaseClass: true).CountGreaterThan(0))
                {
                    using (var args = new Function(sb,
                               $"public Mask"))
                    {
                        foreach (var field in obj.IterateFields(includeBaseClass: true))
                        {
                            args.Add($"{GenItem} {field.Name}");
                        }
                    }
                    if (obj.HasLoquiBaseObject)
                    {
                        using (var args = new Function(sb,
                                   $": base"))
                        {
                            foreach (var field in obj.BaseClass.IterateFields(includeBaseClass: true))
                            {
                                args.Add($"{field.Name}: {field.Name}");
                            }
                        }
                    }
                    using (sb.CurlyBrace())
                    {
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateForCtor(sb, field, typeStr: GenItem, valueStr: field.Name);
                        }
                    }
                }
                sb.AppendLine();

                sb.AppendLine("#pragma warning disable CS8618");
                using (var args = new Function(sb,
                           $"protected Mask"))
                {
                }
                using (sb.CurlyBrace())
                {
                }
                sb.AppendLine("#pragma warning restore CS8618");
                sb.AppendLine();
            }

            using (sb.Region("Members"))
            {
                foreach (var field in obj.IterateFields())
                {
                    GetMaskModule(field.GetType()).GenerateForField(sb, field, GenItem);
                }
            }

            using (sb.Region("Equals"))
            {
                sb.AppendLine("public override bool Equals(object? obj)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"if (!(obj is Mask<{GenItem}> rhs)) return false;");
                    sb.AppendLine($"return Equals(rhs);");
                }
                sb.AppendLine();

                sb.AppendLine($"public bool Equals(Mask<{GenItem}>? rhs)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine("if (rhs == null) return false;");
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($"if (!base.Equals(rhs)) return false;");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForEqual(sb, field, $"rhs.{field.Name}");
                    }
                    sb.AppendLine("return true;");
                }

                sb.AppendLine("public override int GetHashCode()");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine("var hash = new HashCode();");
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForHashCode(sb, field, $"rhs.{field.Name}");
                    }
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($"hash.Add(base.GetHashCode());");
                    }
                    sb.AppendLine("return hash.ToHashCode();");
                }
                sb.AppendLine();
            }

            using (sb.Region("All"))
            {
                sb.AppendLine($"public{obj.FunctionOverride()}bool All(Func<{GenItem}, bool> eval)");
                using (sb.CurlyBrace())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($"if (!base.All(eval)) return false;");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForAll(sb, field, Accessor.FromType(field, "this"), nullCheck: true, indexed: false);
                    }
                    sb.AppendLine("return true;");
                }
            }

            using (sb.Region("Any"))
            {
                sb.AppendLine($"public{obj.FunctionOverride()}bool Any(Func<{GenItem}, bool> eval)");
                using (sb.CurlyBrace())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($"if (base.Any(eval)) return true;");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForAny(sb, field, Accessor.FromType(field, "this"), nullCheck: true, indexed: false);
                    }
                    sb.AppendLine("return false;");
                }
            }

            using (sb.Region("Translate"))
            {
                sb.AppendLine($"public{obj.NewOverride()}Mask<R> Translate<R>(Func<{GenItem}, R> eval)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"var ret = new {obj.GetMaskString("R")}();");
                    sb.AppendLine($"this.Translate_InternalFill(ret, eval);");
                    sb.AppendLine("return ret;");
                }
                sb.AppendLine();

                sb.AppendLine($"protected void Translate_InternalFill<R>(Mask<R> obj, Func<{GenItem}, R> eval)");
                using (sb.CurlyBrace())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine($"base.Translate_InternalFill(obj, eval);");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForTranslate(sb, field, $"obj.{field.Name}", $"this.{field.Name}", indexed: false);
                    }
                }
            }

            using (sb.Region("To String"))
            {
                sb.AppendLine($"public override string ToString() => this.Print();");
                sb.AppendLine();

                sb.AppendLine($"public string Print({obj.GetMaskString("bool")}? printMask = null)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"var sb = new {nameof(StructuredStringBuilder)}();");
                    sb.AppendLine($"Print(sb, printMask);");
                    sb.AppendLine("return sb.ToString();");
                }
                sb.AppendLine();

                sb.AppendLine($"public void Print({nameof(StructuredStringBuilder)} sb, {obj.GetMaskString("bool")}? printMask = null)");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"sb.AppendLine($\"{{nameof({obj.GetMaskString(GenItem)})}} =>\");");
                    sb.AppendLine($"using (sb.Brace())");
                    using (sb.CurlyBrace())
                    {
                        foreach (var field in obj.IterateFields())
                        {
                            GetMaskModule(field.GetType()).GenerateMaskToString(sb, field, field.Name, topLevel: true, printMask: true);
                        }
                    }
                }
            }
        }
        sb.AppendLine();
    }

    private async Task GenerateTranslationMask(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = sb.Class(obj.Mask(MaskType.Translation, addClassName: false)))
        {
            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: MaskType.Translation));
            args.BaseClass = obj.HasLoquiBaseObject ? $"{obj.BaseClass.Mask(MaskType.Translation, addClassName: true)}" : string.Empty;
            args.Interfaces.Add(nameof(ITranslationMask));
            args.New = obj.HasLoquiBaseObject;
        }
        using (sb.CurlyBrace())
        {
            using (sb.Region("Members"))
            {
                if (!obj.HasLoquiBaseObject)
                {
                    sb.AppendLine("private TranslationCrystal? _crystal;");
                    sb.AppendLine("public readonly bool DefaultOn;");
                    sb.AppendLine("public bool OnOverall;");
                }

                foreach (var field in obj.IterateFields())
                {
                    GetMaskModule(field.GetType()).GenerateForTranslationMask(sb, field);
                }
            }

            using (sb.Region("Ctors"))
            {
                using (var args = new Function(sb,
                           $"public {obj.Mask_BasicName(MaskType.Translation)}"))
                {
                    args.Add("bool defaultOn");
                    args.Add("bool onOverall = true");
                }
                using (sb.IncreaseDepth())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine(": base(defaultOn, onOverall)");
                    }
                }
                using (sb.CurlyBrace())
                {
                    if (!obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine("this.DefaultOn = defaultOn;");
                        sb.AppendLine("this.OnOverall = onOverall;");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        GetMaskModule(field.GetType()).GenerateForTranslationMaskSet(sb, field, Accessor.FromType(field, "this"), "defaultOn");
                    }
                }
                sb.AppendLine();
            }

            if (!obj.HasLoquiBaseObject)
            {
                sb.AppendLine("public TranslationCrystal GetCrystal()");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine("if (_crystal != null) return _crystal;");
                    sb.AppendLine("var ret = new List<(bool On, TranslationCrystal? SubCrystal)>();");
                    sb.AppendLine($"GetCrystal(ret);");
                    sb.AppendLine($"_crystal = new TranslationCrystal(ret.ToArray());");
                    sb.AppendLine("return _crystal;");
                }
                sb.AppendLine();
            }

            if (!obj.HasLoquiBaseObject || obj.IterateFields().Any())
            {
                sb.AppendLine($"protected{obj.FunctionOverride()}void GetCrystal(List<(bool On, TranslationCrystal? SubCrystal)> ret)");
                using (sb.CurlyBrace())
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        sb.AppendLine("base.GetCrystal(ret);");
                    }
                    foreach (var field in obj.IterateFields())
                    {
                        sb.AppendLine($"ret.Add({GetMaskModule(field.GetType()).GenerateForTranslationMaskCrystalization(field)});");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine($"public static implicit operator {obj.Mask(MaskType.Translation, addClassName: false)}(bool defaultOn)");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"return new {obj.Mask(MaskType.Translation, addClassName: false)}(defaultOn: defaultOn, onOverall: defaultOn);");
            }
            sb.AppendLine();
        }
    }

    public override async Task GenerateInNonGenericClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        lock (_fieldMapping)
        {
            foreach (var item in _fieldMapping.Values)
            {
                item.Module = this;
            }
        }

        await GenerateNormalMask(obj, sb);
        await GenerateErrorMask(obj, sb);
        if (obj.GenerateComplexCopySystems)
        {
            GenerateCopyMask(obj, sb);
        }
        await GenerateTranslationMask(obj, sb);
    }

    public override async Task GenerateInInterface(ObjectGeneration obj, StructuredStringBuilder sb, bool internalInterface, bool getter)
    {
    }

    public override async IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
    {
        yield break;
    }

    public override async Task Modify(LoquiGenerator gen)
    {
    }

    public override async IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
    {
        yield break;
    }

    public void GenerateStandardDefault(StructuredStringBuilder sb, ObjectGeneration obj, string functionName, string indexAccessor, bool ret, params string[] otherParameters)
    {
        sb.AppendLine("default:");
        using (sb.IncreaseDepth())
        {
            if (obj.HasLoquiBaseObject)
            {
                sb.AppendLine($"{(ret ? "return " : string.Empty)}base.{functionName}({string.Join(", ", indexAccessor.AsEnumerable().And(otherParameters))});");
                if (!ret)
                {
                    sb.AppendLine("break;");
                }
            }
            else
            {
                obj.GenerateIndexOutOfRangeEx(sb, indexAccessor);
            }
        }
    }

    public override async Task GenerateInCommon(ObjectGeneration obj, StructuredStringBuilder sb, MaskTypeSet maskTypes)
    {
    }

    public MaskModuleField GetMaskModule(Type t)
    {
        lock (_fieldMapping)
        {
            if (!_fieldMapping.TryGetValue(t, out var fieldGen))
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
    }

    public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
    {
    }
}