using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class LoquiType : PrimitiveType, IEquatable<LoquiType>
    {
        public override string TypeName(bool getter = false)
        {
            switch (RefType)
            {
                case LoquiRefType.Direct:
                    switch (getter ? this.GetterInterfaceType : this.SetterInterfaceType)
                    {
                        case LoquiInterfaceType.Direct:
                            return DirectTypeName;
                        case LoquiInterfaceType.IGetter:
                            return $"{this.Interface(getter: true, internalInterface: this.InternalGetInterface)}";
                        case LoquiInterfaceType.ISetter:
                            return $"{this.Interface(getter: false, internalInterface: this.InternalSetInterface)}";
                        default:
                            throw new NotImplementedException();
                    }
                case LoquiRefType.Generic:
                    return _generic;
                case LoquiRefType.Interface:
                    return getter ? this.GetterInterface : this.SetterInterface;
                default:
                    throw new NotImplementedException();
            }
        }

        public string TypeName(bool getter, bool internalInterface)
        {
            switch (RefType)
            {
                case LoquiRefType.Direct:
                    switch (getter ? this.GetterInterfaceType : this.SetterInterfaceType)
                    {
                        case LoquiInterfaceType.Direct:
                            return DirectTypeName;
                        case LoquiInterfaceType.IGetter:
                            return $"{this.Interface(getter: true, internalInterface: internalInterface)}";
                        case LoquiInterfaceType.ISetter:
                            return $"{this.Interface(getter: false, internalInterface: internalInterface)}";
                        default:
                            throw new NotImplementedException();
                    }
                case LoquiRefType.Generic:
                    return _generic;
                case LoquiRefType.Interface:
                    return getter ? this.GetterInterface : this.SetterInterface;
                default:
                    throw new NotImplementedException();
            }
        }

        public string DirectTypeName => $"{this._TargetObjectGeneration.Name}{this.GenericTypes(getter: false)}";

        // ToDo
        // Perhaps implement detection for those that are
        public override bool IsIEquatable => false;

        public override string ProtectedName
        {
            get
            {
                if (this.SingletonType == SingletonLevel.Singleton
                    && !this.ObjectCentralized)
                {
                    return SingletonObjectName;
                }
                else if (this.SingletonType == SingletonLevel.None)
                {
                    return base.Name;
                }
                else
                {
                    return base.ProtectedName;
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
                        return this._TargetObjectGeneration.Name;
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
                    return this._TargetObjectGeneration.Interface(GenericTypes(getter), getter: getter, internalInterface: internalInterface);
                case LoquiRefType.Generic:
                    return _generic;
                default:
                    throw new NotImplementedException();
            }
        }
        public string GenericTypes(bool getter) => GetGenericTypes(getter, MaskType.Normal);
        public SingletonLevel SingletonType;
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
                        return this.GenericDef.BaseObjectGeneration;
                    case LoquiRefType.Interface:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public string SingletonObjectName => $"_{this.Name}_Object";
        public override Type Type(bool getter) => throw new NotImplementedException();
        public string RefName;
        public string SetterInterface;
        public string GetterInterface;
        public override bool HasBeenSet => base.HasBeenSet && this.SingletonType == SingletonLevel.None;
        public bool CanStronglyType => this.RefType != LoquiRefType.Interface;
        public override bool Copy => base.Copy && !(this.SetterInterfaceType == LoquiInterfaceType.IGetter && this.SingletonType == SingletonLevel.Singleton);
        // Adds "this" to constructor parameters as it is a common pattern to tie child to parent
        // Can probably be replaced with a more robust parameter configuration setup later
        public bool ThisConstruction;

        public enum LoquiRefType
        {
            Direct,
            Interface,
            Generic
        }


        public override string SkipCheck(string copyMaskAccessor, bool deepCopy)
        {
            if (this.SingletonType == SingletonLevel.Singleton
                || deepCopy)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
                case LoquiRefType.Generic:
                    return $"{copyMaskAccessor}?.{this.Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual string GetMaskString(string str)
        {
            return this.TargetObjectGeneration?.GetMaskString(str) ?? $"IMask<{str}>";
        }

        public override string EqualsMaskAccessor(string accessor) => $"{accessor}.Overall";

        public string Mask(MaskType type)
        {
            if (this.GenericDef != null)
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
            else if (this.GenericSpecification != null)
            {
                return this.TargetObjectGeneration.Mask_Specified(type, this.GenericSpecification);
            }
            else if (this.TargetObjectGeneration != null)
            {
                return this.TargetObjectGeneration.Mask(type);
            }
            else if (this.RefType == LoquiRefType.Interface)
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

        public override async Task GenerateForCtor(FileGeneration fg)
        {
            await base.GenerateForCtor(fg);

            if (this.ThisConstruction)
            {
                switch (this.SingletonType)
                {
                    case SingletonLevel.None:
                    case SingletonLevel.NotNull:
                        fg.AppendLine($"_{this.Name} = new {this.DirectTypeName}(this);");
                        break;
                    case SingletonLevel.Singleton:
                        fg.AppendLine($"{this.SingletonObjectName} = new {this.DirectTypeName}(this);");
                        break;
                    default:
                        break;
                }
            }

            if (this.Bare) return;
            if (this.Notifying && this.ObjectCentralized)
            {
                if (this.SingletonType == SingletonLevel.Singleton)
                {
                    fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = true;");
                }
                return;
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    if (!this.ObjectCentralized)
                    {
                        throw new NotImplementedException();
                    }
                    if (this.SingletonType == SingletonLevel.Singleton)
                    {
                        fg.AppendLine($"private readonly {this.DirectTypeName} {this.SingletonObjectName}{(this.ThisConstruction ? null : $" = new {this.DirectTypeName}()")};");
                        fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))} => true;");
                        fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName()} {this.Name} => {this.SingletonObjectName};");
                    }
                    else
                    {
                        fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))}");
                        using (new BraceWrapper(fg))
                        {
                            if (this.ObjectCentralized)
                            {
                                fg.AppendLine($"get => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                                fg.AppendLine($"{SetPermissionStr}set => this.RaiseAndSetIfChanged(_hasBeenSetTracker, value, (int){this.ObjectCentralizationEnumName}, nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                            }
                        }
                        fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                        switch (this.SingletonType)
                        {
                            case SingletonLevel.None:
                                fg.AppendLine($"private {this.TypeName()} _{this.Name};");
                                break;
                            case SingletonLevel.NotNull:
                                fg.AppendLine($"private {this.TypeName()} _{this.Name}{(this.ThisConstruction ? null : $" = new {this.TypeName()}();")}");
                                break;
                            case SingletonLevel.Singleton:
                                throw new NotImplementedException();
                            default:
                                break;
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName()} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => _{this.Name};");
                            fg.AppendLine($"{SetPermissionStr}set => {this.Name}_Set(value);");
                        }

                        using (var args = new FunctionWrapper(fg,
                            $"public void {this.Name}_Set"))
                        {
                            args.Add($"{this.TypeName()} value");
                            args.Add($"bool hasBeenSet = true");
                        }
                        using (new BraceWrapper(fg))
                        {
                            if (this.SingletonType == SingletonLevel.NotNull)
                            {
                                fg.AppendLine($"if (value == null) value = new {this.TypeName()}({(this.ThisConstruction ? "this" : null)});");
                            }
                            fg.AppendLine($"this.RaiseAndSetIfChanged(ref _{this.Name}, value, _hasBeenSetTracker, hasBeenSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                        }

                        using (var args = new FunctionWrapper(fg,
                            $"public void {this.Name}_Unset"))
                        {
                        }
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"this.{this.Name}_Set({(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName()})")}, false);");
                        }
                    }
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => this.{this.ProtectedName};");
                }
                else
                {
                    switch (this.SingletonType)
                    {
                        case SingletonLevel.None:
                            fg.AppendLine($"public {this.TypeName()} {this.Name} {{ get; {SetPermissionStr}set; }}");
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => {this.Name};");
                            }
                            break;
                        case SingletonLevel.NotNull:
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"private {this.TypeName()} _{this.Name}{(this.ThisConstruction ? null : $" = new {this.TypeName()}();")}");
                            fg.AppendLine($"public {this.TypeName()} {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => _{this.Name};");
                                fg.AppendLine($"{SetPermissionStr}set => _{this.Name} = value ?? new {this.DirectTypeName}({(this.ThisConstruction ? "this" : null)});");
                            }
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => _{this.Name};");
                            }
                            break;
                        case SingletonLevel.Singleton:
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"private readonly {this.DirectTypeName} {this.SingletonObjectName} = new {this.DirectTypeName}();");
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"public {this.TypeName()} {this.Name} => {this.SingletonObjectName};");
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    if (this.PrefersProperty)
                    {
                        if (this.SingletonType == SingletonLevel.Singleton)
                        {
                            fg.AppendLine($"private readonly {this.DirectTypeName} {this.SingletonObjectName}{(this.ThisConstruction ? null : $" = new {this.DirectTypeName}()")};");
                        }
                        fg.AppendLine(GetNotifyingProperty() + ";");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName()} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this.{this.ProtectedProperty}.Item;");
                            if (this.SingletonType != SingletonLevel.Singleton)
                            {
                                fg.AppendLine($"{SetPermissionStr}set => this.{this.ProtectedProperty}.Item = value;");
                            }
                        }
                        if (this.ReadOnly)
                        {
                            fg.AppendLine($"public IHasBeenSetItemGetter<{this.TypeName()}> {this.Property} => this.{this.ProtectedProperty};");
                        }
                        else
                        {
                            fg.AppendLine($"public IHasBeenSetItem<{this.TypeName()}> {this.Property} => {this.ProtectedProperty};");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName()} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => this.{this.ProtectedName};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName()}> {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Property} => this.{this.GetName(true, true)};");
                    }
                    else
                    {
                        if (!this.ObjectCentralized)
                        {
                            throw new NotImplementedException();
                        }
                        if (this.SingletonType == SingletonLevel.Singleton)
                        {
                            fg.AppendLine($"private readonly {this.DirectTypeName} {this.SingletonObjectName}{(this.ThisConstruction ? null : $" = new {this.DirectTypeName}()")};");
                            fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))} => true;");
                            fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"public {this.TypeName()} {this.Name} => {this.SingletonObjectName};");
                        }
                        else
                        {
                            fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))}");
                            using (new BraceWrapper(fg))
                            {
                                if (this.ObjectCentralized)
                                {
                                    fg.AppendLine($"get => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                                    fg.AppendLine($"{SetPermissionStr}set => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = value;");
                                }
                            }
                            fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                            switch (this.SingletonType)
                            {
                                case SingletonLevel.None:
                                    fg.AppendLine($"private {this.TypeName()} _{this.Name};");
                                    break;
                                case SingletonLevel.NotNull:
                                    fg.AppendLine($"private {this.TypeName()} _{this.Name}{(this.ThisConstruction ? null : $" = new {this.TypeName()}();")}");
                                    break;
                                case SingletonLevel.Singleton:
                                    throw new NotImplementedException();
                                default:
                                    break;
                            }
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"public {this.TypeName()} {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => _{this.Name};");
                                fg.AppendLine($"{SetPermissionStr}set => {this.Name}_Set(value);");
                            }

                            using (var args = new FunctionWrapper(fg,
                                $"public void {this.Name}_Set"))
                            {
                                args.Add($"{this.TypeName()} value");
                                args.Add($"bool hasBeenSet = true");
                            }
                            using (new BraceWrapper(fg))
                            {
                                if (this.SingletonType == SingletonLevel.NotNull)
                                {
                                    fg.AppendLine($"if (value == null) value = new {this.TypeName()}({(this.ThisConstruction ? "this" : null)});");
                                }
                                if (this.NotifyingType == NotifyingType.ReactiveUI)
                                {
                                    fg.AppendLine($"this.RaiseAndSetIfChanged(ref _{this.Name}, value, _hasBeenSetTracker, hasBeenSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                                }
                                else
                                {
                                    fg.AppendLine($"_{this.Name} = value;");
                                    fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = hasBeenSet;");
                                }
                            }

                            using (var args = new FunctionWrapper(fg,
                                $"public void {this.Name}_Unset"))
                            {
                            }
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"this.{this.Name}_Set({(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName()})")}, false);");
                            }
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName(getter: true, internalInterface: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => this.{this.ProtectedName};");
                    }
                }
                else
                {
                    switch (this.SingletonType)
                    {
                        case SingletonLevel.None:
                            fg.AppendLine($"public {this.TypeName()} {this.Name} {{ get; {SetPermissionStr}set; }}");
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => {this.Name};");
                            }
                            break;
                        case SingletonLevel.NotNull:
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"private {this.TypeName()} _{this.Name}{(this.ThisConstruction ? null : $" = new {this.DirectTypeName}()")};");
                            fg.AppendLine($"public {this.TypeName()} {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => _{this.Name};");
                                fg.AppendLine($"{SetPermissionStr}set => _{this.Name} = value ?? new {this.DirectTypeName}({(this.ThisConstruction ? "this" : null)});");
                            }
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => _{this.Name};");
                            }
                            break;
                        case SingletonLevel.Singleton:
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"private readonly {this.DirectTypeName} {this.SingletonObjectName}{(this.ThisConstruction ? null : $" = new {this.DirectTypeName}()")};");
                            fg.AppendLine($"public {this.TypeName()} {this.Name} => {this.SingletonObjectName};");
                            if (this.GetterInterfaceType != LoquiInterfaceType.Direct)
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: false)}.{this.Name} => {this.SingletonObjectName};");
                            }
                            break;
                        default:
                            throw new NotImplementedException();
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
            this.SingletonType = node.GetAttribute(Constants.NULLABLE, SingletonLevel.None);

            XElement refNode = GetRefNode(node);

            int refTypeCount = 0;

            var genericName = node.Element(XName.Get(Constants.GENERIC, LoquiGenerator.Namespace))?.Value;
            if (!string.IsNullOrWhiteSpace(genericName))
            {
                refTypeCount++;
            }

            if (this.RefName == null)
            {
                this.RefName = refNode?.GetAttribute(Constants.REF_NAME);
            }
            if (!string.IsNullOrWhiteSpace(this.RefName))
            {
                refTypeCount++;
            }

            bool usingInterface = false;
            foreach (var interfNode in Node.Elements(XName.Get(Constants.INTERFACE, LoquiGenerator.Namespace)))
            {
                switch (interfNode.GetAttribute<LoquiInterfaceType>(Constants.TYPE, LoquiInterfaceType.Direct))
                {
                    case LoquiInterfaceType.ISetter:
                        this.SetterInterface = interfNode.Value;
                        break;
                    case LoquiInterfaceType.IGetter:
                        this.GetterInterface = interfNode.Value;
                        break;
                    case LoquiInterfaceType.Direct:
                        this.SetterInterface = interfNode.Value;
                        this.GetterInterface = interfNode.Value;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            if (!string.IsNullOrWhiteSpace(this.SetterInterface) || !string.IsNullOrWhiteSpace(this.GetterInterface))
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
                    this.RefType = LoquiRefType.Generic;
                    this._generic = genericName;
                    this.GenericDef = this.ObjectGen.Generics[this._generic];
                    this.GenericDef.Loqui = true;
                    if (this.SingletonType == SingletonLevel.Singleton)
                    {
                        throw new ArgumentException("Cannot be a generic and singleton.");
                    }
                }
                else if (usingInterface)
                {
                    this.RefType = LoquiRefType.Interface;
                }
                else
                {
                    throw new ArgumentException("Ref type needs a target.");
                }
            }

            this.ReadOnly = this.ReadOnly || this.SingletonType == SingletonLevel.Singleton;
            this.ThisConstruction = node.GetAttribute(Constants.THIS_CTOR, this.ThisConstruction);
        }

        public bool ParseRefNode(XElement refNode)
        {
            if (string.IsNullOrWhiteSpace(this.RefName)) return false;

            this.SetterInterfaceType = refNode.GetAttribute<LoquiInterfaceType>(Constants.SET_INTERFACE_TYPE, this.ObjectGen.SetterInterfaceTypeDefault);
            this.GetterInterfaceType = refNode.GetAttribute<LoquiInterfaceType>(Constants.GET_INTERFACE_TYPE, this.ObjectGen.GetterInterfaceTypeDefault);

            this.RefType = LoquiRefType.Direct;
            if (!ObjectNamedKey.TryFactory(this.RefName, this.ProtoGen.Protocol, out var namedKey)
                || !this.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(namedKey, out _TargetObjectGeneration))
            {
                throw new ArgumentException("Loqui type cannot be found: " + this.RefName);
            }

            this.GenericSpecification = new GenericSpecification();
            foreach (var specNode in refNode.Elements(XName.Get(Constants.GENERIC_SPECIFICATION, LoquiGenerator.Namespace)))
            {
                this.GenericSpecification.Specifications.Add(
                    specNode.Attribute(Constants.TYPE_TO_SPECIFY).Value,
                    specNode.Attribute(Constants.DEFINITION).Value);
            }
            foreach (var mapNode in refNode.Elements(XName.Get(Constants.GENERIC_MAPPING, LoquiGenerator.Namespace)))
            {
                this.GenericSpecification.Mappings.Add(
                    mapNode.Attribute(Constants.TYPE_ON_REF).Value,
                    mapNode.Attribute(Constants.TYPE_ON_OBJECT).Value);
            }
            foreach (var generic in this.TargetObjectGeneration.Generics)
            {
                if (this.GenericSpecification.Specifications.ContainsKey(generic.Key)) continue;
                if (this.GenericSpecification.Mappings.ContainsKey(generic.Key)) continue;
                this.GenericSpecification.Mappings.Add(
                    generic.Key,
                    generic.Key);
            }
            return true;
        }

        public string CommonClassInstance(Accessor accessor, bool getter, LoquiInterfaceType interfaceType, CommonGenerics commonGen, params MaskType[] types)
        {
            return $"(({this._TargetObjectGeneration.CommonClass(interfaceType, commonGen, types)})(({this.Interface(getter: true, internalInterface: false)}){accessor}).Common{this._TargetObjectGeneration.CommonNameAdditions(interfaceType, types)}Instance{this.GetGenericTypes(getter: getter, types)}())";
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            bool protectedMembers,
            bool deepCopy)
        {

            if (_TargetObjectGeneration is StructGeneration)
            {
                fg.AppendLine($"{accessor.DirectAccess} = new {this.TargetObjectGeneration.Name}({rhsAccessorPrefix}.{this.Name});");
                return;
            }

            if (this.SingletonType == SingletonLevel.Singleton)
            {
                this.GenerateCopyFieldsFrom(
                    fg,
                    accessor: accessor,
                    rhsAccessorPrefix: rhsAccessorPrefix,
                    copyMaskAccessor: copyMaskAccessor,
                    deepCopy: deepCopy);
                return;
            }

            if (!this.HasBeenSet)
            {
                if (deepCopy)
                {
                    fg.AppendLine($"if ({this.GetTranslationIfAccessor(copyMaskAccessor)})");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if ({rhsAccessorPrefix}.{this.Name} == null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{accessor.DirectAccess} = null;");
                        }
                        fg.AppendLine($"else");
                        using (new BraceWrapper(fg))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.Name}.DeepCopy{(this.SetterInterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                            {
                                if (deepCopy)
                                {
                                    args.Add($"copyMask: {copyMaskAccessor}?.GetSubCrystal({this.IndexEnumInt})");
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
                    fg.AppendLine($"switch ({copyMaskAccessor}?.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                        using (new DepthWrapper(fg))
                        {
                            if (this.GetterInterfaceType == LoquiInterfaceType.IGetter)
                            {
                                fg.AppendLine($"{accessor.DirectAccess} = Utility.GetGetterInterfaceReference<{this.TypeName()}>({rhsAccessorPrefix}.{this.Name});");
                            }
                            else
                            {
                                fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.Name};");
                            }
                            fg.AppendLine("break;");
                        }
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                        if (this.SetterInterfaceType != LoquiInterfaceType.IGetter)
                        {
                            using (new DepthWrapper(fg))
                            {
                                this.GenerateCopyFieldsFrom(
                                    fg,
                                    accessor: accessor,
                                    rhsAccessorPrefix: rhsAccessorPrefix,
                                    copyMaskAccessor: copyMaskAccessor,
                                    deepCopy: deepCopy);
                                fg.AppendLine("break;");
                            }
                        }
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.Name} == null)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"{accessor.DirectAccess} = null;");
                            }
                            fg.AppendLine($"else");
                            using (new BraceWrapper(fg))
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.Name}.Copy{(this.SetterInterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                                {
                                    args.Add($"{copyMaskAccessor}?.Specific");
                                }
                            }
                            fg.AppendLine("break;");
                        }
                        fg.AppendLine($"default:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}{(this.RefType == LoquiRefType.Direct ? $"?.Overall" : this.Name)}}}. Cannot execute copy.\");");
                        }
                    }
                }
                return;
            }

            fg.AppendLine($"if({this.HasBeenSetAccessor(new Accessor(this, $"{rhsAccessorPrefix}."))})");
            using (new BraceWrapper(fg))
            {
                if (this.ObjectGen.GenerateComplexCopySystems)
                {
                    fg.AppendLine($"switch ({copyMaskAccessor}{(this.RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                        using (new DepthWrapper(fg))
                        {
                            if (this.GetterInterfaceType == LoquiInterfaceType.IGetter)
                            {
                                fg.AppendLine("throw new NotImplementedException(\"Need to implement an ISetter copy function to support reference copies.\");");
                            }
                            else
                            {
                                fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.Name};");
                                fg.AppendLine("break;");
                            }
                        }
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                        if (this.SetterInterfaceType != LoquiInterfaceType.IGetter)
                        {
                            using (new DepthWrapper(fg))
                            {
                                this.GenerateCopyFieldsFrom(
                                    fg,
                                    accessor: accessor,
                                    rhsAccessorPrefix: rhsAccessorPrefix,
                                    copyMaskAccessor: copyMaskAccessor,
                                    deepCopy: deepCopy);
                                fg.AppendLine("break;");
                            }
                        }
                        fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                        using (new DepthWrapper(fg))
                        {
                            GenerateTypicalMakeCopy(
                                fg,
                                retAccessor: $"{accessor.DirectAccess} = ",
                                rhsAccessor: new Accessor($"{rhsAccessorPrefix}.{this.Name}"),
                                copyMaskAccessor: copyMaskAccessor,
                                deepCopy: deepCopy,
                                doTranslationMask: true);
                            fg.AppendLine("break;");
                        }
                        fg.AppendLine($"default:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}{(this.RefType == LoquiRefType.Direct ? $"?.Overall" : string.Empty)}}}. Cannot execute copy.\");");
                        }
                    }
                }
                else
                {
                    GenerateTypicalMakeCopy(
                        fg,
                        retAccessor: $"{accessor.DirectAccess} = ",
                        rhsAccessor: new Accessor($"{rhsAccessorPrefix}.{this.Name}"),
                        copyMaskAccessor: copyMaskAccessor,
                        deepCopy: deepCopy,
                        doTranslationMask: true);
                }
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessor.DirectAccess}_Set"))
                {
                    args.Add($"value: default({this.TypeName()})");
                    args.Add($"hasBeenSet: false");
                }
            }
        }

        public virtual void GenerateTypicalMakeCopy(
            FileGeneration fg,
            string retAccessor,
            Accessor rhsAccessor,
            string copyMaskAccessor,
            bool deepCopy,
            bool doTranslationMask)
        {
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    using (var args = new ArgsWrapper(fg,
                        $"{retAccessor}{rhsAccessor.DirectAccess}.DeepCopy{this.GetGenericTypes(getter: true, MaskType.Normal, MaskType.NormalGetter, MaskType.Translation)}"))
                    {
                        args.AddPassArg("errorMask");
                        if (this.RefType == LoquiRefType.Direct)
                        {
                            if (!doTranslationMask)
                            {
                                args.Add($"default(TranslationCrystal)");
                            }
                            else if(deepCopy)
                            {
                                args.Add($"{copyMaskAccessor}?.GetSubCrystal({this.IndexEnumInt})");
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
                        fg.AppendLine($"{retAccessor}r.DeepCopy() as {_generic};");
                    }
                    else
                    {
                        fg.AppendLine($"{retAccessor}{nameof(LoquiRegistration)}.GetCopyFunc<{_generic}, {_generic}Getter>()({rhsAccessor.DirectAccess}, null);");
                    }
                    break;
                case LoquiRefType.Interface:
                    if (deepCopy)
                    {
                        fg.AppendLine($"{retAccessor}r.DeepCopy() as {this.TypeName(getter: false, internalInterface: true)};");
                    }
                    else
                    {
                        fg.AppendLine($"{retAccessor}{nameof(LoquiRegistration)}.GetCopyFunc<{this.TypeName()}>(r.GetType())({rhsAccessor.DirectAccess}, null);");
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GenerateCopyFieldsFrom(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            bool deepCopy)
        {
            if (this.RefType == LoquiRefType.Direct)
            {
                var funcStr = deepCopy
                    ? $"{accessor.DirectAccess}.DeepCopyFieldsFrom"
                    : $"{accessor.DirectAccess}.CopyFieldsFrom";
                using (var args = new ArgsWrapper(fg, funcStr))
                {
                    args.Add($"rhs: {rhsAccessorPrefix}.{this.Name}");
                    if (this.RefType == LoquiRefType.Direct)
                    {
                        args.Add($"errorMask: errorMask");
                        if (deepCopy)
                        {
                            args.Add($"copyMask: {copyMaskAccessor}?.GetSubCrystal({this.IndexEnumInt})");
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
                fg.AppendLine("throw new NotImplementedException();");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateUnsetNth(fg, identifier);
                return;
            }
            if (this.SetterInterfaceType == LoquiInterfaceType.IGetter)
            {
                fg.AppendLine($"throw new ArgumentException(\"Cannot unset a get only singleton: {this.Name}\");");
            }
            else
            {
                fg.AppendLine($"{this.TargetObjectGeneration.CommonClassName(LoquiInterfaceType.ISetter)}.Clear({identifier.DirectAccess});");
                fg.AppendLine("break;");
            }
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateSetNthHasBeenSet(fg, identifier, onIdentifier);
                return;
            }
            fg.AppendLine($"throw new ArgumentException(\"Cannot mark set status of a singleton: {this.Name}\");");
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
            if (this.SingletonType == SingletonLevel.Singleton)
            {
                if (!internalUse && this.SetterInterfaceType == LoquiInterfaceType.IGetter)
                {
                    fg.AppendLine($"throw new ArgumentException(\"Cannot set singleton member {this.Name}\");");
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.CopyFieldsFrom{this.GetGenericTypes(getter: false, MaskType.Normal, MaskType.Copy)}(rhs: {rhsAccessorPrefix});");
                    fg.AppendLine("break;");
                }
            }
            else
            {
                base.GenerateSetNth(fg, accessorPrefix, rhsAccessorPrefix, internalUse);
            }
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (getter)
            {
                fg.AppendLine($"{this.TypeName(getter: true, internalInterface: true)} {this.Name} {{ get; }}");
                if (this.SingletonType != SingletonLevel.None) return;
                switch (this.NotifyingType)
                {
                    case NotifyingType.None:
                        if (this.HasBeenSet)
                        {
                            if (this.PrefersProperty)
                            {
                                fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: true)}> {this.Property} {{ get; }}");
                            }
                            else
                            {
                                fg.AppendLine($"bool {this.HasBeenSetAccessor(new Accessor(this.Name))} {{ get; }}");
                            }
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case NotifyingType.ReactiveUI:
                        if (this.HasBeenSet)
                        {
                            fg.AppendLine($"bool {this.HasBeenSetAccessor(new Accessor(this.Name))} {{ get; }}");
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
                fg.AppendLine();
            }
            else
            {
                if (this.SingletonType == SingletonLevel.Singleton)
                {
                    if (this.GetterInterfaceType != this.SetterInterfaceType)
                    {
                        fg.AppendLine($"new {this.TypeName(getter: false)} {this.Name} {{ get; }}");
                    }
                }
                else
                {
                    base.GenerateForInterface(fg, getter, internalInterface);
                }
            }
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            if (TargetObjectGeneration != null)
            {
                yield return TargetObjectGeneration.Namespace;
                yield return TargetObjectGeneration.InternalNamespace;
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateClear(fg, accessorPrefix);
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return $"{this._TargetObjectGeneration.ObjectName}.Copy({rhsAccessor})";
        }

        public string GenerateMaskString(string type)
        {
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    return this.TargetObjectGeneration.GetMaskString(type);
                case LoquiRefType.Generic:
                    if (this.TargetObjectGeneration != null)
                    {
                        return this.TargetObjectGeneration.GetMaskString(type);
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

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.HasBeenSet)
            {
                if (this.PrefersProperty)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{retAccessor} = {accessor.PropertyOrDirectAccess}.{nameof(EqualsMaskHelper.EqualsHelper)}"))
                    {
                        args.Add(rhsAccessor.PropertyOrDirectAccess);
                        args.Add($"(loqLhs, loqRhs) => {this.TargetObjectGeneration.CommonClassName(LoquiInterfaceType.IGetter)}.GetEqualsMask(loqLhs, loqRhs)");
                        args.Add("include");
                    }
                }
                else
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{retAccessor} = EqualsMaskHelper.{nameof(EqualsMaskHelper.EqualsHelper)}"))
                    {
                        args.Add(this.HasBeenSetAccessor(accessor));
                        args.Add(this.HasBeenSetAccessor(rhsAccessor));
                        args.Add(accessor.DirectAccess);
                        args.Add(rhsAccessor.DirectAccess);
                        args.Add($"(loqLhs, loqRhs) => loqLhs.GetEqualsMask(loqRhs)");
                        args.Add("include");
                    }
                }
            }
            else
            {
                if (this.TargetObjectGeneration == null)
                {
                    fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GenerateMaskString("bool")}>();");
                    using (new BraceWrapper(fg) { AppendSemicolon = true })
                    {
                        fg.AppendLine($"Overall = object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})");
                    }
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = MaskItemExt.Factory({accessor.DirectAccess}.GetEqualsMask({rhsAccessor.DirectAccess}, include), include);");
                }
            }
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            fg.AppendLine($"{accessor.DirectAccess}?.ToString({fgAccessor}, \"{name}\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            if (!this.HasBeenSet) return;
            fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {this.HasBeenSetAccessor(accessor)}) return false;");
            if (this.TargetObjectGeneration != null)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.Specific != null && ({accessor.DirectAccess} == null || !{accessor.DirectAccess}.HasBeenSet({checkMaskAccessor}.Specific))) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            if (this.TargetObjectGeneration == null)
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GetMaskString("bool")}>({(this.HasBeenSet ? $"{accessor.PropertyAccess}.HasBeenSet" : "true")}, null);");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GetMaskString("bool")}>({(this.HasBeenSet ? $"{this.HasBeenSetAccessor(accessor)}" : "true")}, {accessor.DirectAccess}.GetHasBeenSetMask());");
            }
        }

        public IEnumerable<string> GetGenericTypesEnumerable(bool getter, params MaskType[] additionalMasks)
        {
            return GetGenericTypesEnumerable(getter: getter, typeOverride: null, additionalMasks: additionalMasks);
        }

        public IEnumerable<string> GetGenericTypesEnumerable(bool getter, LoquiInterfaceType? typeOverride, MaskType[] additionalMasks)
        {
            if (this.GenericSpecification == null) return null;
            if (this.TargetObjectGeneration.Generics.Count == 0) return null;
            List<string> ret = new List<string>();
            foreach (var gen in this.TargetObjectGeneration.Generics)
            {
                if (this.GenericSpecification.Specifications.TryGetValue(gen.Key, out var spec))
                {
                    if (ObjectNamedKey.TryFactory(spec, this.ObjectGen.ProtoGen.Protocol, out var namedKey)
                        && this.ObjectGen.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(
                            namedKey,
                            out var targetObjGen))
                    {
                        foreach (var mType in additionalMasks)
                        {
                            switch (mType)
                            {
                                case MaskType.Normal:
                                    ret.Add(targetObjGen.GetTypeName(typeOverride ?? (getter && !additionalMasks.Contains(MaskType.NormalGetter) ? this.GetterInterfaceType : this.SetterInterfaceType)));
                                    break;
                                case MaskType.NormalGetter:
                                    ret.Add(targetObjGen.GetTypeName(typeOverride ?? this.GetterInterfaceType));
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
                else if (this.GenericSpecification.Mappings.TryGetValue(gen.Key, out var mapping))
                {
                    ret.Add(mapping);
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
            LoquiType ret = this.ObjectGen.ProtoGen.Gen.GetTypeGeneration<LoquiType>();
            ret._TargetObjectGeneration = target;
            ret.RefName = target.Name;
            ret.RefType = this.RefType;
            ret.Name = this.Name;
            ret.SetObjectGeneration(this.ObjectGen, setDefaults: true);
            foreach (var custom in this.CustomData)
            {
                ret.CustomData[custom.Key] = custom.Value;
            }
            return ret;
        }

        public override bool IsNullable()
        {
            return this.SingletonType == SingletonLevel.None;
        }

        public bool TryGetSpecificationAsObject(string genName, out ObjectGeneration obj)
        {
            var specifications = this.GenericSpecification?.Specifications;
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

            return this.ObjectGen.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(
                objKey,
                out obj);
        }

        public bool SupportsMask(MaskType maskType)
        {
            if (this.RefType != LoquiRefType.Interface) return true;
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
            var ret = this.RefType.GetHashCode();
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    ret = ret.CombineHashCode(HashHelper.GetHashCode(this.TargetObjectGeneration));
                    break;
                case LoquiRefType.Interface:
                    ret = ret.CombineHashCode(HashHelper.GetHashCode(this.GetterInterface, this.SetterInterface));
                    break;
                case LoquiRefType.Generic:
                    ret = ret.CombineHashCode(HashHelper.GetHashCode(this._generic));
                    break;
                default:
                    throw new NotImplementedException();
            }
            return ret;
        }

        public bool Equals(LoquiType other)
        {
            if (object.ReferenceEquals(this, other)) return true;
            if (other == null) return true;
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    return object.ReferenceEquals(this.TargetObjectGeneration, other.TargetObjectGeneration);
                case LoquiRefType.Interface:
                    return string.Equals(this.GetterInterface, other.GetterInterface)
                        && string.Equals(this.SetterInterface, other.SetterInterface);
                case LoquiRefType.Generic:
                    return string.Equals(this._generic, other._generic);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
