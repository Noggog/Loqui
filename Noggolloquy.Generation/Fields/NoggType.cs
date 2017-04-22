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
        public bool _allowNull = true;
        public bool AllowNull => _allowNull && !SingletonMember;
        public bool SingletonMember;
        public NoggRefType RefType { get; private set; }
        public NoggInterfaceType InterfaceType = NoggInterfaceType.Direct;
        private string _generic;
        public override string SkipAccessor(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name}.Overall";

        public enum NoggRefType
        {
            Direct,
            Generic
        }

        public override bool CopyNeedsTryCatch => true;

        public override void GenerateForClass(FileGeneration fg)
        {
            switch (this.Notifying)
            {
                case NotifyingOption.None:
                    fg.AppendLine($"public {this.TypeName} {this.Name} {{ get; {(this.ReadOnly ? "protected " : string.Empty)}set; }}");
                    break;
                case NotifyingOption.HasBeenSet:
                    fg.AppendLine($"private {(this.ReadOnly ? "readonly" : string.Empty)} HasBeenSetItem<{this.TypeName}> {this.ProtectedProperty} = new HasBeenSetItem<{this.TypeName}>();");
                    fg.AppendLine($"public {this.TypeName} {this.Name} {{ get {{ return this.{this.ProtectedName}; }} {(Protected ? "protected " : string.Empty)}set {{ {this.ProtectedName} = value; }} }}");
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
                    if (AllowNull)
                    {
                        fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItem<{TypeName}>();");
                    }
                    else if (SingletonMember)
                    { // Singleton
                        fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItemNoggSingleton<{TypeName}, {this.RefGen.InterfaceStr}, {this.TypeName}>(new {this.RefGen.ObjectName}());");
                    }
                    else
                    {
                        fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItem<{TypeName}>(");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"defaultVal: new {TypeName}(),");
                            fg.AppendLine("incomingConverter: (oldV, i) =>");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("if (i == null)");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine($"i = new {TypeName}();");
                                }
                                fg.AppendLine($"return new Tuple<{TypeName}, bool>(i, true);");
                            }
                        }
                        fg.AppendLine(");");
                    }
                    fg.AppendLine($"public INotifyingItem{(Protected ? "Getter" : string.Empty)}<{TypeName}> {this.Property} => this._{this.Name};");
                    fg.AppendLine($"{this.TypeName} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => this.{this.Name};");
                    fg.AppendLine($"public {TypeName} {this.Name} {{ get {{ return _{this.Name}.Value; }} {(this.Protected ? string.Empty : $"set {{ _{this.Name}.Value = value; }} ")}}}");
                    if (!this.ReadOnly)
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
            _allowNull = node.GetAttribute<bool>("allowNull", true);
            SingletonMember = node.GetAttribute<bool>("singleton", false);

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
                if (SingletonMember)
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
            return $"{TypeName}{(this.AllowNull ? "?" : string.Empty)}";
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
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}.Overall ?? {nameof(CopyType)}.{nameof(CopyType.Reference)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Reference)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedMembers, false)} = {rhsAccessorPrefix}.{this.Name};");
                        fg.AppendLine("break;");
                    }
                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Deep)}:");
                    using (new DepthWrapper(fg))
                    {
                        if (this.InterfaceType == NoggInterfaceType.IGetter)
                        {
                            fg.AppendLine($"throw new ArgumentException($\"Cannot deep copy a getter reference.\");");
                        }
                        else
                        {
                            using (var args = new ArgsWrapper(fg, true,
                                $"{accessorPrefix}.{this.GetName(protectedMembers, false)}.CopyFieldsFrom"))
                            {
                                args.Add($"{rhsAccessorPrefix}.{this.Name}");
                                args.Add($"{copyMaskAccessor}.{this.Name}.Specific");
                                args.Add($"{defaultFallbackAccessor}?.{this.Name}");
                                args.Add("cmds");
                            }
                            fg.AppendLine("break;");
                        }
                    }
                    fg.AppendLine($"default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyType)} nameof({copyMaskAccessor}?.Overall). Cannot execute copy.\");");
                    }
                }
                return;
            }
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                GenerateCopyFrom(
                    fg: fg,
                    accessorPrefix: accessorPrefix, 
                    rhsAccessorPrefix: rhsAccessorPrefix, 
                    copyMaskAccessor: copyMaskAccessor,
                    defaultAccessorPrefix: defaultFallbackAccessor,
                    cmdAccessor: cmdsAccessor, 
                    protectedUse: protectedMembers);
            }
            fg.AppendLine($"else if ({defaultFallbackAccessor} == null)");
            using (new BraceWrapper(fg))
            {
                GenerateClear(fg, accessorPrefix, cmdsAccessor);
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                GenerateCopyFrom(
                    fg: fg,
                    accessorPrefix: accessorPrefix, 
                    rhsAccessorPrefix: defaultFallbackAccessor, 
                    copyMaskAccessor: copyMaskAccessor,
                    defaultAccessorPrefix: null,
                    cmdAccessor: cmdsAccessor, 
                    protectedUse: protectedMembers);
            }
            fg.AppendLine();
        }

        private void GenerateCopyFrom(
            FileGeneration fg, 
            string accessorPrefix, 
            string rhsAccessorPrefix,
            string copyMaskAccessor, 
            string defaultAccessorPrefix, 
            string cmdAccessor, 
            bool protectedUse)
        {
            if (this.RefType == NoggRefType.Generic
                || RefGen.Obj is ClassGeneration)
            {
                using (var args = new ArgsWrapper(fg, true,
                    $"{accessorPrefix}.{this.GetName(protectedUse, true)}.Set"))
                {
                    args.Add($"{rhsAccessorPrefix}.{this.Name}");
                    if (this.Notifying == NotifyingOption.Notifying)
                    {
                        args.Add($"cmds: {cmdAccessor}");
                    }
                }
            }
            else if (RefGen.Obj is StructGeneration)
            {
                fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedUse, false)} = new {this.RefGen.Obj.Name}({rhsAccessorPrefix}.{this.Name});");
            }
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

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            if (RefType == NoggRefType.Direct)
            {
                yield return RefGen.Obj.Namespace;
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.SingletonMember)
            {
            }
            else
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
