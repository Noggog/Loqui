using Noggog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class DictType_Typical : TypeGeneration, IDictType
    {
        public TypeGeneration ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen => this.ValueTypeGen;
        protected bool ValueIsLoqui;
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen => this.KeyTypeGen;
        protected bool KeyIsLoqui;
        public DictMode Mode => DictMode.KeyValue;
        public bool BothAreLoqui => KeyIsLoqui && ValueIsLoqui;

        public override bool CopyNeedsTryCatch => true;
        public override bool IsEnumerable => true;
        public override bool IsClass => true;
        public override bool HasDefault => false;
        public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
        {
            if (KeyTypeGen is LoquiType
                || ValueTypeGen is LoquiType)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            else
            {
                return $"{copyMaskAccessor}?.{this.Name} ?? true";
            }
        }

        public override string TypeName(bool getter, bool needsCovariance = false) => $"Dictionary<{KeyTypeGen.TypeName(getter, needsCovariance)}, {ValueTypeGen.TypeName(getter, needsCovariance)}>";

        public string TypeTuple(bool getter) => $"{KeyTypeGen.TypeName(getter)}, {ValueTypeGen.TypeName(getter)}";

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            var keyNode = node.Element(XName.Get("Key", LoquiGenerator.Namespace));
            if (keyNode == null)
            {
                throw new ArgumentException("Dict had no key element.");
            }

            var keyTypeGen = await ObjectGen.LoadField(
                keyNode.Elements().FirstOrDefault(),
                requireName: false,
                setDefaults: false);
            if (keyTypeGen.Succeeded)
            {
                this.KeyTypeGen = keyTypeGen.Value;
                KeyIsLoqui = keyTypeGen.Value as LoquiType != null;
            }
            else
            {
                throw new NotImplementedException();
            }

            var valNode = node.Element(XName.Get("Value", LoquiGenerator.Namespace));
            if (valNode == null)
            {
                throw new ArgumentException("Dict had no value element.");
            }

            var valueTypeGen = await ObjectGen.LoadField(
                valNode.Elements().FirstOrDefault(),
                requireName: false,
                setDefaults: false);
            if (valueTypeGen.Succeeded)
            {
                this.ValueTypeGen = valueTypeGen.Value;
                ValueIsLoqui = valueTypeGen.Value is LoquiType;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (keyTypeGen.Value is ContainerType
                || keyTypeGen.Value is DictType)
            {
                throw new NotImplementedException();
            }
            if (valueTypeGen.Value is ContainerType
                || valueTypeGen.Value is DictType)
            {
                throw new NotImplementedException();
            }
        }

        public void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key)
        {
            LoquiType keyLoquiType = this.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = this.ValueTypeGen as LoquiType;
            var item2 = $"KeyValuePair<{(keyLoquiType == null ? "Exception" : keyLoquiType.TargetObjectGeneration.GetMaskString("Exception"))}, {(valueLoquiType == null ? "Exception" : valueLoquiType.TargetObjectGeneration.GetMaskString("Exception"))}>";

            fg.AppendLine($"{errorMaskMemberAccessor}?.{this.Name}.Value.Add(new {item2}({(key ? exception : "null")}, {(key ? "null" : exception)}));");
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier}.{this.GetName(false)}.Clear();");
            }
            fg.AppendLine("break;");
        }

        public override string GetName(bool internalUse)
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
            fg.AppendLine($"private readonly Dictionary<{TypeTuple(getter: false)}> _{this.Name} = new Dictionary<{TypeTuple(getter: false)}>();");
            Comments?.Apply(fg, LoquiInterfaceType.Direct);
            fg.AppendLine($"public IDictionary<{TypeTuple(getter: false)}> {this.Name} => _{this.Name};");

            var member = "_" + this.Name;
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"IDictionary{(this.ReadOnly ? "Getter" : string.Empty)}<{this.TypeTuple(getter: false)}> {this.ObjectGen.Interface()}.{this.Name} => {member};");
                }
                if (this.ValueIsLoqui)
                {
                    fg.AppendLine($"IReadOnlyDictionary<{this.TypeTuple(getter: true)}> {this.ObjectGen.Interface(getter: true)}.{this.Name} => {member}.Covariant<{TypeTuple(getter: false)}, {this.ValueTypeGen.TypeName(getter: true)}>();");
                }
                else
                {
                    fg.AppendLine($"IReadOnlyDictionary<{this.TypeTuple(getter: true)}> {this.ObjectGen.Interface(getter: true)}.{this.Name} => {member};");
                }
            }
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (getter)
            {
                Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
                fg.AppendLine($"IReadOnlyDictionary<{this.TypeTuple(getter: true)}> {this.Name} {{ get; }}");
            }
            else
            {
                if (!this.ReadOnly)
                {
                    Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
                    fg.AppendLine($"new IDictionary{(this.ReadOnly ? "Getter" : string.Empty)}<{this.TypeTuple(getter: false)}> {this.Name} {{ get; }}");
                }
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            Accessor rhs,
            Accessor copyMaskAccessor,
            bool protectedMembers,
            bool deepCopy)
        {
            if (!this.AlwaysCopy)
            {
                fg.AppendLine($"if ({(deepCopy ? this.GetTranslationIfAccessor(copyMaskAccessor) : this.SkipCheck(copyMaskAccessor, deepCopy))})");
            }
            using (new BraceWrapper(fg, doIt: !AlwaysCopy))
            {
                if (!this.KeyIsLoqui && !this.ValueIsLoqui)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor}.SetTo"))
                    {
                        args.Add($"rhs.{this.Name}");
                    }
                    return;
                }
                if (deepCopy)
                {
                    if (this.KeyIsLoqui)
                    {
                        throw new NotImplementedException();
                    }
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor}.SetTo"))
                    {
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"rhs.{this.Name}");
                            using (new DepthWrapper(gen))
                            {
                                gen.AppendLine(".Select((r) =>");
                                using (new BraceWrapper(gen) { AppendParenthesis = true })
                                {
                                    (this.ValueTypeGen as LoquiType).GenerateTypicalMakeCopy(
                                        gen,
                                        retAccessor: $"var value = ",
                                        rhsAccessor: new Accessor("r.Value"),
                                        copyMaskAccessor: copyMaskAccessor,
                                        deepCopy: deepCopy,
                                        doTranslationMask: false);
                                    gen.AppendLine($"return new KeyValuePair<{this.KeyTypeGen.TypeName(getter: true)}, {this.ValueTypeGen.TypeName(getter: false)}>(r.Key, value);");
                                }
                            }
                        });
                    }
                    return;
                }
                throw new NotImplementedException();
                MaskGenerationUtility.WrapErrorFieldIndexPush(
                    fg,
                    () =>
                    {
                        if (!this.KeyIsLoqui && !this.ValueIsLoqui)
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{accessor}.SetTo"))
                            {
                                args.Add($"rhs.{this.Name}");
                            }
                            return;
                        }
                        using (var args = new ArgsWrapper(fg,
                            $"{accessor}.SetTo"))
                        {
                            args.Add((gen) =>
                            {
                                gen.AppendLine($"rhs.{this.Name}");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine(".Select((r) =>");
                                    using (new BraceWrapper(gen) { AppendParenthesis = true })
                                    {
                                        if (this.KeyIsLoqui)
                                        {
                                            throw new NotImplementedException();
                                            gen.AppendLine($"{this.KeyTypeGen.TypeName(getter: false)} key;");
                                            gen.AppendLine($"switch ({copyMaskAccessor}?.Specific.{(this.BothAreLoqui ? "Key." : string.Empty)}Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                                            using (new BraceWrapper(gen))
                                            {
                                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine($"key = r.Key;");
                                                    gen.AppendLine($"break;");
                                                }
                                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.MakeCopy)}:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine($"key = r.Key.Copy(copyMask: {copyMaskAccessor}?.Specific.{(this.BothAreLoqui ? "Key." : string.Empty)}Mask);");
                                                    gen.AppendLine($"break;");
                                                }
                                                gen.AppendLine($"default:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                                }
                                            }
                                        }
                                        if (this.ValueTypeGen is LoquiType valLoqui)
                                        {
                                            gen.AppendLine($"{this.ValueTypeGen.TypeName(getter: false)} val;");
                                            gen.AppendLine($"switch ({copyMaskAccessor}?.Specific.{(this.BothAreLoqui ? "Value." : string.Empty)}Type ?? {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)})");
                                            using (new BraceWrapper(gen))
                                            {
                                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.Reference)}:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine($"val = r.Value;");
                                                    gen.AppendLine($"break;");
                                                }
                                                gen.AppendLine($"case {nameof(RefCopyType)}.{nameof(RefCopyType.MakeCopy)}:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    valLoqui.GenerateTypicalMakeCopy(
                                                        gen,
                                                        retAccessor: $"val = ",
                                                        rhsAccessor: new Accessor("r.Value"),
                                                        copyMaskAccessor: copyMaskAccessor,
                                                        deepCopy: deepCopy,
                                                        doTranslationMask: false);
                                                    gen.AppendLine($"break;");
                                                }
                                                gen.AppendLine($"default:");
                                                using (new DepthWrapper(gen))
                                                {
                                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(RefCopyType)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                                }
                                            }
                                        }

                                        gen.AppendLine($"return new KeyValuePair<{this.KeyTypeGen.TypeName(getter: false)}, {this.ValueTypeGen.TypeName(getter: false)}>({(this.KeyIsLoqui ? "key" : "r.Key")}, {(this.ValueIsLoqui ? "val" : "r.Value")});");
                                    }
                                }
                            });
                        }
                    },
                    errorMaskAccessor: "errorMask",
                    indexAccessor: this.HasIndex ? this.IndexEnumInt : default(Accessor),
                    doIt: this.CopyNeedsTryCatch);
            }
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"{rhsAccessorPrefix}.{this.Name}.Select(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"(i) => new KeyValuePair<{this.KeyTypeGen.TypeName(getter: false)}, {this.ValueTypeGen.TypeName(getter: false)}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"i.Key{(this.KeyIsLoqui ? ".CopyFieldsFrom()" : string.Empty) },");
                        fg.AppendLine($"i.Value{(this.ValueIsLoqui ? ".CopyFieldsFrom()" : string.Empty)})),");
                    }
                }
            }
        }

        public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
        {
            fg.AppendLine($"{accessor}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"({rhs}).Select(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"(i) => new KeyValuePair<{this.KeyTypeGen.TypeName(getter: false)}, {this.ValueTypeGen.TypeName(getter: false)}>(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"i.Key{(this.KeyIsLoqui ? ".Copy()" : string.Empty) },");
                        fg.AppendLine($"i.Value{(this.ValueIsLoqui ? ".Copy()" : string.Empty)})),");
                    }
                }
            }
            fg.AppendLine($"break;");
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            fg.AppendLine($"return {identifier.Access};");
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            fg.AppendLine($"{accessorPrefix.Access}.Clear();");
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})";
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
        {
            fg.AppendLine($"if ({this.GetTranslationIfAccessor(maskAccessor)})");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!{accessor.Access}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor.Access})) return false;");
            }
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
        }

        public void GenerateForEqualsMaskCheck(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            LoquiType keyLoqui = KeyTypeGen as LoquiType;
            LoquiType valLoqui = ValueTypeGen as LoquiType;
            if (keyLoqui != null
                && valLoqui != null)
            {
                throw new NotImplementedException();
            }
            else if (keyLoqui != null)
            {
                throw new NotImplementedException();
            }
            else if (valLoqui != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{retAccessor} = EqualsMaskHelper.DictEqualsHelper"))
                {
                    args.Add($"lhs: {accessor}");
                    args.Add($"rhs: {rhsAccessor}");
                    args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
                    args.AddPassArg("include");
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{retAccessor} = EqualsMaskHelper.DictEqualsHelper"))
                {
                    args.Add($"lhs: {accessor}");
                    args.Add($"rhs: {rhsAccessor}");
                    args.AddPassArg("include");
                }
            }
        }

        public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
        {
            fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool", getter: true)}();");
            fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
        }

        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            fg.AppendLine($"{hashResultAccessor}.Add({accessor});");
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"{name} =>\");");
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
            fg.AppendLine($"using (new DepthWrapper(fg))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"foreach (var subItem in {accessor.Access})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                    fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                    using (new BraceWrapper(fg))
                    {
                        this.KeyTypeGen.GenerateToString(fg, "Key", new Accessor("subItem.Key"), fgAccessor);
                        this.ValueTypeGen.GenerateToString(fg, "Value", new Accessor("subItem.Value"), fgAccessor);
                    }
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
                }
            }
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            fg.AppendLine($"if ({checkMaskAccessor}?.Overall.HasValue ?? false) return false;");
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }
    }
}
