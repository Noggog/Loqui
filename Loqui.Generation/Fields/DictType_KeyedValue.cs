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
        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";
        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
        
        public override bool CopyNeedsTryCatch => true;

        public override string TypeName => $"SourceSetCache<{BackwardsTypeTuple}>";

        public string TypeTuple => $"{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}";
        public string BackwardsTypeTuple => $"{ValueTypeGen.TypeName}, {KeyTypeGen.TypeName}";

        public string GetterTypeName => this.ValueTypeGen.TypeName;

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            yield return "CSharpExt.Rx";
        }

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
                this.KeyTypeGen = this.ValueTypeGen.TargetObjectGeneration.IterateFields().First((f) => f.Name.Equals(keyAccessorAttr.Value));
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

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{this.HasBeenSetAccessor(identifier)} = {onIdentifier};");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.Unset();");
            }
            fg.AppendLine("break;");
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

        public override string HasBeenSetAccessor(Accessor accessor = null)
        {
            if (accessor == null)
            {
                return $"{this.Property}.HasBeenSet";
            }
            else
            {
                return $"{accessor.PropertyAccess}.HasBeenSet";
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"private readonly SourceSetCache<{BackwardsTypeTuple}> _{this.Name} = new SourceSetCache<{BackwardsTypeTuple}>((item) => item.{this.KeyAccessorString});");
            fg.AppendLine($"public ISourceSetCache<{BackwardsTypeTuple}> {this.Name} => _{this.Name};");

            var member = $"_{this.Name}";
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"{(this.ReadOnly ? "IObservableSetCache" : "ISourceSetCache")}<{this.BackwardsTypeTuple}> {this.ObjectGen.Interface()}.{this.Name} => {member};");
                }
                fg.AppendLine($"IObservableSetCache<{this.BackwardsTypeTuple}> {this.ObjectGen.Interface(getter: true)}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (getter)
            {
                fg.AppendLine($"IObservableSetCache<{this.BackwardsTypeTuple}> {this.Name} {{ get; }}");
            }
            else
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"new {(this.ReadOnly ? "IObservableSetCache" : "ISourceSetCache")}<{this.BackwardsTypeTuple}> {this.Name} {{ get; }}");
                }
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            bool protectedMembers)
        {
            var loqui = this.ValueTypeGen as LoquiType;
            using (var args = new ArgsWrapper(fg,
                $"{accessor.PropertyOrDirectAccess}.SetToWithDefault"))
            {
                args.Add($"rhs.{this.Name}");
                args.Add($"def?.{this.Name}");
                args.Add((gen) =>
                {
                    gen.AppendLine("(r, d) =>");
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
                                    defAccessor: new Accessor("d"),
                                    copyMaskAccessor: copyMaskAccessor);
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

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool protectedUse)
        {
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.GetName(protectedUse)}.SetTo"))
            {
                args.Add($"(IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}.{this.GetName(false)}).Select((i) => i.Copy())");
            }
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.Name}.SetTo"))
            {
                args.Add($"(IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            fg.AppendLine($"return {identifier.DirectAccess};");
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            fg.AppendLine($"{accessorPrefix.PropertyAccess}.Unset();");
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}{accessor.DirectAccess}.SequenceEqual({rhsAccessor.DirectAccess})";
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            fg.AppendLine($"if (!{accessor.DirectAccess}.SequenceEqual({rhsAccessor.DirectAccess})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.HasBeenSet)
            {
                this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
            }
            else
            {
                fg.AppendLine($"if ({this.HasBeenSetAccessor(accessor)} == {this.HasBeenSetAccessor(rhsAccessor)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"if ({this.HasBeenSetAccessor(accessor)})");
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
                $"{retAccessor} = EqualsMaskHelper.DictEqualsHelper"))
            {
                args.Add($"lhs: {accessor}");
                args.Add($"rhs: {rhsAccessor}");
                args.Add($"maskGetter: (k, l, r) => l.GetEqualsMask(r, include)");
                args.Add("include: include");
            }
        }

        public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
        {
            fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool")}();");
            fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({this.Name}).CombineHashCode({hashResultAccessor});");
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"{name} =>\");");
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
            fg.AppendLine($"using (new DepthWrapper(fg))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"foreach (var subItem in {accessor.PropertyOrDirectAccess})");
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

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor.PropertyOrDirectAccess}.HasBeenSet) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            LoquiType loqui = this.ValueTypeGen as LoquiType;
            fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool")}({(this.HasBeenSet ? $"{accessor.PropertyOrDirectAccess}.HasBeenSet" : "true")}, {accessor.PropertyOrDirectAccess}.Values.Select((i) => new MaskItemIndexed<{this.KeyTypeGen.TypeName}, bool, {loqui.GetMaskString("bool")}>(i.{this.KeyAccessorString}, true, i.GetHasBeenSetMask())));");
        }

        public override bool IsNullable()
        {
            return false;
        }
    }
}
