using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class DictType_KeyedValue : TypeGeneration, IDictType
    {
        public LoquiType ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen => this.ValueTypeGen;
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen => this.KeyTypeGen;
        public string KeyAccessorString { get; protected set; }
        public DictMode Mode => DictMode.KeyedValue;

        public override bool IsClass => true;
        public override bool HasDefault => false;
        public override bool IsEnumerable => true;
        public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
        {
            if (deepCopy)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            else
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            }
        }
        
        public override bool CopyNeedsTryCatch => true;

        public override string TypeName(bool getter, bool needsCovariance = false) => $"ICache<{BackwardsTypeTuple(getter, needsCovariance)}>";

        public string TypeTuple(bool getter) => $"{KeyTypeGen.TypeName(getter)}, {ValueTypeGen.TypeName(getter)}";
        public string BackwardsTypeTuple(bool getter, bool needsCovariance = false) => $"{ValueTypeGen.TypeName(getter, needsCovariance)}, {KeyTypeGen.TypeName(getter, needsCovariance)}";

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            var keyedValNode = node.Element(XName.Get(Constants.KEYED_VALUE, LoquiGenerator.Namespace));
            if (keyedValNode == null)
            {
                throw new ArgumentException("Dict had no keyed value element.");
            }

            var valType = await ObjectGen.LoadField(
                keyedValNode.Elements().FirstOrDefault(),
                requireName: false,
                setDefaults: false);
            if (valType.Succeeded
                && valType.Value is LoquiType)
            {
                this.ValueTypeGen = valType.Value as LoquiType;
            }
            else
            {
                throw new NotImplementedException();
            }

            var keyAccessorAttr = keyedValNode.Attribute(XName.Get(Constants.KEY_ACCESSOR));
            if (keyAccessorAttr == null)
            {
                throw new ArgumentException("Dict had no key accessor attribute.");
            }

            this.KeyAccessorString = keyAccessorAttr.Value;
            if (this.ValueTypeGen.GenericDef == null)
            {
                await this.ValueTypeGen.TargetObjectGeneration.LoadingCompleteTask.Task;
                this.KeyTypeGen = this.ValueTypeGen.TargetObjectGeneration.IterateFields(includeBaseClass: true).FirstOrDefault((f) => f.Name.Equals(keyAccessorAttr.Value));
                if (this.KeyTypeGen == null)
                {
                    throw new ArgumentException($"Dict had a key accessor attribute that didn't correspond to a field: {keyAccessorAttr.Value}");
                }
            }
            else
            {
                if (!keyedValNode.TryGetAttribute<string>(Constants.KEY_TYPE, out var keyTypeName))
                {
                    throw new ArgumentException("Cannot have a generic keyed reference without manually specifying keyType");
                }
                if (!this.ObjectGen.ProtoGen.Gen.TryGetTypeGeneration(keyTypeName, out var keyTypeGen))
                {
                    throw new ArgumentException($"Generic keyed type specification did not link to a known field type: {keyTypeName}");
                }
                this.KeyTypeGen = keyTypeGen;
            }
            await base.Resolve();

            if (KeyTypeGen is ContainerType
                || KeyTypeGen is DictType)
            {
                throw new NotImplementedException();
            }
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception, bool key)
        {
            LoquiType valueLoquiType = this.ValueTypeGen as LoquiType;
            var valStr = valueLoquiType == null ? "Exception" : $"Tuple<Exception, {valueLoquiType.TargetObjectGeneration.GetMaskString("Exception")}>";

            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Value.Add({(key ? "null" : exception)});");
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier}.Unset();");
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

        protected virtual string GetActualItemClass(bool getter)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.Nullable)
                {
                    return $"SourceSetCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{this.KeyAccessorString})";
                }
                else
                {
                    return $"SourceCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{this.KeyAccessorString})";
                }
            }
            else
            {
                if (this.Nullable)
                {
                    return $"SetCache<{BackwardsTypeTuple(getter: false)}>((item) => item.{this.KeyAccessorString})";
                }
                else
                {
                    return $"Cache<{BackwardsTypeTuple(getter: false)}>((item) => item.{this.KeyAccessorString})";
                }
            }
        }

        public string DictInterface(bool getter)
        {
            if (this.ReadOnly || getter)
            {
                if (this.Notifying)
                {
                    if (this.Nullable)
                    {
                        return $"IObservableSetCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                    else
                    {
                        return $"IObservableCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                }
                else
                {
                    if (this.Nullable)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        return $"IReadOnlyCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                }
            }
            else
            {
                if (this.Notifying)
                {
                    if (this.Nullable)
                    {
                        return $"ISourceSetCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                    else
                    {
                        return $"ISourceCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                }
                else
                {
                    if (this.Nullable)
                    {
                        return $"ISetCache<{this.BackwardsTypeTuple(getter)}>";
                    }
                    else
                    {
                        return $"ICache<{this.BackwardsTypeTuple(getter)}>";
                    }
                }
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"private readonly {this.DictInterface(getter: false)} _{this.Name} = new {GetActualItemClass(getter: false)};");
            Comments?.Apply(fg, LoquiInterfaceType.Direct);
            fg.AppendLine($"public {this.DictInterface(getter: false)} {this.Name} => _{this.Name};");

            var member = $"_{this.Name}";
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"{DictInterface(getter: false)} {this.ObjectGen.Interface(internalInterface: false)}.{this.Name} => {member};");
                }
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"{DictInterface(getter: true)} {this.ObjectGen.Interface(getter: true, internalInterface: false)}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
            Comments?.Apply(fg, getter ? LoquiInterfaceType.IGetter : LoquiInterfaceType.ISetter);
            fg.AppendLine($"{(getter ? null : "new ")}{DictInterface(getter: getter)} {this.Name} {{ get; }}");
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
                MaskGenerationUtility.WrapErrorFieldIndexPush(
                    fg,
                    () =>
                    {
                        var loqui = this.ValueTypeGen as LoquiType;
                        if (this.Nullable)
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{accessor}.SetTo"))
                            {
                                args.Add($"rhs.{this.Name}");
                                args.Add((gen) =>
                                {
                                    gen.AppendLine("(r) =>");
                                    using (new BraceWrapper(gen))
                                    {
                                        gen.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                        using (new BraceWrapper(gen))
                                        {
                                            gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                            using (new DepthWrapper(gen))
                                            {
                                                gen.AppendLine("return r;");
                                            }
                                            gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                            using (new DepthWrapper(gen))
                                            {
                                                loqui.GenerateTypicalMakeCopy(
                                                    gen,
                                                    retAccessor: $"return ",
                                                    rhsAccessor: new Accessor("r"),
                                                    copyMaskAccessor: copyMaskAccessor,
                                                    deepCopy: deepCopy,
                                                    doTranslationMask: false);
                                            }
                                            gen.AppendLine($"default:");
                                            using (new DepthWrapper(gen))
                                            {
                                                gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                            }
                                        }
                                    }
                                });
                            }
                        }
                        else
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{accessor}.SetTo"))
                            {
                                args.Add((gen) =>
                                {
                                    gen.AppendLine($"rhs.{this.Name}.Items");
                                    using (new DepthWrapper(gen))
                                    {
                                        gen.AppendLine(".Select((r) =>");
                                        using (new BraceWrapper(gen) { AppendParenthesis = true })
                                        {
                                            if (deepCopy)
                                            {
                                                loqui.GenerateTypicalMakeCopy(
                                                    gen,
                                                    retAccessor: $"return ",
                                                    rhsAccessor: new Accessor("r"),
                                                    copyMaskAccessor: copyMaskAccessor,
                                                    deepCopy: deepCopy,
                                                    doTranslationMask: false);
                                            }
                                            else
                                            {
                                                gen.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                                using (new BraceWrapper(gen))
                                                {
                                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                                    using (new DepthWrapper(gen))
                                                    {
                                                        gen.AppendLine("return r;");
                                                    }
                                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                                    using (new DepthWrapper(gen))
                                                    {
                                                        loqui.GenerateTypicalMakeCopy(
                                                            gen,
                                                            retAccessor: $"return ",
                                                            rhsAccessor: new Accessor("r"),
                                                            copyMaskAccessor: copyMaskAccessor,
                                                            deepCopy: deepCopy,
                                                            doTranslationMask: false);
                                                    }
                                                    gen.AppendLine($"default:");
                                                    using (new DepthWrapper(gen))
                                                    {
                                                        gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                });
                            }
                        }
                    },
                    errorMaskAccessor: "errorMask",
                    indexAccessor: this.HasIndex ? this.IndexEnumInt : default(Accessor),
                    doIt: this.CopyNeedsTryCatch);
            }
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool protectedUse)
        {
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.GetName(protectedUse)}.SetTo"))
            {
                args.Add($"(IEnumerable<{this.ValueTypeGen.TypeName(getter: true)}>){rhsAccessorPrefix}.{this.GetName(false)}).Select((i) => i.Copy())");
            }
        }

        public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
        {
            using (var args = new ArgsWrapper(fg,
                $"{accessor}.SetTo"))
            {
                args.Add($"(IEnumerable<{this.ValueTypeGen.TypeName(getter: true)}>){rhs}");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            fg.AppendLine($"return {identifier.Access};");
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            if (this.Nullable)
            {
                fg.AppendLine($"{accessorPrefix}.Unset();");
            }
            else
            {
                fg.AppendLine($"{accessorPrefix}.Clear();");
            }
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
            if (!this.Nullable)
            {
                this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
            }
            else
            {
                fg.AppendLine($"if ({this.NullableAccessor(getter: true, accessor: accessor)} == {this.NullableAccessor(getter: true, accessor: rhsAccessor)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"if ({this.NullableAccessor(getter: true, accessor: accessor)})");
                    using (new BraceWrapper(fg))
                    {
                        this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
                    }
                    fg.AppendLine($"else");
                    using (new BraceWrapper(fg))
                    {
                        this.GenerateForEqualsMask(fg, $"ret.{this.Name}", true);
                    }
                }
                fg.AppendLine($"else");
                using (new BraceWrapper(fg))
                {
                    this.GenerateForEqualsMask(fg, $"ret.{this.Name}", false);
                }
            }
        }

        public void GenerateForEqualsMaskCheck(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor} = EqualsMaskHelper.CacheEqualsHelper"))
            {
                args.Add($"lhs: {accessor}");
                args.Add($"rhs: {rhsAccessor}");
                args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
                args.Add("include: include");
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
                fg.AppendLine($"foreach (var subItem in {accessor})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                    fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                    using (new BraceWrapper(fg))
                    {
                        this.ValueTypeGen.GenerateToString(fg, "Item", new Accessor("subItem.Value"), fgAccessor);
                    }
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
                }
            }
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            if (this.Nullable)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor}.HasBeenSet) return false;");
            }
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }
    }
}
