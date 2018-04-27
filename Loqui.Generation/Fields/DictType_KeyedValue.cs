using System;
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

        public override bool IsEnumerable => true;
        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";
        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
        
        public override bool CopyNeedsTryCatch => true;

        public override string TypeName => $"NotifyingDictionary<{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}>";

        public string TypeTuple => $"{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}";

        public string GetterTypeName => this.ValueTypeGen.TypeName;

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

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier}.{this.GetName(internalUse: false)}.HasBeenSet = {onIdentifier};");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"{identifier}.{this.GetName(false)}.Unset({cmdsAccessor});");
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

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"private readonly INotifyingKeyedCollection<{TypeTuple}> _{this.Name} = new NotifyingKeyedCollection<{TypeTuple}>((item) => item.{this.KeyAccessorString});");
            fg.AppendLine($"public INotifyingKeyedCollection<{TypeTuple}> {this.Name} => _{this.Name};");

            var member = $"_{this.Name}";
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingKeyedCollection{(this.ReadOnly ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                }
                fg.AppendLine($"INotifyingKeyedCollectionGetter<{this.TypeTuple}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"new INotifyingKeyedCollection{(this.ReadOnly ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.Name} {{ get; }}");
            }
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingKeyedCollectionGetter<{this.TypeTuple}> {this.Name} {{ get; }}");
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
            var loqui = this.ValueTypeGen as LoquiType;
            using (var args = new ArgsWrapper(fg,
                $"{accessorPrefix}.{this.Name}.SetToWithDefault"))
            {
                args.Add($"rhs.{this.Name}");
                args.Add($"def?.{this.Name}");
                args.Add($"cmds");
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

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor, bool protectedUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedUse)}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"((IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}.{this.GetName(false)}).Select((i) => i.Copy()),");
                fg.AppendLine($"{cmdAccessor});");
            }
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"((IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}),");
                fg.AppendLine($"{cmdsAccessor});");
            }
            fg.AppendLine($"break;");
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

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!{this.Name}.SequenceEqual({rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (!this.HasBeenSet)
            {
                this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
            }
            else
            {
                fg.AppendLine($"if (item.{this.HasBeenSetAccessor} == rhs.{this.HasBeenSetAccessor})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"if (item.{this.HasBeenSetAccessor})");
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
            fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool")}();");
            LoquiType valueLoquiType = this.ValueTypeGen as LoquiType;
            var maskStr = $"MaskItem<bool, {valueLoquiType.GetMaskString("bool")}>";
            fg.AppendLine($"{retAccessor}.Specific = {accessor}.Values.SelectAgainst<{valueLoquiType.TypeName}, {maskStr}>({rhsAccessor}.Values, ((l, r) =>");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{maskStr} itemRet;");
                valueLoquiType.GenerateForEqualsMask(fg, new Accessor("l"), new Accessor("r"), "itemRet");
                fg.AppendLine("return itemRet;");
            }
            fg.AppendLine($"), out {retAccessor}.Overall);");
            fg.AppendLine($"{retAccessor}.Overall = {retAccessor}.Overall && {retAccessor}.Specific.All((b) => b.Overall);");
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
                fg.AppendLine($"foreach (var subItem in {accessor.PropertyOrDirectAccess}.Values)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                    fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                    using (new BraceWrapper(fg))
                    {
                        this.ValueTypeGen.GenerateToString(fg, "Item", new Accessor("subItem"), fgAccessor);
                    }
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
                }
            }
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor.PropertyOrDirectAccess}.HasBeenSet) return false;");
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            LoquiType loqui = this.ValueTypeGen as LoquiType;
            fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(this, "bool")}({accessor.PropertyOrDirectAccess}.HasBeenSet, {accessor.PropertyOrDirectAccess}.Values.Select((i) => new MaskItem<bool, {loqui.GetMaskString("bool")}>(true, i.GetHasBeenSetMask())));");
        }

        public override bool IsNullable()
        {
            return false;
        }
    }
}
