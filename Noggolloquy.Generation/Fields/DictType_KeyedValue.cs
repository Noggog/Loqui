using System;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    class DictType_KeyedValue : TypeGeneration, IDictType
    {
        public NoggType ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen => this.ValueTypeGen;
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen => this.KeyTypeGen;
        public string KeyAccessorString { get; private set; }
        public DictMode Mode => DictMode.KeyedValue;

        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";
        public override string SkipAccessor(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name}.Overall";

        public override bool Imports
        {
            get
            {
                if (!base.Imports) return false;
                if (!ValueTypeGen.Imports || !KeyTypeGen.Imports) return false;
                return true;
            }
        }

        public override bool CopyNeedsTryCatch => true;

        public override string TypeName => $"NotifyingDictionary<{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}>";

        public string TypeTuple => $"{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}";

        public string GetterTypeName => this.ValueTypeGen.TypeName;

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            var keyedValNode = node.Element(XName.Get("KeyedValue", NoggolloquyGenerator.Namespace));
            if (keyedValNode == null)
            {
                throw new ArgumentException("Dict had no keyed value element.");
            }

            if (ObjectGen.LoadField(
                keyedValNode.Elements().FirstOrDefault(),
                false,
                out TypeGeneration valType)
                && valType is NoggType)
            {
                this.ValueTypeGen = valType as NoggType;
            }
            else
            {
                throw new NotImplementedException();
            }

            var keyAccessorAttr = keyedValNode.Attribute(XName.Get("keyAccessor"));
            if (keyAccessorAttr == null)
            {
                throw new ArgumentException("Dict had no key accessor attribute.");
            }

            this.KeyAccessorString = keyAccessorAttr.Value;
            this.KeyTypeGen = this.ValueTypeGen.RefGen.Obj.Fields.First((f) => f.Name.Equals(keyAccessorAttr.Value));
            if (this.KeyTypeGen == null)
            {
                throw new ArgumentException($"Dict had a key accessor attribute that didn't correspond to a field: {keyAccessorAttr.Value}");
            }
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception, bool key)
        {
            NoggType valueNoggType = this.ValueTypeGen as NoggType;
            var valStr = valueNoggType == null ? "Exception" : $"Tuple<Exception, {valueNoggType.RefGen.Obj.GetMaskString("Exception")}>";

            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Value.Add({(key ? "null" : exception)});");
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
            fg.AppendLine($"private readonly INotifyingKeyedCollection<{TypeTuple}> _{this.Name} = new NotifyingKeyedCollection<{TypeTuple}>((item) => item.{this.KeyAccessorString});");
            fg.AppendLine($"public INotifyingKeyedCollection<{TypeTuple}> {this.Name} => _{this.Name};");

            var member = $"_{this.Name}";
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingKeyedCollection{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                }
                fg.AppendLine($"INotifyingKeyedCollectionGetter<{this.TypeTuple}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"new INotifyingKeyedCollection{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.Name} {{ get; }}");
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
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor, protectedMembers);
            }
            fg.AppendLine($"else if ({defaultFallbackAccessor} == null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedMembers)}.Unset({cmdsAccessor}.ToUnsetParams());");
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdsAccessor, protectedMembers);
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

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
            using (new DepthWrapper(fg))
            {
                fg.AppendLine($"((IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}),");
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
