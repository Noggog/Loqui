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
        public override bool CopyNeedsTryCatch => true;
        public override string SkipCheck(string copyMaskAccessor)
        {
            if (KeyTypeGen is NoggType
                || ValueTypeGen is NoggType)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            else
            {
                return $"{copyMaskAccessor}?.{this.Name} ?? true";
            }
        }

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
                fg.AppendLine($"{identifier}.{this.GetName(internalUse)}.HasBeenSet = {onIdentifier};");
            }
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetName(false)}.Unset({cmdsAccessor});");
            }
        }

        public override string GetName(bool internalUse, bool property = true)
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
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingDictionary{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                }
                fg.AppendLine($"INotifyingDictionaryGetter<{this.TypeTuple}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"new INotifyingDictionary{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.Name} {{ get; }}");
            }
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingDictionaryGetter<{this.TypeTuple}> {this.Name} {{ get; }}");
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
            if (!this.KeyIsNogg && !this.ValueIsNogg)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessorPrefix}.{this.Name}.SetToWithDefault"))
                {
                    args.Add($"rhs.{this.Name}");
                    args.Add($"def?.{this.Name}");
                    args.Add($"cmds");
                }
                return;
            }
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.Name}.SetToWithDefault"))
            {
                args.Add($"rhs.{this.Name}");
                args.Add($"def?.{this.Name}");
                args.Add($"cmds");
                args.Add((gen) =>
                {
                    gen.AppendLine("(k, v, d) =>");
                    using (new BraceWrapper(gen))
                    {
                        if (this.KeyIsNogg)
                        {
                            gen.AppendLine($"{this.KeyTypeGen.TypeName} key;");
                            gen.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}.Specific.Key.Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"key = k;");
                                    gen.AppendLine($"break;");
                                }
                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Deep)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"key = k.Copy({copyMaskAccessor}?.{this.Name}.Specific.Key.Mask);");
                                    gen.AppendLine($"break;");
                                }
                                gen.AppendLine($"default:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                }
                            }
                        }
                        if (this.ValueIsNogg)
                        { 
                            gen.AppendLine($"{this.ValueTypeGen.TypeName} val;");
                            gen.AppendLine($"switch ({copyMaskAccessor}?.{this.Name}.Specific.Value.Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"val = v;");
                                    gen.AppendLine($"break;");
                                }
                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Deep)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"val = v.Copy({copyMaskAccessor}?.{this.Name}.Specific.Value.Mask, d);");
                                    gen.AppendLine($"break;");
                                }
                                gen.AppendLine($"default:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                }
                            }
                        }

                        gen.AppendLine($"return new KeyValuePair<{this.KeyTypeGen.TypeName}, {this.ValueTypeGen.TypeName}>({(this.KeyIsNogg ? "key" : "k")}, {(this.ValueIsNogg ? "val" : "v")});");
                    }
                });
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
                        fg.AppendLine($"i.Key{(this.KeyIsNogg ? ".CopyFieldsFrom()" : string.Empty) },");
                        fg.AppendLine($"i.Value{(this.ValueIsNogg ? ".CopyFieldsFrom()" : string.Empty)})),");
                    }
                }
                fg.AppendLine($"{cmdAccessor});");
            }
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"({rhsAccessorPrefix}).Select(");
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
