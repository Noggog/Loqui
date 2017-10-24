using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class LoquiType : PrimitiveType
    {
        public Ref RefGen { get; protected set; }
        public override string TypeName
        {
            get
            {
                switch (RefType)
                {
                    case LoquiRefType.Direct:
                        switch (this.InterfaceType)
                        {
                            case LoquiInterfaceType.Direct:
                                return $"{this.RefGen.Name}{this.GenericTypes}";
                            case LoquiInterfaceType.IGetter:
                                return $"{this.RefGen.Getter_InterfaceStr}{this.GenericTypes}";
                            case LoquiInterfaceType.ISetter:
                                return $"{this.RefGen.InterfaceStr}{this.GenericTypes}";
                            default:
                                throw new NotImplementedException();
                        }
                    case LoquiRefType.Generic:
                        return _generic;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public override string ProtectedName
        {
            get
            {
                if (this.SingletonType == SingletonLevel.Singleton)
                {
                    return SingletonObjectName;
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
                        return this.RefGen.Name;
                    case LoquiRefType.Generic:
                        return _generic;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public string GenericTypes => _GenericTypes();
        public SingletonLevel SingletonType;
        public LoquiRefType RefType { get; private set; }
        public LoquiInterfaceType InterfaceType = LoquiInterfaceType.Direct;
        private string _generic;
        public GenericDefinition GenericDef;
        public GenericSpecification GenericSpecification;
        public ObjectGeneration TargetObjectGeneration
        {
            get
            {
                switch (RefType)
                {
                    case LoquiRefType.Direct:
                        return this.RefGen.Obj;
                    case LoquiRefType.Generic:
                        return this.GenericDef.BaseObjectGeneration;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public string SingletonObjectName => $"_{this.Name}_Object";
        public override Type Type => throw new NotImplementedException();
        public string RefName;

        public override string SkipCheck(string copyMaskAccessor)
        {
            if (this.SingletonType == SingletonLevel.Singleton)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            switch (this.RefType)
            {
                case LoquiRefType.Direct:
                    return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
                case LoquiRefType.Generic:
                    if (this.TargetObjectGeneration == null)
                    {
                        return $"{copyMaskAccessor}?.{this.Name} != {nameof(GetterCopyOption)}.{nameof(GetterCopyOption.Skip)}";
                    }
                    else
                    {
                        return $"{copyMaskAccessor}?.{this.Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        public override bool Copy => base.Copy && !(this.InterfaceType == LoquiInterfaceType.IGetter && this.SingletonType == SingletonLevel.Singleton);

        public enum LoquiRefType
        {
            Direct,
            Generic
        }

        public enum SingletonLevel
        {
            None,
            NotNull,
            Singleton
        }

        public string GetMaskString(string str)
        {
            return this.TargetObjectGeneration?.GetMaskString(str) ?? "object";
        }

        public string MaskItemString(MaskType type)
        {
            if (this.GenericDef != null)
            {
                if (this.TargetObjectGeneration == null)
                {
                    return "object";
                }
                else
                {
                    switch (type)
                    {
                        case MaskType.Error:
                            return $"{GenericDef.Name}_{MaskModule.ErrMaskNickname}";
                        case MaskType.Copy:
                            return $"{GenericDef.Name}_{MaskModule.CopyMaskNickname}";
                        case MaskType.Normal:
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            else if (this.GenericSpecification != null)
            {
                return this.TargetObjectGeneration.Mask_Specified(type, this.GenericSpecification);
            }
            return this.TargetObjectGeneration.Mask(type);
        }

        public override bool CopyNeedsTryCatch => true;

        public override void GenerateForCtor(FileGeneration fg)
        {
            base.GenerateForCtor(fg);

            switch (this.Notifying)
            {
                case NotifyingOption.HasBeenSet:
                case NotifyingOption.Notifying:
                    if ((!this.TrueReadOnly
                        && this.RaisePropertyChanged)
                        || (this.Notifying != NotifyingOption.None
                            && this.SingletonType == SingletonLevel.Singleton))
                    {
                        GenerateNotifyingConstruction(fg, $"_{this.Name}");
                    }
                    break;
                default:
                    break;
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    switch (this.SingletonType)
                    {
                        case SingletonLevel.None:
                            if (this.RaisePropertyChanged)
                            {
                                fg.AppendLine($"private {TypeName} _{this.Name};");
                                fg.AppendLine($"public {TypeName} {this.Name}");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine($"get => _{this.Name};");
                                    fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set {{ this._{this.Name} = value; OnPropertyChanged(nameof({this.Name})); }}");
                                }
                            }
                            else
                            {
                                fg.AppendLine($"public {this.TypeName} {this.Name} {{ get; {(this.Protected ? "protected " : string.Empty)}set; }}");
                            }
                            break;
                        case SingletonLevel.NotNull:
                            fg.AppendLine($"private {this.TypeName} _{this.Name} = new {this.ObjectTypeName}();");
                            fg.AppendLine($"public {this.TypeName} {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => _{this.Name};");
                                fg.AppendLine($"{(this.Protected ? "protected " : string.Empty)}set => _{this.Name} = value ?? new {this.ObjectTypeName}();");
                            }
                            break;
                        case SingletonLevel.Singleton:
                            fg.AppendLine($"private {this.ObjectTypeName} {this.SingletonObjectName} = new {this.ObjectTypeName}();");
                            fg.AppendLine($"public {this.TypeName} {this.Name} => {this.SingletonObjectName};");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case NotifyingOption.HasBeenSet:
                    if (this.SingletonType == SingletonLevel.Singleton)
                    {
                        fg.AppendLine($"private {this.ObjectTypeName} {this.SingletonObjectName} = new {this.ObjectTypeName}();");
                    }
                    if (this.RaisePropertyChanged
                        || this.SingletonType == SingletonLevel.Singleton)
                    {
                        fg.AppendLine(GetNotifyingProperty() + ";");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this.{this.ProtectedName};");
                        if (this.SingletonType != SingletonLevel.Singleton)
                        {
                            fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this.{this.ProtectedName} = value;");
                        }
                    }
                    if (this.Protected)
                    {
                        fg.AppendLine($"public IHasBeenSetItemGetter<{this.TypeName}> {this.Property} => this.{this.ProtectedProperty};");
                    }
                    else
                    {
                        fg.AppendLine($"public IHasBeenSetItem<{this.TypeName}> {this.Property} => {this.ProtectedProperty};");
                    }
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.ProtectedName};");
                    fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.GetName(true, true)};");
                    break;
                case NotifyingOption.Notifying:
                    switch (this.SingletonType)
                    {
                        case SingletonLevel.None:
                            fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItem<{TypeName}>();");
                            break;
                        case SingletonLevel.NotNull:
                            fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItemConvertWrapper<{TypeName}>(");
                            using (new DepthWrapper(fg))
                            {
                                fg.AppendLine($"defaultVal: new {this.RefGen.Name}(),");
                                fg.AppendLine("incomingConverter: (change) =>");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("if (change.New == null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"return TryGet<{this.TypeName}>.Succeed(new {this.RefGen.Name}());");
                                    }
                                    fg.AppendLine($"return TryGet<{this.TypeName}>.Succeed(change.New);");
                                }
                            }
                            fg.AppendLine(");");
                            break;
                        case SingletonLevel.Singleton:
                            fg.AppendLine($"private {this.ObjectTypeName} {this.SingletonObjectName} = new {this.ObjectTypeName}();");
                            fg.AppendLine(GetNotifyingProperty() + ";");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    fg.AppendLine($"public INotifyingItem{((Protected || this.SingletonType == SingletonLevel.Singleton) ? "Getter" : string.Empty)}<{TypeName}> {this.Property} => this._{this.Name};");
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get => _{this.Name}.Item; {(this.Protected ? string.Empty : $"set => _{this.Name}.Item = value; ")}}}");
                    if (!this.Protected && this.SingletonType != SingletonLevel.Singleton)
                    {
                        fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void GenerateNotifyingConstruction(FileGeneration fg, string prepend)
        {
            using (var args = new ArgsWrapper(fg,
                $"{prepend} = {(this.Notifying == NotifyingOption.Notifying ? "NotifyingItem" : $"HasBeenSetItem")}.Factory{(this.SingletonType == SingletonLevel.NotNull && this.InterfaceType == LoquiInterfaceType.Direct ? "NoNull" : string.Empty)}<{TypeName}>"))
            {
                switch (this.SingletonType)
                {
                    case SingletonLevel.None:
                    case SingletonLevel.NotNull:
                        break;
                    case SingletonLevel.Singleton:
                        args.Add($"defaultVal: {this.SingletonObjectName}");
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (this.RaisePropertyChanged)
                {
                    args.Add($"onSet: (i) => this.OnPropertyChanged(nameof({this.Name}))");
                }
                if (this.SingletonType == SingletonLevel.NotNull
                    && this.InterfaceType != LoquiInterfaceType.Direct)
                {
                    args.Add($"noNullFallback: () => new {this.ObjectTypeName}()");
                }
                args.Add($"markAsSet: {(this.SingletonType == SingletonLevel.Singleton ? "true" : "false")}");
            }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            this.SingletonType = node.GetAttribute(Constants.SINGLETON, SingletonLevel.None);

            XElement refNode;
            if (node.Name.LocalName.Equals(Constants.REF_DIRECT)
                || node.Name.LocalName.Equals(Constants.REF_LIST))
            {
                refNode = node;
            }
            else
            {
                refNode = node.Element(XName.Get(Constants.DIRECT, LoquiGenerator.Namespace));
            }
            var genericNode = node.Element(XName.Get(Constants.GENERIC, LoquiGenerator.Namespace));

            if (refNode != null
                && !string.IsNullOrWhiteSpace(refNode.Value)
                && genericNode != null
                && !string.IsNullOrWhiteSpace(genericNode.Value))
            {
                throw new ArgumentException("Cannot both be generic and have specific object specified.");
            }

            if (this.RefName == null)
            {
                this.RefName = refNode?.GetAttribute(Constants.REF_NAME);
            }
            var genericName = genericNode?.Value;

            if (!string.IsNullOrWhiteSpace(this.RefName))
            {
                var r = new Ref();
                this.InterfaceType = refNode.GetAttribute<LoquiInterfaceType>(Constants.INTERFACE_TYPE, this.ObjectGen.InterfaceTypeDefault);

                var genElems = refNode?.Elements(XName.Get(Constants.GENERIC, LoquiGenerator.Namespace)).ToList();
                if (genElems?.Count > 0)
                {
                    r = new GenRef()
                    {
                        Generics = genElems.Select((e) => e.Value).ToList()
                    };
                }

                this.RefType = LoquiRefType.Direct;
                if (!this.ProtoGen.ObjectGenerationsByName.TryGetValue(this.RefName, out ObjectGeneration refGen))
                {
                    throw new ArgumentException("Loqui type cannot be found: " + this.RefName);
                }
                r.Obj = refGen;
                this.RefGen = r;

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
                foreach (var generic in this.RefGen.Obj.Generics)
                {
                    if (this.GenericSpecification.Specifications.ContainsKey(generic.Key)) continue;
                    if (this.GenericSpecification.Mappings.ContainsKey(generic.Key)) continue;
                    this.GenericSpecification.Mappings.Add(
                        generic.Key,
                        generic.Key);
                }
            }
            else if (!string.IsNullOrWhiteSpace(genericName))
            {
                this.RefType = LoquiRefType.Generic;
                this._generic = genericName;
                this.GenericDef = this.ObjectGen.Generics[this._generic];
                this.GenericDef.Add(nameof(ILoquiObjectGetter));
                if (this.SingletonType == SingletonLevel.Singleton)
                {
                    throw new ArgumentException("Cannot be a generic and singleton.");
                }
            }
            else
            {
                throw new ArgumentException("Ref type needs a target.");
            }

            this.Protected = this.Protected || this.SingletonType == SingletonLevel.Singleton;

            if (this.RefType != LoquiRefType.Generic || !this.GenericDef.Wheres.Any()) return;
            if (!this.ObjectGen.ProtoGen.ObjectGenerationsByName.TryGetValue(this.GenericDef.Wheres.First(), out var baseObjGen)) return;
            this.GenericDef.BaseObjectGeneration = baseObjGen;
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            string accessorPrefix,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {

            if (RefGen?.Obj is StructGeneration)
            {
                fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedMembers, false)} = new {this.RefGen.Obj.Name}({rhsAccessorPrefix}.{this.Name});");
                return;
            }

            if (this.SingletonType == SingletonLevel.Singleton)
            {
                this.GenerateCopyFieldsFrom(fg);
                return;
            }

            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"switch ({copyMaskAccessor}?.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix}.{this.Name};");
                        fg.AppendLine("break;");
                    }
                    fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                    if (this.InterfaceType != LoquiInterfaceType.IGetter)
                    {
                        using (new DepthWrapper(fg))
                        {
                            this.GenerateCopyFieldsFrom(fg);
                            fg.AppendLine("break;");
                        }
                    }
                    fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"if ({rhsAccessorPrefix}.{this.Name} == null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{accessorPrefix}.{this.Name} = null;");
                        }
                        fg.AppendLine($"else");
                        using (new BraceWrapper(fg))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{accessorPrefix}.{this.Name} = {this.ObjectTypeName}{this.GenericTypes}.Copy{(this.InterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                            {
                                args.Add($"{rhsAccessorPrefix}.{this.Name}");
                                args.Add($"{copyMaskAccessor}?.Specific");
                                args.Add($"{defaultFallbackAccessor}?.{this.Name}");
                            }
                        }
                        fg.AppendLine("break;");
                    }
                    fg.AppendLine($"default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}?.{(this.RefType == LoquiRefType.Direct ? $"{this.Name}.Overall" : this.Name)}}}. Cannot execute copy.\");");
                    }
                }
                return;
            }

            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.Property}.SetToWithDefault"))
            {
                args.Add($"{rhsAccessorPrefix}.{this.Property}");
                args.Add($"{defaultFallbackAccessor}?.{this.Property}");
                if (this.Notifying == NotifyingOption.Notifying)
                {
                    args.Add($"cmds");
                }
                args.Add((gen) =>
                {
                    gen.AppendLine($"(r, d) =>");
                    using (new BraceWrapper(gen))
                    {
                        if (this.RefType == LoquiRefType.Generic
                            && this.GenericDef?.BaseObjectGeneration == null)
                        {
                            gen.AppendLine($"switch ({copyMaskAccessor}?{(this.RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(GetterCopyOption)}.{nameof(GetterCopyOption.Reference)})");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"case {nameof(GetterCopyOption)}.{nameof(CopyOption.Reference)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine("return r;");
                                }
                                gen.AppendLine($"case {nameof(GetterCopyOption)}.{nameof(GetterCopyOption.MakeCopy)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"if (r == null) return default({this.TypeName});");
                                    if (this.RefType == LoquiRefType.Direct)
                                    {
                                        using (var args2 = new ArgsWrapper(gen,
                                            $"return {this.ObjectTypeName}.Copy{(this.InterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                                        {
                                            args2.Add($"r");
                                            if (this.RefType == LoquiRefType.Direct)
                                            {
                                                args2.Add($"{copyMaskAccessor}?.Specific");
                                            }
                                            args2.Add($"def: d");
                                        }
                                    }
                                    else
                                    {
                                        gen.AppendLine($"var copyFunc = {nameof(LoquiRegistration)}.GetCopyFunc<{_generic}>();");
                                        gen.AppendLine($"return copyFunc(r, null, d);");
                                    }
                                }
                                gen.AppendLine($"default:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(GetterCopyOption)} {{{copyMaskAccessor}?.{(this.RefType == LoquiRefType.Direct ? $".Overall" : string.Empty)}}}. Cannot execute copy.\");");
                                }
                            }
                        }
                        else
                        {
                            GenerateTypicalCopyFieldsFrom(
                                gen,
                                copyMaskAccessor: copyMaskAccessor);
                        }
                    }
                });
            }
        }

        public void GenerateTypicalCopyFieldsFrom(
            FileGeneration fg,
            string copyMaskAccessor)
        {
            fg.AppendLine($"switch ({copyMaskAccessor}?{(this.RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine("return r;");
                }
                fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                if (this.InterfaceType != LoquiInterfaceType.IGetter)
                {
                    using (new DepthWrapper(fg))
                    {
                        this.GenerateCopyFieldsFrom(fg);
                        fg.AppendLine("return r;");
                    }
                }
                fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                using (new DepthWrapper(fg))
                {
                    GenerateTypicalMakeCopy(fg, copyMaskAccessor);
                }
                fg.AppendLine($"default:");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}?{(this.RefType == LoquiRefType.Direct ? $".Overall" : string.Empty)}}}. Cannot execute copy.\");");
                }
            }
        }

        public void GenerateTypicalMakeCopy(
            FileGeneration fg,
            string copyMaskAccessor)
        {
            fg.AppendLine($"if (r == null) return default({this.TypeName});");
            if (this.RefType == LoquiRefType.Direct)
            {
                using (var args2 = new ArgsWrapper(fg,
                    $"return {this.ObjectTypeName}{this.GenericTypes}.Copy{(this.InterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                {
                    args2.Add($"r");
                    if (this.RefType == LoquiRefType.Direct)
                    {
                        args2.Add($"{copyMaskAccessor}?.Specific");
                    }
                    args2.Add($"def: d");
                }
            }
            else
            {
                fg.AppendLine($"var copyFunc = {nameof(LoquiRegistration)}.GetCopyFunc<{_generic}>();");
                fg.AppendLine($"return copyFunc(r, null, d);");
            }
        }

        private void GenerateCopyFieldsFrom(FileGeneration fg)
        {
            if (this.RefType == LoquiRefType.Direct)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{this.RefGen?.Obj.ExtCommonName}.CopyFieldsFrom"))
                {
                    args.Add($"item: item.{this.Name}");
                    args.Add($"rhs: rhs.{this.Name}");
                    args.Add($"def: def?.{this.Name}");
                    if (this.RefType == LoquiRefType.Direct)
                    {
                        args.Add($"doErrorMask: doErrorMask");
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"errorMask: (doErrorMask ? new Func<{this.MaskItemString(MaskType.Error)}>(() =>");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"var baseMask = errorMask();");
                                gen.AppendLine($"if (baseMask.{this.Name}.Specific == null)");
                                using (new BraceWrapper(gen))
                                {
                                    gen.AppendLine($"baseMask.{this.Name} = new MaskItem<Exception, {this.MaskItemString(MaskType.Error)}>(null, new {this.MaskItemString(MaskType.Error)}());");
                                }
                                gen.AppendLine($"return baseMask.{this.Name}.Specific;");
                            }
                            gen.Append($") : null)");
                        });
                        args.Add($"copyMask: copyMask?.{this.Name}.Specific");
                    }
                    else
                    {
                        args.Add($"doErrorMask: false");
                        args.Add($"errorMask: null");
                        args.Add($"copyMask: null");
                    }
                    args.Add($"cmds: cmds");
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"ILoquiObjectExt.CopyFieldsIn"))
                {
                    args.Add("obj: r");
                    args.Add($"rhs: item.{this.Name}");
                    args.Add($"def: def == null ? default({this.TypeName}) : def.{this.Name}");
                    args.Add("skipProtected: true");
                    args.Add("cmds: cmds");
                }
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateUnsetNth(fg, identifier, cmdsAccessor);
                return;
            }
            if (this.InterfaceType == LoquiInterfaceType.IGetter)
            {
                fg.AppendLine($"throw new ArgumentException(\"Cannot unset a get only singleton: {this.Name}\");");
            }
            else
            {
                fg.AppendLine($"{this.RefGen.Obj.ExtCommonName}.Clear({identifier}.{this.Name}, cmds.ToUnsetParams());");
                fg.AppendLine("break;");
            }
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateSetNthHasBeenSet(fg, identifier, onIdentifier, internalUse);
                return;
            }
            fg.AppendLine($"throw new ArgumentException(\"Cannot mark set status of a singleton: {this.Name}\");");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"{this.TypeName} {this.Name} {{ get; }}");
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    break;
                case NotifyingOption.HasBeenSet:
                    fg.AppendLine($"IHasBeenSetItemGetter<{TypeName}> {this.Property} {{ get; }}");
                    break;
                case NotifyingOption.Notifying:
                    fg.AppendLine($"INotifyingItemGetter<{TypeName}> {this.Property} {{ get; }}");
                    break;
                default:
                    throw new NotImplementedException();
            }
            fg.AppendLine();
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            if (this.SingletonType == SingletonLevel.Singleton)
            {
                if (!internalUse && this.InterfaceType == LoquiInterfaceType.IGetter)
                {
                    fg.AppendLine($"throw new ArgumentException(\"Cannot set singleton member {this.Name}\");");
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.CopyFieldsFrom(rhs: {rhsAccessorPrefix}, cmds: {cmdsAccessor});");
                    fg.AppendLine("break;");
                }
            }
            else
            {
                base.GenerateSetNth(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor, internalUse);
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateForInterface(fg);
            }
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            if (RefType == LoquiRefType.Direct)
            {
                yield return RefGen.Obj.Namespace;
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateClear(fg, accessorPrefix, cmdAccessor);
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return $"{this.RefGen.ObjectName}.Copy({rhsAccessor})";
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
                        return "object";
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!object.Equals({this.Name}, {rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GenerateMaskString("bool")}>();");
                if (this.TargetObjectGeneration == null)
                {
                    fg.AppendLine($"{retAccessor}.Overall = object.Equals({accessor}, {rhsAccessor});");
                }
                else
                {
                    fg.AppendLine($"{retAccessor}.Specific = {this.TargetObjectGeneration.ExtCommonName}.GetEqualsMask({accessor}, {rhsAccessor});");
                    fg.AppendLine($"{retAccessor}.Overall = {retAccessor}.Specific.AllEqual((b) => b);");
                }
            }
            else
            {
                if (this.TargetObjectGeneration == null)
                {
                    fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GenerateMaskString("bool")}>();");
                    fg.AppendLine($"{retAccessor}.Overall = {accessor}.Equals({rhsAccessor}, (loqLhs, loqRhs) => object.Equals(loqLhs, loqRhs));");
                }
                else
                {
                    fg.AppendLine($"{retAccessor} = {accessor}.{nameof(IHasBeenSetExt.LoquiEqualsHelper)}({rhsAccessor}, (loqLhs, loqRhs) => {this.TargetObjectGeneration.ExtCommonName}.GetEqualsMask(loqLhs, loqRhs));");
                }
            }
        }

        public override void GenerateToString(FileGeneration fg, string name, string accessor, string fgAccessor)
        {
            fg.AppendLine($"{accessor}?.ToString({fgAccessor}, \"{name}\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, string accessor, string checkMaskAccessor)
        {
            fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor}.HasBeenSet) return false;");
            if (this.TargetObjectGeneration != null)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.Specific != null && ({accessor}.Item == null || !{accessor}.Item.HasBeenSet({checkMaskAccessor}.Specific))) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, string accessor, string retAccessor)
        {
            if (this.TargetObjectGeneration == null)
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GetMaskString("bool")}>({(this.Notifying == NotifyingOption.None ? "true" : $"{accessor}.HasBeenSet")}, null);");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, {this.GetMaskString("bool")}>({(this.Notifying == NotifyingOption.None ? "true" : $"{accessor}.HasBeenSet")}, {(this.TargetObjectGeneration.ExtCommonName)}.GetHasBeenSetMask({(this.Notifying == NotifyingOption.None ? accessor : $"{accessor}.Item")}));");
            }
        }

        private string _GenericTypes()
        {
            if (this.GenericSpecification == null) return null;
            if (this.RefGen.Obj.Generics.Count == 0) return null;
            List<string> ret = new List<string>();
            foreach (var gen in this.RefGen.Obj.Generics)
            {
                if (this.GenericSpecification.Specifications.TryGetValue(gen.Key, out var spec))
                {
                    ret.Add(spec);
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
            return $"<{string.Join(", ", ret)}>";
        }
    }
}
