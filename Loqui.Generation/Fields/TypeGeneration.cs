using Noggog;
using System.Reactive.Subjects;
using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TypeGeneration
{
    public ObjectGeneration ObjectGen { get; private set; }
    public ProtocolGeneration ProtoGen => ObjectGen.ProtoGen;
    public bool KeyField { get; protected set; }
    public abstract string TypeName(bool getter, bool needsCovariance = false);
    public virtual string Name { get; set; }
    public string IndexEnumName => $"{ObjectGen.FieldIndexName}.{Name}";
    public string ObjectCentralizationEnumName => IndexEnumName;
    public string IndexEnumInt => $"(int){IndexEnumName}";
    public bool HasIndex => !string.IsNullOrWhiteSpace(Name) && IntegrateField;
    public virtual string ProtectedName => Name;
    protected bool? _derivative;
    public virtual bool Derivative => _derivative ?? false;
    public virtual bool IntegrateField { get; set; } = true;
    public bool Enabled = true;
    public bool ReadOnly;
    public PermissionLevel SetPermission = PermissionLevel.@public;
    public PermissionLevel GetPermission = PermissionLevel.@public;
    private CopyLevel _copy = CopyLevel.All;
    public virtual CopyLevel CopyLevel => _copy;
    public bool AlwaysCopy { get; set; }
    public bool TrueReadOnly => ObjectGen is StructGeneration;
    public bool GenerateClassMembers = true;
    public bool GenerateInterfaceMembers = true;
    public abstract bool IsEnumerable { get; }
    public readonly BehaviorSubject<(bool Item, bool HasBeenSet)> NullableProperty = new BehaviorSubject<(bool Item, bool HasBeenSet)>((default, default));
    public virtual bool Nullable => NullableProperty.Value.Item;
    public virtual bool CanBeNullable(bool getter) => true;
    public Dictionary<object, object> CustomData = new Dictionary<object, object>();
    public XElement Node;
    public virtual bool Namable => true;
    public abstract bool IsClass { get; }
    public virtual bool IsReference => IsClass;
    public virtual bool ReferenceChanged => IsReference;
    public abstract bool HasDefault { get; }
    public bool InternalSetInterface { get; set; }
    public bool InternalGetInterface { get; set; }
    public virtual bool IsIEquatable => true;
    public string SetPermissionStr => SetPermission == PermissionLevel.@public ? null : $"{SetPermission.ToStringFast_Enum_Only()} ";
    public TypeGeneration Parent;
    public virtual bool IsNullable => Nullable;
    public string NullChar => IsNullable ? "?" : null;
    public bool CustomClear { get; set; }
    public bool Override { get; set; }
    public string OverrideStr => Override ? "override " : string.Empty;

    public CommentCollection Comments;

    public void SetObjectGeneration(ObjectGeneration obj, bool setDefaults)
    {
        ObjectGen = obj;
        if (!setDefaults) return;
        if (!NullableProperty.Value.HasBeenSet)
        {
            NullableProperty.OnNext((ObjectGen.NullableDefault, false));
        }
        if (_derivative == null)
        {
            _derivative = ObjectGen.DerivativeDefault;
        }
    }

    public virtual async Task Load(XElement node, bool requireName = true)
    {
        Node = node;
        LoadTypeGenerationFromNode(node, requireName);
    }

    protected void LoadTypeGenerationFromNode(XElement node, bool requireName = true)
    {
        // TODO load comments.
        node.TransferAttribute<bool>(Constants.HIDDEN_FIELD, i => IntegrateField = !i);
        Name = node.GetAttribute<string>(Constants.NAME);
        node.TransferAttribute<bool>(Constants.KEY_FIELD, i => KeyField = i);
        node.TransferAttribute<bool>(Constants.DERIVATIVE, i => _derivative = i);
        if (_derivative ?? false)
        {
            SetPermission = PermissionLevel.@protected;
        }
        node.TransferAttribute<PermissionLevel>(Constants.SET_PERMISSION, i => SetPermission = i);
        ReadOnly = SetPermission != PermissionLevel.@public || Derivative;
        node.TransferAttribute<PermissionLevel>(Constants.GET_PERMISSION, i => GetPermission = i);
        if (Derivative || !IntegrateField)
        {
            _copy = CopyLevel.None;
        }
        _copy = node.GetAttribute<CopyLevel>(Constants.COPY, _copy);
        node.TransferAttribute<bool>(Constants.GENERATE_CLASS_MEMBERS, i => GenerateClassMembers = i);
        node.TransferAttribute<bool>(Constants.GENERATE_INTERFACE_MEMBERS, i => GenerateInterfaceMembers = i);
        node.TransferAttribute<bool>(Constants.NULLABLE, i => NullableProperty.OnNext((i, true)));
        node.TransferAttribute<bool>(Constants.INTERNAL_SET_INTERFACE, i => InternalSetInterface = i);
        node.TransferAttribute<bool>(Constants.INTERNAL_GET_INTERFACE, i => InternalGetInterface = i);
        node.TransferAttribute<bool>(Constants.CUSTOM_CLEAR, i => CustomClear = i);
        node.TransferAttribute<bool>(Constants.ALWAYS_COPY, i => AlwaysCopy = i);
        node.TransferAttribute<bool>(Constants.OVERRIDE, i => Override = i);
        if (requireName && Namable && Name == null && IntegrateField)
        {
            throw new ArgumentException("Type field needs a name.");
        }
    }

    public virtual string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
    {
        return $"{(negate ? "!" : null)}object.Equals({accessor.Access}, {rhsAccessor.Access})";
    }

    public void FinalizeField()
    {
        if ((_derivative ?? false) && !ReadOnly)
        {
            throw new ArgumentException("Cannot mark field as non-readonly if also derivative.  Being derivative implied being readonly.");
        }
    }

    public virtual IEnumerable<string> GetRequiredNamespaces()
    {
        yield break;
    }

    public virtual async Task GenerateForCtor(FileGeneration fg)
    {
    }

    public abstract Task GenerateForClass(FileGeneration fg);

    public abstract void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface);

    public abstract bool CopyNeedsTryCatch { get; }

    public abstract string SkipCheck(Accessor copyMaskAccessor, bool deepCopy);

    public abstract void GenerateForCopy(
        FileGeneration fg,
        Accessor accessor,
        Accessor rhs,
        Accessor copyMaskAccessor,
        bool protectedMembers,
        bool deepCopy);

    public virtual void GenerateCopySetToConverter(FileGeneration fg) { }

    public abstract string GenerateACopy(string rhsAccessor);

    public abstract void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse);

    public abstract void GenerateUnsetNth(FileGeneration fg, Accessor identifier);

    public abstract void GenerateGetNth(FileGeneration fg, Accessor identifier);

    public abstract void GenerateClear(FileGeneration fg, Accessor accessorPrefix);

    public abstract void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor);

    public abstract void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor);

    public abstract void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor);

    public virtual void GenerateInCommon(FileGeneration fg, MaskTypeSet maskTypes) { }

    public virtual void GenerateForStaticCtor(FileGeneration fg) { }

    public virtual void GenerateGetNameIndex(FileGeneration fg)
    {
        if (!IntegrateField || !Enabled) return;
        fg.AppendLine($"return (ushort){ObjectGen.FieldIndexName}.{Name};");
    }

    public virtual void GenerateGetNthName(FileGeneration fg)
    {
        if (!IntegrateField || !Enabled) return;
        fg.AppendLine($"return \"{Name}\";");
    }

    public virtual void GenerateGetNthType(FileGeneration fg)
    {
        if (!IntegrateField || !Enabled) return;
        fg.AppendLine($"return typeof({TypeName(getter: false)});");
    }

    public virtual void GenerateInRegistration(FileGeneration fg)
    { 
    }

    public abstract void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor);

    public abstract void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor);

    public virtual string EqualsMaskAccessor(string accessor) => accessor;

    public virtual string GetName(bool internalUse)
    {
        if (internalUse)
        {
            return ProtectedName;
        }
        else
        {
            return Name;
        }
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Name)) return base.ToString();
        return $"{base.ToString()}: {Name}";
    }

    public virtual async Task Resolve()
    {
        ObjectGen.RequiredNamespaces.Add(GetRequiredNamespaces());
    }

    public virtual string NullableAccessor(bool getter, Accessor accessor = null)
    {
        if (accessor == null)
        {
            if (CanBeNullable(getter))
            {
                return $"({Name} != null)";
            }
            else
            {
                return $"{Name}_IsSet";
            }
        }
        else
        {
            if (CanBeNullable(getter))
            {
                return $"({accessor.Access} != null)";
            }
            else
            {
                return $"{accessor.Access}_IsSet";
            }
        }
    }

    public bool ApplicableInterfaceField(bool getter, bool internalInterface)
    {
        if (!IntegrateField) return false;
        if (internalInterface)
        {
            if (getter && !InternalGetInterface) return false;
            if (!getter && !InternalSetInterface) return false;
        }
        else
        {
            if (getter && InternalGetInterface) return false;
            if (!getter && (ReadOnly || InternalSetInterface)) return false;
        }
        return true;
    }

    public virtual string GetTranslationIfAccessor(Accessor translationCrystalAccessor)
    {
        return $"({translationCrystalAccessor}?.GetShouldTranslate({IndexEnumInt}) ?? true)";
    }

    public virtual string GetDefault(bool getter) => "default";

    public abstract string GetDuplicate(Accessor accessor);

    public (int PublicIndex, int InternalIndex, TypeGeneration Field) GetIndexData()
    {
        return ObjectGen.IterateFieldIndices().First(i => i.Field == this);
    }
}