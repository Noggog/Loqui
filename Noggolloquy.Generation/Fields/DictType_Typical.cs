using System;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    class DictType_Typical : TypeGeneration, IDictType
    {
        public TypeGeneration ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen { get { return this.ValueTypeGen; } }
        protected bool ValueIsLev;
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen { get { return this.KeyTypeGen; } }
        protected bool KeyIsLev;
        public DictMode Mode { get { return DictMode.KeyValue; } }

        public override string Property { get { return $"{this.Name}"; } }
        public override string ProtectedName { get { return $"{this.ProtectedProperty}"; } }

        public override bool Imports
        {
            get
            {
                if (!base.Imports) return false;
                if (!ValueTypeGen.Imports || !KeyTypeGen.Imports) return false;
                return true;
            }
        }

        public override string TypeName
        {
            get
            {
                return $"KeyValuePair<{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}>";
            }
        }

        public string TypeTuple { get { return $"{ KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}"; } }

        public string GetterTypeName
        {
            get { return (this.ValueIsLev ? "I" + TypeName + "Getter" : TypeName); }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            var keyNode = node.Element(XName.Get("Key", NoggolloquyGenerator.Namespace));
            if (keyNode == null)
            {
                throw new ArgumentException("Dict had no key element.");
            }

            if (ObjectGen.LoadField(
                    keyNode.Elements().FirstOrDefault(),
                    false,
                    out KeyTypeGen))
            {
                KeyIsLev = KeyTypeGen as LevType != null;
            }
            else
            {
                throw new NotImplementedException();
            }

            var valNode = node.Element(XName.Get("Value", NoggolloquyGenerator.Namespace));
            if (valNode == null)
            {
                throw new ArgumentException("Dict had no value element.");
            }

            if (ObjectGen.LoadField(
                    valNode.Elements().FirstOrDefault(),
                    false,
                    out ValueTypeGen))
            {
                ValueIsLev = ValueTypeGen as LevType != null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void SetMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskMemberAccessor}.{this.Name}.Overall = {exception};");
        }

        public void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key)
        {
            LevType keyLevType = this.KeyTypeGen as LevType;
            LevType valueLevType = this.ValueTypeGen as LevType;
            var item2 = $"KeyValuePair<{(keyLevType == null ? "Exception" : keyLevType.RefGen.Obj.GetMaskString("Exception"))}, {(valueLevType == null ? "Exception" : valueLevType.RefGen.Obj.GetMaskString("Exception"))}>";
            
            fg.AppendLine($"{errorMaskMemberAccessor}?.{this.Name}.Value.Add(new {item2}({(key ? exception : "null")}, {(key ? "null" : exception)}));");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            fg.AppendLine(identifier + "." + this.GetPropertyString(internalUse) + ".Unset();");
        }

        public override string GetPropertyString(bool internalUse)
        {
            if (internalUse)
            {
                return "_" + this.Name;
            }
            else
            {
                return this.Name;
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            WildcardType wild = this.ValueTypeGen as WildcardType;
            if (wild != null)
            {
                fg.AppendLine($"private readonly INotifyingDictionary<{TypeTuple}> _{this.Name} = new NotifyingDictionary<{TypeTuple}>(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine("valConv: (o) => WildcardLink.Validate(o));");
                }
            }
            else
            {
                fg.AppendLine($"private readonly INotifyingDictionary<{TypeTuple}> _{this.Name} = new NotifyingDictionary<{TypeTuple}>();");
            }
            fg.AppendLine($"public INotifyingDictionary<{TypeTuple}> {this.Name} {{ get {{ return _{this.Name}; }} }}");

            var member = "_" + this.Name;
            using (new RegionWrapper(fg, "Interface Members"))
            {
                fg.AppendLine("INotifyingDictionary" + (this.Protected ? "Getter" : string.Empty) + "<" + this.TypeTuple + "> " + this.ObjectGen.InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
                fg.AppendLine("INotifyingDictionaryGetter<" + this.TypeTuple + "> " + this.ObjectGen.Getter_InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            fg.AppendLine($"new INotifyingDictionary{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.Name} {{ get; }}");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingDictionaryGetter<{this.TypeTuple}> {this.Name} {{ get; }}");
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                if (defaultFallbackAccessor == null || (!this.KeyIsLev && !this.ValueIsLev))
                {
                    GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor);
                }
                else
                {
                    fg.AppendLine("int i = 0;");
                    fg.AppendLine($"List<{this.TypeTuple}> defList = {defaultFallbackAccessor}?.{this.Name}.ToList();");
                    fg.AppendLine(accessorPrefix + "." + this.Name + ".SetTo(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine(rhsAccessorPrefix + "." + this.Name + ".Select((s) =>");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine("var ret = new " + this.TypeName + "();");
                            fg.AppendLine("if (defList != null && defList.InRange(i))");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("ret.CopyFieldsFrom(s, defList[i++]);");
                            }
                            fg.AppendLine("else");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("ret.CopyFieldsFrom(s);");
                            }
                            fg.AppendLine("return ret;");
                        }
                    }
                    fg.AppendLine("), " + cmdsAccessor + ");");
                }
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("if (" + defaultFallbackAccessor + " == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine(accessorPrefix + "." + this.Name + ".Unset(" + cmdsAccessor + ".ToUnsetParams());");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdsAccessor);
                }
            }
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"{ rhsAccessorPrefix}.{ this.Name}.Select(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"(i) => new KeyValuePair<{this.KeyTypeGen.TypeName}, {this.ValueTypeGen.TypeName}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"i.Key{ (this.KeyIsLev ? ".Copy())" : string.Empty) },");
                        fg.AppendLine($"i.Value{ (this.ValueIsLev ? ".Copy())" : string.Empty)})),");
                    }
                }
                fg.AppendLine($"{ cmdAccessor});");
            }
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine("return " + identifier + "." + this.Name + ";");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine(accessorPrefix + "." + this.Name + ".Unset(" + cmdAccessor + ".ToUnsetParams());");
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }
    }
}
