using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class NoggType : TypicalGeneration
    {
        public Ref RefGen { get; protected set; }
        public override string TypeName
        {
            get
            {
                switch (RefType)
                {
                    case NoggRefType.Direct:
                        switch (this.InterfaceType)
                        {
                            case NoggInterfaceType.Direct:
                                return this.RefGen.Name;
                            case NoggInterfaceType.IGetter:
                                return this.RefGen.Getter_InterfaceStr;
                            case NoggInterfaceType.ISetter:
                                return this.RefGen.InterfaceStr;
                            default:
                                throw new NotImplementedException();
                        }
                    case NoggRefType.Generic:
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
                    case NoggRefType.Direct:
                        return this.RefGen.Name;
                    case NoggRefType.Generic:
                        return _generic;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public SingletonLevel SingletonType;
        public NoggRefType RefType { get; private set; }
        public NoggInterfaceType InterfaceType = NoggInterfaceType.Direct;
        private string _generic;
        public override string SkipCheck(string copyMaskAccessor)
        {
            if (this.SingletonType == SingletonLevel.Singleton)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            switch (this.RefType)
            {
                case NoggRefType.Direct:
                    return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyType)}.{nameof(CopyType.Skip)}";
                case NoggRefType.Generic:
                    return $"{copyMaskAccessor}?.{this.Name} != {nameof(CopyType)}.{nameof(CopyType.Skip)}";
                default:
                    throw new NotImplementedException();
            }
        }
        public override bool Copy => base.Copy && !(this.InterfaceType == NoggInterfaceType.IGetter && this.SingletonType == SingletonLevel.Singleton);

        public enum NoggRefType
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

        public override void GenerateForClass(FileGeneration fg)
        {
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    switch (this.SingletonType)
                    {
                        case SingletonLevel.None:
                            fg.AppendLine($"public {this.TypeName} {this.Name} {{ get; {(this.Protected ? "protected " : string.Empty)}set; }}");
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
                    string hasBeenSet;
                    if (this.SingletonType != SingletonLevel.NotNull)
                    {
                        hasBeenSet = $"HasBeenSetItem<{this.TypeName}>";
                    }
                    else
                    {
                        hasBeenSet = $"HasBeenSetItem{(this.SingletonType == SingletonLevel.NotNull ? "NoNull" : string.Empty)}";
                        hasBeenSet += $"<{this.TypeName}{((this.InterfaceType != NoggInterfaceType.Direct ? $", {this.RefGen.Name}" : string.Empty))}>";
                    }
                    fg.AppendLine($"private readonly {hasBeenSet} {this.ProtectedProperty} = new {hasBeenSet}();");
                    fg.AppendLine($"public {this.TypeName} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get {{ return this.{ this.ProtectedName}; }}");
                        if (this.SingletonType != SingletonLevel.Singleton)
                        {
                            fg.AppendLine($"{(Protected ? "protected " : string.Empty)}set {{ this.{this.ProtectedName} = value; }}");
                        }
                    }
                    if (this.ReadOnly)
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
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get {{ return _{this.Name}.Value; }} {(this.Protected ? string.Empty : $"set {{ _{this.Name}.Value = value; }} ")}}}");
                    if (!this.ReadOnly && this.SingletonType != SingletonLevel.Singleton)
                    {
                        fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} => this.{this.Property};");
                    }
                    fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} => this.{this.Property};");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            this.SingletonType = node.GetAttribute("singleton", SingletonLevel.None);

            var refNode = node.Element(XName.Get("Direct", NoggolloquyGenerator.Namespace));
            var genericNode = node.Element(XName.Get("Generic", NoggolloquyGenerator.Namespace));

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
                Ref r = new Ref();
                this.InterfaceType = refNode.GetAttribute<NoggInterfaceType>("interfaceType", this.ObjectGen.InterfaceTypeDefault);

                var genElems = refNode.Elements(XName.Get("Generic", NoggolloquyGenerator.Namespace)).ToList();
                if (genElems.Count > 0)
                {
                    r = new GenRef()
                    {
                        Generics = genElems.Select((e) => e.Value).ToList()
                    };
                }

                this.RefType = NoggRefType.Direct;
                if (!this.ProtoGen.ObjectGenerationsByName.TryGetValue(refName, out ObjectGeneration refGen))
                {
                    throw new ArgumentException("Nogg type cannot be found: " + refName);
                }
                r.Obj = refGen;
                this.RefGen = r;
            }
            else if (!string.IsNullOrWhiteSpace(genericName))
            {
                this.RefType = NoggRefType.Generic;
                this._generic = genericName;
                var gen = this.ObjectGen.Generics[this._generic];
                gen.Wheres.Add(nameof(INoggolloquyObjectGetter));
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

        private string StructTypeName()
        {
            return $"{TypeName}?";
        }

        private string[] GenerateMaskFunc()
        {
            if (this.RefType == NoggRefType.Direct)
            {
                return new string[]
                {
                    $"errorMask: (doErrorMask ? () =>",
                    "{",
                    $"   var errMask = errorMask();",
                    $"   if (errMask.{this.Name}.Specific == null)",
                    "   {",
                    $"      errMask.{this.Name} = new MaskItem<Exception, {this.RefGen.ErrorMask}>(",
                    $"         null,",
                    $"         new {this.RefGen.ErrorMask}());",
                    "   }",
                    $"   return errMask.{this.Name}.Specific;",
                    $"}} : default(Func<{this.RefGen.ErrorMask}>))"
                };
            }
            else
            {
                return new string[]
                {
                    $"errorMask: null"
                };
            }
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
                fg.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}.Overall ?? {nameof(CopyType)}.{nameof(CopyType.Reference)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Reference)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"{accessorPrefix}.{this.Name} = {rhsAccessorPrefix}.{this.Name};");
                        fg.AppendLine("break;");
                    }
                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.CopyIn)}:");
                    if (this.InterfaceType != NoggInterfaceType.IGetter)
                    {
                        using (new DepthWrapper(fg))
                        {
                            this.GenerateCopyFieldsFrom(fg);
                            fg.AppendLine("break;");
                        }
                    }
                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.MakeCopy)}:");
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
                                $"{accessorPrefix}.{this.Name} = {this.ObjectTypeName}.Copy{(this.InterfaceType == NoggInterfaceType.IGetter ? "_ToNoggolloquy" : string.Empty)}"))
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
                        fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyType)} {{{copyMaskAccessor}?.{(this.RefType == NoggRefType.Direct ? $"{this.Name}.Overall" : this.Name)}}}. Cannot execute copy.\");");
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
                        gen.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}{(this.RefType == NoggRefType.Generic ? string.Empty : ".Overall")} ?? {nameof(CopyType)}.{nameof(CopyType.Reference)})");
                        using (new BraceWrapper(gen))
                        {
                            gen.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Reference)}:");
                            using (new DepthWrapper(gen))
                            {
                                gen.AppendLine("return r;");
                            }
                            gen.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.CopyIn)}:");
                            if (this.InterfaceType != NoggInterfaceType.IGetter)
                            {
                                using (new DepthWrapper(gen))
                                {
                                    this.GenerateCopyFieldsFrom(gen);
                                    gen.AppendLine("return r;");
                                }
                            }
                            gen.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.MakeCopy)}:");
                            using (new DepthWrapper(gen))
                            {
                                gen.AppendLine("if (r == null) return null;");
                                if (this.RefType == NoggRefType.Direct)
                                {
                                    using (var args2 = new ArgsWrapper(gen,
                                        $"return {this.ObjectTypeName}.Copy{(this.InterfaceType == NoggInterfaceType.IGetter ? "_ToNoggolloquy" : string.Empty)}"))
                                    {
                                        args2.Add($"r");
                                        if (this.RefType == NoggRefType.Direct)
                                        {
                                            args2.Add($"{copyMaskAccessor}?.{this.Name}.Specific");
                                        }
                                        args2.Add($"def: d");
                                    }
                                }
                                else
                                {
                                    gen.AppendLine($"var copyFunc = {nameof(NoggolloquyRegistration)}.GetCopyFunc<{_generic}>();");
                                    gen.AppendLine($"return copyFunc(r, null, d);");
                                }
                            }
                            gen.AppendLine($"default:");
                            using (new DepthWrapper(gen))
                            {
                                gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyType)} {{{copyMaskAccessor}?.{(this.RefType == NoggRefType.Direct ? $"{this.Name}.Overall" : this.Name)}}}. Cannot execute copy.\");");
                            }
                        }
                    }
                });
            }
        }

        private void GenerateCopyFieldsFrom(FileGeneration fg)
        {
            if (this.RefType == NoggRefType.Direct)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{this.RefGen?.Obj.ExtCommonName}.CopyFieldsFrom"))
                {
                    args.Add($"item: item.{this.Name}");
                    args.Add($"rhs: rhs.{this.Name}");
                    args.Add($"def: def?.{this.Name}");
                    if (this.RefType == NoggRefType.Direct)
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
                    $"INoggolloquyObjectExt.CopyFieldsIn"))
                {
                    args.Add("obj: r");
                    args.Add("rhs: item.Ref");
                    args.Add("def: def?.Ref");
                    args.Add("skipReadonly: true");
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
            if (this.InterfaceType == NoggInterfaceType.IGetter)
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
            if (RefType == NoggRefType.Direct)
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
                case NoggRefType.Direct:
                    return this.RefGen.Obj.GetMaskString(type);
                case NoggRefType.Generic:
                    return "object";
                default:
                    throw new NotImplementedException();
            }
        }

        public string GenerateErrorMaskItemString()
        {
            switch (this.RefType)
            {
                case NoggRefType.Direct:
                    return this.RefGen.Obj.ErrorMask;
                case NoggRefType.Generic:
                    return "object";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
