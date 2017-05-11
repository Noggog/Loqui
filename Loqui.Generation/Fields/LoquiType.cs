using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class LoquiType : TypicalGeneration
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
                                return this.RefGen.Name;
                            case LoquiInterfaceType.IGetter:
                                return this.RefGen.Getter_InterfaceStr;
                            case LoquiInterfaceType.ISetter:
                                return this.RefGen.InterfaceStr;
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
        public SingletonLevel SingletonType;
        public LoquiRefType RefType { get; private set; }
        public LoquiInterfaceType InterfaceType = LoquiInterfaceType.Direct;
        private string _generic;
        private ObjectGeneration _genericBaseObject;
        public GenericDefinition Generics;
        public string ErrorMaskItemString => this.ObjectGeneration?.ErrorMask ?? "object";
        public ObjectGeneration ObjectGeneration
        {
            get
            {
                switch (RefType)
                {
                    case LoquiRefType.Direct:
                        return this.RefGen.Obj;
                    case LoquiRefType.Generic:
                        return this._genericBaseObject;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

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
                    if (this.ObjectGeneration == null)
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

        public override bool CopyNeedsTryCatch => true;

        public override void GenerateForCtor(FileGeneration fg)
        {
            base.GenerateForCtor(fg);

            switch (this.Notifying)
            {
                case NotifyingOption.HasBeenSet:
                case NotifyingOption.Notifying:
                    if (!this.TrueReadOnly)
                    {
                        if (this.RaisePropertyChanged)
                        {
                            GenerateNotifyingConstruction(fg, $"_{this.Name}");
                        }
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
                            fg.AppendLine($"public {this.TypeName} {this.Name} {{ get; }} = new {this.ObjectTypeName}();");
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                case NotifyingOption.HasBeenSet:
                    if (this.RaisePropertyChanged)
                    {
                        fg.AppendLine($"protected readonly IHasBeenSetItem<{TypeName}> _{this.Name};");
                    }
                    else
                    {
                        GenerateNotifyingCtor(fg);
                    }
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => this.{ this.ProtectedName};");
                        if (this.SingletonType != SingletonLevel.Singleton)
                        {
                            fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set => this.{this.ProtectedName} = value;");
                        }
                    }
                    if (this.Protected)
                    {
                        fg.AppendLine($"public IHasBeenSetItemGetter<{this.TypeName}> {this.Property} => this.{this.Property};");
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
                            fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItem<{TypeName}>(new {this.RefGen.ObjectName}());");
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
                        args.Add($"defaultVal: new {this.RefGen.ObjectName}()");
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
                args.Add("markAsSet: false");
            }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            this.SingletonType = node.GetAttribute("singleton", SingletonLevel.None);

            var refNode = node.Element(XName.Get("Direct", LoquiGenerator.Namespace));
            var genericNode = node.Element(XName.Get("Generic", LoquiGenerator.Namespace));

            if (refNode != null
                && !string.IsNullOrWhiteSpace(refNode.Value)
                && genericNode != null
                && !string.IsNullOrWhiteSpace(genericNode.Value))
            {
                throw new ArgumentException("Cannot both be generic and have specific object specified.");
            }

            var refName = refNode?.GetAttribute("refName");
            var genericName = genericNode?.Value;

            if (!string.IsNullOrWhiteSpace(refName))
            {
                var r = new Ref();
                this.InterfaceType = refNode.GetAttribute<LoquiInterfaceType>("interfaceType", this.ObjectGen.InterfaceTypeDefault);

                var genElems = refNode.Elements(XName.Get("Generic", LoquiGenerator.Namespace)).ToList();
                if (genElems.Count > 0)
                {
                    r = new GenRef()
                    {
                        Generics = genElems.Select((e) => e.Value).ToList()
                    };
                }

                this.RefType = LoquiRefType.Direct;
                if (!this.ProtoGen.ObjectGenerationsByName.TryGetValue(refName, out ObjectGeneration refGen))
                {
                    throw new ArgumentException("Loqui type cannot be found: " + refName);
                }
                r.Obj = refGen;
                this.RefGen = r;
            }
            else if (!string.IsNullOrWhiteSpace(genericName))
            {
                this.RefType = LoquiRefType.Generic;
                this._generic = genericName;
                this.Generics = this.ObjectGen.Generics[this._generic];
                this.Generics.Add(nameof(ILoquiObjectGetter));
                if (this.SingletonType == SingletonLevel.Singleton)
                {
                    throw new ArgumentException("Cannot be a generic and singleton.");
                }
            }
            else
            {
                throw new ArgumentException("Ref type needs a target.");
            }
        }

        public override void Resolve()
        {
            base.Resolve();
            if (this.RefType != LoquiRefType.Generic || !this.Generics.Wheres.Any()) return;
            if (!this.ObjectGen.ProtoGen.ObjectGenerationsByName.TryGetValue(this.Generics.Wheres.First(), out var baseObjGen)) return;
            this._genericBaseObject = baseObjGen;
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
                fg.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
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
                                $"{accessorPrefix}.{this.Name} = {this.ObjectTypeName}.Copy{(this.InterfaceType == LoquiInterfaceType.IGetter ? "_ToLoqui" : string.Empty)}"))
                            {
                                args.Add($"{rhsAccessorPrefix}.{this.Name}");
                                args.Add($"{copyMaskAccessor}?.{this.Name}.Specific");
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
                            && this._genericBaseObject == null)
                        {
                            gen.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}{(this.RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(GetterCopyOption)}.{nameof(GetterCopyOption.Reference)})");
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
                                                args2.Add($"{copyMaskAccessor}?.{this.Name}.Specific");
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
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(GetterCopyOption)} {{{copyMaskAccessor}?.{(this.RefType == LoquiRefType.Direct ? $"{this.Name}.Overall" : this.Name)}}}. Cannot execute copy.\");");
                                }
                            }
                        }
                        else
                        {
                            gen.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}{(this.RefType == LoquiRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine("return r;");
                                }
                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.CopyIn)}:");
                                if (this.InterfaceType != LoquiInterfaceType.IGetter)
                                {
                                    using (new DepthWrapper(gen))
                                    {
                                        this.GenerateCopyFieldsFrom(gen);
                                        gen.AppendLine("return r;");
                                    }
                                }
                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
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
                                                args2.Add($"{copyMaskAccessor}?.{this.Name}.Specific");
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
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{copyMaskAccessor}?.{(this.RefType == LoquiRefType.Direct ? $"{this.Name}.Overall" : this.Name)}}}. Cannot execute copy.\");");
                                }
                            }
                        }
                    }
                });
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
                            gen.AppendLine($"errorMask: (doErrorMask ? new Func<{this.RefGen.Obj.ErrorMask}>(() =>");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"var baseMask = errorMask();");
                                gen.AppendLine($"if (baseMask.{this.Name}.Specific == null)");
                                using (new BraceWrapper(gen))
                                {
                                    gen.AppendLine($"baseMask.{this.Name} = new MaskItem<Exception, {this.RefGen.Obj.ErrorMask}>(null, new {this.RefGen.Obj.ErrorMask}());");
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

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            if (this.SingletonType != SingletonLevel.Singleton)
            {
                base.GenerateInterfaceSet(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor);
                return;
            }
            fg.AppendLine($"throw new ArgumentException(\"Cannot set singleton member {this.Name}\");");
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
                    return this.RefGen.Obj.GetMaskString(type);
                case LoquiRefType.Generic:
                    return "object";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
