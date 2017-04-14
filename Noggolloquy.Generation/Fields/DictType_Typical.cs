using System;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    class DictType_Typical : TypeGeneration, IDictType
    {
        public TypeGeneration ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen => this.ValueTypeGen;
        protected bool ValueIsNogg;
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen => this.KeyTypeGen;
        protected bool KeyIsNogg;
        public DictMode Mode => DictMode.KeyValue;

        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";

        public override bool Imports
        {
            get
            {
                if (!base.Imports) return false;
                if (!ValueTypeGen.Imports || !KeyTypeGen.Imports) return false;
                return true;
            }
        }

        public override string TypeName => $"NotifyingDictionary<{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}>";

        public string TypeTuple => $"{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}";

        public string GetterTypeName => (this.ValueIsNogg ? $"I{TypeName}Getter" : TypeName);

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
                KeyIsNogg = KeyTypeGen as NoggType != null;
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
                ValueIsNogg = ValueTypeGen as NoggType != null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        
        public void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key)
        {
            NoggType keyNoggType = this.KeyTypeGen as NoggType;
            NoggType valueNoggType = this.ValueTypeGen as NoggType;
            var item2 = $"KeyValuePair<{(keyNoggType == null ? "Exception" : keyNoggType.RefGen.Obj.GetMaskString("Exception"))}, {(valueNoggType == null ? "Exception" : valueNoggType.RefGen.Obj.GetMaskString("Exception"))}>";

            fg.AppendLine($"{errorMaskMemberAccessor}?.{this.Name}.Value.Add(new {item2}({(key ? exception : "null")}, {(key ? "null" : exception)}));");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetPropertyString(internalUse)}.HasBeenSet = {onIdentifier};");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetPropertyString(false)}.Unset({cmdsAccessor});");
            }
        }

        public override string GetPropertyString(bool internalUse)
        {
            if (internalUse)
            {
                return $"_{this.Name}";
            }
            else
            {
                return this.Name;
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (this.ValueTypeGen is WildcardType wild)
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
                fg.AppendLine($"INotifyingDictionary{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                fg.AppendLine($"INotifyingDictionaryGetter<{this.TypeTuple}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
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
                if (defaultFallbackAccessor == null || (!this.KeyIsNogg && !this.ValueIsNogg))
                {
                    GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor);
                }
                else
                {
                    fg.AppendLine("int i = 0;");
                    fg.AppendLine($"List<KeyValuePair<{this.TypeTuple}>> defList = {defaultFallbackAccessor}?.{this.Name}.ToList();");
                    fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"{rhsAccessorPrefix}.{this.Name}.Select((s) =>");
                        using (new BraceWrapper(fg))
                        {
                            if (KeyTypeGen is NoggType)
                            {
                                fg.AppendLine($"var key = new {this.KeyTypeGen.TypeName}();");
                                fg.AppendLine("if (defList != null && defList.InRange(i))");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("key.CopyFieldsFrom(s.Key, defList[i++].Key);");
                                }
                                fg.AppendLine("else");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("key.CopyFieldsFrom(s.Key);");
                                }
                            }
                            else
                            {
                                fg.AppendLine("var key = s.Key;");
                            }
                            if (ValueTypeGen is NoggType)
                            {
                                fg.AppendLine($"var value = new {this.KeyTypeGen.TypeName}();");
                                fg.AppendLine("if (defList != null && defList.InRange(i))");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("value.CopyFieldsFrom(s.Value, defList[i++].Value);");
                                }
                                fg.AppendLine("else");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine("value.CopyFieldsFrom(s.Value);");
                                }
                            }
                            else
                            {
                                fg.AppendLine("var value = s.Value;");
                            }
                            fg.AppendLine($"return new KeyValuePair<{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}>(key, value);");
                        }
                    }
                    fg.AppendLine($"), {cmdsAccessor});");
                }
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if ({defaultFallbackAccessor} == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdsAccessor}.ToUnsetParams());");
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
                fg.AppendLine($"{rhsAccessorPrefix}.{this.Name}.Select(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"(i) => new KeyValuePair<{this.KeyTypeGen.TypeName}, {this.ValueTypeGen.TypeName}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"i.Key{(this.KeyIsNogg ? ".Copy()" : string.Empty) },");
                        fg.AppendLine($"i.Value{(this.ValueIsNogg ? ".Copy()" : string.Empty)})),");
                    }
                }
                fg.AppendLine($"{cmdAccessor});");
            }
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"{rhsAccessorPrefix}.Select(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"(i) => new KeyValuePair<{this.KeyTypeGen.TypeName}, {this.ValueTypeGen.TypeName}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"i.Key{(this.KeyIsNogg ? ".Copy()" : string.Empty) },");
                        fg.AppendLine($"i.Value{(this.ValueIsNogg ? ".Copy()" : string.Empty)})),");
                    }
                }
                fg.AppendLine($"{cmdsAccessor});");
            }
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine($"return {identifier}.{this.Name};");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }
    }
}
