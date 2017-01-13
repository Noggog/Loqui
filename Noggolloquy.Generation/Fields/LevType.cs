using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class LevType : TypicalGeneration
    {
        public Ref RefGen { get; protected set; }
        public override string TypeName
        {
            get
            {
                switch (RefType)
                {
                    case LevRefType.Direct:
                        return RefGen.Name;
                    case LevRefType.Generic:
                        return _generic;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public string Getter
        {
            get
            {
                switch (RefType)
                {
                    case LevRefType.Direct:
                        return this.RefGen.Getter_InterfaceStr;
                    case LevRefType.Generic:
                        return this._generic;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public bool _allowNull = true;
        public bool AllowNull { get { return _allowNull && !SingletonMember; } }
        public bool SingletonMember;
        public LevRefType RefType { get; private set; }
        private string _generic;

        public enum LevRefType
        {
            Direct,
            Generic
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.Notifying)
            {
                if (AllowNull)
                {
                    fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItem<{TypeName}>();");
                }
                else if (SingletonMember)
                { // Singleton
                    fg.AppendLine($"private readonly INotifyingItem<{TypeName}> {this.ProtectedProperty} = new NotifyingItemLevSingleton<{TypeName}, {this.RefGen.InterfaceStr}, {this.Getter}>(new {this.RefGen.ObjectName}());");
                }
                else
                {
                    fg.AppendLine("private readonly INotifyingItem<" + TypeName + "> " + this.ProtectedProperty + " = new NotifyingItem<" + TypeName + ">(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine("defaultVal: new " + TypeName + "(),");
                        fg.AppendLine("incomingConverter: (oldV, i) =>");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine("if (i == null)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("i = new " + TypeName + "();");
                            }
                            fg.AppendLine("return new Tuple<" + TypeName + ", bool>(i, true);");
                        }
                    }
                    fg.AppendLine(");");
                }
                fg.AppendLine($"public INotifyingItem{(Protected ? "Getter" : string.Empty)}<{TypeName}> {this.Property} {{ get {{ return _{this.Name}; }} }}");
                fg.AppendLine($"{this.Getter} {this.ObjectGen.Getter_InterfaceStr}.{this.Name}");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("get { return " + this.Name + "; }");
                }
                fg.AppendLine("public " + TypeName + " " + this.Name + " { get { return _" + this.Name + ".Value; } " + (this.Protected ? string.Empty : "set { _" + this.Name + ".Value = value; } ") + "}");
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingItem<{this.TypeName}> {this.ObjectGen.InterfaceStr}.{this.Property} {{ get {{ return this.{this.Property}; }} }}");
                }
                fg.AppendLine($"INotifyingItemGetter<{this.TypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Property} {{ get {{ return this.{this.Property}; }} }}");
            }
            else
            {
                fg.AppendLine($"private {(this.ReadOnly ? "readonly" : string.Empty)} HasBeenSetItem<{this.TypeName}> {this.ProtectedProperty} = new HasBeenSetItem<{this.TypeName}>();");
                fg.AppendLine($"public {this.Getter} {this.Name} {{ get {{ return this.{this.ProtectedName}; }} {(Protected ? "protected " : string.Empty)}set {{ {this.ProtectedName} = value; }} }}");
                if (this.ReadOnly)
                {
                    fg.AppendLine($"public IHasBeenSetGetter {this.Property} {{ get {{ return this.{this.Property}; }} }}");
                }
                else
                {
                    fg.AppendLine($"public IHasBeenSet<{this.TypeName}> {this.Property} {{ get {{ return {this.ProtectedProperty}; }} }}");
                }
                fg.AppendLine($"{this.Getter} {this.ObjectGen.Getter_InterfaceStr}.{this.Name} {{ get {{ return this.{this.ProtectedName}; }} }}");
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

            var refName = refNode?.GetAttribute("levName");
            var genericName = genericNode?.Value;

            if (!string.IsNullOrWhiteSpace(refName))
            {
                Ref r = new Ref();

                var genElems = refNode.Elements(XName.Get("Generic", NoggolloquyGenerator.Namespace)).ToList();
                if (genElems.Count > 0)
                {
                    r = new GenRef()
                    {
                        Generics = genElems.Select((e) => e.Value).ToList()
                    };
                }

                this.RefType = LevRefType.Direct;
                ObjectGeneration refGen;
                if (!this.ProtoGen.ObjectGenerationsByName.TryGetValue(refName, out refGen))
                {
                    throw new ArgumentException("Lev type cannot be found: " + refName);
                }
                r.Obj = refGen;
                this.RefGen = r;
            }
            else if (!string.IsNullOrWhiteSpace(genericName))
            {
                this.RefType = LevRefType.Generic;
                this._generic = genericName;
                var gen = this.ObjectGen.Generics[this._generic];
                gen.MustBeClass = true;
                gen.Wheres.Add($"INoggolloquyCopyInSerializer");
                if (SingletonMember)
                {
                    throw new ArgumentException("Cannot be a generic and singleton.");
                }
            }
            else
            {
                throw new ArgumentException("Lev type needs a target.");
            }
        }

        public override void SetMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskMemberAccessor}.Overall = {exception};");
        }

        private string StructTypeName()
        {
            return TypeName + (this.AllowNull ? "?" : string.Empty);
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdAccessor)
        {
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                GenerateCopyFrom(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdAccessor);
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("if (" + defaultFallbackAccessor + " == null)");
                using (new BraceWrapper(fg))
                {
                    GenerateClear(fg, accessorPrefix, cmdAccessor);
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    GenerateCopyFrom(fg, accessorPrefix, rhsAccessorPrefix: defaultFallbackAccessor, defaultAccessorPrefix: null, cmdAccessor: cmdAccessor);
                }
            }
            fg.AppendLine();
        }

        private void GenerateCopyFrom(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultAccessorPrefix, string cmdAccessor)
        {
            if (this.RefType == LevRefType.Generic
                || RefGen.Obj is ClassGeneration)
            {
                fg.AppendLine($"if (rhs.{this.Name} == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Name} = null;");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"if ({accessorPrefix}.{this.Name} == null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{accessorPrefix}.{this.Name} = ({this.TypeName}){rhsAccessorPrefix}.{this.Name}.Copy();");
                    }
                    fg.AppendLine("else");
                    using (new BraceWrapper(fg))
                    {
                        using (new LineWrapper(fg))
                        {
                            fg.Append(accessorPrefix + "." + this.Name + ".CopyFieldsFrom(" + rhsAccessorPrefix + "." + this.Name);
                            if (defaultAccessorPrefix != null)
                            {
                                fg.Append(", def: " + defaultAccessorPrefix + "?." + this.Name);
                            }
                            else
                            {
                                fg.Append(", null");
                            }
                            fg.Append(", cmds: " + cmdAccessor);
                            fg.Append(");");
                        }
                    }
                }
            }
            else if (RefGen.Obj is StructGeneration)
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name} = new {this.RefGen.Obj.Name}({rhsAccessorPrefix}.{this.Name});");
            }
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdAccessor)
        {
            if (!this.SingletonMember)
            {
                base.GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdAccessor);
            }
            else
            {
                this.GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdAccessor);
            }
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"{this.Getter} {this.Name} {{ get; }}");
            if (this.Notifying)
            {
                fg.AppendLine($"INotifyingItemGetter<{TypeName}> {this.Property} {{ get; }}");
            }
            else
            {
                fg.AppendLine($"IHasBeenSetGetter {this.Property} {{ get; }}");
            }
            fg.AppendLine();
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            if (RefType == LevRefType.Direct)
            {
                yield return RefGen.Obj.Namespace;
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            if (this.SingletonMember)
            {
                fg.AppendLine(accessorPrefix + "." + this.Name + ".Clear(" + cmdAccessor + ".ToUnsetParams());");
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
                case LevRefType.Direct:
                    return this.RefGen.Obj.GetMaskString(type);
                case LevRefType.Generic:
                    return "object";
                default:
                    throw new NotImplementedException();
            }
        }

        public string GenerateMaskItemString(string type)
        {
            switch (this.RefType)
            {
                case LevRefType.Direct:
                    return this.RefGen.Obj.GetMaskItemString(type);
                case LevRefType.Generic:
                    return "object";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
