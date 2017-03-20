using System;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    class DictType_KeyedValue : TypeGeneration, IDictType
    {
        public LevType ValueTypeGen;
        TypeGeneration IDictType.ValueTypeGen { get { return this.ValueTypeGen; } }
        public TypeGeneration KeyTypeGen;
        TypeGeneration IDictType.KeyTypeGen { get { return this.KeyTypeGen; } }
        public string KeyAccessorString { get; private set; }
        public DictMode Mode { get { return DictMode.KeyedValue; } }

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

        public string TypeTuple { get { return $"{KeyTypeGen.TypeName}, {ValueTypeGen.TypeName}"; } }

        public string GetterTypeName
        {
            get { return this.ValueTypeGen.Getter; }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            var keyedValNode = node.Element(XName.Get("KeyedValue", NoggolloquyGenerator.Namespace));
            if (keyedValNode == null)
            {
                throw new ArgumentException("Dict had no keyed value element.");
            }

            TypeGeneration valType;
            if (ObjectGen.LoadField(
                    keyedValNode.Elements().FirstOrDefault(),
                    false,
                    out valType)
                    && valType is LevType)
            {
                this.ValueTypeGen = valType as LevType;
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

        public override void SetMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskMemberAccessor}.Overall = {exception};");
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception, bool key)
        {
            LevType valueLevType = this.ValueTypeGen as LevType;
            var valStr = valueLevType == null ? "Exception" : $"Tuple<Exception, {valueLevType.RefGen.Obj.GetMaskString("Exception")}>";
            
            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Value.Add({(key ? "null" : exception)});");
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
            fg.AppendLine($"private readonly INotifyingKeyedCollection<{TypeTuple}> _{this.Name} = new NotifyingKeyedCollection<{TypeTuple}>((item) => item.{this.KeyAccessorString});");
            fg.AppendLine($"public INotifyingKeyedCollection<{TypeTuple}> {this.Name} {{ get {{ return _{this.Name}; }} }}");

            var member = "_" + this.Name;
            using (new RegionWrapper(fg, "Interface Members"))
            {
                fg.AppendLine("INotifyingKeyedCollection" + (this.Protected ? "Getter" : string.Empty) + "<" + this.TypeTuple + "> " + this.ObjectGen.InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
                fg.AppendLine("INotifyingKeyedCollectionGetter<" + this.TypeTuple + "> " + this.ObjectGen.Getter_InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            fg.AppendLine($"new INotifyingKeyedCollection{(this.Protected ? "Getter" : string.Empty)}<{this.TypeTuple}> {this.Name} {{ get; }}");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingKeyedCollectionGetter<{this.TypeTuple}> {this.Name} {{ get; }}");
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor);
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
                fg.AppendLine($"((IEnumerable<{this.ValueTypeGen.TypeName}>){rhsAccessorPrefix}.{this.Name}).Select((i) => i.Copy()),");
                fg.AppendLine($"{cmdAccessor});");
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
