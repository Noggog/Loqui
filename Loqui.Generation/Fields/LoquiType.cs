using Noggog;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class LoquiType : PrimitiveType, IEquatable<LoquiType>
{
    public override string TypeName(bool getter = false, bool needsCovariance = false)
    {
        return TypeName(getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
    }

    public string TypeName(LoquiInterfaceType interfaceType)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
                switch (interfaceType)
                {
                    case LoquiInterfaceType.Direct:
                        break;
                    case LoquiInterfaceType.ISetter:
                        interfaceType = SetterInterfaceType;
                        break;
                    case LoquiInterfaceType.IGetter:
                        interfaceType = GetterInterfaceType;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                switch (interfaceType)
                {
                    case LoquiInterfaceType.Direct:
                        return DirectTypeName;
                    case LoquiInterfaceType.IGetter:
                        return $"{Interface(getter: true, internalInterface: InternalGetInterface)}";
                    case LoquiInterfaceType.ISetter:
                        return $"{Interface(getter: false, internalInterface: InternalSetInterface)}";
                    default:
                        throw new NotImplementedException();
                }
            case LoquiRefType.Generic:
                return _generic;
            case LoquiRefType.Interface:
                switch (interfaceType)
                {
                    case LoquiInterfaceType.Direct:
                    case LoquiInterfaceType.ISetter:
                        return SetterInterface;
                    case LoquiInterfaceType.IGetter:
                        return GetterInterface;
                    default:
                        throw new NotImplementedException();
                }
            default:
                throw new NotImplementedException();
        }
    }

    public string TypeNameInternal(bool getter, bool internalInterface)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
                switch (getter ? GetterInterfaceType : SetterInterfaceType)
                {
                    case LoquiInterfaceType.Direct:
                        return DirectTypeName;
                    case LoquiInterfaceType.IGetter:
                        return $"{Interface(getter: true, internalInterface: internalInterface)}";
                    case LoquiInterfaceType.ISetter:
                        return $"{Interface(getter: false, internalInterface: internalInterface)}";
                    default:
                        throw new NotImplementedException();
                }
            case LoquiRefType.Generic:
                return _generic;
            case LoquiRefType.Interface:
                return getter ? GetterInterface : SetterInterface;
            default:
                throw new NotImplementedException();
        }
    }

    public string DirectTypeName => $"{_TargetObjectGeneration.Name}{GenericTypes(getter: false)}";

    // ToDo
    // Perhaps implement detection for those that are
    public override bool IsIEquatable => false;

    public override string ProtectedName
    {
        get
        {
            if (Singleton)
            {
                return SingletonObjectName;
            }
            else
            {
                return base.Name;
            }
        }
    }

    public string ObjectTypeName
    {
        get
        {
            switch (RefType)
            {
                case LoquiRefType.Direct:
                    return _TargetObjectGeneration.Name;
                case LoquiRefType.Generic:
                    return _generic;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public string Interface(bool getter = false, bool internalInterface = false)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
                return _TargetObjectGeneration.Interface(GenericTypes(getter), getter: getter, internalInterface: internalInterface);
            case LoquiRefType.Generic:
                return _generic;
            case LoquiRefType.Interface:
                if (internalInterface)
                {
                    throw new ArgumentException();
                }
                return getter ? GetterInterface : SetterInterface;
            default:
                throw new NotImplementedException();
        }
    }
    public string GenericTypes(bool getter) => GetGenericTypes(getter, MaskType.Normal);
    public bool Singleton { get; set; }
    public LoquiRefType RefType { get; private set; }
    public LoquiInterfaceType SetterInterfaceType;
    public LoquiInterfaceType GetterInterfaceType;
    protected string _generic;
    public GenericDefinition GenericDef;
    public GenericSpecification GenericSpecification;
    protected ObjectGeneration _TargetObjectGeneration;
    public ObjectGeneration TargetObjectGeneration
    {
        get
        {
            switch (RefType)
            {
                case LoquiRefType.Direct:
                    return _TargetObjectGeneration;
                case LoquiRefType.Generic:
                    return GenericDef.BaseObjectGeneration;
                case LoquiRefType.Interface:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public string SingletonObjectName => $"_{Name}_Object";
    public override Type Type(bool getter) => throw new NotImplementedException();
    public string RefName;
    public string SetterInterface;
    public string GetterInterface;
    public override bool Nullable => base.Nullable && !Singleton;
    public bool CanStronglyType => RefType != LoquiRefType.Interface;
    // Adds "this" to constructor parameters as it is a common pattern to tie child to parent
    // Can probably be replaced with a more robust parameter configuration setup later
    private bool? _thisConstruction;

    public bool ThisConstruction
    {
        get => _thisConstruction ?? false;
        set => _thisConstruction = value;
    }
    public bool HasInternalGetInterface => TargetObjectGeneration?.HasInternalGetInterface ?? false;
    public bool HasInternalSetInterface => TargetObjectGeneration?.HasInternalSetInterface ?? false;
    public string DefaultType;

    public enum LoquiRefType
    {
        Direct,
        Interface,
        Generic
    }


    public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
    {
        if (Singleton
            || deepCopy)
        {
            return $"{copyMaskAccessor}?.{Name}.Overall ?? true";
        }
        switch (RefType)
        {
            case LoquiRefType.Direct:
                return $"{copyMaskAccessor}?.{Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            case LoquiRefType.Generic:
                return $"{copyMaskAccessor}?.{Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            default:
                throw new NotImplementedException();
        }
    }

    public virtual string GetMaskString(string str)
    {
        return TargetObjectGeneration?.GetMaskString(str) ?? $"IMask<{str}>";
    }

    public override string EqualsMaskAccessor(string accessor) => $"{accessor}.Overall";

    public string Mask(MaskType type)
    {
        if (GenericDef != null)
        {
            switch (type)
            {
                case MaskType.Error:
                    return $"{GenericDef.Name}_{MaskModule.ErrMaskNickname}";
                case MaskType.Copy:
                    return $"{GenericDef.Name}_{MaskModule.CopyMaskNickname}";
                case MaskType.Translation:
                    return $"{GenericDef.Name}_{MaskModule.TranslationMaskNickname}";
                case MaskType.Normal:
                default:
                    throw new NotImplementedException();
            }
        }
        else if (GenericSpecification != null)
        {
            return TargetObjectGeneration.Mask_Specified(type, GenericSpecification);
        }
        else if (TargetObjectGeneration != null)
        {
            return TargetObjectGeneration.Mask(type);
        }
        else if (RefType == LoquiRefType.Interface)
        {
            switch (type)
            {
                case MaskType.Error:
                    return nameof(IErrorMask);
                case MaskType.Translation:
                    return nameof(ITranslationMask);
                case MaskType.Copy:
                case MaskType.Normal:
                default:
                    throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public override bool CopyNeedsTryCatch => true;

    public override async Task GenerateForClass(StructuredStringBuilder sb)
    {
        if (_TargetObjectGeneration != null)
        {
            await _TargetObjectGeneration.LoadingCompleteTask.Task;
            _thisConstruction = _thisConstruction ?? _TargetObjectGeneration.Abstract && !Nullable && DefaultType.IsNullOrWhitespace();
        }

        if (Nullable)
        {
            if (Singleton)
            {
                sb.AppendLine($"private readonly {DirectTypeName} {SingletonObjectName}{(ThisConstruction ? null : $" = new {DirectTypeName}()")};");
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public {TypeName()} {Name} => {SingletonObjectName};");
            }
            else
            {
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                sb.AppendLine($"private {TypeName()}{NullChar} _{Name};");
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public {OverrideStr}{TypeName()}{NullChar} {Name}");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"get => _{Name};");
                    sb.AppendLine($"{SetPermissionStr}set => _{Name} = value;");
                }
            }
            sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            sb.AppendLine($"{TypeNameInternal(getter: true, internalInterface: true)}{NullChar} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => this.{ProtectedName};");
        }
        else
        {
            if (Singleton)
            {
                sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                sb.AppendLine($"private {(ThisConstruction ? null : "readonly ")}{DirectTypeName} {SingletonObjectName}{(ThisConstruction ? null : $" = new {DirectTypeName}()")};");
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                sb.AppendLine($"public {OverrideStr}{TypeName()} {Name} => {SingletonObjectName};");
                if (GetterInterfaceType != LoquiInterfaceType.Direct)
                {
                    sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    sb.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: false)}.{Name} => {SingletonObjectName};");
                }
            }
            else
            {
                Comments?.Apply(sb, LoquiInterfaceType.Direct);
                string? construction;
                if (ThisConstruction)
                {
                    if (Nullable)
                    {
                        construction = null;
                    }
                    else
                    {
                        construction = " = default!;";
                    }
                }
                else if (!DefaultType.IsNullOrWhitespace())
                {
                    construction = $" = new {DefaultType}();";
                }
                else
                {
                    construction = $" = new {DirectTypeName}();";
                }
                sb.AppendLine($"public {OverrideStr}{TypeName()} {Name} {{ get; {SetPermissionStr}set; }}{construction}");
                if (GetterInterfaceType != LoquiInterfaceType.Direct)
                {
                    sb.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    sb.AppendLine($"{TypeName(getter: true)} {ObjectGen.Interface(getter: true, internalInterface: InternalGetInterface)}.{Name} => {Name};");
                }
            }
        }
    }

    protected virtual XElement GetRefNode(XElement node)
    {
        if (node.Name.LocalName.Equals(Constants.REF_DIRECT)
            || node.Name.LocalName.Equals(Constants.REF_LIST))
        {
            return node;
        }
        else
        {
            var ret = node.Element(XName.Get(Constants.DIRECT, LoquiGenerator.Namespace));
            if (ret != null) return ret;
        }
        return node;
    }

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        Singleton = node.GetAttribute(Constants.SINGLETON, false);
        DefaultType = node.GetAttribute(Constants.DefaultType, null);

        XElement refNode = GetRefNode(node);

        int refTypeCount = 0;

        var genericName = node.Element(XName.Get(Constants.GENERIC, LoquiGenerator.Namespace))?.Value;
        if (!string.IsNullOrWhiteSpace(genericName))
        {
            refTypeCount++;
        }

        if (RefName == null)
        {
            RefName = refNode?.GetAttribute(Constants.REF_NAME);
        }
        if (!string.IsNullOrWhiteSpace(RefName))
        {
            refTypeCount++;
        }

        bool usingInterface = false;
        foreach (var interfNode in Node.Elements(XName.Get(Constants.INTERFACE, LoquiGenerator.Namespace)))
        {
            switch (interfNode.GetAttribute<LoquiInterfaceDefinitionType>(Constants.TYPE, LoquiInterfaceDefinitionType.Dual))
            {
                case LoquiInterfaceDefinitionType.ISetter:
                    SetterInterface = interfNode.Value;
                    break;
                case LoquiInterfaceDefinitionType.IGetter:
                    GetterInterface = interfNode.Value;
                    break;
                case LoquiInterfaceDefinitionType.Direct:
                    SetterInterface = interfNode.Value;
                    GetterInterface = interfNode.Value;
                    break;
                case LoquiInterfaceDefinitionType.Dual:
                    SetterInterface = interfNode.Value;
                    GetterInterface = $"{interfNode.Value}Getter";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        if (!string.IsNullOrWhiteSpace(SetterInterface) || !string.IsNullOrWhiteSpace(GetterInterface))
        {
            refTypeCount++;
            usingInterface = true;
        }

        if (refTypeCount > 1)
        {
            throw new ArgumentException("Cannot specify multiple reference systems.  Either pick direct, interface, or generic.");
        }

        if (!ParseRefNode(refNode))
        {
            if (!string.IsNullOrWhiteSpace(genericName))
            {
                RefType = LoquiRefType.Generic;
                _generic = genericName;
                GenericDef = ObjectGen.Generics[_generic];
                GenericDef.Loqui = true;
                if (Singleton)
                {
                    throw new ArgumentException("Cannot be a generic and singleton.");
                }
            }
            else if (usingInterface)
            {
                RefType = LoquiRefType.Interface;
            }
            else
            {
                throw new ArgumentException("Ref type needs a target.");
            }
        }

        ReadOnly = ReadOnly || Singleton;
        _thisConstruction = node.GetAttribute(Constants.THIS_CTOR, _thisConstruction);
    }

    public bool ParseRefNode(XElement refNode)
    {
        if (string.IsNullOrWhiteSpace(RefName)) return false;

        SetterInterfaceType = refNode.GetAttribute<LoquiInterfaceType>(Constants.SET_INTERFACE_TYPE, ObjectGen.SetterInterfaceTypeDefault);
        GetterInterfaceType = refNode.GetAttribute<LoquiInterfaceType>(Constants.GET_INTERFACE_TYPE, ObjectGen.GetterInterfaceTypeDefault);

        RefType = LoquiRefType.Direct;
        if (!ObjectNamedKey.TryFactory(RefName, ProtoGen.Protocol, out var namedKey)
            || !ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(namedKey, out _TargetObjectGeneration))
        {
            throw new ArgumentException("Loqui type cannot be found: " + RefName);
        }

        GenericSpecification = new GenericSpecification();
        foreach (var specNode in refNode.Elements(XName.Get(Constants.GENERIC_SPECIFICATION, LoquiGenerator.Namespace)))
        {
            GenericSpecification.Specifications.Add(
                specNode.Attribute(Constants.TYPE_TO_SPECIFY).Value,
                specNode.Attribute(Constants.DEFINITION).Value);
        }
        foreach (var mapNode in refNode.Elements(XName.Get(Constants.GENERIC_MAPPING, LoquiGenerator.Namespace)))
        {
            GenericSpecification.Mappings.Add(
                mapNode.Attribute(Constants.TYPE_ON_REF).Value,
                mapNode.Attribute(Constants.TYPE_ON_OBJECT).Value);
        }
        foreach (var generic in TargetObjectGeneration.Generics)
        {
            if (GenericSpecification.Specifications.ContainsKey(generic.Key)) continue;
            if (GenericSpecification.Mappings.ContainsKey(generic.Key)) continue;
            GenericSpecification.Mappings.Add(
                generic.Key,
                generic.Key);
        }
        return true;
    }

    public string CommonClassInstance(Accessor accessor, bool getter, LoquiInterfaceType interfaceType, CommonGenerics commonGen, params MaskType[] types)
    {
        return $"(({_TargetObjectGeneration.CommonClass(interfaceType, commonGen, types)})(({Interface(getter: true, internalInterface: false)}){accessor}).Common{_TargetObjectGeneration.CommonNameAdditions(interfaceType, types)}Instance{GetGenericTypes(getter: getter, types)}())";
    }

    public override void GenerateForCopy(
        StructuredStringBuilder sb,
        Accessor accessor,
        Accessor rhs, 
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy)
    {
        if (!AlwaysCopy)
        {
            sb.AppendLine($"if ({(deepCopy ? GetTranslationIfAccessor(copyMaskAccessor) : SkipCheck(copyMaskAccessor, deepCopy))})");
        }
        using (sb.CurlyBrace(doIt: !AlwaysCopy))
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                sb,
                () =>
                {
                    if (_TargetObjectGeneration is StructGeneration)
                    {
                        sb.AppendLine($"{accessor.Access} = new {TargetObjectGeneration.Name}({rhs});");
                        return;
                    }

                    if (Singleton)
                    {
                        GenerateCopyIn(
                            sb,
                            accessor: accessor,
                            rhs: rhs,
                            copyMaskAccessor: copyMaskAccessor,
                            deepCopy: deepCopy);
                        return;
                    }

                    if (!Nullable)
                    {
                        if (deepCopy)
                        {
                            sb.AppendLine($"if ({GetTranslationIfAccessor(copyMaskAccessor)})");
                            using (sb.CurlyBrace())
                            {
                                if (Nullable)
                                {
                                    sb.AppendLine($"if ({rhs} == null)");
                                    using (sb.CurlyBrace())
                                    {
                                        sb.AppendLine($"{accessor.Access} = null;");
                                    }
                                    sb.AppendLine($"else");
                                    using (sb.CurlyBrace())
                                    {
                                        using (var args = sb.Call(
                                                   $"{accessor.Access} = {rhs}.DeepCopy{(SetterInterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                                        {
                                            if (deepCopy)
                                            {
                                                args.Add($"copyMask: {copyMaskAccessor}?.GetSubCrystal({IndexEnumInt})");
                                            }
                                            else
                                            {
                                                args.Add($"copyMask: {copyMaskAccessor}.Specific");
                                            }
                                            args.AddPassArg("errorMask");
                                        }
                                    }
                                }
                                else
                                {
                                    using (var args = sb.Call(
                                               $"{accessor.Access} = {rhs}.DeepCopy{(SetterInterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                                    {
                                        if (deepCopy)
                                        {
                                            args.Add($"copyMask: {copyMaskAccessor}?.GetSubCrystal({IndexEnumInt})");
                                        }
                                        else
                                        {
                                            args.Add($"copyMask: {copyMaskAccessor}.Specific");
                                        }
                                        args.AddPassArg("errorMask");
                                    }
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine($"switch ({copyMaskAccessor}?.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                            using (sb.CurlyBrace())
                            {
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                using (sb.IncreaseDepth())
                                {
                                    if (GetterInterfaceType == LoquiInterfaceType.IGetter)
                                    {
                                        sb.AppendLine($"{accessor.Access} = Utility.GetGetterInterfaceReference<{TypeName()}>({rhs});");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"{accessor.Access} = {rhs};");
                                    }
                                    sb.AppendLine("break;");
                                }
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                                if (SetterInterfaceType != LoquiInterfaceType.IGetter)
                                {
                                    using (sb.IncreaseDepth())
                                    {
                                        GenerateCopyIn(
                                            sb,
                                            accessor: accessor,
                                            rhs: rhs,
                                            copyMaskAccessor: copyMaskAccessor,
                                            deepCopy: deepCopy);
                                        sb.AppendLine("break;");
                                    }
                                }
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                using (sb.IncreaseDepth())
                                {
                                    sb.AppendLine($"if ({rhs} == null)");
                                    using (sb.CurlyBrace())
                                    {
                                        sb.AppendLine($"{accessor.Access} = null;");
                                    }
                                    sb.AppendLine($"else");
                                    using (sb.CurlyBrace())
                                    {
                                        using (var args = sb.Call(
                                                   $"{accessor.Access} = {rhs}.Copy{(SetterInterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                                        {
                                            args.Add($"{copyMaskAccessor}?.Specific");
                                        }
                                    }
                                    sb.AppendLine("break;");
                                }
                                sb.AppendLine($"default:");
                                using (sb.IncreaseDepth())
                                {
                                    sb.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}{(RefType == LoquiRefType.Direct ? $"?.Overall" : Name)}}}. Cannot execute copy.\");");
                                }
                            }
                        }
                        return;
                    }

                    sb.AppendLine($"if({rhs} is {{}} rhs{Name})");
                    using (sb.CurlyBrace())
                    {
                        if (ObjectGen.GenerateComplexCopySystems)
                        {
                            sb.AppendLine($"switch ({copyMaskAccessor}{(RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                            using (sb.CurlyBrace())
                            {
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                using (sb.IncreaseDepth())
                                {
                                    if (GetterInterfaceType == LoquiInterfaceType.IGetter)
                                    {
                                        sb.AppendLine("throw new NotImplementedException(\"Need to implement an ISetter copy function to support reference copies.\");");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"{accessor.Access} = {rhs};");
                                        sb.AppendLine("break;");
                                    }
                                }
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                                if (SetterInterfaceType != LoquiInterfaceType.IGetter)
                                {
                                    using (sb.IncreaseDepth())
                                    {
                                        GenerateCopyIn(
                                            sb,
                                            accessor: accessor,
                                            rhs: rhs,
                                            copyMaskAccessor: copyMaskAccessor,
                                            deepCopy: deepCopy);
                                        sb.AppendLine("break;");
                                    }
                                }
                                sb.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                using (sb.IncreaseDepth())
                                {
                                    GenerateTypicalMakeCopy(
                                        sb,
                                        retAccessor: $"{accessor.Access} = ",
                                        rhsAccessor: rhs,
                                        copyMaskAccessor: copyMaskAccessor,
                                        deepCopy: deepCopy,
                                        doTranslationMask: true);
                                    sb.AppendLine("break;");
                                }
                                sb.AppendLine($"default:");
                                using (sb.IncreaseDepth())
                                {
                                    sb.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}{(RefType == LoquiRefType.Direct ? $"?.Overall" : string.Empty)}}}. Cannot execute copy.\");");
                                }
                            }
                        }
                        else
                        {
                            GenerateTypicalMakeCopy(
                                sb,
                                retAccessor: $"{accessor.Access} = ",
                                rhsAccessor: new Accessor($"rhs{Name}"),
                                copyMaskAccessor: copyMaskAccessor,
                                deepCopy: deepCopy,
                                doTranslationMask: true);
                        }
                    }
                    sb.AppendLine("else");
                    using (sb.CurlyBrace())
                    {
                        sb.AppendLine($"{accessor.Access} = default;");
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: HasIndex ? IndexEnumInt : default(Accessor),
                doIt: CopyNeedsTryCatch);
        }
    }

    public virtual void GenerateTypicalMakeCopy(
        StructuredStringBuilder sb,
        Accessor retAccessor,
        Accessor rhsAccessor,
        Accessor copyMaskAccessor,
        bool deepCopy,
        bool doTranslationMask)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
                using (var args = sb.Call(
                           $"{retAccessor}{rhsAccessor.Access}.DeepCopy{GetGenericTypes(getter: true, MaskType.Normal, MaskType.NormalGetter)}"))
                {
                    args.AddPassArg("errorMask");
                    if (RefType == LoquiRefType.Direct)
                    {
                        if (!doTranslationMask)
                        {
                            args.Add($"default(TranslationCrystal)");
                        }
                        else if (deepCopy)
                        {
                            args.Add($"{copyMaskAccessor}?.GetSubCrystal({IndexEnumInt})");
                        }
                        else
                        {
                            args.Add($"{copyMaskAccessor}?.Specific");
                        }
                    }
                }
                break;
            case LoquiRefType.Generic:
                if (deepCopy)
                {
                    sb.AppendLine($"{retAccessor}(r.DeepCopy() as {_generic})!;");
                }
                else
                {
                    sb.AppendLine($"{retAccessor}{nameof(LoquiRegistration)}.GetCopyFunc<{_generic}, {_generic}Getter>()({rhsAccessor.Access}, null);");
                }
                break;
            case LoquiRefType.Interface:
                if (deepCopy)
                {
                    sb.AppendLine($"{retAccessor}r.DeepCopy() as {TypeNameInternal(getter: false, internalInterface: true)};");
                }
                else
                {
                    sb.AppendLine($"{retAccessor}{nameof(LoquiRegistration)}.GetCopyFunc<{TypeName()}>(r.GetType())({rhsAccessor.Access}, null);");
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void GenerateCopyIn(
        StructuredStringBuilder sb,
        Accessor accessor,
        Accessor rhs,
        Accessor copyMaskAccessor,
        bool deepCopy)
    {
        if (RefType == LoquiRefType.Direct)
        {
            var funcStr = deepCopy
                ? $"{accessor.Access}.DeepCopyIn"
                : $"{accessor.Access}.CopyIn";
            using (var args = sb.Call( funcStr))
            {
                args.Add($"rhs: {rhs}");
                if (RefType == LoquiRefType.Direct)
                {
                    args.Add($"errorMask: errorMask");
                    if (deepCopy)
                    {
                        args.Add($"copyMask: {copyMaskAccessor}?.GetSubCrystal({IndexEnumInt})");
                    }
                    else
                    {
                        args.Add($"copyMask: {copyMaskAccessor}.Specific");
                    }
                }
                else
                {
                    args.Add($"errorMask: null");
                    args.Add($"copyMask: null");
                }
            }
        }
        else
        {
            sb.AppendLine("throw new NotImplementedException();");
        }
    }

    public override void GenerateUnsetNth(StructuredStringBuilder sb, Accessor identifier)
    {
        if (Singleton)
        {
            base.GenerateUnsetNth(sb, identifier);
            return;
        }
        if (SetterInterfaceType == LoquiInterfaceType.IGetter)
        {
            sb.AppendLine($"throw new ArgumentException(\"Cannot unset a get only singleton: {Name}\");");
        }
        else
        {
            sb.AppendLine($"{TargetObjectGeneration.CommonClassName(LoquiInterfaceType.ISetter)}.Clear({identifier.Access});");
            sb.AppendLine("break;");
        }
    }

    public override void GenerateSetNth(StructuredStringBuilder sb, Accessor accessor, Accessor rhs, bool internalUse)
    {
        if (Singleton)
        {
            if (!internalUse && SetterInterfaceType == LoquiInterfaceType.IGetter)
            {
                sb.AppendLine($"throw new ArgumentException(\"Cannot set singleton member {Name}\");");
            }
            else
            {
                sb.AppendLine($"{accessor}.CopyIn{GetGenericTypes(getter: false, MaskType.Normal, MaskType.Copy)}(rhs: {rhs});");
                sb.AppendLine("break;");
            }
        }
        else
        {
            base.GenerateSetNth(sb, accessor, rhs, internalUse);
        }
    }

    public override void GenerateForInterface(StructuredStringBuilder sb, bool getter, bool internalInterface)
    {
        if (getter)
        {
            Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            sb.AppendLine($"{TypeNameInternal(getter: true, internalInterface: true)}{NullChar} {Name} {{ get; }}");
        }
        else
        {
            if (Singleton)
            {
                if (GetterInterfaceType != SetterInterfaceType)
                {
                    Comments?.Apply(sb, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
                    sb.AppendLine($"new {TypeName(getter: false)} {Name} {{ get; }}");
                }
            }
            else
            {
                base.GenerateForInterface(sb, getter, internalInterface);
            }
        }
    }

    public override IEnumerable<string> GetRequiredNamespaces()
    {
        if (TargetObjectGeneration != null)
        {
            yield return TargetObjectGeneration.Namespace;
        }
    }

    public override void GenerateClear(StructuredStringBuilder sb, Accessor accessorPrefix)
    {
        if (Singleton
            || !Nullable)
        {
            sb.AppendLine($"{accessorPrefix}.Clear();");
        }
        else
        {
            sb.AppendLine($"{accessorPrefix} = null;");
        }
    }

    public override string GenerateACopy(string rhsAccessor)
    {
        return $"{_TargetObjectGeneration.ObjectName}.Copy({rhsAccessor})";
    }

    public string GenerateMaskString(string type)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
                return TargetObjectGeneration.GetMaskString(type);
            case LoquiRefType.Generic:
                if (TargetObjectGeneration != null)
                {
                    return TargetObjectGeneration.GetMaskString(type);
                }
                else
                {
                    return $"IMask<{type}>";
                }
            case LoquiRefType.Interface:
                return $"IMask<{type}>";
            default:
                throw new NotImplementedException();
        }
    }

    public override void GenerateForEquals(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
    {
        if (!IntegrateField) return;
        sb.AppendLine($"if ({GetTranslationIfAccessor(maskAccessor)})");
        using (sb.CurlyBrace())
        {
            if (GenericSpecification.Specifications.Count == 0)
            {
                sb.AppendLine($"if (EqualsMaskHelper.RefEquality({accessor.Access}, {rhsAccessor.Access}, out var lhs{Name}, out var rhs{Name}, out var is{Name}Equal))");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"if (!{TargetObjectGeneration.CommonClassInstance($"lhs{Name}", LoquiInterfaceType.IGetter, CommonGenerics.Class)}.Equals(lhs{Name}, rhs{Name}, crystal?.GetSubCrystal({IndexEnumInt}))) return false;");
                }
                sb.AppendLine($"else if (!is{Name}Equal) return false;");

            }
            else
            {
                // ToDo
                // Upgrade to pass along translation crystal
                sb.AppendLine($"if (EqualsMaskHelper.RefEquality({accessor.Access}, {rhsAccessor.Access}, out var lhs{Name}, out var rhs{Name}, out var is{Name}Equal))");
                using (sb.CurlyBrace())
                {
                    sb.AppendLine($"if (!object.Equals(lhs{Name}, rhs{Name})) return false;");
                }
                sb.AppendLine($"else if (!is{Name}Equal) return false;");
            }
        }
    }

    public override void GenerateForEqualsMask(StructuredStringBuilder sb, Accessor accessor, Accessor rhsAccessor, string retAccessor)
    {
        if (Nullable)
        {
            using (var args = sb.Call(
                       $"{retAccessor} = EqualsMaskHelper.{nameof(EqualsMaskHelper.EqualsHelper)}"))
            {
                args.Add(accessor.Access);
                args.Add(rhsAccessor.Access);
                args.Add($"(loqLhs, loqRhs, incl) => loqLhs.GetEqualsMask(loqRhs, incl)");
                args.Add("include");
            }
        }
        else
        {
            if (TargetObjectGeneration == null)
            {
                sb.AppendLine($"{retAccessor} = new MaskItem<bool, {GenerateMaskString("bool")}>();");
                using (sb.CurlyBrace(appendSemiColon: true))
                {
                    sb.AppendLine($"Overall = object.Equals({accessor.Access}, {rhsAccessor.Access})");
                }
            }
            else
            {
                sb.AppendLine($"{retAccessor} = MaskItemExt.Factory({accessor.Access}.GetEqualsMask({rhsAccessor.Access}, include), include);");
            }
        }
    }

    public override void GenerateToString(StructuredStringBuilder sb, string name, Accessor accessor, string sbAccessor)
    {
        sb.AppendLine($"{accessor.Access}?.Print({sbAccessor}, \"{name}\");");
    }

    public override void GenerateForNullableCheck(StructuredStringBuilder sb, Accessor accessor, string checkMaskAccessor)
    {
        if (!Nullable) return;
        sb.AppendLine($"if ({checkMaskAccessor}?.Overall.HasValue ?? false && {checkMaskAccessor}.Overall.Value != {NullableAccessor(getter: true, accessor: accessor)}) return false;");
        if (TargetObjectGeneration != null)
        {
            sb.AppendLine($"if ({checkMaskAccessor}?.Specific != null && ({accessor.Access} == null || !{accessor.Access}.HasBeenSet({checkMaskAccessor}.Specific))) return false;");
        }
    }

    public IEnumerable<string> GetGenericTypesEnumerable(bool getter, params MaskType[] additionalMasks)
    {
        return GetGenericTypesEnumerable(getter: getter, typeOverride: null, additionalMasks: additionalMasks);
    }

    public IEnumerable<string> GetGenericTypesEnumerable(bool getter, LoquiInterfaceType? typeOverride, MaskType[] additionalMasks)
    {
        if (GenericSpecification == null) return null;
        if (TargetObjectGeneration.Generics.Count == 0) return null;
        List<string> ret = new List<string>();
        foreach (var gen in TargetObjectGeneration.Generics)
        {
            if (GenericSpecification.Specifications.TryGetValue(gen.Key, out var spec))
            {
                if (ObjectNamedKey.TryFactory(spec, ObjectGen.ProtoGen.Protocol, out var namedKey)
                    && ObjectGen.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(
                        namedKey,
                        out var targetObjGen))
                {
                    foreach (var mType in additionalMasks)
                    {
                        switch (mType)
                        {
                            case MaskType.Normal:
                                ret.Add(targetObjGen.GetTypeName(typeOverride ?? (getter && !additionalMasks.Contains(MaskType.NormalGetter) ? GetterInterfaceType : SetterInterfaceType)));
                                break;
                            case MaskType.NormalGetter:
                                ret.Add(targetObjGen.GetTypeName(typeOverride ?? GetterInterfaceType));
                                break;
                            case MaskType.Error:
                                ret.Add(targetObjGen.Mask(MaskType.Error));
                                break;
                            case MaskType.Copy:
                                ret.Add(targetObjGen.Mask(MaskType.Copy));
                                break;
                            case MaskType.Translation:
                                ret.Add(targetObjGen.Mask(MaskType.Translation));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                else
                {
                    ret.Add(spec);
                }
            }
            else if (GenericSpecification.Mappings.TryGetValue(gen.Key, out var mapping))
            {
                foreach (var mType in additionalMasks)
                {
                    ret.Add(TargetObjectGeneration.MaskNickname(mapping, mType));
                }
            }
            else
            {
                throw new ArgumentException("Generic specifications were missing some needing mappings.");
            }
        }
        return ret;
    }

    public string GetGenericTypes(bool getter, params MaskType[] additionalMasks)
    {
        return GetGenericTypes(getter: getter, typeOverride: null, additionalMasks: additionalMasks);
    }

    public string GetGenericTypes(bool getter, LoquiInterfaceType? typeOverride, MaskType[] additionalMasks)
    {
        var e = GetGenericTypesEnumerable(getter, typeOverride, additionalMasks);
        if (e == null) return null;
        return $"<{string.Join(", ", e)}>";
    }

    public LoquiType Spawn(ObjectGeneration target)
    {
        switch (RefType)
        {
            case LoquiRefType.Direct:
            case LoquiRefType.Interface:
                break;
            case LoquiRefType.Generic:
            default:
                throw new NotImplementedException();
        }
        LoquiType ret = ObjectGen.ProtoGen.Gen.GetTypeGeneration<LoquiType>();
        ret._TargetObjectGeneration = target;
        ret.RefName = target.Name;
        ret.RefType = RefType;
        ret.Name = Name;
        ret.SetObjectGeneration(ObjectGen, setDefaults: true);
        foreach (var custom in CustomData)
        {
            ret.CustomData[custom.Key] = custom.Value;
        }
        return ret;
    }

    public bool TryGetSpecificationAsObject(string genName, out ObjectGeneration obj)
    {
        var specifications = GenericSpecification?.Specifications;
        if (specifications == null)
        {
            obj = null;
            return false;
        }

        if (!specifications.TryGetValue(genName, out var specVal))
        {
            obj = null;
            return false;
        }

        if (!ObjectNamedKey.TryFactory(specVal, out var objKey))
        {
            obj = null;
            return false;
        }

        return ObjectGen.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(
            objKey,
            out obj);
    }

    public bool SupportsMask(MaskType maskType)
    {
        if (RefType != LoquiRefType.Interface) return true;
        switch (maskType)
        {
            case MaskType.Error:
            case MaskType.Normal:
                return true;
            case MaskType.Copy:
            case MaskType.Translation:
                return false;
            default:
                throw new NotImplementedException();
        }
    }

    public override bool Equals(object obj)
    {
        if (!(obj is LoquiType rhs)) return false;
        return Equals(rhs);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(RefType);

        var ret = RefType.GetHashCode();
        switch (RefType)
        {
            case LoquiRefType.Direct:
                hash.Add(TargetObjectGeneration);
                break;
            case LoquiRefType.Interface:
                hash.Add(HashCode.Combine(GetterInterface, SetterInterface));
                break;
            case LoquiRefType.Generic:
                hash.Add(_generic);
                break;
            default:
                throw new NotImplementedException();
        }
        return ret;
    }

    public bool Equals(LoquiType other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return true;
        switch (RefType)
        {
            case LoquiRefType.Direct:
                return ReferenceEquals(TargetObjectGeneration, other.TargetObjectGeneration);
            case LoquiRefType.Interface:
                return string.Equals(GetterInterface, other.GetterInterface)
                       && string.Equals(SetterInterface, other.SetterInterface);
            case LoquiRefType.Generic:
                return string.Equals(_generic, other._generic);
            default:
                throw new NotImplementedException();
        }
    }

    public override string GetDuplicate(Accessor accessor)
    {
        return $"{accessor}.DeepCopy()";
    }
}